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
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Dac;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
namespace SqlSync.SqlBuild
{
    public class DacPacHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        public static bool ExtractDacPac(string sourceDatabase, string sourceServer, string userName, string password, string dacPacFileName)
        {

            try
            {
                log.LogInformation($"Extracting dacpac from {sourceServer} : {sourceDatabase}");

                DacExtractOptions opts = new DacExtractOptions();
                opts.IgnoreExtendedProperties = true;
                opts.IgnoreUserLoginMappings = true;


                SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
                connBuilder.UserID = userName;
                connBuilder.Password = password;
                connBuilder.DataSource = sourceServer;
                connBuilder.InitialCatalog = sourceDatabase;

                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                DacServices service = new DacServices(connBuilder.ConnectionString);
                service.Extract(dacPacFileName, sourceDatabase, "Sql Build Manager", ver);
                log.LogInformation($"dacpac from {sourceServer}.{sourceDatabase} saved to {dacPacFileName}");
                return true;
            }
            catch(Exception exe)
            {
                log.LogError($"Problem creating DACPAC from {sourceServer}.{sourceDatabase}: {exe.ToString()}");
                return false;
            }
           

        }

        internal static string ScriptDacPacDeltas(string platinumDacPacFileName, string targetDacPacFileName, string path)
        {
            try
            {
                log.LogInformation($"Generating scripts: {Path.GetFileName(platinumDacPacFileName)} vs {Path.GetFileName(targetDacPacFileName)}");
                string tmpFile = Path.Combine(path, Path.GetFileName(targetDacPacFileName) + ".sql");
                DacDeployOptions opts = new DacDeployOptions();
                opts.IgnoreExtendedProperties = true;
                opts.BlockOnPossibleDataLoss = false;
                opts.IgnoreUserSettingsObjects = true;


                DacPackage platPackage = DacPackage.Load(platinumDacPacFileName);
                DacPackage targPackage = DacPackage.Load(targetDacPacFileName);
                string script =  DacServices.GenerateDeployScript(platPackage, targPackage, Path.GetFileNameWithoutExtension(targetDacPacFileName), opts);
                return script;

            }
            catch (Exception exe)
            {
                log.LogError($"Problem creating scripts between {platinumDacPacFileName} and {targetDacPacFileName}: {exe.ToString()}");
                return string.Empty;
            }

           
        }
      
        public static DacpacDeltasStatus CreateSbmFromDacPacDifferences(string platinumDacPacFileName, string targetDacPacFileName, bool batchScripts, string buildRevision, int defaultScriptTimeout, out string buildPackageName)
        {
            log.LogInformation($"Generating SBM build from dacpac differences: {Path.GetFileName(platinumDacPacFileName)} vs { Path.GetFileName(targetDacPacFileName)}");
            string path = Path.GetDirectoryName(targetDacPacFileName);
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

                string baseFileName = Path.Combine(path, string.Format("{0}_to_{1}", Path.GetFileNameWithoutExtension(targetDacPacFileName), Path.GetFileNameWithoutExtension(platinumDacPacFileName)));
                
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
                if (SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildPackageName, files, "client", true, defaultScriptTimeout))
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
            log.LogInformation("Parsing our master update script into batch scripts");
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
       
