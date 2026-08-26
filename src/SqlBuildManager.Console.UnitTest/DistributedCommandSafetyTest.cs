using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Batch;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerShared;
using SqlBuildManager.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class DistributedCommandSafetyTest
    {
        [TestMethod]
        public void BatchManager_CompileTaskDefinitions_UsesFixedWorkerCommandAndEnvironment()
        {
            var cmdLine = new CommandLineArgs
            {
                JobName = "batch-job",
                BuildFileName = "package.sbm",
                Concurrency = 3,
                TimeoutRetryCount = 4,
                DefaultScriptTimeout = 120
            };
            cmdLine.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;
            cmdLine.AuthenticationArgs.UserName = "database-user";
            cmdLine.AuthenticationArgs.Password = "database-password";
            cmdLine.ConnectionArgs.StorageAccountName = "storage";
            cmdLine.ConnectionArgs.StorageAccountKey = "storage-key";
            var manager = new BatchManager(cmdLine);
            var resourceFiles = new List<ResourceFile>
            {
                ResourceFile.FromUrl("https://storage/package.sbm", "package.sbm")
            };

            var definition = manager.CompileTaskDefinitions(
                cmdLine,
                resourceFiles,
                "https://storage/output?sas",
                1,
                OsType.Linux,
                BatchManager.BatchType.Run,
                unitTest: true)[0];

            Assert.AreEqual("--loglevel Information batch worker", definition.CommandLine);
            Assert.IsFalse(definition.CommandLine.Contains("database-password"));
            Assert.IsFalse(definition.CommandLine.Contains("storage-key"));
            Assert.AreEqual("database-password", definition.EnvironmentVariables[ContainerEnvVariables.Password]);
            Assert.AreEqual("storage-key", definition.EnvironmentVariables[ContainerEnvVariables.StorageAccountKey]);
            Assert.AreEqual("PostgreSQL", definition.EnvironmentVariables[ContainerEnvVariables.DatabasePlatform]);
            Assert.AreEqual("https://storage/output?sas", definition.EnvironmentVariables[ContainerEnvVariables.OutputContainerSasUrl]);
            Assert.AreEqual("True", definition.EnvironmentVariables[ContainerEnvVariables.UnitTest]);
            Assert.IsFalse(definition.EnvironmentVariables.Keys.Any(key => key.StartsWith("AZ_BATCH_APP_PACKAGE_")));
        }

        [TestMethod]
        public void BatchManager_CompileTaskDefinitions_QueryUsesQueryWorker()
        {
            var cmdLine = new CommandLineArgs();
            var manager = new BatchManager(cmdLine);

            var definition = manager.CompileTaskDefinitions(
                cmdLine,
                new List<ResourceFile>(),
                "sas",
                1,
                OsType.Linux,
                BatchManager.BatchType.Query,
                unitTest: false)[0];

            Assert.AreEqual("--loglevel Information batch worker query", definition.CommandLine);
        }

        [TestMethod]
        public void EnvironmentVariableHelper_RuntimeSettingsRoundTrip_PreservesBatchExecutionValues()
        {
            var source = new CommandLineArgs
            {
                JobName = "round-trip",
                BuildFileName = "package.sbm",
                Concurrency = 7,
                ConcurrencyType = ConcurrencyType.Server,
                Transactional = false,
                TimeoutRetryCount = 2,
                DefaultScriptTimeout = 90,
                Silent = true,
                AllowObjectDelete = true,
                QueryFile = new FileInfo("query.sql"),
                OutputFile = new FileInfo("results.csv")
            };
            source.MultiDbRunConfigFileName = "targets.cfg";
            source.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
            source.AuthenticationArgs.DatabasePlatform = DatabasePlatform.MySQL;
            source.AuthenticationArgs.UserName = "user";
            source.AuthenticationArgs.Password = "password";
            source.AuthenticationArgs.TrustServerCertificate = true;
            source.ConnectionArgs.StorageAccountName = "storage";
            source.ConnectionArgs.StorageAccountKey = "key";
            source.DacPacArgs.PlatinumDacpac = "platinum.dacpac";
            source.DacPacArgs.TargetDacpac = "target.dacpac";
            source.DacPacArgs.ForceCustomDacPac = true;
            source.BatchArgs.OutputContainerSasUrl = "https://storage/output?sas";

            var values = EnvironmentVariableHelper.CreateRuntimeEnvironmentVariables(source, unitTest: true);
            var previousValues = values.Keys.ToDictionary(key => key, Environment.GetEnvironmentVariable);
            try
            {
                foreach (var value in values)
                {
                    Environment.SetEnvironmentVariable(value.Key, value.Value);
                }

                var result = EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(new CommandLineArgs());

                Assert.AreEqual(source.JobName, result.JobName);
                Assert.AreEqual(source.BuildFileName, result.BuildFileName);
                Assert.AreEqual(source.MultiDbRunConfigFileName, result.MultiDbRunConfigFileName);
                Assert.AreEqual(source.Concurrency, result.Concurrency);
                Assert.AreEqual(source.ConcurrencyType, result.ConcurrencyType);
                Assert.AreEqual(source.Transactional, result.Transactional);
                Assert.AreEqual(source.TimeoutRetryCount, result.TimeoutRetryCount);
                Assert.AreEqual(source.DefaultScriptTimeout, result.DefaultScriptTimeout);
                Assert.AreEqual(source.AuthenticationArgs.DatabasePlatform, result.AuthenticationArgs.DatabasePlatform);
                Assert.AreEqual(source.AuthenticationArgs.TrustServerCertificate, result.AuthenticationArgs.TrustServerCertificate);
                Assert.AreEqual(source.DacPacArgs.TargetDacpac, result.DacPacArgs.TargetDacpac);
                Assert.AreEqual(source.BatchArgs.OutputContainerSasUrl, result.BatchArgs.OutputContainerSasUrl);
                Assert.IsTrue(EnvironmentVariableHelper.IsUnitTest());
            }
            finally
            {
                foreach (var previousValue in previousValues)
                {
                    Environment.SetEnvironmentVariable(previousValue.Key, previousValue.Value);
                }
            }
        }

        [TestMethod]
        public void BatchManager_GetBatchContainerImage_NormalizesRegistryServer()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.ContainerRegistryArgs.RegistryServer = "https://example.azurecr.io/";
            cmdLine.ContainerRegistryArgs.ImageName = "sqlbuildmanager";
            cmdLine.ContainerRegistryArgs.ImageTag = "latest-vNext";

            var image = BatchManager.GetBatchContainerImage(cmdLine);

            Assert.AreEqual("example.azurecr.io/sqlbuildmanager:latest-vNext", image);
        }

        [TestMethod]
        public void BatchManager_CreateTaskContainerSettings_UsesTaskWorkingDirectory()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.ContainerRegistryArgs.RegistryServer = "example.azurecr.io";
            cmdLine.ContainerRegistryArgs.ImageName = "sqlbuildmanager";
            cmdLine.ContainerRegistryArgs.ImageTag = "latest-vNext";

            var settings = BatchManager.CreateTaskContainerSettings(cmdLine);

            Assert.IsNull(settings.ContainerRunOptions);
            Assert.AreEqual(ContainerWorkingDirectory.TaskWorkingDirectory, settings.WorkingDirectory);
        }

        [TestMethod]
        public async Task AciDeploy_MissingSubscriptionId_ReturnsFailure()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.IdentityArgs.SubscriptionId = string.Empty;

            var result = await Worker.AciDeploy(cmdLine, monitor: false, unittest: true);

            Assert.AreEqual(1, result);
        }
    }
}
