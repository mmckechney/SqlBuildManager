using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation;
using System.Collections.Generic;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.UnitTest;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ThreadedRunnerTest and is intended
    ///to contain all ThreadedRunnerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ThreadedRunnerTest
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

        private static List<Initialization> initColl;

        [ClassInitialize()]
        public static void InitializeTests(TestContext testContext)
        {
            initColl = new List<Initialization>();
        }
        private Initialization GetInitializationObject()
        {
            Initialization init = new Initialization();
            initColl.Add(init);
            return init;
        }
        [ClassCleanup()]
        public static void Cleanup()
        {
            for (int i = 0; i < initColl.Count; i++)
            {
                initColl[i].Dispose();
            }
        }
        
        //
        ///// <summary>
        /////A test for RunDatabaseBuild
        /////</summary>
        //[TestMethod()]
        //public void RunDatabaseBuildTest()
        //{
        //    Initialization init = GetInitializationObject();
        //    string serverName = init.serverName;
            
        //    //Set overrides
        //    DatabaseOverride ovr1 = new DatabaseOverride(init.testDatabaseNames[0], init.testDatabaseNames[0]);
        //    DatabaseOverride ovr2 = new DatabaseOverride(init.testDatabaseNames[0], init.testDatabaseNames[1]);

        //    List<DatabaseOverride> overrides = new List<DatabaseOverride>();
        //    overrides.Add(ovr1);
        //    overrides.Add(ovr2);

        //    string sbmFile = init.GetTrulyUniqueFile();
        //    string multiDbFile = init.GetTrulyUniqueFile();
        //    string tempDir = Path.GetTempPath() + @"\" + System.Guid.NewGuid().ToString();
        //    Directory.CreateDirectory(tempDir);
        //    //Copy the resource files to temp dir...
        //    File.WriteAllBytes(sbmFile,Properties.Resources.NoTrans_MultiDb_sbm);
        //    File.WriteAllBytes(multiDbFile,Properties.Resources.NoTrans_MultiDb_multidb);

        //    CommandLineArgs cmdArgs = new CommandLineArgs();
        //    cmdArgs.BuildFileName = sbmFile;
        //    cmdArgs.BuildDesignated = true;
        //    cmdArgs.MultiDbRunConfigFileName = multiDbFile;
        //    cmdArgs.OverrideDesignated = true;
        //    cmdArgs.RootLoggingPath = tempDir;

            
        //    ThreadedRunner target = new ThreadedRunner(serverName, overrides, cmdArgs); // TODO: Initialize to an appropriate value
        //    target.RunDatabaseBuild();

        //    Assert.IsTrue(File.Exists(tempDir + @"\errors.html"));
        //}
    }
}
