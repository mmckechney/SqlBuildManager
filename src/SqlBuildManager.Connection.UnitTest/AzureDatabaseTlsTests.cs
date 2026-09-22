using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using System;

namespace SqlBuildManager.Connection.UnitTest
{
    [TestClass]
    public class AzureDatabaseTlsTests
    {
        [TestMethod]
        [DataRow("test.mysql.database.azure.com")]
        [DataRow("TEST.MYSQL.DATABASE.AZURE.COM.")]
        [DataRow("test.mysql.database.azure.com:3307")]
        [DataRow("test.privatelink.mysql.database.azure.com")]
        [DataRow("test.mysql.database.usgovcloudapi.net")]
        [DataRow("test.mysql.database.chinacloudapi.cn")]
        [DataRow("test.mysql.database.microsoftazure.de")]
        [DataRow("localhost,test.mysql.database.azure.com")]
        public void MySql_AzureAllModes_VerifyServer(string host)
        {
            var factory = new MySqlConnectionFactory(_ => "synthetic-token");
            foreach (var auth in Enum.GetValues<AuthenticationType>())
            {
                var builder = new MySqlConnectionStringBuilder(factory.BuildConnectionString(
                    "db", host, "user", "unused-password", auth, 10, "client", true));
                Assert.AreEqual(MySqlSslMode.VerifyFull, builder.SslMode, $"{host}: {auth}");
            }
        }

        [TestMethod]
        [DataRow("localhost")]
        [DataRow("127.0.0.1:3307")]
        [DataRow("::1")]
        [DataRow("mysql")]
        [DataRow("mysql-container")]
        [DataRow("test.mysql.database.azure.com.attacker.example")]
        public void MySql_LocalNative_DoesNotRequireTls(string host)
        {
            var factory = new MySqlConnectionFactory(_ => throw new InvalidOperationException("Native auth must not request tokens."));
            var builder = new MySqlConnectionStringBuilder(factory.BuildConnectionString(
                "db", host, "user", "password", AuthenticationType.Password, 10, ""));
            Assert.AreEqual(MySqlSslMode.Preferred, builder.SslMode);
        }

        [TestMethod]
        [DataRow("test.database.windows.net")]
        [DataRow("tcp:TEST.DATABASE.WINDOWS.NET.,1433")]
        [DataRow("test.privatelink.database.windows.net")]
        [DataRow("instance.zone.database.windows.net,3342")]
        [DataRow("test.database.usgovcloudapi.net")]
        [DataRow("test.database.chinacloudapi.cn")]
        [DataRow("test.database.cloudapi.de")]
        public void SqlServer_AzureAllModes_IgnoreLocalCertificateBypass(string host)
        {
            foreach (var auth in Enum.GetValues<AuthenticationType>())
            {
                foreach (var connectionString in new[]
                {
                    new SqlServerConnectionFactory().BuildConnectionString("db", host, "user", "password", auth, 10, "client", true),
                    ConnectionHelper.GetConnectionString("db", host, "user", "password", auth, 10, "client", true)
                })
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, builder.Encrypt);
                    Assert.IsFalse(builder.TrustServerCertificate, $"{host}: {auth}");
                }
            }
        }

        [TestMethod]
        [DataRow("localhost")]
        [DataRow("tcp:127.0.0.1,1433")]
        [DataRow(@"localhost\SQLEXPRESS")]
        [DataRow("sqlserver")]
        [DataRow("test.database.windows.net.attacker.example")]
        public void SqlServer_LocalNative_PreservesCertificateBypass(string host)
        {
            foreach (var connectionString in new[]
            {
                new SqlServerConnectionFactory().BuildConnectionString("db", host, "user", "password", AuthenticationType.Password, 10, "", true),
                ConnectionHelper.GetConnectionString("db", host, "user", "password", AuthenticationType.Password, 10, "", true)
            })
                Assert.IsTrue(new SqlConnectionStringBuilder(connectionString).TrustServerCertificate);
        }

        [TestMethod]
        [DoNotParallelize]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void SqlServer_AzureConnectionData_OverridesGlobalAndPerConnectionBypass(bool globalBypass, bool connectionBypass)
        {
            var original = ConnectionHelper.TrustServerCertificate;
            try
            {
                ConnectionHelper.TrustServerCertificate = globalBypass;
                var data = new ConnectionData
                {
                    SQLServerName = "test.database.windows.net",
                    DatabaseName = "db",
                    UserId = "user",
                    Password = "password",
                    AuthenticationType = AuthenticationType.Password,
                    TrustServerCertificate = connectionBypass
                };
                foreach (var value in new[]
                {
                    ConnectionHelper.GetConnectionString(data),
                    new SqlServerConnectionFactory().BuildConnectionString(data)
                })
                {
                    var builder = new SqlConnectionStringBuilder(value);
                    Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, builder.Encrypt);
                    Assert.IsFalse(builder.TrustServerCertificate);
                }
            }
            finally
            {
                ConnectionHelper.TrustServerCertificate = original;
            }
        }
    }
}
