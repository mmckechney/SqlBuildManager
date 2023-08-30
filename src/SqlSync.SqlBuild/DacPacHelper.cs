using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
namespace SqlSync.SqlBuild
{
    public class DacPacHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ExtractDacPac(string sourceDatabase, string sourceServer, AuthenticationType authType, string userName, string password, string dacPacFileName, int timeouts, string managedIdentityClientId)
        {

            try
            {
                log.LogInformation($"Extracting dacpac from {sourceServer} : {sourceDatabase}");

                DacExtractOptions opts = new DacExtractOptions();
                opts.IgnoreExtendedProperties = true;
                opts.IgnoreUserLoginMappings = true;
                opts.LongRunningCommandTimeout = timeouts;
                opts.CommandTimeout = timeouts;
                opts.DatabaseLockTimeout = timeouts;

                ConnectionData connData = new ConnectionData(sourceServer, sourceDatabase);
                connData.AuthenticationType = authType;
                if (!string.IsNullOrWhiteSpace(userName)) connData.UserId = userName;
                if (!string.IsNullOrWhiteSpace(password)) connData.Password = password;

                //Pre-test the connection. the DacServices can hang for a long time if the connection is bad
                if (!ConnectionHelper.TestDatabaseConnection(sourceDatabase, sourceServer, userName, password,authType, timeouts, managedIdentityClientId))
                {
                    log.LogError($"Unable to create Dacpac for {sourceServer}.{sourceDatabase}. Database connection test failed.");
                    return false;
                }
                var connString = ConnectionHelper.GetConnectionString(connData);
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                DacServices service = new DacServices(connString);
                service.Extract(dacPacFileName, sourceDatabase, "Sql Build Manager", ver, "Sql Build Manager",null, opts);
                log.LogInformation($"dacpac from {sourceServer}.{sourceDatabase} saved to {dacPacFileName}");
                return true;
            }
            catch (Exception exe)
            {
                log.LogError($"Problem creating DACPAC from {sourceServer}.{sourceDatabase}: {exe.ToString()}");
                return false;
            }


        }

        internal static string ScriptDacPacDeltas(string platinumDacPacFileName, string targetDacPacFileName, string path, bool allowObjectDelete, bool ignoreCompatError)
        {
            try
            {
                log.LogInformation($"Generating scripts: {Path.GetFileName(platinumDacPacFileName)} vs {Path.GetFileName(targetDacPacFileName)}");
                string tmpFile = Path.Combine(path, Path.GetFileName(targetDacPacFileName) + ".sql");
                DacDeployOptions opts = new DacDeployOptions();
                opts.IgnoreExtendedProperties = true;
                opts.BlockOnPossibleDataLoss = false;
                opts.IgnoreUserSettingsObjects = true;
                if (allowObjectDelete)
                {
                    opts.DropObjectsNotInSource = true;
                }

                if (ignoreCompatError)
                {
                    opts.AllowIncompatiblePlatform = true;
                }


                DacPackage platPackage = DacPackage.Load(platinumDacPacFileName);
                DacPackage targPackage = DacPackage.Load(targetDacPacFileName);
                string script = DacServices.GenerateDeployScript(platPackage, targPackage, Path.GetFileNameWithoutExtension(targetDacPacFileName), opts);
                return script;

            }
            catch (Microsoft.SqlServer.Dac.DacServicesException dexe)
            {
                if (dexe.ToString().Contains("DeploymentCompatibilityException"))
                {
                    log.LogWarning("Encountered a Deployment Compatibility Exception! Generating scripts with \"AllowIncompatiblePlatform=true\" setting. This may result in script runtime errors.");
                    return ScriptDacPacDeltas(platinumDacPacFileName, targetDacPacFileName, path, allowObjectDelete, true);
                }
                else
                {
                    log.LogError($"Problem creating scripts between {platinumDacPacFileName} and {targetDacPacFileName}: {dexe.ToString()}");
                    return string.Empty;
                }
            }
            catch (Exception exe)
            {
                log.LogError($"Problem creating scripts between {platinumDacPacFileName} and {targetDacPacFileName}: {exe.ToString()}");
                return string.Empty;
            }


        }

