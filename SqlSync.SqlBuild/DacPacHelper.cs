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
            pHelper.AddArgument("/p:IgnoreUserLoginMappings", "True", "=");
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
        
        public static bool CreateSbmFromDacPacDifferences(string platinumDacPacFileName, string targetDacPacFileName, out string buildPackageName)
        {
            string rawScript = ScriptDacPacDeltas(platinumDacPacFileName, targetDacPacFileName);
            if(!string.IsNullOrEmpty(rawScript))
            {
                string path = Path.GetTempPath() + "dacpac-" + Guid.NewGuid().ToString() + @"\";
                Directory.CreateDirectory(path);

                string cleaned = CleanDacPacScript(rawScript);
                string fileName = path + string.Format("{0} to {1}", Path.GetFileNameWithoutExtension(targetDacPacFileName), Path.GetFileNameWithoutExtension(platinumDacPacFileName));
                File.WriteAllText(fileName + ".sql", cleaned);
                buildPackageName = fileName + ".sbm";
                return SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildPackageName, new List<string>() { fileName + ".sql" }, "client",true);
            }

            buildPackageName = string.Empty;
            return false;
        }
        internal static string ScriptDacPacDeltas(string platinumDacPacFileName, string targetDacPacFileName)
        {
            string tmpFile = Path.GetTempFileName();

            ProcessHelper pHelper = new ProcessHelper();
            pHelper.AddArgument("/Action", "Script");

            pHelper.AddArgument("/SourceFile", platinumDacPacFileName);
            pHelper.AddArgument("/TargetFile", targetDacPacFileName);
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


        internal static bool UpdateBuildRunDataForDacPacSync(ref SqlBuildRunData runData, string targetServerName, string targetDatabase, string userName, string password, string workingDirectory)
        {
            string tmpDacPacName = Path.GetTempPath() + targetDatabase + ".dacpac";
            if(!ExtractDacPac(targetDatabase, targetServerName, userName, password, tmpDacPacName))
            {
                return false;
            }

            string sbmFileName;
            if(!CreateSbmFromDacPacDifferences(runData.PlatinumDacPacFileName,tmpDacPacName,out sbmFileName))
            {
                return false;
            }

          
            string projectFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
            string projectFileName = null;
            string result;
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, false, out result))
            {
                return false;
            }

            SqlSyncBuildData buildData;
            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projectFileName, false))
            {
                return false;
            }

            runData.BuildData = buildData;
            runData.BuildFileName = sbmFileName;

            return true;
        }
    }
}
