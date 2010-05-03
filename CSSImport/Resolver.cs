using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CSSImport
{
    public class Resolver
    {
        List<string> ImportedFiles = new List<string>();

        public Resolver() { }

        public string ProcessFile(string path)
        {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }

            path = Path.GetFullPath(path);

            if (ImportedFiles.Contains(path)) {
                return "/* Already Imported */";
            }

            ImportedFiles.Add(path);

            // get css file content
            string css = File.ReadAllText(path);

            // find all import statements within the css
            string[] imports = GetImportsInString(File.ReadAllText(path), @"@import[ ]+url\((""|')?[\w-\/\.]+(""|')?\)(;)?");

            // iterate through and append the result of ProcessFile on each.
            foreach (string import in imports) {
                // extract path out of import statement
                Match importPathMatch = Regex.Match(import, @"(?<=@import[ ]+url\((""|')?)[\w-\/\.]+(?=(""|')?\)(;)?)", RegexOptions.IgnoreCase);

                if (importPathMatch != null) {
                    string importPath = Path.Combine(Path.GetDirectoryName(path), importPathMatch.Value.Replace(@"\", @"/"));
                    css = css.Replace(import, "/* Import: " + importPathMatch.Value + " */\n" + ProcessFile(importPath)) + "\n";
                }
            }

            return css;
        }

        string[] GetImportsInString(string content, string pattern)
        {
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            List<string> result = new List<string>();
            foreach (Match m in matches) {
                result.Add(m.Value);
            }
            return result.ToArray();
        }
    }
}