        public static DacpacDeltasStatus CreateSbmFromDacPacDifferences(string platinumDacPacFileName, string targetDacPacFileName, bool batchScripts, string buildRevision, int defaultScriptTimeout, bool allowObjectDelete, out string buildPackageName)
        {
            log.LogInformation($"Generating SBM build from dacpac differences: {Path.GetFileName(platinumDacPacFileName)} vs {Path.GetFileName(targetDacPacFileName)}");
            string path = Path.GetDirectoryName(targetDacPacFileName);
            buildPackageName = string.Empty;
            string rawScript = ScriptDacPacDeltas(platinumDacPacFileName, targetDacPacFileName, path, allowObjectDelete, false);
            if (!string.IsNullOrEmpty(rawScript))
            {

                Directory.CreateDirectory(path);

                string cleaned;
                var cleanStatus = CleanDacPacScript(rawScript, out cleaned);
                switch (cleanStatus)
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

                if (!string.IsNullOrWhiteSpace(buildRevision))
                {
                    var versionIns = Properties.Resources.VersionsInsert.Replace("{{BuildRevision}}", buildRevision);
                    string verName = Path.Combine(path, "Versions Insert.sql");
                    File.WriteAllText(verName, versionIns);
                    files.Add(verName);
                }

                buildPackageName = baseFileName + ".sbm";
                if (SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildPackageName, files, "client", true, defaultScriptTimeout))
                {

                    //Clean up generated scripts files
                    files.ForEach(f =>
                    {
                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception exe)
                        {
                            log.LogError($"Unable to delete file {f}. {exe.ToString()}");
                        }
                    });
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
                if (script.Trim().Length == 0)
                {
                    continue;
                }
                counter++;

                //Clean up to make a "nicer" file name
                tmp = regNewLine.Replace(script, " ", 1);
                tmp = new string(tmp.Where(x => !invalidChars.Contains(x)).ToArray()).Replace(";", "").Replace("SET XACT_ABORT ON", "").Trim();
                tmp = counter.ToString().PadLeft(4, '0') + "_" + ((tmp.Length > 50) ? tmp.Substring(0, 50) : tmp);
                if (tmp.IndexOf('(') > -1)
                {
                    tmp = tmp.Substring(0, tmp.IndexOf('('));
                }
                while (tmp.EndsWith('.') || tmp.EndsWith('\'')) tmp = tmp.Substring(0, tmp.Length - 1);
                tmp = regDupeSpaces.Replace(tmp, " ");

                fileName = Path.Combine(workingPath, tmp.Trim() + ".sql");
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
            int index = -1;
            bool matchFound = false;
            cleanedScript = dacPacGeneratedScript;

            //look for basic header 
            string endofHeader = Regex.Escape(@"Please run the below section of statements against the database");
            MatchCollection endMatchs = Regex.Matches(dacPacGeneratedScript, endofHeader);
            var endMatch = endMatchs.Cast<Match>().LastOrDefault();
            index = (endMatch == null) ? -1 : endMatch.Index;
            if (endMatch != null && endMatch.Index != -1)
            {
                matchFound = true;
                var endOfLineIndex = dacPacGeneratedScript.IndexOf(Environment.NewLine, index);
                cleanedScript = dacPacGeneratedScript.Substring(endOfLineIndex + 1);
            }

            //look for the database settings header 
            endMatchs = Regex.Matches(cleanedScript, Regex.Escape("$(DatabaseName)"));
            endMatch = endMatchs.Cast<Match>().LastOrDefault();
            if (endMatch != null && endMatch.Index != -1)
            {
                matchFound = true;
                //get the "GO" delimiter after this match
                index = 2 + Regex.Matches(cleanedScript, "GO").Where(m => m.Index > endMatch.Index).FirstOrDefault().Index;
                cleanedScript = cleanedScript.Substring(index);
            }

            if (!matchFound)
            {
                log.LogInformation("Unable to find script headers, nothing to update?");
                log.LogDebug($"Script contents:{Environment.NewLine}{dacPacGeneratedScript}");
                return DacpacDeltasStatus.InSync;
            }

            //Look for the "Post-Deployment Script Template"
            string matchString = Regex.Escape(@"Post-Deployment Script Template");
            var postDeploy = Regex.Match(cleanedScript, matchString);
            if (postDeploy.Success)
            {
                var lineNumber = cleanedScript.Take(postDeploy.Index).Count(c => c == '\n') + 1;
                if (lineNumber < 25)
                {
                    log.LogDebug($"Cleaned script contents:{Environment.NewLine}{cleanedScript}");
                    return DacpacDeltasStatus.OnlyPostDeployment;
                }
            }

            //Look for no real scripts...
            var realLines = cleanedScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !x.StartsWith("PRINT", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !x.StartsWith("GO", StringComparison.InvariantCultureIgnoreCase));
            if (!realLines.Any())
            {
                log.LogDebug($"Cleaned script contents:{Environment.NewLine}{cleanedScript}");
                return DacpacDeltasStatus.InSync;
            }

            log.LogDebug($"Cleaned script contents:{Environment.NewLine}{cleanedScript}");
            return DacpacDeltasStatus.Success;

        }


