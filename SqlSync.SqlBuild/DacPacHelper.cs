using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
namespace SqlSync.SqlBuild
{
    public class DacPacHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string sqlPack = null;
        private static string sqlPackageExe
        {
            get
            {
                if(sqlPack == null)
                {
                    sqlPack = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Microsoft_SqlDB_DAC\sqlpackage.exe";
                    if(!File.Exists(sqlPack))
                    {
                        sqlPack = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\120\sqlpackage.exe";
                        if(!File.Exists(sqlPack))
                        {
                            throw new ArgumentException("Can not file sqlpackage.exe");
                        }
                    }
                }
                return sqlPack;
            }
        }
        public static bool ExtractDacPac(string sourceDatabase, string sourceServer, string userName, string password, string dacPacFileName)
        {
            ProcessHelper pHelper = new ProcessHelper();

            //Extract Action
            pHelper.AddArgument("/Action", "Extract");
            
            //Connection settings
            pHelper.AddArgument("/SourceServerName", sourceServer);
            pHelper.AddArgument("/SourceDatabaseName", sourceDatabase);
            pHelper.AddArgument("/SourceUser", userName);
            pHelper.AddArgument("/SourcePassword", password);

            //Output
            pHelper.AddArgument("/TargetFile", dacPacFileName);
            
            //Extraction settings
            pHelper.AddArgument("/p:IgnoreUserLoginMapping", "True", "=");
            pHelper.AddArgument("/p:IgnorePermissions", "True", "=");

            int result =  pHelper.ExecuteProcess(sqlPackageExe);

            log.Info(pHelper.Output);
            if(result != 0)
            {
                log.Error(pHelper.Error);
                return false;
            }

            return true;

        }
        
        public static bool CreateSbmFromDacPacDifferences(string platinumDacPacFileName, string tarnishedDacPacFileName, string buildPackageName)
        {
            string rawScript = ScriptDacPacDeltas(platinumDacPacFileName, tarnishedDacPacFileName);
            if(!string.IsNullOrEmpty(rawScript))
            {
                string cleaned = CleanDacPacScript(rawScript);
                string fileName = Path.GetTempPath() + string.Format("{0} to {1}.sql", Path.GetFileNameWithoutExtension(tarnishedDacPacFileName), Path.GetFileNameWithoutExtension(platinumDacPacFileName));
                File.WriteAllText(fileName, cleaned);

                return SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildPackageName, new List<string>() { fileName }, "PLACEHOLDER");
            }
            return false;
        }
        internal static string ScriptDacPacDeltas(string platinumDacPacFileName, string tarnishedDacPacFileName)
        {
            string tmpFile = Path.GetTempFileName();

            ProcessHelper pHelper = new ProcessHelper();
            pHelper.AddArgument("/Action", "Script");

            pHelper.AddArgument("/SourceFile", platinumDacPacFileName);
            pHelper.AddArgument("/TargetFile", tarnishedDacPacFileName);
            pHelper.AddArgument("/TargetDatabaseName", "PLACEHOLDER");

            //Output
            pHelper.AddArgument("/OutputPath", tmpFile);

            //Scripting properties
            pHelper.AddArgument("/p:BlockOnPossibleDataLoss", "False","=");
            pHelper.AddArgument("/p:IgnorePermissions", "True", "=");
            pHelper.AddArgument("/p:IgnoreRoleMembership", "True", "=");
            pHelper.AddArgument("/p:IgnoreUserSettingsObjects", "True", "=");

            int result = pHelper.ExecuteProcess(sqlPackageExe);
            log.Info(pHelper.Output);
            if(result == 0)
            {
                string script = File.ReadAllText(tmpFile);
                File.Delete(tmpFile);
                return script;
            }
            else
            {
                log.Error(pHelper.Error);
                return string.Empty;
            }
        }
        internal static string CleanDacPacScript(string dacPacGeneratedScript)
        {
            string cutLine = @"PRINT N'The following operation was generated from a refactoring log file";

            int startOfLine = dacPacGeneratedScript.IndexOf(cutLine);
            int crAfter = dacPacGeneratedScript.IndexOf("\r\n", startOfLine);


            string cleaned = dacPacGeneratedScript.Substring(crAfter + 2);

            return cleaned;

        }
        
    }
}
