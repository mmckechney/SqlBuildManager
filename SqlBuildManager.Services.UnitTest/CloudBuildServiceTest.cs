using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Services;

namespace SqlBuildManager.Services.UnitTest
{
    [TestClass]
    public class CloudBuildServiceTest
    {
        [TestMethod]
        public void GetServiceStatus_Test()
        {
            CloudBuildService cbs = new CloudBuildService();
            var status = cbs.GetInstanceServiceStatus();
        }
    }
}
