using System;
using SqlBuildManager.AzureStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace SqlBuildManager.AzureStorage.UnitTest
{
    [TestClass]
    public class RoleManagerTest
    {
        string connectionstring;
        [TestInitialize]
        public void SetConnectionString()
        {
            this.connectionstring = ConfigurationManager.AppSettings["StorageConnectionString"];
        }
        [TestMethod]
        public void ManagerConstruction_Test()
        {

            RoleManager rm = new RoleManager(this.connectionstring);

            Assert.IsNotNull(rm);
        }

        [TestMethod]
        public void InsertAndDeleteNewRole_Test()
        {
            RoleManager rm = new RoleManager(this.connectionstring);
            bool result = rm.InsertCloudRoleEntity("MyTestVM", "10.10.10.10");

            Assert.IsTrue(result);

            result = rm.DeleteCloudRoleEntity("MyTestVM");
            Assert.IsTrue(result);
        }

    }
}
