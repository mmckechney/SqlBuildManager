using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Relay;
using SqlBuildManager.Connection;
using System;
using System.Collections.Generic;
using System.CommandLine;

using System.IO;
using static SqlBuildManager.Console.CommandLine.CommandLineArgs;

namespace SqlBuildManager.Console.AzureTest.TestBase
{
    public class DatabaseHelper
    {
        public static (string, string) ExtractServerAndDbFromLine(string overrideLine)
        {
            string server = overrideLine.Split(":")[0];
            string database = overrideLine.Split(":")[1].Split(",")[1];

            return (server, database);
        }
        public static List<string> ModifyTargetList(List<string> original, int removeCount)
        {
            var trimmed = original.GetRange(removeCount, original.Count - removeCount);
            List<string> clientized = new List<string>();

            trimmed.ForEach(t => clientized.Add(t.Replace(":SqlBuildTest,", ":client,")));

            return clientized;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <param name="overrideLines">Assumes that the line is a delmited list of:   SERVER:overide,target</param>
        /// <returns></returns>
        public static string CreateRandomTable(CommandLineArgs cmdLine, string overrideLine)
        {
            return CreateRandomTable(cmdLine, new List<string>(new string[] { overrideLine }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <param name="overrideLines">Assumes that the line is a delmited list of:   SERVER:overide,target</param>
        /// <returns></returns>
        public static string CreateRandomTable(CommandLineArgs cmdLine, List<string> overrideLines)
        {
            string server, database;
            string randomTableName = "R" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string randomColumnName = "R" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string createTable = $"CREATE TABLE {randomTableName} ( {randomColumnName} VARCHAR(10) ) ";

            foreach (var line in overrideLines)
            {
                (server, database) = ExtractServerAndDbFromLine(line);

                try
                {
                    using (var conn = CreateSqlConnection(cmdLine, server, database))
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = createTable;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                catch (SqlException exe) when (
                    !string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.RelayProxyEndpoint) &&
                    RelayProxyClient.IsSqlPrivateNetworkDenial(exe))
                {
                    System.Console.WriteLine(
                        $"Direct SQL access to {server}/{database} is blocked; creating the test table through Azure Relay.");
                    new RelayProxyClient(cmdLine.ConnectionArgs.RelayProxyEndpoint)
                        .CreateSqlTestTableAsync(
                            server,
                            database,
                            randomTableName,
                            randomColumnName)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception exe)
                {
                    throw new Exception($"Unable to create random table in {server}: {database}. Authentication: {cmdLine.AuthenticationArgs.AuthenticationType}; database client ID: {cmdLine.IdentityArgs.ClientId}.", exe);
                }
            }

            return string.Empty;
        }

        public static string CreateDacpac(CommandLineArgs cmdLine, string server, string database)
        {
            var log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<DatabaseHelper>("SqlBuildManager.Console.log", Path.GetTempPath());
            log.LogInformation("Creating DACPAC for tests");
            string fullname = Path.GetFullPath($"TestConfig/{database}.dacpac");
            if (RequiresSqlRelay(cmdLine, server, database))
            {
                log.LogInformation($"Extracting DACPAC for {server}/{database} through Azure Relay");
                new RelayProxyClient(cmdLine.ConnectionArgs.RelayProxyEndpoint)
                    .ExtractSqlTestDacpacAsync(server, database, fullname)
                    .GetAwaiter()
                    .GetResult();
                return fullname;
            }

            var args = CreateDacpacArguments(cmdLine, server, database, fullname);

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            if (result != 0)
                throw new InvalidOperationException($"DACPAC extraction failed for {server}/{database} with exit code {result}. Authentication: {cmdLine.AuthenticationArgs.AuthenticationType}; database client ID: {cmdLine.IdentityArgs.ClientId}.");
            return fullname;
        }

        public static string[] CreateDacpacArguments(CommandLineArgs cmdLine, string server, string database, string outputFile)
        {
            var args = new List<string>{
                "dacpac",
                "--authtype" , cmdLine.AuthenticationArgs.AuthenticationType.ToString(),
                "--dacpacname", outputFile,
                "--database", database,
                "--server", server };
            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
                args.AddRange(new[] { "--clientid", cmdLine.IdentityArgs.ClientId });
            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.Password)
                args.AddRange(new[] { "--username", cmdLine.AuthenticationArgs.UserName, "--password", cmdLine.AuthenticationArgs.Password });
            if (cmdLine.AuthenticationArgs.TrustServerCertificate)
                args.AddRange(new[] { "--trustservercertificate", "true" });
            return args.ToArray();
        }

        /// <summary>Loads database authentication and Relay settings without changing the orchestrator environment.</summary>
        public static void ConfigureRelayEndpoint(
            CommandLineArgs cmdLine,
            string settingsFilePath,
            string settingsFileKeyPath)
        {
            var settingsArgs = new CommandLineArgs
            {
                FileInfoSettingsFile = new FileInfo(settingsFilePath),
                SettingsFileKey = settingsFileKeyPath
            };
            var (_, decrypted) = Cryptography.DecryptSensitiveFields(settingsArgs);
            cmdLine.RelayProxyEndpoint = decrypted.ConnectionArgs.RelayProxyEndpoint;
            cmdLine.AuthenticationArgs = decrypted.AuthenticationArgs;
            cmdLine.IdentityArgs = decrypted.IdentityArgs;
        }

        public static System.Data.Common.DbConnection CreateSqlConnection(CommandLineArgs cmdLine, string server, string database) =>
            new SqlServerConnectionFactory().CreateConnection(new ConnectionData(server, database)
            {
                AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType,
                UserId = cmdLine.AuthenticationArgs.UserName,
                Password = cmdLine.AuthenticationArgs.Password,
                ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId,
                TrustServerCertificate = cmdLine.AuthenticationArgs.TrustServerCertificate,
                ScriptTimeout = 15
            });

        public static bool RequiresSqlRelay(
            CommandLineArgs cmdLine,
            string server,
            string database)
        {
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.RelayProxyEndpoint))
            {
                return false;
            }

            try
            {
                using var connection = CreateSqlConnection(cmdLine, server, database);
                connection.Open();
                return false;
            }
            catch (SqlException exe) when (RelayProxyClient.IsSqlPrivateNetworkDenial(exe))
            {
                return true;
            }
        }


    }
}
