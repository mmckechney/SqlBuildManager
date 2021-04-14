using Microsoft.Data.SqlClient;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;

namespace SqlBuildManager.Console.Dependent.UnitTest
{
    class DatabaseHelper
    {
        internal static (string,string) ExtractServerAndDbFromLine(string overrideLine)
        {
            string server = overrideLine.Split(":")[0];
            string database = overrideLine.Split(":")[1].Split(",")[1];

            return (server, database);
        }
        internal static List<string> ModifyTargetList(List<string> original, int removeCount)
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
        internal static string CreateRandomTable(CommandLineArgs cmdLine, string overrideLine)
        {
            return CreateRandomTable(cmdLine, new List<string>(new string[] { overrideLine }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <param name="overrideLines">Assumes that the line is a delmited list of:   SERVER:overide,target</param>
        /// <returns></returns>
        internal static string CreateRandomTable(CommandLineArgs cmdLine, List<string> overrideLines)
        {
            string server, database;
            string randomTableName = "R" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string randomColumnName = "R" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string createTable = $"CREATE TABLE {randomTableName} ( {randomColumnName} VARCHAR(10) ) ";

            var connStr = new SqlConnectionStringBuilder()
            {
                UserID = cmdLine.AuthenticationArgs.UserName,
                Password = cmdLine.AuthenticationArgs.Password,
            };

            foreach(var line in overrideLines)
            {
                (server, database) = ExtractServerAndDbFromLine(line);
                connStr.DataSource = server;
                connStr.InitialCatalog = database;

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand(createTable, conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }catch(Exception exe)
                {
                    throw new Exception($"Unable to create random table in {server}: {database}\r\n{exe.ToString()}");
                }
            }

            return string.Empty;
        }


    }
}
