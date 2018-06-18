using SqlBuildManager.ScriptHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlSync.SqlBuild;
using System.IO;
namespace SqlBuildManager.ScriptHandling.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ScriptTagProcessingTest and is intended
    ///to contain all ScriptTagProcessingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptTagProcessingTest
    {
        static List<string> regex = null;
        [ClassInitialize()]
        public static void SetRegex(TestContext context)
        {
            regex = new List<string>();
            regex.Add(@"\bCR *-*#* *\d{3,10}");
            regex.Add(@"\bP *-*#* *\d{3,10}");
        }
        /// <summary>
        ///A test for ScriptTagProcessing Constructor
        ///</summary>
        [TestMethod()]
        public void ScriptTagProcessingConstructorTest()
        {
            ScriptTagProcessing target = new ScriptTagProcessing();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target,typeof(ScriptTagProcessing));
        }

        #region InferScriptTagFromFileContentsTest
        /// <summary>
        ///A test for InferScriptTagFromFileContents
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileContents_GoodMatchTest()
        {
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileContents(scriptContents, regexFormats);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for InferScriptTagFromFileContents
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileContentsTest_GetLastInMixedMatches()
        {
            string scriptContents = Properties.Resources.LastTagFromMixedContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "P23533";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileContents(scriptContents, regexFormats);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for InferScriptTagFromFileContents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ScriptHandling.dll")]
        public void InferScriptTagFromFileContents_NullRegexTest()
        {
            string scriptContents = string.Empty;
            List<string> regexFormats = null;
            string expected = string.Empty;
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileContents(scriptContents, regexFormats);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region InferScriptTagFromFileNameTest
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessNoSpaces()
        {
            string scriptFileName = @"C:\mypath\path2\CR2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);
           
        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithSpaces()
        {
            string scriptFileName = @"C:\mypath\path2\CR 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithDash()
        {
            string scriptFileName = @"C:\mypath\path2\CR-2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithDashAndSpace()
        {
            string scriptFileName = @"C:\mypath\path2\CR- 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithNumber()
        {
            string scriptFileName = @"C:\mypath\path2\CR# 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithNumberAndSpace()
        {
            string scriptFileName = @"C:\mypath\path2\CR# 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_NoCRMatch()
        {
            string scriptFileName = @"C:\mypath\path2\No CR test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_NoCrMatch_EmbeddedText()
        {
            string scriptFileName = @"C:\mypath\path2\MyCR# 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for InferScriptTagFromFileName
        ///</summary>
        [TestMethod()]
        public void InferScriptTagFromFileNameTest_SuccessWithPNumber()
        {
            string scriptFileName = @"C:\mypath\path2\P 2123456-test script.sql";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string expected = "P2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTagFromFileName(scriptFileName, regexFormats);
            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region InferScriptTagTest
        // <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_ScriptName()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.ScriptName;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);
 
        }

        [TestMethod()]
        public void InferScriptTagTest_NameOverText()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.NameOverText;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_NameOverText_GetFromText()
        {
            string scriptPathAndName = @"C:\mypath\path2\test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.NameOverText;
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_ScriptText()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.ScriptText;
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_TextOverName()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.TextOverName;
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_TextOverName_GetFromName()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = "This is content that doesn't have a single tag in it.";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.TextOverName;
            string expected = "CR2123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_NothingInEither()
        {
            string scriptPathAndName = @"C:\mypath\path2\NothingHereCR2123456-test script.sql";
            string scriptContents = @"There is nothing here that 
should match either the CR number 1234 content match or the 
P number 1234 content match";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.TextOverName;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_EmptyRegexListSet()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = new List<string>();
            TagInferenceSource source = TagInferenceSource.TextOverName;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_NullRegexListSet()
        {
            string scriptPathAndName = @"C:\mypath\path2\CR2123456-test script.sql";
            string scriptContents = Properties.Resources.TagFromContents;
            List<string> regexFormats = null;
            TagInferenceSource source = TagInferenceSource.TextOverName;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void InferScriptTagTest_SourceIsNone()
        {
            string scriptPathAndName = @"C:\mypath\path2\NothingHereCR2123456-test script.sql";
            string scriptContents = @"There is nothing here that 
should match either the CR number 1234 content match or the 
P number 1234 content match";
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            TagInferenceSource source = TagInferenceSource.None;
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(scriptPathAndName, scriptContents, regexFormats, source);
            Assert.AreEqual(expected, actual);

        }



        #endregion

        #region InferScriptTagTest - Overloaded
        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_NoTagInferenceSource()
        {
            TagInferenceSource source = TagInferenceSource.None;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = "CR 343423 Test Script.sql";
            string scriptPath = @"C:\test";
            string expected = string.Empty; 
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_TagInferenceFromScriptText ()
        {
            TagInferenceSource source = TagInferenceSource.ScriptText;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + @"CR 123456 test me.sql";
            File.WriteAllText(scriptName, Properties.Resources.TagFromContents);
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            if (File.Exists(scriptName))
                File.Delete(scriptName);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_TagInferenceFromTextOverName()
        {
            TagInferenceSource source = TagInferenceSource.TextOverName;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + @"CR 123456 test me.sql";
            File.WriteAllText(scriptName, Properties.Resources.TagFromContents);
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "CR987654";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            if (File.Exists(scriptName))
                File.Delete(scriptName);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_TagInferenceFromNameOverText()
        {
            TagInferenceSource source = TagInferenceSource.NameOverText;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + @"CR 123456 test me.sql";
            File.WriteAllText(scriptName, Properties.Resources.TagFromContents);
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "CR123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            if (File.Exists(scriptName))
                File.Delete(scriptName);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_TagInferenceFromScriptName()
        {
            TagInferenceSource source = TagInferenceSource.ScriptName;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + @"CR 123456 test me.sql";
            File.WriteAllText(scriptName, Properties.Resources.TagFromContents);
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "CR123456";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            if (File.Exists(scriptName))
                File.Delete(scriptName);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_FileDoesntExist()
        {
            TagInferenceSource source = TagInferenceSource.TextOverName;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + Guid.NewGuid().ToString();
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_TextOverNameNothingFound()
        {
            TagInferenceSource source = TagInferenceSource.ScriptName;
            List<string> regexFormats = ScriptTagProcessingTest.regex;
            string scriptName = Path.GetTempPath() + @"empty tagless test me.sql";
            File.WriteAllText(scriptName, "This doesn't have a tag in it");
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = "";
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for InferScriptTag
        ///</summary>
        [TestMethod()]
        public void InferScriptTagTest_NullRegex()
        {
            TagInferenceSource source = TagInferenceSource.ScriptName;
            List<string> regexFormats = null;
            string scriptName = Path.GetTempPath() + @"CR 123456 test me.sql";
            File.WriteAllText(scriptName, Properties.Resources.TagFromContents);
            string scriptPath = Path.GetDirectoryName(scriptName);
            string expected = string.Empty;
            string actual;
            actual = ScriptTagProcessing.InferScriptTag(source, regexFormats, scriptName, scriptPath);

            if (File.Exists(scriptName))
                File.Delete(scriptName);

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
