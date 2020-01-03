using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.Dependent.UnitTest
{
    [TestClass]
    public class RegistreredServerHelperTest
    {
        /// <summary>
        ///A test for GetRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void GetRegisteredServersTest_GoodFile()
        {
            string xmlFile = SqlBuildManager.Logging.Configure.AppDataPath + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            File.WriteAllText(xmlFile, Properties.Resources.RegisteredServers_Good);
            RegisteredServerHelper.RegisteredServerFileName = string.Empty;
            RegisteredServers actual;
            actual = RegisteredServerHelper.GetRegisteredServers();
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(RegisteredServers));
            Assert.AreEqual(2, actual.ServerGroup.Length);
            Assert.AreEqual("Group1", actual.ServerGroup[0].Name);
            Assert.AreEqual("Group2", actual.ServerGroup[1].Name);
            Assert.AreEqual("Server4", actual.ServerGroup[1].RegServer[1].Name);
        }

        /// <summary>
        ///A test for ReloadRegisteredServerData
        ///</summary>
        [TestMethod()]
        public void ReloadRegisteredServerDataTest_GoodSaveAndLoad()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, Properties.Resources.RegisteredServers_Good);

            string xmlFile = SqlBuildManager.Logging.Configure.AppDataPath + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            bool expected = true;
            bool actual;
            actual = RegisteredServerHelper.ReloadRegisteredServerData(fileName);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(File.Exists(fileName));
        }

        /// <summary>
        ///A test for GetRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void GetRegisteredServersTest_BadFile()
        {
            string xmlFile = SqlBuildManager.Logging.Configure.AppDataPath + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            File.WriteAllText(xmlFile, "Bad File contents");

            RegisteredServers actual;
            actual = RegisteredServerHelper.GetRegisteredServers();
            Assert.IsNull(actual);
        }


        /// <summary>
        ///A test for RegisteredServerData
        ///</summary>
        [TestMethod()]
        public void RegisteredServerDataTest()
        {
            string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            File.WriteAllText(xmlFile, Properties.Resources.RegisteredServers_Good);

            RegisteredServerHelper.RegisteredServerFileName = xmlFile;
            RegisteredServers actual;
            actual = RegisteredServerHelper.RegisteredServerData;
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(RegisteredServers));
            Assert.AreEqual(2, actual.ServerGroup.Length);
            Assert.AreEqual("Group1", actual.ServerGroup[0].Name);
            Assert.AreEqual("Group2", actual.ServerGroup[1].Name);
            Assert.AreEqual("Server4", actual.ServerGroup[1].RegServer[1].Name);
        }
    }
}