        public static DacpacDeltasStatus UpdateBuildRunDataForDacPacSync(ref SqlBuildRunData runData, string targetServerName, string targetDatabase, AuthenticationType authType, string userName, string password, string workingDirectory, string buildRevision, int defaultScriptTimeout, bool allowObjectDelete, string managedIdentityClientId)
        {
            string tmpDacPacName = Path.Combine(workingDirectory, targetDatabase + ".dacpac");
            if (!ExtractDacPac(targetDatabase, targetServerName, authType, userName, password, tmpDacPacName, runData.DefaultScriptTimeout, managedIdentityClientId))
            {
                return DacpacDeltasStatus.ExtractionFailure;
            }

            string sbmFileName;

            var stat = CreateSbmFromDacPacDifferences(runData.PlatinumDacPacFileName, tmpDacPacName, false, buildRevision, defaultScriptTimeout, allowObjectDelete, out sbmFileName);
            if (stat != DacpacDeltasStatus.Success)
            {
                return stat;
            }


            string projectFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
            string projectFileName = null;
            string result;

            log.LogInformation("Preparing build package for processing");
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, false, false, out result))
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

        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string targetDacpac, string database, string server, AuthenticationType authType, string username, string password, string buildRevision, int defaultScriptTimeout, MultiDbData multiDb, out string sbmName, bool batchScripts, bool allowObjectDelete, string managedIdentityClientId)
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
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacpac, batchScripts, buildRevision, defaultScriptTimeout, allowObjectDelete, out sbmName);
            }
            else if (!string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(server))
            {
                string targetDacPac = Path.Combine(workingFolder, database + ".dacpac");
                if (!DacPacHelper.ExtractDacPac(database, server, authType, username, password, targetDacPac, defaultScriptTimeout, managedIdentityClientId))
                {
                    log.LogError($"Error extracting dacpac from {database} : {server}");
                    return DacpacDeltasStatus.ExtractionFailure;
                }
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, batchScripts, buildRevision, defaultScriptTimeout, allowObjectDelete, out sbmName);
            }

            if (stat == DacpacDeltasStatus.Processing && multiDb != null)
            {
                foreach (var serv in multiDb)
                {
                    server = serv.ServerName;
                    for (int i = 0; i < serv.Overrides.Count; i++)
                    {
                        database = serv.Overrides.ElementAt(i).OverrideDbTarget;

                        string targetDacPac = Path.Combine(workingFolder, database + ".dacpac");
                        if (!DacPacHelper.ExtractDacPac(database, server, authType, username, password, targetDacPac, defaultScriptTimeout, managedIdentityClientId))
                        {
                            log.LogError($"Error extracting dacpac from {server} : {database}");
                            return DacpacDeltasStatus.ExtractionFailure;
                        }
                        stat = DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacPac, targetDacPac, batchScripts, buildRevision, defaultScriptTimeout, allowObjectDelete, out sbmName);

                        if (stat == DacpacDeltasStatus.InSync)
                        {
                            log.LogInformation($"{Path.GetFileName(platinumDacPac)} and {Path.GetFileName(targetDacPac)} are already in  sync. Looping to next database.");
                            stat = DacpacDeltasStatus.Processing;
                        }
                        else if (stat == DacpacDeltasStatus.OnlyPostDeployment)
                        {
                            log.LogInformation($"{Path.GetFileName(platinumDacPac)} to {Path.GetFileName(targetDacPac)} appears to have only Post-Deployment steps. Will not be used as a source - looping to next database.");
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
        public static DacpacDeltasStatus GetSbmFromDacPac(string rootLoggingPath, string platinumDacPac, string database, AuthenticationType authType, string server, string username, string password, string buildRevision, int defaultScriptTimeout, MultiDbData multiDb, out string sbmName, bool batchScripts, bool allowObjectDelete, string managedIdentityClientId)
        {
            return GetSbmFromDacPac(rootLoggingPath, platinumDacPac, string.Empty, database, server, authType, username, password, buildRevision, defaultScriptTimeout, multiDb, out sbmName, batchScripts, allowObjectDelete, managedIdentityClientId);
        }


    }


}
