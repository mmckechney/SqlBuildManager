using SqlSync.SqlBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;

namespace SqlBuildManager.ScriptHandling.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for CommandLine_ArgumentsTest and is intended
    ///to contain all CommandLine_ArgumentsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandLine_ArgumentsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ParseArguments
        ///</summary>
        [TestMethod()]
        public void ParseArgumentsTest()
        {
            string[] Args = new string[] { "/testNoVal", "/testColon:colonValue", "/testEqual=equalValue", "/testMiddleParam","/testSpace","spaceValue","/testSpaceQuoted","\"Space Value Quoted\"","/testEqualQuoted=\"Equal Quoted\"","/testColonQuoted:\"Colon Quoted\"","/testEndParam" };
            StringDictionary actual = CommandLine.Arguments.ParseArguments(Args);
            Assert.AreEqual("colonValue",actual["testColon"]);
            Assert.AreEqual("true",actual["testNoVal"]);
            Assert.AreEqual("equalValue",actual["testEqual"]);
            Assert.AreEqual("spaceValue",actual["testSpace"]);
            Assert.AreEqual("Space Value Quoted",actual["testSpaceQuoted"]);
            Assert.AreEqual("Equal Quoted",actual["testEqualQuoted"]);
            Assert.AreEqual("Colon Quoted",actual["testColonQuoted"]);
            Assert.AreEqual("true", actual["testEndParam"]);
            Assert.AreEqual("true", actual["testMiddleParam"]);
        }

        /// <summary>
        ///A test for Arguments Constructor
        ///</summary>
        [TestMethod()]
        public void CommandLine_ArgumentsConstructorTest()
        {
            CommandLine.Arguments target = new CommandLine.Arguments();
            Assert.AreEqual(typeof(CommandLine.Arguments), target.GetType());
        }
    }
}
