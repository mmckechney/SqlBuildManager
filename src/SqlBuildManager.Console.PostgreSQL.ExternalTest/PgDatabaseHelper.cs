using Npgsql;
using System;
using System.Collections.Generic;

namespace SqlBuildManager.Console.PostgreSQL.ExternalTest
{
    class PgDatabaseHelper
    {
        /// <summary>
        /// Extracts server and database from an override line.
        /// Format: server:override,target
        /// </summary>
        internal static (string, string) ExtractServerAndDbFromLine(string overrideLine)
        {
            string server = overrideLine.Split(":")[0];
            string database = overrideLine.Split(":")[1].Split(",")[1];
            return (server, database);
        }

        internal static List<string> ModifyTargetList(List<string> original, int removeCount)
        {
            var trimmed = original.GetRange(removeCount, original.Count - removeCount);
            List<string> clientized = new List<string>();
            trimmed.ForEach(t => clientized.Add(t.Replace(":sbm_pg_test,", ":client,")));
            return clientized;
        }

        /// <summary>
        /// Creates a random table in each target PostgreSQL database.
        /// Uses password auth from the test config files.
        /// </summary>
        internal static string CreateRandomTable(string pgServer, string pgUser, string pgPassword, string overrideLine)
        {
            return CreateRandomTable(pgServer, pgUser, pgPassword, new List<string>(new string[] { overrideLine }));
        }

        /// <summary>
        /// Creates a random table in each target PostgreSQL database.
        /// Uses password auth from the test config files.
        /// </summary>
        internal static string CreateRandomTable(string pgServer, string pgUser, string pgPassword, List<string> overrideLines)
        {
            string randomTableName = "r" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string randomColumnName = "r" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string createTable = $"CREATE TABLE {randomTableName} ( {randomColumnName} VARCHAR(10) )";

            foreach (var line in overrideLines)
            {
                string server, database;
                (server, database) = ExtractServerAndDbFromLine(line);

                var connStr = new NpgsqlConnectionStringBuilder()
                {
                    Host = server,
                    Database = database,
                    Username = pgUser,
                    Password = pgPassword,
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };

                try
                {
                    using (var conn = new NpgsqlConnection(connStr.ConnectionString))
                    {
                        conn.Open();
                        using var cmd = new NpgsqlCommand(createTable, conn);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception exe)
                {
                    throw new Exception($"Unable to create random table in {server}: {database}\r\n{exe.ToString()}");
                }
            }

            return randomTableName;
        }
    }
}
