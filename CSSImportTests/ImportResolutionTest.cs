using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSSImport;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;

namespace CSSImportTests
{
    /// <summary>
    /// Summary description for ImportResolutionTest
    /// </summary>
    [TestClass]
    public class ImportResolutionTest
    {
        Resolver resolver = new Resolver();

        public ImportResolutionTest() { }

        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestCSSFileImportMerge()
        {
            string path = Path.GetFullPath(Path.Combine(TestContext.TestDir, @"..\..\"));
            string projectName = GetType().Assembly.FullName.Split(new string[]{","}, StringSplitOptions.None)[0];
            path = Path.Combine(path, projectName + @"\test-files\main.css");

            if (!File.Exists(path)) {
                throw new FileNotFoundException("Could not find css file to process.", path);
            }

            string result = resolver.ProcessFile(path);

            Assert.IsTrue(!string.IsNullOrEmpty(result), "Resolver did not return any content");
            Assert.IsTrue(Regex.IsMatch(result, @"one(\r)?\n"), "'one' was not found.");
            Assert.IsTrue(Regex.IsMatch(result, @"Already Imported"), "already imported note was not found.");
            Assert.IsTrue(Regex.IsMatch(result, @"two(\r)?\n"), "'two' was not found.");
            Assert.IsTrue(Regex.IsMatch(result, @"three(\r)?\n"), "'three' was not found.");

            MatchCollection remaining = Regex.Matches(result, @"@import");
            Assert.AreEqual(1, remaining.Count,
                "Remaining @import statements should be 1 but is " + remaining.Count);
        }

        [TestMethod]
        public void TestCSSImportHandler()
        {
            string result = string.Empty;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://localhost/css/main.css");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream stream = response.GetResponseStream();
            StringBuilder sb = new StringBuilder();
            int count = 0;
            byte[] buffer = new byte[8192];

            do {
                // fill the buffer with data
                count = stream.Read(buffer, 0, buffer.Length);

                // make sure we read some data
                if (count != 0) {
                    // translate from bytes to ASCII text
                    result = Encoding.UTF8.GetString(buffer, 0, count);

                    // continue building the string
                    sb.Append(result);
                }
            }
            while (count > 0); // any more data to read?

            Assert.IsFalse(string.IsNullOrEmpty(result.Trim()));
        }
    }
}
