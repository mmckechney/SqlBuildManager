using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Console.ExternalTest
{
    public class TestHelper
    {

        public static string GetUniqueJobName(string prefix)
        {
            string name = prefix + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 3);
            return name;
        }

        /// <summary>
        /// Extracts the SimpleSelect.sbm test file from resources, always overwriting any existing file
        /// to prevent stale/corrupted files from causing test failures.
        /// </summary>
        public static string GetSimpleSelectSbm()
        {
            var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            return sbmFileName;
        }

        /// <summary>
        /// Extracts the SimpleSelect_DoubleClient.sbm test file from resources, always overwriting any existing file
        /// to prevent stale/corrupted files from causing test failures.
        /// </summary>
        public static string GetSimpleSelectDoubleClientSbm()
        {
            var sbmFileName = Path.GetFullPath("SimpleSelect_DoubleClient.sbm");
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_DoubleClient);
            return sbmFileName;
        }

        /// <summary>
        /// Extracts the LongRunning.sbm test file from resources, always overwriting any existing file
        /// to prevent stale/corrupted files from causing test failures.
        /// </summary>
        public static string GetLongRunningSbm()
        {
            var sbmFileName = Path.GetFullPath("LongRunning.sbm");
            File.WriteAllBytes(sbmFileName, Properties.Resources.long_running);
            return sbmFileName;
        }

        public static IEnumerable<string> ReadLines(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
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