        /// <summary>
        /// Tries to eliminate the header information that the DACPAC tooling adds 
        /// </summary>
        /// <param name="dacPacGeneratedScript"></param>
        /// <param name="cleanedScript"></param>
        /// <returns></returns>
        internal static DacpacDeltasStatus CleanDacPacScript(string dacPacGeneratedScript, out string cleanedScript)
        {
            cleanedScript = dacPacGeneratedScript;

            //string matchString = Regex.Escape(@"USE [$(DatabaseName)];");
            //MatchCollection useMatches = Regex.Matches(dacPacGeneratedScript, matchString);
            //var lastMatch = useMatches.Cast<Match>().Select(m => m.Index).LastOrDefault();


            //if(lastMatch == -1) //Odd, there should be something, but oh well...
            //{
            //    return DacpacDeltasStatus.InSync;
            //}

            ////Get rid of the SQLCMD header scripts
            //if (useMatches.Count < 3)
            //{
            //    int crAfter = dacPacGeneratedScript.IndexOf("GO", lastMatch);
            //    cleanedScript = dacPacGeneratedScript.Substring(crAfter + 2);
            //}else
            //{
            //    int crAfter = dacPacGeneratedScript.IndexOf("GO", useMatches[1].Index);
            //    cleanedScript = dacPacGeneratedScript.Substring(crAfter + 2);
            //    cleanedScript = cleanedScript.Replace("USE [$(DatabaseName)];", "--USE [$(DatabaseName)];");
            //}

            //string loginString = @"(CREATE USER)|(CREATE LOGIN)|(REVOKE CONNECT)|(EXECUTE sp_addrolemember)";
            //while(Regex.Match(cleanedScript,loginString).Success)
            //{
            //    var mLogin = Regex.Matches(cleanedScript, loginString).Cast<Match>().Select(m => m.Index).FirstOrDefault();
            //    int crAfter = cleanedScript.IndexOf("GO", mLogin);
            //    cleanedScript = cleanedScript.Substring(0, mLogin) + cleanedScript.Substring(crAfter + 2);
            //}

           string endofHeader = Regex.Escape(@"Please run the below section of statements against the database");
            MatchCollection endMatchs = Regex.Matches(dacPacGeneratedScript, endofHeader);
            var endMatch = endMatchs.Cast<Match>().LastOrDefault();
            if(endMatch == null || endMatch.Index == -1 || string.IsNullOrWhiteSpace(endMatch.Value)) //Odd, there should be something, but oh well...
            {
                return DacpacDeltasStatus.InSync;
            }
            else
            {
                var endOfLineIndex = cleanedScript.IndexOf("\n", endMatch.Index);
                cleanedScript = cleanedScript.Substring(endOfLineIndex + 1);
               // var lineNumber = dacPacGeneratedScript.Take(endMatch).Count(c => c == '\n') + 1;

            }

            //Look for the "Post-Deployment Script Template"
            string matchString = Regex.Escape(@"Post-Deployment Script Template");
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


        public static DacpacDeltasStatus UpdateBuildRunDataForDacPacSync(ref SqlBuildRunData runData, string targetServerName, string targetDatabase, string userName, string password, string workingDirectory, string buildRevision, int defaultScriptTimeout)
        {
            string tmpDacPacName = Path.Combine(workingDirectory,targetDatabase + ".dacpac");
            if(!ExtractDacPac(targetDatabase, targetServerName, userName, password, tmpDacPacName))
            {
                return DacpacDeltasStatus.ExtractionFailure;
            }

            string sbmFileName;

            var stat = CreateSbmFromDacPacDifferences(runData.PlatinumDacPacFileName, tmpDacPacName, false, buildRevision, defaultScriptTimeout, out sbmFileName);
            if(stat != DacpacDeltasStatus.Success)
            {
                return stat;
            }

          
            string projectFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
            string projectFileName = null;
            string result;

            log.LogInformation("Preparing build package for processing");
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, false, false,out result))
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

