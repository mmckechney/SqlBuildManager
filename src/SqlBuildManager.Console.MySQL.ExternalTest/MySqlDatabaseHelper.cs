using MySqlConnector;
using System;
using System.Collections.Generic;

namespace SqlBuildManager.Console.MySQL.ExternalTest
{
    class MySqlDatabaseHelper
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
            trimmed.ForEach(t => clientized.Add(t.Replace(":sbm_mysql_test,", ":client,")));
            return clientized;
        }

        /// <summary>
        /// Creates a random table in each target MySQL database.
        /// Uses password auth from the test config files.
        /// </summary>
        internal static string CreateRandomTable(string mySqlServer, string mySqlUser, string mySqlPassword, string overrideLine)
        {
            return CreateRandomTable(mySqlServer, mySqlUser, mySqlPassword, new List<string>(new string[] { overrideLine }));
        }

        /// <summary>
        /// Creates a random table in each target MySQL database.
        /// Uses password auth from the test config files.
        /// </summary>
        internal static string CreateRandomTable(string mySqlServer, string mySqlUser, string mySqlPassword, List<string> overrideLines)
        {
            string randomTableName = "r" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string randomColumnName = "r" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string createTable = $"CREATE TABLE {randomTableName} ( {randomColumnName} VARCHAR(10) )";

            foreach (var line in overrideLines)
            {
                string server, database;
                (server, database) = ExtractServerAndDbFromLine(line);

                var connStr = new MySqlConnectionStringBuilder()
                {
                    Server = server,
                    Database = database,
                    UserID = mySqlUser,
                    Password = mySqlPassword,
                    SslMode = MySqlSslMode.Required
                };

                try
                {
                    using (var conn = new MySqlConnection(connStr.ConnectionString))
                    {
                        conn.Open();
                        using var cmd = new MySqlCommand(createTable, conn);
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
