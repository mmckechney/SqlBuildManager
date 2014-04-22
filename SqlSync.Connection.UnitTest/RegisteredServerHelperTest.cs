﻿using SqlSync.Connection;
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
            actual = RegisteredServerHelper_Accessor.DeserializeRegisteredServers(serverFileContents);
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
            actual = RegisteredServerHelper_Accessor.DeserializeRegisteredServers(serverFileContents);
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
            actual = RegisteredServerHelper_Accessor.DeserializeRegisteredServers(serverFileContents);
            Assert.IsNull(actual);

        }
        #endregion

        #region GetRegisteredServersTest
        /// <summary>
        ///A test for GetRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void GetRegisteredServersTest_GoodFile()
        {
            string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            File.WriteAllText(xmlFile, Properties.Resources.RegisteredServers_Good);
            RegisteredServerHelper.RegisteredServerFileName = string.Empty;
            RegisteredServers actual;
            actual = RegisteredServerHelper_Accessor.GetRegisteredServers();
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(RegisteredServers));
            Assert.AreEqual(2, actual.ServerGroup.Length);
            Assert.AreEqual("Group1", actual.ServerGroup[0].Name);
            Assert.AreEqual("Group2", actual.ServerGroup[1].Name);
            Assert.AreEqual("Server4", actual.ServerGroup[1].RegServer[1].Name);
        }

        /// <summary>
        ///A test for GetRegisteredServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.Connection.dll")]
        public void GetRegisteredServersTest_BadFile()
        {
            string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            File.WriteAllText(xmlFile, "Bad File contents");

            RegisteredServers actual;
            actual = RegisteredServerHelper_Accessor.GetRegisteredServers();
            Assert.IsNull(actual);
       }
        #endregion

        /// <summary>
        ///A test for ReloadRegisteredServerData
        ///</summary>
        [TestMethod()]
        public void ReloadRegisteredServerDataTest_GoodSaveAndLoad()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, Properties.Resources.RegisteredServers_Good);

            string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RegisteredServers.xml";
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);

            bool expected = true; 
            bool actual;
            actual = RegisteredServerHelper.ReloadRegisteredServerData(fileName);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(File.Exists(fileName));
        }

        /// <summary>
        ///A test for ReloadRegisteredServerData
        ///</summary>
        [TestMethod()]
        public void ReloadRegisteredServerDataTest_FileExists()
        {
            string fileName = string.Empty;

            string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\RegisteredServers.xml";
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
            actual = RegisteredServerHelper_Accessor.SerializeRegisteredServers(regServers, fileName);
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
            actual = RegisteredServerHelper_Accessor.SerializeRegisteredServers(regServers, fileName);
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
            actual = RegisteredServerHelper_Accessor.SerializeRegisteredServers(regServers, fileName);
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
            actual = RegisteredServerHelper_Accessor.SerializeRegisteredServers(regServers, fileName);
            Assert.AreEqual(expected, actual);
        }
        #endregion

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

            RegisteredServerHelper.RegisteredServerFileName = string.Empty;
            RegisteredServers actual;
            actual = RegisteredServerHelper.RegisteredServerData;
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(RegisteredServers));
            Assert.AreEqual(2, actual.ServerGroup.Length);
            Assert.AreEqual("Group1", actual.ServerGroup[0].Name);
            Assert.AreEqual("Group2", actual.ServerGroup[1].Name);
            Assert.AreEqual("Server4", actual.ServerGroup[1].RegServer[1].Name);
        }

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
