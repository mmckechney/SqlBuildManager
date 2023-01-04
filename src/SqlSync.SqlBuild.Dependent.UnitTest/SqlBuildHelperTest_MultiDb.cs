using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    class SqlBuildHelperTest_MultiDb
    {

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
    }
}
