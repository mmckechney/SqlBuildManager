using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild;
namespace SqlSync.SqlBuild
{
    public class DacPacHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string sqlPack = null;
        private static List<string> appRoots = new List<string> (new string[] { @"E:\approot", @"F:\approot", @"G:\approot" }); //This really should be dynamic, but we can see it now. 
        private static string sqlPackageExe
        {
            get
            {
                if(string.IsNullOrWhiteSpace(sqlPack))
                {
                    lock (appRoots)
                    {
                        appRoots.Insert(0, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        appRoots.Insert(0, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Microsoft_SqlDB_DAC"); 
                        appRoots.Add(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\Extensions\Microsoft");
                    }
                    foreach(var dir in appRoots)
                    {
                        if(Directory.Exists(dir))
                        {
                            var files = Directory.GetFiles(dir, "sqlpackage.exe",SearchOption.AllDirectories);
                            if(files.Any())
                            {
                                sqlPack = files.First();
                                return sqlPack;
                            }
                        }
                    }
                    log.ErrorFormat("Unable to find sqlpackage.exe in directories: {0}", string.Join(" | ", appRoots.ToArray()));
                    throw new ArgumentException("Can not file sqlpackage.exe");
                }
                return sqlPack;
            }
        }
        public static bool ExtractDacPac(string sourceDatabase, string sourceServer, string userName, string password, string dacPacFileName)
        {
            log.InfoFormat("Extracting dacpac from {0} : {1}", sourceServer, sourceDatabase);
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
            pHelper.AddArgument("/p:IgnoreExtendedProperties", "True", "=");
            pHelper.AddArgument("/p:IgnoreUserLoginMappings", "True", "=");
            
            //pHelper.AddArgument("/p:IgnorePermissions", "True", "=");

            int result =  pHelper.ExecuteProcess(sqlPackageExe);

            log.Info(pHelper.Output);
            if(result != 0)
            {
                log.Error(pHelper.Error);
                return false;
            }

            return true;

        }
        
        public static DacpacDeltasStatus CreateSbmFromDacPacDifferences(string platinumDacPacFileName, string targetDacPacFileName, bool batchScripts, string buildRevision, out string buildPackageName)
        {
            log.InfoFormat("Generating SBM build from dacpac differences: {0} vs {1}", Path.GetFileName(platinumDacPacFileName), Path.GetFileName(targetDacPacFileName));
            string path = Path.GetDirectoryName(targetDacPacFileName) + @"\";
            buildPackageName = string.Empty;
            string rawScript = ScriptDacPacDeltas(platinumDacPacFileName, targetDacPacFileName, path);
            if (!string.IsNullOrEmpty(rawScript))
            {
                
                Directory.CreateDirectory(path);

                string cleaned;
                var cleanStatus = CleanDacPacScript(rawScript, out cleaned);
                switch(cleanStatus)
                {
                    case DacpacDeltasStatus.InSync:
                    case DacpacDeltasStatus.OnlyPostDeployment:
                        return cleanStatus;
                }

                string baseFileName = path + string.Format("{0} to {1}", Path.GetFileNameWithoutExtension(targetDacPacFileName), Path.GetFileNameWithoutExtension(platinumDacPacFileName));
                
                List<string> files = new List<string>();
                if (batchScripts)
                {
                    files = BatchAndSaveScripts(cleaned, path);
                }
                else
                {
                    File.WriteAllText(baseFileName + ".sql", cleaned);
                    files.Add(baseFileName + ".sql");
                }
                
                if(!string.IsNullOrWhiteSpace(buildRevision))
                {
                    var versionIns = Properties.Resources.VersionsInsert.Replace("{{BuildRevision}}", buildRevision);
                    string verName = Path.Combine(path, "Versions Insert.sql");
                    File.WriteAllText(verName, versionIns);
                    files.Add(verName);
                }

                buildPackageName = baseFileName + ".sbm";
                if (SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildPackageName, files, "client", true))
                {
                    return DacpacDeltasStatus.Success;
                }

            }
           
            return DacpacDeltasStatus.Failure;
        }

        internal static List<string> BatchAndSaveScripts(string masterScript, string workingPath)
        {
            string tmp;
            string fileName;
            Regex regNewLine = new Regex("\r\n", RegexOptions.Compiled);
            Regex regDupeSpaces = new Regex(" {2,}", RegexOptions.Compiled);
            var invalidChars = Path.GetInvalidFileNameChars();
            int counter = 0;

            
            List<string> files = new List<string>();
            log.Info("Parsing our master update script into batch scripts");
            var batched = SqlBuildHelper.ReadBatchFromScriptText(masterScript, true, false);
            foreach (var script in batched)
            {
                if(script.Trim().Length == 0)
                {
                    continue;
                }
                counter++;

                //Clean up to make a "nicer" file name
                tmp = regNewLine.Replace(script, " ", 1);
                tmp = new string(tmp.Where(x => !invalidChars.Contains(x)).ToArray()).Replace(";", "").Replace("SET XACT_ABORT ON", "").Trim();
                tmp = counter.ToString().PadLeft(4, '0') + "_" + ((tmp.Length > 50) ? tmp.Substring(0, 50) : tmp);
                tmp = regDupeSpaces.Replace(tmp, " ");
            
                fileName = Path.Combine(workingPath, tmp + ".sql");
                File.WriteAllText(fileName, script);
    
                files.Add(fileName);
            }
            return files;
        }
        internal static string ScriptDacPacDeltas(string platinumDacPacFileName, string targetDacPacFileName, string path)
        {
            log.InfoFormat("Generating scripts: {0} vs {1}", Path.GetFileName(platinumDacPacFileName), Path.GetFileName(targetDacPacFileName));

            string tmpFile = path + Path.GetFileName(targetDacPacFileName) + ".sql";

            ProcessHelper pHelper = new ProcessHelper();
            pHelper.AddArgument("/Action", "Script");

            pHelper.AddArgument("/SourceFile", platinumDacPacFileName);
            pHelper.AddArgument("/TargetFile", targetDacPacFileName);
            pHelper.AddArgument("/TargetDatabaseName", Path.GetFileNameWithoutExtension(targetDacPacFileName));

            //Output
            pHelper.AddArgument("/OutputPath", tmpFile);

            //Scripting properties

            pHelper.AddArgument("/p:IgnoreExtendedProperties", "True","=");
            pHelper.AddArgument("/p:BlockOnPossibleDataLoss", "False","=");
            pHelper.AddArgument("/p:IgnoreUserSettingsObjects", "True", "=");
            
            
            //pHelper.AddArgument("/p:IgnorePermissions", "True", "=");
            //pHelper.AddArgument("/p:IgnoreRoleMembership", "True", "=");
            //pHelper.AddArgument("/p:IgnoreUserSettingsObjects", "True", "=");

            int result = pHelper.ExecuteProcess(sqlPackageExe);
            log.Info(pHelper.Output);
            if(result == 0)
            {
                string script = File.ReadAllText(tmpFile);
                //File.Delete(tmpFile);
                return script;
            }
            else
            {
                log.Error(pHelper.Error);
                return string.Empty;
            }
        }
        internal static DacpacDeltasStatus CleanDacPacScript(string dacPacGeneratedScript, out string cleanedScript)
        {
            cleanedScript = string.Empty;

            string matchString = Regex.Escape(@"USE [$(DatabaseName)];");
            MatchCollection useMatches = Regex.Matches(dacPacGeneratedScript, matchString);
            var lastMatch = useMatches.Cast<Match>().Select(m => m.Index).LastOrDefault();


            if(lastMatch == -1) //Odd, there should be something, but oh well...
            {
                return DacpacDeltasStatus.InSync;
            }

            //Get rid of the SQLCMD header scripts
            if (useMatches.Count < 3)
            {
                int crAfter = dacPacGeneratedScript.IndexOf("GO", lastMatch);
                cleanedScript = dacPacGeneratedScript.Substring(crAfter + 2);
            }else
            {
                int crAfter = dacPacGeneratedScript.IndexOf("GO", useMatches[1].Index);
                cleanedScript = dacPacGeneratedScript.Substring(crAfter + 2);
                cleanedScript = cleanedScript.Replace("USE [$(DatabaseName)];", "--USE [$(DatabaseName)];");
            }

            string loginString = @"(CREATE USER)|(CREATE LOGIN)|(REVOKE CONNECT)|(EXECUTE sp_addrolemember)";
            while(Regex.Match(cleanedScript,loginString).Success)
            {
                var mLogin = Regex.Matches(cleanedScript, loginString).Cast<Match>().Select(m => m.Index).FirstOrDefault();
                int crAfter = cleanedScript.IndexOf("GO", mLogin);
                cleanedScript = cleanedScript.Substring(0, mLogin) + cleanedScript.Substring(crAfter + 2);
            }
          
            //Look for the "Post-Deployment Script Template"
            matchString = Regex.Escape(@"Post-Deployment Script Template");
            var postDeploy = Regex.Match(cleanedScript,matchString);
            if(postDeploy.Success)
            {
                var lineNumber = cleanedScript.Take(postDeploy.Index).Count(c => c == '\n') + 1;
                if(lineNumber < 25)
                {
                    return DacpacDeltasStatus.OnlyPostDeployment;
                }
            }

            //Look for no real scripts...
            var realLines = cleanedScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !x.StartsWith("PRINT", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !x.StartsWith("GO", StringComparison.InvariantCultureIgnoreCase));
            if(!realLines.Any())
            {
                return DacpacDeltasStatus.InSync;
            }

            return DacpacDeltasStatus.Success;

        }


        public static DacpacDeltasStatus UpdateBuildRunDataForDacPacSync(ref SqlBuildRunData runData, string targetServerName, string targetDatabase, string userName, string password, string workingDirectory, string buildRevision)
        {
            string tmpDacPacName = workingDirectory + targetDatabase + ".dacpac";
            if(!ExtractDacPac(targetDatabase, targetServerName, userName, password, tmpDacPacName))
            {
                return DacpacDeltasStatus.ExtractionFailure;
            }

            string sbmFileName;

            var stat = CreateSbmFromDacPacDifferences(runData.PlatinumDacPacFileName, tmpDacPacName, false, buildRevision, out sbmFileName);
            if(stat != DacpacDeltasStatus.Success)
            {
                return stat;
            }

          
            string projectFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
            string projectFileName = null;
            string result;

            log.InfoFormat("Preparing build package for processing");
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, false, out result))
            {
                return DacpacDeltasStatus.SbmProcessingFailure;
            }

            SqlSyncBuildData buildData;
            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projectFileName, false))
            {
                return DacpacDeltasStatus.SbmProcessingFailure;
            }

            runData.BuildData = buildData;
            runData.BuildFileName = sbmFileName;
            runData.ProjectFileName = projectFileName;

            log.InfoFormat("Build package ready");
            return DacpacDeltasStatus.Success;
        }

        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string targetDacpac, string database, string server, string username, string password, string buildRevision,  MultiDbData multiDb, out string sbmName)
        {
            string workingFolder = (!string.IsNullOrEmpty(rootLoggingPath) ? rootLoggingPath : Path.GetTempPath());
            if (!workingFolder.EndsWith("\\"))
                workingFolder = workingFolder + "\\";

            workingFolder = workingFolder + "Dacpac\\";
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            log.Info("Starting process: create SBM build file from dacpac settings");
            DacpacDeltasStatus stat = DacpacDeltasStatus.Processing;
            sbmName = string.Empty;

            if (!String.IsNullOrEmpty(targetDacpac))
            {
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacpac, false, buildRevision, out sbmName);
            }
            else if (!string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(server))
            {
                string targetDacPac = workingFolder + database + ".dacpac";
                if (!DacPacHelper.ExtractDacPac(database, server, username, password, targetDacPac))
                {
                    log.Error(string.Format("Error extracting dacpac from {0} : {1}", database, server));
                    return DacpacDeltasStatus.ExtractionFailure;
                }
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, false, buildRevision, out sbmName);
            }

            if (stat == DacpacDeltasStatus.Processing)
            {
                foreach (var serv in multiDb)
                {
                    server = serv.ServerName;
                    for (int i = 0; i < serv.OverrideSequence.Count; i++)
                    {
                        database = serv.OverrideSequence.ElementAt(i).Value[0].OverrideDbTarget;

                        string targetDacPac = workingFolder + database + ".dacpac"; ;
                        if (!DacPacHelper.ExtractDacPac(database, server, username, password, targetDacPac))
                        {
                            log.Error(string.Format("Error extracting dacpac from {0} : {1}", server, database));
                            return DacpacDeltasStatus.ExtractionFailure;
                        }
                        stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, false, buildRevision, out sbmName);

                        if (stat == DacpacDeltasStatus.InSync)
                        {
                            log.InfoFormat("{0} and {1} are already in  sync. Looping to next database.", Path.GetFileName(platinumDacPac), Path.GetFileName(targetDacPac));
                            stat = DacpacDeltasStatus.Processing;
                        }
                        else if (stat == DacpacDeltasStatus.OnlyPostDeployment)
                        {
                            log.InfoFormat("{0} to {1} appears to have only Post-Deployment steps. Will not be used as a source - looping to next database.", Path.GetFileName(platinumDacPac), Path.GetFileName(targetDacPac));
                            stat = DacpacDeltasStatus.Processing;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (stat != DacpacDeltasStatus.Processing)
                        break;
                }
            }

            switch (stat)
            {
                case DacpacDeltasStatus.Success:
                    log.Info("Successfully created SBM from two dacpacs");
                    break;
                case DacpacDeltasStatus.InSync:
                case DacpacDeltasStatus.OnlyPostDeployment:
                    log.Info("The database is already in sync with platinum dacpac");
                    break;
                case DacpacDeltasStatus.Processing: //we've looped through them all and they are in sync!
                    stat = DacpacDeltasStatus.InSync;
                    log.Info("All databases are already in sync with platinum dacpac");
                    break;
                default:
                    log.Error("Error creating build package from supplied Platinum and Target dacpac files");
                    break;

            }
            return stat;
        }
        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string database, string server, string username, string password, string buildRevision, MultiDbData multiDb, out string sbmName)
        {
            return GetSbmFromDacPac(rootLoggingPath, platinumDacPac, string.Empty, database, server, username, password, buildRevision, multiDb, out sbmName);
        }
        public static DacpacDeltasStatus GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb, out string sbmName)
        {

            if (cmd.MultiDbRunConfigFileName.Trim().ToLower().EndsWith("sql"))
            {
                //if we are getting the list from a SQL statement, then the database and server settings mean something different! Dont pass them in.
                 return GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.PlatinumDacpac,
                    cmd.TargetDacpac,
                    string.Empty,
                    string.Empty,
                    cmd.UserName,
                    cmd.Password,
                    cmd.BuildRevision,
                    multiDb, out sbmName);
            }
            else
            {
                return GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.PlatinumDacpac,
                    cmd.TargetDacpac,
                    cmd.Database,
                    cmd.Server,
                    cmd.UserName,
                    cmd.Password,
                    cmd.BuildRevision,
                    multiDb, out sbmName);
            }

        }

    }


}
