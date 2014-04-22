using SqlBuildManager.Enterprise.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise;
using System.Collections.Generic;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for TableWatchTest and is intended
    ///to contain all TableWatchTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TableWatchNotificationTest
    {

        [TestInitialize()]
        public void Initialize()
        {
            SqlBuildManager.Enterprise.EnterpriseConfiguration cfg = new EnterpriseConfiguration();
            TableWatch tmp = new TableWatch();
            tmp.Description = "For Testing";
            tmp.EmailBody = "Email Body for Testing";
            tmp.EmailSubject = "Email subject for testing";
            Notify n = new Notify();
            n.EMail = "michael@mckechney.com";
            Notify b = new Notify();
            n.EMail = "help@sqlbuildmanager.com";
            tmp.Notify = new Notify[2];
            tmp.Notify[0]= n;
            tmp.Notify[1]= b;
            tmp.Table = new Table[2];
            Table t = new Table();
            t.Name = "SqlBuild_Logging";
            Table a = new Table();
            a.Name = "master";
            tmp.Table[0] = t;
            tmp.Table[1] = a;
            cfg.TableWatch = new TableWatch[1];
            cfg.TableWatch[0] = tmp;
            
            EnterpriseConfigHelper.EnterpriseConfig = cfg;

        }


        /// <summary>
        ///A test for CheckForTableWatch
        ///</summary>
        [TestMethod()]
        public void CheckForTableWatchTest_MatchingTable()
        {
            string script = @"ALTER TABLE
        dbo.SqlBuild_Logging ALTER COLUMN test BIT

";
            List<TableWatch> matches = null; 
            bool expected = false; 
            bool actual;
            actual = SqlBuildManager.Enterprise.Notification.TableWatchNotification.CheckForTableWatch(script, out matches);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(script, matches[0].Script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckForTableWatch
        ///</summary>
        [TestMethod()]
        public void CheckForTableWatchTest_NoMatchingTable()
        {
            string script = @"ALTER TABLE
        NotHERE ALTER COLUMN test BIT

";
            List<TableWatch> matches = null;
            bool expected = true;
            bool actual;
            actual = SqlBuildManager.Enterprise.Notification.TableWatchNotification.CheckForTableWatch(script, out matches);
            Assert.IsNull(matches);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TableWatchNotification Constructor
        ///</summary>
        [TestMethod()]
        public void TableWatchNotificationConstructorTest()
        {
            TableWatchNotification target = new TableWatchNotification();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(TableWatchNotification));
        }
    }
}
