using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CSSImport
{
    /// <summary>
    /// Usage:
    /// <code>
    /// Resolver r = new Resolver(@"C:\Projects\Rad\");
    /// string css = r.ProcessFile(cssFilePath);
    /// </code>
    /// </summary>
    public class Resolver
    {
        List<string> ImportedFiles = new List<string>();
        string CssFilePath { get; set; }

        public Resolver() { }

        /// <summary>
        /// Processes the file and returns the merged results. Also
        /// collects an array of merged files which is available via the
        /// GetListOfFiles method.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string ProcessFile(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }

            ImportedFiles.Clear();

            // Store the value of the file path
            CssFilePath = path;
            string result = ProcessFileRecursive(path);

            // Reset the value of the file path
            CssFilePath = null;

            return result;
        }

        /// <summary>
        /// Gets the list of files imported during the last ProcessFile call.
        /// </summary>
        /// <returns></returns>
        public string[] GetListOfFiles()
        {
            return ImportedFiles.ToArray();
        }

        string ProcessFileRecursive(string path)
        {
            path = Path.GetFullPath(path);

            if (ImportedFiles.Contains(path)) {
                return "/* Already Imported */";
            }

            ImportedFiles.Add(path);

            if (!File.Exists(path)) {
                // Silently ignore nonexistant css files
                // TODO: should we throw a FileNotFoundException ?
                return string.Empty;
            }

            // get css file content
            string css = File.ReadAllText(path);

            // replace all url's which aren't part of import statements
            string[] urls = Search(css, @"(?<!@import[ ]+)url\((""|')?[\w-\/\.]+(""|')?\)");
            string currentDirectory = Path.GetDirectoryName(path);

            foreach (string url in urls) {
                Match urlPathMatch = Regex.Match(url, @"(?<=url\((""|')?)[\w-\/\.]+(?=(""|')?\))", RegexOptions.IgnoreCase);

                if (urlPathMatch != null) {
                    string urlPath = urlPathMatch.Value;

                    // Do not rewrite urls that start with "/" or "http"
                    bool rewrite = !Regex.IsMatch(urlPath, @"^(/|http(s)?:)", RegexOptions.IgnoreCase);

                    if (rewrite) {
                        string sourceDir = Path.GetDirectoryName(CssFilePath);
                        string currentDir = Path.GetDirectoryName(path);

                        // The prefix is the current directory minus the source directory
                        string prefix = Regex.Replace(currentDir, "^" + Regex.Escape(sourceDir), "", RegexOptions.IgnoreCase);

                        if (!string.IsNullOrEmpty(prefix)) {
                            // Convert windows-style path to url path
                            prefix = Regex
                                // Remove leading slashes
                                .Replace(prefix, @"^\\", "", RegexOptions.IgnoreCase)
                                // Make back-slashes into forward slashes
                                .Replace(@"\", "/");

                            // Join prefix and url, ensuring that no back-slashes remain
                            string rewrittenPath = Regex.Replace((prefix + "/" + urlPath), @"\/+", "/");

                            // TODO: correctly replace path
                            // Replace original url in css with new rewritten url.
                            css = Regex.Replace(css, Regex.Escape(url), "url(" + rewrittenPath + ")", RegexOptions.IgnoreCase);
                        }
                    }
                }

                // 1. rewrite each url so that it is relative to the same directory as CssFilePath.
                // 2. Ignore urls that start with "/" since they're ROOT
                // 3. Ignore urls that start with "http(s)?://" since they're ROOT
                // 4. "./path..." and "path..." directories should be rewritten to the cwd

                // NOTE: it may be as simple as prepending the current url with the
                //       CWD, which is the full current path minus DIR of CssFilePath.
            }

            // find all import statements within the css
            string[] imports = Search(css, @"@import[ ]+url\((""|')?[\w-\/\.]+(""|')?\)(;)?");

            // iterate through and append the result of ProcessFile on each.
            foreach (string import in imports) {
                // extract path out of import statement
                Match importPathMatch = Regex.Match(import, @"(?<=@import[ ]+url\((""|')?)[\w-\/\.]+(?=(""|')?\)(;)?)", RegexOptions.IgnoreCase);

                if (importPathMatch != null) {
                    string importPath = Path.Combine(Path.GetDirectoryName(path), importPathMatch.Value.Replace(@"/", @"\"));
                    css = css.Replace(import, "/* Import: " + importPathMatch.Value + " */\n" + ProcessFileRecursive(importPath)) + "\n";
                }
            }

            return css;
        }

        /// <summary>
        /// Searches `content` string using `pattern`. A String array of match values is returned.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        string[] Search(string content, string pattern)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            List<string> result = new List<string>();
            foreach (Match m in matches) {
                result.Add(m.Value);
            }
            return result.ToArray();
        }

        public static string Crush(string cssFilePath)
        {
            return new Resolver().ProcessFile(cssFilePath);
        }
    }
}