            log.LogInformation("Build package ready");
            return DacpacDeltasStatus.Success;
        }

        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string targetDacpac, string database, string server, string username, string password, string buildRevision, int defaultScriptTimeout,  MultiDbData multiDb, out string sbmName)
        {
            string workingFolder = (!string.IsNullOrEmpty(rootLoggingPath) ? rootLoggingPath : Path.GetTempPath());
   
            workingFolder = Path.Combine(workingFolder, "Dacpac");
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            log.LogInformation("Starting process: create SBM build file from dacpac settings");
            DacpacDeltasStatus stat = DacpacDeltasStatus.Processing;
            sbmName = string.Empty;

            if (!String.IsNullOrEmpty(targetDacpac))
            {
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacpac, false, buildRevision, defaultScriptTimeout, out sbmName);
            }
            else if (!string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(server))
            {
                string targetDacPac = Path.Combine(workingFolder, database + ".dacpac");
                if (!DacPacHelper.ExtractDacPac(database, server, username, password, targetDacPac))
                {
                    log.LogError($"Error extracting dacpac from {database} : {server}");
                    return DacpacDeltasStatus.ExtractionFailure;
                }
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, false, buildRevision, defaultScriptTimeout, out sbmName);
            }

            if (stat == DacpacDeltasStatus.Processing && multiDb != null)
            {
                foreach (var serv in multiDb)
                {
                    server = serv.ServerName;
                    for (int i = 0; i < serv.OverrideSequence.Count; i++)
                    {
                        database = serv.OverrideSequence.ElementAt(i).Value[0].OverrideDbTarget;

                        string targetDacPac = Path.Combine(workingFolder, database + ".dacpac");
                        if (!DacPacHelper.ExtractDacPac(database, server, username, password, targetDacPac))
                        {
                            log.LogError($"Error extracting dacpac from {server} : {database}");
                            return DacpacDeltasStatus.ExtractionFailure;
                        }
                        stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, false, buildRevision, defaultScriptTimeout, out sbmName);

                        if (stat == DacpacDeltasStatus.InSync)
                        {
                            log.LogInformation($"{Path.GetFileName(platinumDacPac)} and {Path.GetFileName(targetDacPac)} are already in  sync. Looping to next database.");
                            stat = DacpacDeltasStatus.Processing;
                        }
                        else if (stat == DacpacDeltasStatus.OnlyPostDeployment)
                        {
                            log.LogInformation($"{Path.GetFileName(platinumDacPac)} to { Path.GetFileName(targetDacPac)} appears to have only Post-Deployment steps. Will not be used as a source - looping to next database.");
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
                    log.LogInformation("Successfully created SBM from two dacpacs");
                    break;
                case DacpacDeltasStatus.InSync:
                case DacpacDeltasStatus.OnlyPostDeployment:
                    log.LogInformation("The database is already in sync with platinum dacpac");
                    break;
                case DacpacDeltasStatus.Processing: //we've looped through them all and they are in sync!
                    stat = DacpacDeltasStatus.InSync;
                    log.LogInformation("All databases are already in sync with platinum dacpac");
                    break;
                default:
                    log.LogError("Error creating build package from supplied Platinum and Target dacpac files");
                    break;

            }
            return stat;
        }
        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string database, string server, string username, string password, string buildRevision, int defaultScriptTimeout, MultiDbData multiDb, out string sbmName)
        {
            return GetSbmFromDacPac(rootLoggingPath, platinumDacPac, string.Empty, database, server, username, password, buildRevision, defaultScriptTimeout, multiDb, out sbmName);
        }
        public static DacpacDeltasStatus GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb, out string sbmName)
        {

            if (cmd.MultiDbRunConfigFileName.Trim().ToLower().EndsWith("sql"))
            {
                //if we are getting the list from a SQL statement, then the database and server settings mean something different! Dont pass them in.
                 return GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.DacPacArgs.PlatinumDacpac,
                    cmd.DacPacArgs.TargetDacpac,
                    string.Empty,
                    string.Empty,
                    cmd.AuthenticationArgs.UserName,
                    cmd.AuthenticationArgs.Password,
                    cmd.BuildRevision,
                    cmd.DefaultScriptTimeout,
                    multiDb, out sbmName);
            }
            else
            {
                return GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.DacPacArgs.PlatinumDacpac,
                    cmd.DacPacArgs.TargetDacpac,
                    cmd.Database,
                    cmd.Server,
                    cmd.AuthenticationArgs.UserName,
                    cmd.AuthenticationArgs.Password,
                    cmd.BuildRevision,
                    cmd.DefaultScriptTimeout,
                    multiDb, out sbmName);
            }

        }

    }


}
