using SqlBuildManager.ServiceClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using System.Collections.Generic;
using System;
namespace SqlBuildManager.ServiceClient.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for BuildServiceManagerTest and is intended
    ///to contain all BuildServiceManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BuildServiceManagerTest
    {

        #region SplitLoadEvenlyTest
        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_EvenDbTargets_EvenExeServers()
        {
            string[] evenConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4" };

            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = evenConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("ExeServer2", "http://nothomeA.com", "tcp://nothomeA.com", Protocol.Tcp);
            executionServers.Add(exeServer2);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("SERVER2") > -1);

            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("SERVER3") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("SERVER4") > -1);



        }

        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_EvenDbTargets_OddExeServers()
        {
            string[] evenConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4" };

            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = evenConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();

            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("ExeServer2", "http://nothomeA.com", "tcp://nothomeA.com", Protocol.Tcp);
            executionServers.Add(exeServer2);
            ServerConfigData exeServer3 = new ServerConfigData("ExeServer3", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer3);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual(1, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);

            Assert.AreEqual(1, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("SERVER2") > -1);

            Assert.AreEqual(2, actual[exeServer3].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer3].MultiDbTextConfig[0].IndexOf("SERVER3") > -1);
            Assert.IsTrue(actual[exeServer3].MultiDbTextConfig[1].IndexOf("SERVER4") > -1);

        }

        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_OddDbTargets_EvenExeServers()
        {
            string[] oddConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4","SERVER5:default,target;default5,target5" };
            
            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = oddConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("ExeServer2", "http://nothomeA.com", "tcp://nothomeA.com", Protocol.Tcp);
            executionServers.Add(exeServer2);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("SERVER2") > -1);

            Assert.AreEqual(3, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("SERVER3") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("SERVER4") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[2].IndexOf("SERVER5") > -1);

        }

        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_OddDbTargets_OddExeServers()
        {
            string[] oddConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4","SERVER5:default,target;default5,target5" };

            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = oddConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("ExeServer2", "http://nothomeA.com", "tcp://nothomeA.com", Protocol.Tcp);
            executionServers.Add(exeServer2);
            ServerConfigData exeServer3 = new ServerConfigData("ExeServer3", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer3);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(3, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("SERVER2") > -1);

            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("SERVER3") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("SERVER4") > -1);

            Assert.AreEqual(1, actual[exeServer3].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer3].MultiDbTextConfig[0].IndexOf("SERVER5") > -1);

        }

        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_EvenDbTargets_OneExeServers()
        {
            string[] evenConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4" };

            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = evenConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(1, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(4, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[3].IndexOf("SERVER4") > -1);

        }

        /// <summary>
        ///A test for SplitLoadEvenly
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadEvenlyTest_OddDbTargets_OneExeServers()
        {
            string[] oddConfig = new string[]{"SERVER1:default,target;default1,target1","SERVER2:default,target;default2,target2",
                    "SERVER3:default,target;default3,target3","SERVER4:default,target;default4,target4","SERVER5:default,target;default5,target5" };
            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = oddConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("ExeServer1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadEvenly(unifiedSettings, executionServers);
            Assert.AreEqual(1, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(5, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("SERVER1") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[4].IndexOf("SERVER5") > -1);

        }

        #endregion

        #region SplitLoadToOwningServers
        /// <summary>
        ///A test for SplitLoadToOwningServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadToOwningServers_MathchesMultipleServersAndTargets()
        {
            string[] oddConfig = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b",
                    "SERVER2:default,target;default2a,target2a","SERVER2:default,target;default2b,target2b" };
            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = oddConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer2);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);
            Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("target1a") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("target1b") > -1);

            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("target2a") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("target2b") > -1);

        }


        /// <summary>
        ///A test for SplitLoadToOwningServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadToOwningServers_LeavesOffUnassignedServer()
        {
            string[] oddConfig = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b",
                    "SERVER2:default,target;default2a,target2a","SERVER2:default,target;default2b,target2b", "SERVER3:default,target;default3b,target3b" };
            BuildServiceManager target = new BuildServiceManager(); // TODO: Initialize to an appropriate value
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.MultiDbTextConfig = oddConfig;

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData("SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData("SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer2);

            IDictionary<ServerConfigData, BuildSettings> actual;
            actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);
            Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("target1a") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("target1b") > -1);

            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("target2a") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("target2b") > -1);

        }


        /// <summary>
        ///A test for SplitLoadToOwningServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadToOwningServersTest_InstanceNameTest()
        {
            BuildServiceManager target = new BuildServiceManager();
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.DistributionType = DistributionType.OwnMachineName;
            unifiedSettings.MultiDbTextConfig  = new string[]{"SERVER1\\Instance_1:default,target;default1a,target1a","SERVER1\\Instance_1:default,target;default1b,target1b",
                    "SERVER2:default,target;default2a,target2a","SERVER2:default,target;default2b,target2b", "SERVER3:default,target;default3b,target3b" };

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer2); 
            
            IDictionary<ServerConfigData, BuildSettings> actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);
            actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);

              Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("target1a") > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("target1b") > -1);

            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("target2a") > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("target2b") > -1);

        }

        /// <summary>
        ///A test for SplitLoadToOwningServers
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.ServiceClient.dll")]
        public void SplitLoadToOwningServersTest_CaseSensitivityTest()
        {
            BuildServiceManager target = new BuildServiceManager();
            BuildSettings unifiedSettings = new BuildSettings();
            unifiedSettings.DistributionType = DistributionType.OwnMachineName;
            unifiedSettings.MultiDbTextConfig = new string[]{"SERVER1\\INSTANCE_1:DEFAULT,TARGET;DEFAULT1A,TARGET1A","SERVER1\\INSTANCE_1:DEFAULT,TARGET;DEFAULT1B,TARGET1B",
                    "SERVER2:DEFAULT,TARGET;DEFAULT2A,TARGET2A","SERVER2:DEFAULT,TARGET;DEFAULT2B,TARGET2B", "SERVER3:DEFAULT,TARGET;DEFAULT3B,TARGET3B" };

            IList<ServerConfigData> executionServers = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"server1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            executionServers.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"server2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            executionServers.Add(exeServer2);

            IDictionary<ServerConfigData, BuildSettings> actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);
            actual = target.SplitLoadToOwningServers(unifiedSettings, executionServers);

            Assert.AreEqual(2, actual.Count);
            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer1].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[0].IndexOf("target1a",StringComparison.CurrentCultureIgnoreCase) > -1);
            Assert.IsTrue(actual[exeServer1].MultiDbTextConfig[1].IndexOf("target1b", StringComparison.CurrentCultureIgnoreCase) > -1);

            //Assert the values of 1
            Assert.AreEqual(2, actual[exeServer2].MultiDbTextConfig.Length);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[0].IndexOf("target2a", StringComparison.CurrentCultureIgnoreCase) > -1);
            Assert.IsTrue(actual[exeServer2].MultiDbTextConfig[1].IndexOf("target2b", StringComparison.CurrentCultureIgnoreCase) > -1);

        }

        #endregion

        #region ValidateLoadDistributionTest
        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_UnassignedExecutionServer()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);

            string[] configLines = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b",
                    "SERVER1:default,target;default2a,target2a" };

            List<string> lstUntaskedExecutionServers = null; 
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(1, lstUntaskedExecutionServers.Count);
            Assert.AreEqual(exeServer2.ServerName, lstUntaskedExecutionServers[0]);
        }

        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_TwoUnassignedExecutionServers()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);
            ServerConfigData exeServer3 = new ServerConfigData(@"SERVER3", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer3.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer3);

            string[] configLines = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b",
                    "SERVER1:default,target;default2a,target2a" };

            List<string> lstUntaskedExecutionServers = null;
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(2, lstUntaskedExecutionServers.Count);
            Assert.AreEqual(exeServer2.ServerName, lstUntaskedExecutionServers[0]);
            Assert.AreEqual(exeServer3.ServerName, lstUntaskedExecutionServers[1]);

        }

        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_UnassignedDatabaseServer()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);


            string[] configLines = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b","SERVER2:default,target;default2a,target2a",
                    "SERVER3:default,target;default3a,target3a" };

            List<string> lstUntaskedExecutionServers = null;
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(1, lstUnassignedDatabaseServers.Count);
            Assert.AreEqual("SERVER3", lstUnassignedDatabaseServers[0]);


        }
        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_TwoUnassignedDatabaseServers()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER3", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);


            string[] configLines = new string[]{"SERVER1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b","SERVER2:default,target;default2a,target2a",
                    "SERVER3:default,target;default3a,target3a","SERVER4:default,target;default4a,target4a" ,"SERVER4:default,target;default4b,target4b"  };

            List<string> lstUntaskedExecutionServers = null;
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(2, lstUnassignedDatabaseServers.Count);
            Assert.AreEqual("SERVER2", lstUnassignedDatabaseServers[0]);
            Assert.AreEqual("SERVER4", lstUnassignedDatabaseServers[1]);

        }

        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_UnassignedDatabaseServerWithInstance()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER2", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);


            string[] configLines = new string[]{"SERVER1\\Intsance_2:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b","SERVER2\\Instance_2:default,target;default2a,target2a",
                    "SERVER3\\Instance_3:default,target;default3a,target3a" };

            List<string> lstUntaskedExecutionServers = null;
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(1, lstUnassignedDatabaseServers.Count);
            Assert.AreEqual("SERVER3", lstUnassignedDatabaseServers[0]);


        }
        /// <summary>
        ///A test for ValidateLoadDistribution
        ///</summary>
        [TestMethod()]
        public void ValidateLoadDistributionTest_OwnMachineName_TwoUnassignedDatabaseServersWithInstance()
        {
            BuildServiceManager target = new BuildServiceManager();
            DistributionType distType = DistributionType.OwnMachineName;
            List<ServerConfigData> serverConfigs = new List<ServerConfigData>();
            ServerConfigData exeServer1 = new ServerConfigData(@"SERVER1", "http://nothome.com", "tcp://nothome.com", Protocol.Tcp);
            exeServer1.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer1);
            ServerConfigData exeServer2 = new ServerConfigData(@"SERVER3", "http://nothomeB.com", "tcp://nothomeB.com", Protocol.Tcp);
            exeServer2.ServiceReadiness = ServiceReadiness.ReadyToAccept;
            serverConfigs.Add(exeServer2);


            string[] configLines = new string[]{"SERVER1\\Instance_1:default,target;default1a,target1a","SERVER1:default,target;default1b,target1b","SERVER2:default,target;default2a,target2a",
                    "SERVER3\\Instance_3:default,target;default3a,target3a","SERVER4\\Instance_4:default,target;default4a,target4a" ,"SERVER4:default,target;default4b,target4b"  };

            List<string> lstUntaskedExecutionServers = null;
            List<string> lstUntaskedExecutionServersExpected = new List<string>();

            List<string> lstUnassignedDatabaseServers = null;
            List<string> lstUnassignedDatabaseServersExpected = new List<string>();

            target.ValidateLoadDistribution(distType, serverConfigs, configLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            Assert.AreEqual(2, lstUnassignedDatabaseServers.Count);
            Assert.AreEqual("SERVER2", lstUnassignedDatabaseServers[0]);
            Assert.AreEqual("SERVER4", lstUnassignedDatabaseServers[1]);

        }

        #endregion
    }
}
