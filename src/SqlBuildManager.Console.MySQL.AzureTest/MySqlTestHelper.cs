using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;

namespace SqlBuildManager.Console.MySQL.AzureTest
{
    public class MySqlTestHelper
    {
        public static string RequireManagedIdentitySettings(string path)
        {
            string fullPath = Path.GetFullPath(path);
            var settings = JsonSerializer.Deserialize<CommandLineArgs>(File.ReadAllText(fullPath))
                ?? throw new InvalidDataException($"Invalid Azure MySQL settings: {fullPath}");
            var auth = settings.AuthenticationArgs;
            bool usesWorkloadIdentity = settings.KubernetesArgs != null &&
                !string.IsNullOrWhiteSpace(settings.IdentityArgs?.ServiceAccountName);
            if (auth == null || settings.IdentityArgs == null ||
                auth.DatabasePlatform != DatabasePlatform.MySQL ||
                (auth.AuthenticationType != AuthenticationType.ManagedIdentity && auth.AuthenticationType != AuthenticationType.AzureADDefault) ||
                !string.IsNullOrWhiteSpace(auth.Password) ||
                !string.IsNullOrWhiteSpace(auth.UserName) ||
                string.IsNullOrWhiteSpace(settings.IdentityArgs.IdentityName) ||
                (!usesWorkloadIdentity && string.IsNullOrWhiteSpace(settings.IdentityArgs.ClientId)))
            {
                throw new InvalidDataException("Azure MySQL tests require MySQL managed-identity settings with an identity name and client ID (or a Kubernetes workload-identity service account), and no native credentials. Regenerate mysql-mi-only settings with MYSQL_AUTH_MODE=ManagedIdentity.");
            }
            return fullPath;
        }

        public static string GetUniqueJobName(string prefix)
        {
            string name = prefix + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 3);
            return name;
        }

        /// <summary>
        /// Extracts the MySQL simple select test file from resources.
        /// </summary>
        public static string GetMySqlSimpleSelectSbm()
        {
            var sbmFileName = Path.GetFullPath("MySQL_SimpleSelect.sbm");
            File.WriteAllBytes(sbmFileName, Properties.Resources.MySQL_SimpleSelect);
            return sbmFileName;
        }

        /// <summary>
        /// Extracts the MySQL double-client test file from resources.
        /// </summary>
        public static string GetMySqlSimpleSelectDoubleClientSbm()
        {
            var sbmFileName = Path.GetFullPath("MySQL_SimpleSelect_DoubleClient.sbm");
            File.WriteAllBytes(sbmFileName, Properties.Resources.MySQL_SimpleSelect_DoubleClient);
            return sbmFileName;
        }

        /// <summary>
        /// Extracts the MySQL select query file from resources.
        /// </summary>
        public static string GetMySqlSelectQueryFile()
        {
            var queryFile = Path.GetFullPath("mysql_selectquery.sql");
            File.WriteAllText(queryFile, Properties.Resources.mysql_selectquery);
            return queryFile;
        }

        public static IEnumerable<string> ReadLines(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string LogFileName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(SqlBuildManager.Console.Program.applicationLogFileName) + DateTime.Now.ToString("yyyyMMdd") + Path.GetExtension(SqlBuildManager.Console.Program.applicationLogFileName);
            }
        }

        public static int LogFileCurrentLineCount()
        {
            string logFile = Path.Combine(Path.GetTempPath(), LogFileName);
            int startingLines = 0;
            if (File.Exists(logFile))
            {
                startingLines = ReadLines(logFile).Count() - 1;
            }

            return startingLines;
        }

        public static string RelevantLogFileContents(int startingLine)
        {
            string logFile = Path.Combine(Path.GetTempPath(), LogFileName);
            return string.Join(Environment.NewLine, ReadLines(logFile).Skip(startingLine).ToArray());
        }
    }
}
