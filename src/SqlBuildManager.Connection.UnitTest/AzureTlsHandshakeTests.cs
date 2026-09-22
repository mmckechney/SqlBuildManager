using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Npgsql;
using System;
using System.Buffers.Binary;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Connection.UnitTest
{
    [TestClass]
    public class AzureTlsHandshakeTests
    {
        [TestMethod]
        [DataRow("mysql", "valid", true)]
        [DataRow("mysql", "wrong-host", false)]
        [DataRow("mysql", "untrusted", false)]
        [DataRow("mysql", "expired", false)]
        [DataRow("mysql", "plaintext", false)]
        // MySqlConnector explicitly trusts loopback for cleartext-plugin credentials, so this
        // fixture cannot establish remote token-rejection behavior for invalid certificates.
        [DataRow("mysql-token", "valid", true)]
        [DataRow("postgres", "valid", true)]
        [DataRow("postgres", "wrong-host", false)]
        [DataRow("postgres", "untrusted", false)]
        [DataRow("postgres", "expired", false)]
        [DataRow("postgres", "plaintext", false)]
        [DataRow("mysql", "local-plaintext", true)]
        [DataRow("postgres", "local-plaintext", true)]
        public async Task RealDriver_ValidatesAzurePolicyBeforeDatabaseCommands(string platform, string scenario, bool expectLogin)
        {
            var directory = Path.Combine(Path.GetTempPath(), "sbm-tls-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            using var caKey = RSA.Create(2048);
            var caRequest = new CertificateRequest("CN=SBM temporary test CA", caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            caRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
            using var ca = caRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(2));
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var san = new SubjectAlternativeNameBuilder();
            san.AddDnsName(scenario == "wrong-host" ? "wrong.example" : "localhost");
            if (scenario != "wrong-host") san.AddIpAddress(IPAddress.Loopback);
            request.CertificateExtensions.Add(san.Build());
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true));
            using var publicCert = request.Create(ca, DateTimeOffset.UtcNow.AddDays(-2),
                scenario == "expired" ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddDays(1),
                RandomNumberGenerator.GetBytes(16));
            using var ephemeral = publicCert.CopyWithPrivateKey(key);
            using var certificate = X509CertificateLoader.LoadPkcs12(ephemeral.Export(X509ContentType.Pfx), null);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            try
            {
                var rootFile = Path.Combine(directory, "root.pem");
                await File.WriteAllTextAsync(rootFile, ca.ExportCertificatePem());
                bool local = scenario == "local-plaintext";
                bool useTls = scenario != "plaintext" && !local;
                var server = ObserveLoginAsync(listener, platform, useTls, certificate, timeout.Token);
                using DbConnection connection = CreateConnection(platform, local, port,
                    scenario == "untrusted" ? null : rootFile);
                Exception? failure = null;
                try { await connection.OpenAsync(timeout.Token); }
                catch (Exception ex) when (ex is MySqlException or NpgsqlException or AuthenticationException)
                {
                    failure = ex;
                }
                // The fixture deliberately returns a database error after verification; no database is required.
                Assert.IsNotNull(failure, "The protocol fixture must not report a successful database login.");
                var loginReceived = await server;
                Assert.AreEqual(expectLogin, loginReceived, $"{platform}/{scenario}: {failure}");
                if (expectLogin)
                    StringAssert.Contains(failure.ToString(), "SBM_TEST_LOGIN_REACHED");
                else
                {
                    Assert.IsFalse(failure.ToString().Contains("SBM_TEST_LOGIN_REACHED"), failure.ToString());
                    Assert.IsFalse(failure.ToString().Contains(nameof(TimeoutException)), "A timeout is not evidence of certificate rejection.");
                    StringAssert.Contains(failure.ToString(), "SSL");
                }
            }
            finally
            {
                listener.Stop();
                Directory.Delete(directory, true);
            }
        }

        private static DbConnection CreateConnection(string platform, bool local, int port, string? rootFile)
        {
            // Obtain the actual Azure policy first, then route only the test transport to loopback.
            if (platform.StartsWith("mysql", StringComparison.Ordinal))
            {
                var factory = new MySqlConnectionFactory(_ => "synthetic-test-token");
                var builder = new MySqlConnectionStringBuilder(factory.BuildConnectionString("db",
                    local ? "localhost" : "test.mysql.database.azure.com", "user", "test-password",
                    platform == "mysql-token" ? AuthenticationType.ManagedIdentity : AuthenticationType.Password, 5, ""))
                {
                    Server = "127.0.0.1", Port = (uint)port, Pooling = false
                };
                Assert.AreEqual(local ? MySqlSslMode.Preferred : MySqlSslMode.VerifyFull, builder.SslMode);
                if (rootFile != null) builder.SslCa = rootFile;
                return new MySqlConnection(builder.ConnectionString);
            }
            else
            {
                var factory = new PostgresConnectionFactory(_ => throw new InvalidOperationException("No Azure auth in this fixture."));
                var builder = new NpgsqlConnectionStringBuilder(factory.BuildConnectionString("db",
                    local ? "localhost" : "test.postgres.database.azure.com", "user", "test-password",
                    AuthenticationType.Password, 5, ""))
                {
                    Host = "127.0.0.1", Port = port, Pooling = false,
                    GssEncryptionMode = GssEncryptionMode.Disable
                };
                Assert.AreEqual(local ? SslMode.Prefer : SslMode.VerifyFull, builder.SslMode);
                if (rootFile != null) builder.RootCertificate = rootFile;
                return new NpgsqlConnection(builder.ConnectionString);
            }
        }

        private static async Task<bool> ObserveLoginAsync(TcpListener listener, string platform, bool useTls,
            X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            using var client = await listener.AcceptTcpClientAsync(cancellationToken);
            await using var network = client.GetStream();
            SslStream? tls = null;
            Stream transport = network;
            try
            {
                if (platform == "postgres")
                {
                    var sslRequest = new byte[8];
                    await network.ReadExactlyAsync(sslRequest, cancellationToken);
                    if (BinaryPrimitives.ReadInt32BigEndian(sslRequest.AsSpan(4)) != 80877103)
                        throw new InvalidDataException("Expected PostgreSQL SSLRequest.");
                    await network.WriteAsync(new byte[] { useTls ? (byte)'S' : (byte)'N' }, cancellationToken);
                }
                else
                {
                    await SendMySqlPacketAsync(network, Greeting(useTls), 0, cancellationToken);
                    var firstPacket = await ReadMySqlPacketAsync(network, cancellationToken);
                    if (firstPacket.Length == 0) return false;
                    if (!useTls)
                    {
                        await SendMySqlPacketAsync(network, MySqlError(), 2, cancellationToken);
                        return firstPacket.Length > 32;
                    }
                    if (firstPacket.Length != 32)
                        throw new InvalidDataException("Expected MySQL SSLRequest, not credentials.");
                }

                if (useTls)
                {
                    tls = new SslStream(network, leaveInnerStreamOpen: true);
                    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = certificate,
                        EnabledSslProtocols = SslProtocols.Tls12
                    }, cancellationToken);
                    transport = tls;
                }

                if (platform.StartsWith("mysql", StringComparison.Ordinal))
                {
                    var login = await ReadMySqlPacketAsync(transport, cancellationToken);
                    if (login.Length == 0) return false;
                    if (platform == "mysql-token")
                    {
                        await SendMySqlPacketAsync(transport, [0xFE, .. Encoding.ASCII.GetBytes("mysql_clear_password\0")], 3, cancellationToken);
                        var token = await ReadMySqlPacketAsync(transport, cancellationToken);
                        if (token.Length == 0) return false;
                        Assert.AreEqual("synthetic-test-token\0", Encoding.UTF8.GetString(token));
                        await SendMySqlPacketAsync(transport, MySqlError(), 5, cancellationToken);
                        return true;
                    }
                    // MySqlConnector defers certificate validation until the authentication OK packet.
                    await SendMySqlPacketAsync(transport, new byte[] { 0, 0, 0, 2, 0, 0, 0 }, 3, cancellationToken);
                    var command = await ReadMySqlPacketAsync(transport, cancellationToken);
                    if (command.Length == 0) return false;
                    Assert.AreEqual((byte)3, command[0], "Expected the driver's SET NAMES command.");
                    await SendMySqlPacketAsync(transport, MySqlError(), 1, cancellationToken);
                    return true;
                }
                var length = new byte[4];
                await transport.ReadExactlyAsync(length, cancellationToken);
                var startup = new byte[BinaryPrimitives.ReadInt32BigEndian(length) - 4];
                await transport.ReadExactlyAsync(startup, cancellationToken);
                var error = Encoding.UTF8.GetBytes("SFATAL\0C28000\0MSBM_TEST_LOGIN_REACHED\0\0");
                var response = new byte[5 + error.Length];
                response[0] = (byte)'E';
                BinaryPrimitives.WriteInt32BigEndian(response.AsSpan(1), error.Length + 4);
                error.CopyTo(response, 5);
                await transport.WriteAsync(response, cancellationToken);
                return true;
            }
            catch (Exception ex) when (ex is IOException or AuthenticationException)
            {
                Console.WriteLine($"Protocol fixture closed: {ex}");
                return false;
            }
            finally
            {
                if (tls != null) await tls.DisposeAsync();
            }
        }

        private static byte[] Greeting(bool tls)
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer);
            uint capabilities = 0x0008A201u | (tls ? 0x800u : 0);
            writer.Write((byte)10);
            writer.Write(Encoding.ASCII.GetBytes("8.0.36\0"));
            writer.Write(1);
            writer.Write(Encoding.ASCII.GetBytes("12345678"));
            writer.Write((byte)0);
            writer.Write((ushort)capabilities);
            writer.Write((byte)45);
            writer.Write((ushort)2);
            writer.Write((ushort)(capabilities >> 16));
            writer.Write((byte)21);
            writer.Write(new byte[10]);
            writer.Write(Encoding.ASCII.GetBytes("abcdefghijkl\0mysql_native_password\0"));
            return buffer.ToArray();
        }

        private static byte[] MySqlError() =>
            [0xFF, 0x15, 0x04, .. Encoding.ASCII.GetBytes("#28000SBM_TEST_LOGIN_REACHED")];

        private static async Task SendMySqlPacketAsync(Stream stream, byte[] payload, byte sequence, CancellationToken ct)
        {
            var header = new byte[] { (byte)payload.Length, (byte)(payload.Length >> 8), (byte)(payload.Length >> 16), sequence };
            await stream.WriteAsync(header, ct);
            await stream.WriteAsync(payload, ct);
        }

        private static async Task<byte[]> ReadMySqlPacketAsync(Stream stream, CancellationToken ct)
        {
            var header = new byte[4];
            if (await stream.ReadAsync(header.AsMemory(0, 1), ct) == 0) return Array.Empty<byte>();
            await stream.ReadExactlyAsync(header.AsMemory(1), ct);
            var payload = new byte[header[0] | header[1] << 8 | header[2] << 16];
            await stream.ReadExactlyAsync(payload, ct);
            return payload;
        }
    }
}
