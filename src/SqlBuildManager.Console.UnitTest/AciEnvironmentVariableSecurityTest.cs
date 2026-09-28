using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Aci;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerShared;
using SqlBuildManager.Connection;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class AciEnvironmentVariableSecurityTest
    {
        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void GetContainerEnvironmentVariables_ProtectsSecretsAndPreservesMetadata(bool useKeyVault, bool query)
        {
            var cmdLine = new CommandLineArgs
            {
                JobName = "aci-security-test",
                BuildFileName = Path.Combine("packages", "package.sbm"),
                Concurrency = 3,
                ConcurrencyType = ConcurrencyType.Server,
                Transactional = false,
                TimeoutRetryCount = 2,
                DefaultScriptTimeout = 90,
                Silent = true,
                AllowObjectDelete = true
            };
            cmdLine.MultiDbRunConfigFileName = "targets.cfg";
            cmdLine.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.MySQL;
            cmdLine.AuthenticationArgs.TrustServerCertificate = true;
            cmdLine.AuthenticationArgs.UserName = "database-user";
            cmdLine.AuthenticationArgs.Password = "database-password";
            cmdLine.ConnectionArgs.StorageAccountName = "storage-account";
            cmdLine.ConnectionArgs.StorageAccountKey = "storage-key";
            cmdLine.ConnectionArgs.KeyVaultName = useKeyVault ? "test-vault" : "";
            cmdLine.ConnectionArgs.EventHubConnectionString = useKeyVault
                ? "eventhub-namespace|eventhub-name"
                : "Endpoint=sb://eventhub.example/;SharedAccessKey=eventhub-secret";
            cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = useKeyVault
                ? "servicebus-namespace"
                : "Endpoint=sb://servicebus.example/;SharedAccessKey=servicebus-secret";
            cmdLine.BatchArgs.OutputContainerSasUrl = "https://storage.example/output?sv=1&sig=sas-secret";
            cmdLine.IdentityArgs.ClientId = "identity-client-id";
            cmdLine.IdentityArgs.IdentityName = "identity-name";
            cmdLine.IdentityArgs.TenantId = "tenant-id";
            cmdLine.DacPacArgs.PlatinumDacpac = Path.Combine("packages", "platinum.dacpac");
            cmdLine.DacPacArgs.TargetDacpac = "target.dacpac";
            cmdLine.DacPacArgs.ForceCustomDacPac = true;
            if (query)
            {
                cmdLine.QueryFile = new FileInfo(Path.Combine("queries", "query.sql"));
                cmdLine.OutputFile = new FileInfo(Path.Combine("results", "results.csv"));
            }

            var secrets = new Dictionary<string, string>
            {
                [ContainerEnvVariables.EventHubConnectionString] = cmdLine.ConnectionArgs.EventHubConnectionString,
                [ContainerEnvVariables.ServiceBusTopicConnectionString] = cmdLine.ConnectionArgs.ServiceBusTopicConnectionString,
                [ContainerEnvVariables.OutputContainerSasUrl] = cmdLine.BatchArgs.OutputContainerSasUrl
            };
            if (!useKeyVault)
            {
                secrets[ContainerEnvVariables.UserName] = cmdLine.AuthenticationArgs.UserName;
                secrets[ContainerEnvVariables.Password] = cmdLine.AuthenticationArgs.Password;
                secrets[ContainerEnvVariables.StorageAccountKey] = cmdLine.ConnectionArgs.StorageAccountKey;
            }

            var expected = EnvironmentVariableHelper.CreateRuntimeEnvironmentVariables(cmdLine);
            expected.Remove(ContainerEnvVariables.Override);
            expected[ContainerEnvVariables.PackageName] = "package.sbm";
            expected[ContainerEnvVariables.DacpacName] = "platinum.dacpac";
            if (query)
            {
                expected[ContainerEnvVariables.QueryFile] = "query.sql";
                expected[ContainerEnvVariables.OutputFile] = "results.csv";
            }

            var actual = AciManager.GetContainerEnvironmentVariables(cmdLine).ToDictionary(value => value.Name);

            CollectionAssert.AreEquivalent(expected.Keys.ToArray(), actual.Keys.ToArray());
            Assert.AreEqual(useKeyVault ? 3 : 6, actual.Values.Count(value => value.SecureValue != null));
            foreach (var secret in secrets)
            {
                Assert.AreEqual(secret.Value, actual[secret.Key].SecureValue, secret.Key);
                Assert.IsNull(actual[secret.Key].Value, $"{secret.Key} must not expose a plain Value.");
                Assert.IsFalse(actual.Values.Any(value => value.Value == secret.Value),
                    $"{secret.Key} must not leak through another variable.");
            }
            foreach (var metadata in expected.Where(value => !secrets.ContainsKey(value.Key)))
            {
                Assert.AreEqual(metadata.Value, actual[metadata.Key].Value, metadata.Key);
                Assert.IsNull(actual[metadata.Key].SecureValue, metadata.Key);
            }
            if (useKeyVault)
            {
                Assert.IsFalse(actual.ContainsKey(ContainerEnvVariables.UserName));
                Assert.IsFalse(actual.ContainsKey(ContainerEnvVariables.Password));
                Assert.IsFalse(actual.ContainsKey(ContainerEnvVariables.StorageAccountKey));
            }
            Assert.IsFalse(actual.ContainsKey(ContainerEnvVariables.Override));
            Assert.AreEqual(query, actual.ContainsKey(ContainerEnvVariables.QueryFile));
            Assert.AreEqual(query, actual.ContainsKey(ContainerEnvVariables.OutputFile));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(" ")]
        public void GetContainerEnvironmentVariables_OmitsAbsentSecrets(string secret)
        {
            var cmdLine = new CommandLineArgs { BuildFileName = "package.sbm" };
            cmdLine.AuthenticationArgs.UserName = secret;
            cmdLine.AuthenticationArgs.Password = secret;
            cmdLine.ConnectionArgs.StorageAccountKey = secret;
            cmdLine.ConnectionArgs.EventHubConnectionString = secret;
            cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = secret;
            cmdLine.BatchArgs.OutputContainerSasUrl = secret;

            var actual = AciManager.GetContainerEnvironmentVariables(cmdLine);

            var secretNames = new[]
            {
                ContainerEnvVariables.UserName,
                ContainerEnvVariables.Password,
                ContainerEnvVariables.StorageAccountKey,
                ContainerEnvVariables.EventHubConnectionString,
                ContainerEnvVariables.ServiceBusTopicConnectionString,
                ContainerEnvVariables.OutputContainerSasUrl
            };
            Assert.IsFalse(actual.Any(value => secretNames.Contains(value.Name)));
            Assert.IsTrue(actual.All(value => value.SecureValue == null));
        }
    }
}
