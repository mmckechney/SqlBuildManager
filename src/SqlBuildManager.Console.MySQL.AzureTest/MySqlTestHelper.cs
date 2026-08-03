using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SqlBuildManager.Console.MySQL.AzureTest
{
    public class MySqlTestHelper
    {
        private const string MySqlTestDatabase = "sbm_mysql_test";

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
            return CreateMySqlPackage(Properties.Resources.MySQL_SimpleSelect, sbmFileName, rewritePostgresFunctions: false);
        }

        /// <summary>
        /// Extracts the MySQL double-client test file from resources.
        /// </summary>
        public static string GetMySqlSimpleSelectDoubleClientSbm()
        {
            var sbmFileName = Path.GetFullPath("MySQL_SimpleSelect_DoubleClient.sbm");
            return CreateMySqlPackage(Properties.Resources.MySQL_SimpleSelect_DoubleClient, sbmFileName, rewritePostgresFunctions: true);
        }

        private static string CreateMySqlPackage(byte[] packageBytes, string sbmFileName, bool rewritePostgresFunctions)
        {
            var tempSeed = Path.GetTempFileName();
            var workingPath = Path.Combine(Path.GetTempPath(), $"mysql-sbm-{Guid.NewGuid():N}");
            var extractPath = Path.Combine(workingPath, "extract");
            Directory.CreateDirectory(extractPath);

            try
            {
                File.WriteAllBytes(tempSeed, packageBytes);
                ZipFile.ExtractToDirectory(tempSeed, extractPath);

                var projectPath = Directory.EnumerateFiles(extractPath, "SqlSyncBuildProject.xml", SearchOption.AllDirectories).Single();
                var project = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
                var scripts = project.Descendants().Where(element => element.Name.LocalName == "Script").ToList();
                if (scripts.Count == 0)
                {
                    throw new InvalidDataException($"The MySQL test package '{Path.GetFileName(sbmFileName)}' does not contain any scripts.");
                }

                foreach (var script in scripts)
                {
                    script.SetAttributeValue("Database", MySqlTestDatabase);
                }

                project.Save(projectPath, SaveOptions.DisableFormatting);

                if (rewritePostgresFunctions)
                {
                    foreach (var scriptPath in Directory.EnumerateFiles(extractPath, "*.sql", SearchOption.AllDirectories))
                    {
                        var script = File.ReadAllText(scriptPath);
                        script = script.Replace("current_database()", "DATABASE()", StringComparison.OrdinalIgnoreCase);
                        File.WriteAllText(scriptPath, script);
                    }
                }

                if (File.Exists(sbmFileName))
                {
                    File.Delete(sbmFileName);
                }

                ZipFile.CreateFromDirectory(extractPath, sbmFileName);
                return sbmFileName;
            }
            finally
            {
                if (File.Exists(tempSeed))
                {
                    File.Delete(tempSeed);
                }

                if (Directory.Exists(workingPath))
                {
                    Directory.Delete(workingPath, true);
                }
            }
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
