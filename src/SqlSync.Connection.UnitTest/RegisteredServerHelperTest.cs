using SqlSync.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;
namespace SqlSync.Connection.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for RegisteredServerHelperTest and is intended
    ///to contain all RegisteredServerHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RegisteredServerHelperTest
    {

        /// <summary>
        ///A test for RegisteredServerHelper Constructor
        ///</summary>
        [TestMethod()]
        public void RegisteredServerHelperConstructorTest()
        {
            RegisteredServerHelper target = new RegisteredServerHelper();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(RegisteredServerHelper));
        }

        #region DeserializeRegisteredServersTest
        /// <summary>
        ///A test for DeserializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void DeserializeRegisteredServersTest_Good()
        {
            string serverFileContents = Properties.Resources.RegisteredServers_Good;

            RegisteredServers actual;
            actual = RegisteredServerHelper.DeserializeRegisteredServers(serverFileContents);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(RegisteredServers));
            Assert.AreEqual(2, actual.ServerGroup.Length);
            Assert.AreEqual("Group1", actual.ServerGroup[0].Name);
            Assert.AreEqual("Group2", actual.ServerGroup[1].Name);
            Assert.AreEqual("Server4", actual.ServerGroup[1].RegServer[1].Name);
        }

        /// <summary>
        ///A test for DeserializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void DeserializeRegisteredServersTest_Bad()
        {
            string serverFileContents = "This is bad content";

            RegisteredServers actual;
            actual = RegisteredServerHelper.DeserializeRegisteredServers(serverFileContents);
            Assert.IsNull(actual);
            
        }
        /// <summary>
        ///A test for DeserializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void DeserializeRegisteredServersTest_NullContents()
        {
            string serverFileContents = null;

            RegisteredServers actual;
            actual = RegisteredServerHelper.DeserializeRegisteredServers(serverFileContents);
            Assert.IsNull(actual);

        }
        #endregion

        #region GetRegisteredServersTest
      


        #endregion

       

        /// <summary>
        ///A test for ReloadRegisteredServerData
        ///</summary>
        [TestMethod()]
        public void ReloadRegisteredServerDataTest_FileExists()
        {
            string fileName = string.Empty;

            string xmlFile = SqlBuildManager.Logging.Configure.AppDataPath + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            bool expected = false;
            bool actual;
            actual = RegisteredServerHelper.ReloadRegisteredServerData(fileName);
            Assert.AreEqual(expected, actual);

        }

        #region SaveRegisteredServersTest
        /// <summary>
        ///A test for SaveRegisteredServers
        ///</summary>
        [TestMethod()]
        public void SaveRegisteredServersTest_GoodSave()
        {
            RegisteredServers regServers = new RegisteredServers();
            regServers.ServerGroup = new ServerGroup[2];
            regServers.ServerGroup[0] = new ServerGroup() { Name = "Test" };
            regServers.ServerGroup[1] = new ServerGroup() { Name = "Test2" };

            bool expected = true; 
            bool actual;
            RegisteredServerHelper.RegisteredServerFileName = Path.GetTempFileName();
            actual = RegisteredServerHelper.SaveRegisteredServers(regServers);
            Assert.AreEqual(expected, actual);


            Assert.IsTrue(File.Exists(RegisteredServerHelper.RegisteredServerFileName));

            string contents = File.ReadAllText(RegisteredServerHelper.RegisteredServerFileName);
            Regex regexServer = new Regex("ServerGroup", RegexOptions.None);
            Assert.IsTrue(regexServer.Matches(contents).Count == 2);

            File.Delete(RegisteredServerHelper.RegisteredServerFileName);
        }

        /// <summary>
        ///A test for SaveRegisteredServers
        ///</summary>
        [TestMethod()]
        public void SaveRegisteredServersTest_NullRegisteredServersSave()
        {
            RegisteredServers regServers = null;

            bool expected = false;
            bool actual;
            RegisteredServerHelper.RegisteredServerFileName = Path.GetTempFileName();
            actual = RegisteredServerHelper.SaveRegisteredServers(regServers);
            Assert.AreEqual(expected, actual);

       }
        #endregion

        #region SerializeRegisteredServersTest
        /// <summary>
        ///A test for SerializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void SerializeRegisteredServersTest_GoodSerialization()
        {
            RegisteredServers regServers = new RegisteredServers();
            regServers.ServerGroup = new ServerGroup[2];
            regServers.ServerGroup[0] = new ServerGroup() { Name = "Test" };
            regServers.ServerGroup[1] = new ServerGroup() { Name = "Test2" };

            string fileName = Path.GetTempFileName();
            bool expected = true;
            bool actual;
            actual = RegisteredServerHelper.SerializeRegisteredServers(regServers, fileName);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(File.Exists(fileName));

            string contents = File.ReadAllText(fileName);
            Regex regexServer = new Regex("ServerGroup", RegexOptions.None);
            Assert.IsTrue(regexServer.Matches(contents).Count == 2);

            File.Delete(fileName);
        }

        /// <summary>
        ///A test for SerializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void SerializeRegisteredServersTest_ReadOnlyDestination()
        {
            RegisteredServers regServers = new RegisteredServers();
            regServers.ServerGroup = new ServerGroup[2];
            regServers.ServerGroup[0] = new ServerGroup() { Name = "Test" };
            regServers.ServerGroup[1] = new ServerGroup() { Name = "Test2" };

            string fileName = Path.GetTempFileName();
            File.SetAttributes(fileName, FileAttributes.ReadOnly);
            bool expected = false;
            bool actual;
            actual = RegisteredServerHelper.SerializeRegisteredServers(regServers, fileName);
            Assert.AreEqual(expected, actual);

            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }

        /// <summary>
        ///A test for SerializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void SerializeRegisteredServersTest_SerializationWithNullObject()
        {
            RegisteredServers regServers = null;

            string fileName = Path.GetTempFileName();
            bool expected = false;
            bool actual;
            actual = RegisteredServerHelper.SerializeRegisteredServers(regServers, fileName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SerializeRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void SerializeRegisteredServersTest_SerializationWithEmptyFileName()
        {
            RegisteredServers regServers = new RegisteredServers();

            string fileName = string.Empty;
            bool expected = false;
            bool actual;
            actual = RegisteredServerHelper.SerializeRegisteredServers(regServers, fileName);
            Assert.AreEqual(expected, actual);
        }
        #endregion


        /// <summary>
        ///A test for RegisteredServerFileName
        ///</summary>
        [TestMethod()]
        public void RegisteredServerFileNameTest()
        {
            string expected = Path.GetTempFileName();
            string actual;
            RegisteredServerHelper.RegisteredServerFileName = expected;
            actual = RegisteredServerHelper.RegisteredServerFileName;
            Assert.AreEqual(expected, actual);
            
        }
    }
}
