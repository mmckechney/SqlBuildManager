using Microsoft.Extensions.Logging;
using Spectre.Console;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SqlSync.SqlBuild;
using sqlB = SqlSync.SqlBuild;
using sqlM = SqlSync.SqlBuild.Models;
using Cryptography = SqlBuildManager.Console.CommandLine.Cryptography;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int CreateDacpac(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log.LogInformation("Creating DACPAC");
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }


            string fullName = Path.GetFullPath(cmdLine.DacpacName);
            string path = Path.GetDirectoryName(fullName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!sqlB.DacPacHelper.ExtractDacPac(cmdLine.Database, cmdLine.Server, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, fullName, cmdLine.DefaultScriptTimeout, cmdLine.IdentityArgs.ClientId))
            {
                log.LogError($"Error creating the dacpac from {cmdLine.Server} : {cmdLine.Database}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"DACPAC created from {cmdLine.Server} : {cmdLine.Database} saved to -- {fullName}");
            }
            return 0;
        }

        internal static async Task<int> CreateFromDacpacDiff(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            #region Validate flags
            string[] errorMessages;
            int pwVal = Validation.ValidateUserNameAndPassword(cmdLine, out errorMessages);
            if (pwVal != 0)
            {
                log.LogError(errorMessages.Aggregate((a, b) => a + "; " + b));
            }

            if (File.Exists(cmdLine.OutputSbm))
            {
                log.LogError($"The --outputsbm file already exists at {cmdLine.OutputSbm}. Please choose another name or delete the existing file.");
                System.Environment.Exit(-1);
            }
            #endregion

            string name;
            cmdLine.RootLoggingPath = Path.GetDirectoryName(cmdLine.OutputSbm);

            var status = Worker.GetSbmFromDacPac(cmdLine, new SqlSync.SqlBuild.MultiDb.MultiDbData(), out name, true);
            if (status == sqlB.DacpacDeltasStatus.Success)
            {
                File.Move(name, cmdLine.OutputSbm);
                await ListPackageScriptsAsync(new FileInfo[] { new FileInfo(cmdLine.OutputSbm) }, true).ConfigureAwait(false);
                log.LogInformation($"SBM package successfully created at {cmdLine.OutputSbm}");
            }
            else
            {
                log.LogError($"Error creating SBM package: {status.ToString()}");
            }

            return 0;
        }



        internal static void SyncronizeDatabase(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            bool success = Synchronize.SyncDatabases(cmdLine);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }


        internal static void GetDifferences(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            string history = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            log.LogInformation(history);
            System.Environment.Exit(0);
        }

        internal static async Task CreateBackout(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            string packageName = await BackoutCommandLine.CreateBackoutPackage(cmdLine).ConfigureAwait(false);
            if (!String.IsNullOrEmpty(packageName))
            {
                log.LogInformation(packageName);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(856);
            }
        }

        internal static async Task<int> AddScriptsToPackage(CommandLineArgs cmdLine)
        {
            if (!File.Exists(cmdLine.OutputSbm))
            {
                log.LogWarning($"The specified output file '{cmdLine.OutputSbm}' does not exists. If you want to create a package, please use the \"sbm create\" command");
                return -646;
            }

            if (Path.GetExtension(cmdLine.OutputSbm).ToLower() == ".sbx")
            {
                string sbxFileName = cmdLine.OutputSbm;
                string workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
                if (string.IsNullOrWhiteSpace(workingDir))
                {
                    workingDir = Directory.GetCurrentDirectory();
                }
                log.LogInformation("Creating Base Build File XML");
                var buildModel = sqlB.SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                var projFile = Path.Combine(workingDir, sqlB.XmlFileNames.MainProjectFile);
                await sqlM.SqlSyncBuildDataXmlSerializer.SaveAsync(projFile, buildModel).ConfigureAwait(false);
                await sqlB.SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildModel, workingDir, sbxFileName, includeHistoryAndLogs: true).ConfigureAwait(false);
                var counter = 1.0;
                foreach (var file in cmdLine.Scripts)
                {
                    if (file.Directory.ToString().ToLower() != workingDir.ToLower())
                    {
                        File.Copy(file.FullName, Path.Combine(workingDir, file.Name));
                    }
                    buildModel = await sqlB.SqlBuildFileHelper.AddScriptFileToBuildAsync(buildModel, projFile, file.Name, counter, "", true, true, "client", true, sbxFileName, true, true, Environment.UserName, 500, Guid.NewGuid(), "").ConfigureAwait(false);
                    counter++;
                }
                // Note: AddScriptFileToBuildAsync with saveToZip=true already persists the project file and packages the zip.
                // Avoid overwriting the .sbx zip with plain XML.
                await sqlM.SqlSyncBuildDataXmlSerializer.SaveAsync(projFile, buildModel).ConfigureAwait(false);
            }
            else
            {
                string workingDir = "", projFilePath = "", projectFileName = "";
                (_, workingDir, projFilePath, projectFileName, _) = await sqlB.SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(cmdLine.OutputSbm, null, true, true).ConfigureAwait(false);
                var loadResult = await sqlB.SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projectFileName, true).ConfigureAwait(false);
                bool success = loadResult.Item1;
                sqlM.SqlSyncBuildDataModel buildModel = loadResult.Item2;
                if (success)
                {
                    List<string> copied = new List<string>();
                    cmdLine.Scripts.ToList().ForEach(f =>
                    {
                        if (f.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, f.Name)))
                        {
                            File.Copy(f.FullName, Path.Combine(workingDir, f.Name));
                            copied.Add(Path.Combine(workingDir, f.Name));
                        }
                    });
                    sqlB.BuildDataHelper.GetLastBuildNumberAndDb(buildModel, out double lastBuildNumber, out string lastDatabase);
                    foreach (var file in cmdLine.Scripts)
                    {
                        lastBuildNumber++;
                        buildModel = await sqlB.SqlBuildFileHelper.AddScriptFileToBuildAsync(buildModel, projectFileName, file.Name, lastBuildNumber, "", true, true, "client", true, cmdLine.OutputSbm, true, true, Environment.UserName, 500, Guid.NewGuid(), "").ConfigureAwait(false);
                    }
                    await sqlB.SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(workingDir).ConfigureAwait(false);
                }
                else
                {
                    log.LogError($"Unable to extract and read the build file at {cmdLine.OutputSbm}!");
                    return -952;
                }
            }
            log.LogInformation($"Added {cmdLine.Scripts.Count()} scripts to '{cmdLine.OutputSbm}'");
            var fi = new FileInfo(cmdLine.OutputSbm);
            await ListPackageScriptsAsync(new FileInfo[] { fi }, true).ConfigureAwait(false);
            return 0;

        }

        internal static async Task<int> CreatePackageFromDacpacs(string outputSbm, FileInfo platinumDacpac, FileInfo targetDacpac, bool allowObjectDelete)
        {
            var outputSbmFile = Path.GetFullPath(outputSbm);
            var (res, tmpSbm) = await sqlB.DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumDacpac.FullName, targetDacpac.FullName, true, string.Empty, 500, allowObjectDelete).ConfigureAwait(false);

            if (res == sqlB.DacpacDeltasStatus.Success)
            {
                File.Move(tmpSbm, outputSbmFile, true);
                log.LogInformation($"Created SBM package:  {outputSbmFile}");
                await ListPackageScriptsAsync(new FileInfo[] { new FileInfo(outputSbmFile) }, true).ConfigureAwait(false);
                return 0;
            }
            else
            {
                switch (res)
                {
                    case sqlB.DacpacDeltasStatus.InSync:
                    case sqlB.DacpacDeltasStatus.OnlyPostDeployment:
                        log.LogWarning("No package created -- the databases are already in sync");
                        break;
                    default:
                        log.LogError("There was an error creating the requested package");
                        break;
                }
                return -232323;
            }
        }

        internal static async Task<int> CreatePackageFromDiff(CommandLineArgs cmdLine)
        {
            string sbmFileName = Path.GetFullPath(cmdLine.OutputSbm);
            if (File.Exists(sbmFileName))
            {
                log.LogError($"The output file '{sbmFileName}' already exists. Please delete the file or use 'sbm add' if you want to add new scripts to the file");
                return -343;
            }
            string path = Path.GetDirectoryName(sbmFileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string id = Guid.NewGuid().ToString();
            string goldTmp = Path.Combine(path, $"gold-{id}.dacpac");
            string targetTmp = Path.Combine(path, $"target-{id}.dacpac");
            if (!sqlB.DacPacHelper.ExtractDacPac(cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.SynchronizeArgs.GoldServer, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, goldTmp, cmdLine.DefaultScriptTimeout, cmdLine.IdentityArgs.ClientId))
            {
                log.LogError($"Error creating the tempprary dacpac from {cmdLine.SynchronizeArgs.GoldServer} : {cmdLine.SynchronizeArgs.GoldDatabase}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"Temporary DACPAC created from {cmdLine.SynchronizeArgs.GoldServer} : {cmdLine.SynchronizeArgs.GoldDatabase} saved to -- {goldTmp}");
            }

            if (!sqlB.DacPacHelper.ExtractDacPac(cmdLine.Database, cmdLine.Server, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, targetTmp, cmdLine.DefaultScriptTimeout, cmdLine.IdentityArgs.ClientId))
            {
                log.LogError($"Error creating the tempprary dacpac from {cmdLine.Server} : {cmdLine.Database}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"Temporary DACPAC created from {cmdLine.Server} : {cmdLine.Database} saved to -- {targetTmp}");
            }

            var (res, tmpSbm) = await sqlB.DacPacHelper.CreateSbmFromDacPacDifferencesAsync(goldTmp, targetTmp, true, string.Empty, 500, cmdLine.AllowObjectDelete).ConfigureAwait(false);
            log.LogInformation("Cleaning up temporary files");
            File.Delete(goldTmp);
            File.Delete(targetTmp);

            if (res == sqlB.DacpacDeltasStatus.Success)
            {
                File.Move(tmpSbm, sbmFileName);
                log.LogInformation($"Created SBM package:  {sbmFileName}");
                await ListPackageScriptsAsync(new FileInfo[] { new FileInfo(sbmFileName) }, true).ConfigureAwait(false);
                return 0;
            }
            else
            {
                switch (res)
                {
                    case sqlB.DacpacDeltasStatus.InSync:
                    case sqlB.DacpacDeltasStatus.OnlyPostDeployment:
                        log.LogWarning("No package created -- the databases are already in sync");
                        break;
                    default:
                        log.LogError("There was an error creating the requested package");
                        break;
                }
                return -232323;
            }
        }

        internal static async Task ListPackageScriptsAsync(FileInfo[] packages, bool withHash)
        {
            string workingDir = "", projFilePath = "", projectFileName = "";

            foreach (var file in packages)
            {
                (_, workingDir, projFilePath, projectFileName, _) = await sqlB.SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(file.FullName, null, true, true).ConfigureAwait(false);
                var buildModel = await sqlM.SqlSyncBuildDataXmlSerializer.LoadAsync(projectFileName).ConfigureAwait(false);
                List<string[]> contents = new List<string[]>();
                string dateformat = "yyyy-MM-dd hh:mm:ss";
                if (!withHash)
                {
                    contents.Add(new string[] { "", "", "", "", "" });
                    contents.Add(new string[] { "Order", "Script Name", "Last Date", "Last User", "Script Id" });
                    contents.Add(new string[] { "", "", "", "", "" });
                }
                else
                {
                    contents.Add(new string[] { "", "", "", "", "", "" });
                    contents.Add(new string[] { "Order", "Script Name", "Last Date", "Last User", "Script Id", "SHA1 Hash" });
                    contents.Add(new string[] { "", "", "", "", "", "" });
                }
                var rows = buildModel.Script.OrderBy(r => r.BuildOrder).ToList();
                foreach (var s in rows)
                {
                    var buildOrder = s.BuildOrder?.ToString() ?? string.Empty;
                    var fileName = s.FileName ?? string.Empty;
                    var dateAdded = s.DateAdded ?? DateTime.MinValue;
                    var dateModified = s.DateModified ?? DateTime.MinValue;
                    var lastDate = (dateModified == DateTime.MinValue) ? dateAdded : dateModified;
                    var addedBy = s.AddedBy ?? string.Empty;
                    var modifiedBy = s.ModifiedBy ?? string.Empty;
                    var user = string.IsNullOrWhiteSpace(modifiedBy) ? addedBy : modifiedBy;
                    var scriptId = s.ScriptId ?? string.Empty;
                    if (withHash)
                    {
                        sqlB.SqlBuildFileHelper.GetSHA1Hash(Path.Combine(projFilePath, fileName), out string fileHash, out string textHash, s.StripTransactionText ?? false);
                        contents.Add(new string[] { buildOrder, fileName, lastDate.ToString(dateformat), user, scriptId, textHash });
                    }
                    else
                    {
                        contents.Add(new string[] { buildOrder, fileName, lastDate.ToString(dateformat), user, scriptId });
                    }
                }
                var sizing = TablePrintSizing(contents);
                var output = ConsoleTableBuilder(contents, sizing);
                string hash = "";
                if (withHash)
                {
                    hash = $" (Package Hash: {await sqlB.SqlBuildFileHelper.CalculateSha1HashFromPackageAsync(file.FullName).ConfigureAwait(false)})";
                }
                System.Console.WriteLine();
                System.Console.WriteLine(file.FullName + hash);
                System.Console.WriteLine(output);
                System.Console.WriteLine();
            }
        }

        // Wrapper for CLI handler compatibility
        internal static Task ListPackageScripts(FileInfo[] packages, bool withHash)
        {
            return ListPackageScriptsAsync(packages, withHash);
        }

        internal static async Task<int> CreatePackageFromScripts(CommandLineArgs cmdLine)
        {

            if (File.Exists(cmdLine.OutputSbm))
            {
                log.LogWarning($"The specified output file '{cmdLine.OutputSbm}' already exists and can not be created. If you want to add scripts to this file, please use the \"sbm add\" command");
                return -432;
            }

            string workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
            if (string.IsNullOrWhiteSpace(workingDir))
            {
                workingDir = Directory.GetCurrentDirectory();
            }
            if (Path.GetExtension(cmdLine.OutputSbm).ToLower() == ".sbx")
            {
                string sbxFileName = cmdLine.OutputSbm;
                workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
                if (string.IsNullOrWhiteSpace(workingDir))
                {
                    workingDir = Directory.GetCurrentDirectory();
                }
                log.LogInformation("Creating Base Build File XML");
                var buildModel = sqlB.SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                var projFile = Path.Combine(workingDir, sqlB.XmlFileNames.MainProjectFile);
                await sqlM.SqlSyncBuildDataXmlSerializer.SaveAsync(projFile, buildModel).ConfigureAwait(false);
                await sqlB.SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildModel, workingDir, sbxFileName, includeHistoryAndLogs: true).ConfigureAwait(false);
                var counter = 1.0;
                foreach (var file in cmdLine.Scripts)
                {
                    if (file.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, file.Name)))
                    {
                        File.Copy(file.FullName, Path.Combine(workingDir, file.Name));
                    }
                    buildModel = await sqlB.SqlBuildFileHelper.AddScriptFileToBuildAsync(buildModel, projFile, file.Name, counter, "", true, true, "client", true, sbxFileName, true, true, Environment.UserName, 500, Guid.NewGuid(), "").ConfigureAwait(false);
                    counter++;
                }
                await sqlM.SqlSyncBuildDataXmlSerializer.SaveAsync(projFile, buildModel).ConfigureAwait(false);

            }
            else
            {
                List<string> copied = new List<string>();
                cmdLine.Scripts.ToList().ForEach(f =>
                {
                    if (f.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, f.Name)))
                    {
                        File.Copy(f.FullName, Path.Combine(workingDir, f.Name));
                        copied.Add(Path.Combine(workingDir, f.Name));
                    }
                });

                bool success = await sqlB.SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(cmdLine.OutputSbm, cmdLine.Scripts.Select(f => f.FullName).ToList(), "client", 500, false).ConfigureAwait(false);
                copied.ForEach(f => File.Delete(f));
                if (!success)
                {
                    log.LogError("Unable to create the build file!");
                    return -425;
                }
                await ListPackageScriptsAsync(new FileInfo[] { new FileInfo(cmdLine.OutputSbm) }, true).ConfigureAwait(false);
            }
            log.LogInformation($"Successfully created build file '{cmdLine.OutputSbm}' with {cmdLine.Scripts.Count()} scripts");
            return 0;
        }

        internal static async Task GetPackageHash(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.LogError("No --packagename was specified. This is required for 'sbm gethash' command");
                System.Environment.Exit(626);

            }
            string packageName = cmdLine.BuildFileName;
            string hash = await sqlB.SqlBuildFileHelper.CalculateSha1HashFromPackageAsync(packageName).ConfigureAwait(false);
            if (!String.IsNullOrEmpty(hash))
            {
                //log.LogInformation(hash);
                System.Console.WriteLine(hash);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(621);
            }
        }

        internal static async Task ExecutePolicyCheck(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }


            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.LogError("No --packagename was specified. This is required for 'sbm policycheck' command");
                System.Environment.Exit(34536);

            }
            string packageName = cmdLine.BuildFileName;
            PolicyHelper helper = new PolicyHelper();
            var (policyMessages, passed) = await helper.CommandLinePolicyCheckAsync(packageName).ConfigureAwait(false);
            if (policyMessages.Count > 0)
            {
                List<string[]> tmp = new List<string[]>();
                tmp.Add(new string[] { "", "", "" });
                tmp.Add(new string[] { "Severity", "Script Name", "Message" });
                tmp.Add(new string[] { "", "", "" });
                tmp.AddRange(policyMessages);
                var sizing = TablePrintSizing(tmp);
                var table = ConsoleTableBuilder(tmp, sizing);

                System.Console.WriteLine();
                System.Console.WriteLine($"Policy results for: {cmdLine.BuildFileName}");
                System.Console.WriteLine(table);
                System.Console.WriteLine();

            }

            if (passed)
            {
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(739);
            }
        }

        internal static void PackageSbxFilesIntoSbmFiles(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);


            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.Directory))
            {
                log.LogError("The --directory argument is required for 'sbm package' command");
                System.Environment.Exit(9835);
            }
            string directory = cmdLine.Directory;
            string message;
            List<string> sbmFiles = sqlB.SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directory, out message);
            if (sbmFiles.Count > 0)
            {
                foreach (string sbm in sbmFiles)
                    log.LogInformation(sbm);

                System.Environment.Exit(0);
            }
            else if (message.Length > 0)
            {
                log.LogWarning(message);
                System.Environment.Exit(604);
            }
            else
            {
                System.Environment.Exit(0);
            }
        }


        internal static sqlB.DacpacDeltasStatus GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb, out string sbmName, bool batchScripts = false)
        {
            if (cmd.MultiDbRunConfigFileName.Trim().ToLower().EndsWith("sql"))
            {
                //if we are getting the list from a SQL statement, then the database and server settings mean something different! Dont pass them in.
                return sqlB.DacPacHelper.GetSbmFromDacPac(cmd.RootLoggingPath,
                   cmd.DacPacArgs.PlatinumDacpac,
                   cmd.DacPacArgs.TargetDacpac,
                   string.Empty,
                   string.Empty,
                   cmd.AuthenticationArgs.AuthenticationType,
                   cmd.AuthenticationArgs.UserName,
                   cmd.AuthenticationArgs.Password,
                   cmd.BuildRevision,
                   cmd.DefaultScriptTimeout,
                   multiDb, out sbmName, batchScripts, cmd.AllowObjectDelete, cmd.IdentityArgs.ClientId);
            }
            else
            {
                return sqlB.DacPacHelper.GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.DacPacArgs.PlatinumDacpac,
                    cmd.DacPacArgs.TargetDacpac,
                    cmd.Database,
                    cmd.Server,
                    cmd.AuthenticationArgs.AuthenticationType,
                    cmd.AuthenticationArgs.UserName,
                    cmd.AuthenticationArgs.Password,
                    cmd.BuildRevision,
                    cmd.DefaultScriptTimeout,
                    multiDb, out sbmName, batchScripts, cmd.AllowObjectDelete, cmd.IdentityArgs.ClientId);
            }
        }


        internal static async Task UnpackSbmFile(DirectoryInfo directory, FileInfo package)
        {
            var projectFileName = "";
            var projectFilePath = "";
            string result;
            string dir = directory.FullName;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            bool success;
            (success, dir, projectFilePath, projectFileName, result) = await SqlSync.SqlBuild.SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package.FullName, dir, false, true).ConfigureAwait(false);
            if (File.Exists(Path.Combine(dir, projectFileName)))
            {
                var sbmName = Path.GetFileNameWithoutExtension(package.FullName) + ".sbx";
                File.Move(Path.Combine(dir, projectFileName), Path.Combine(dir, sbmName));
            }
            if (success)
            {
                log.LogInformation($"SBM file extracted to: {Path.GetFullPath(directory.FullName)}");
            }
        }


        internal static int GenerateOverrideFileFromSqlScript(CommandLineArgs cmdLine, bool force)
        {
            (bool success, cmdLine) = Init(cmdLine);
            string tmpFile = string.Empty;
            if (cmdLine.ScriptFile == null && string.IsNullOrWhiteSpace(cmdLine.ScriptText))
            {
                log.LogError("Please specify a --scriptfile or --scripttext parameter.");
                return -1;
            }
            else if (cmdLine.ScriptFile != null && !string.IsNullOrWhiteSpace(cmdLine.ScriptText))
            {
                log.LogError("Please specify either a --scriptfile or --scripttext parameter, not both.");
                return -2;
            }
            var outputFileName = Path.Join(Path.GetDirectoryName(cmdLine.OutputFile.FullName), Path.GetFileNameWithoutExtension(cmdLine.OutputFile.FullName) + ".cfg");
            if (File.Exists(outputFileName) && !force)
            {
                log.LogError($"Output file already exists: \"{outputFileName}\". To overwite it, please use the --force flag");
                return -3;
            }

            log.LogInformation("Generating override file from SQL script...");

            if (cmdLine.ScriptFile != null)
            {
                cmdLine.MultiDbRunConfigFileName = cmdLine.ScriptFile.FullName;
            }
            else
            {
                tmpFile = Path.Combine(Path.GetDirectoryName(cmdLine.OutputFile.FullName), $"{Guid.NewGuid().ToString()}.sql");
                File.WriteAllText(tmpFile, cmdLine.ScriptText);
                cmdLine.ScriptFile = new FileInfo(tmpFile);
                cmdLine.MultiDbRunConfigFileName = tmpFile;
            }


            var res = Validation.ValidateAndLoadMultiDbData(cmdLine.ScriptFile.FullName, cmdLine, out MultiDbData multiDb, out string[] errorMessage);
            if (res != 0)
            {
                log.LogError(string.Join("\r\n", errorMessage));
            }
            else
            {
                string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb);
                File.WriteAllText(outputFileName, cfg);
                log.LogInformation($"Configuration file saved to: {outputFileName}");
            }
            if (!string.IsNullOrWhiteSpace(tmpFile))
            {
                try
                {
                    File.Delete(tmpFile);
                }
                catch { }
            }

            return 0;

        }

        internal static List<int> TablePrintSizing(List<string[]> input)
        {
            var delimiterCount = input.Select(s => s.Length).Max();
            List<int> sectionLength = new List<int>();
            int len = 0;
            for (var t = 0; t < delimiterCount; t++)
            {
                if (t == 2 && input.Count > 0)
                {
                    len = input.Select(x => (t < x.Length) ? x[t].Length : 0).Max();
                }
                else
                {
                    len = input.Select(x => (t < x.Length) ? x[t].Length : 0).Max();
                }

                sectionLength.Add(len);
            }

            return sectionLength;
        }
        internal static string ConsoleTableBuilder(List<string[]> splits, List<int> sectionLengths)
        {
            StringBuilder sb = new StringBuilder();
            var total = sectionLengths.Sum(e => e) + sectionLengths.Count() * 3 - 1;

            var tmpLine = string.Empty;
            foreach (var splitLine in splits)
            {
                int currentLoc = 0;
                int endLength = 0;
                string current = "| ";
                for (int i = 0; i < sectionLengths.Count(); i++)
                {
                    endLength += sectionLengths[i] + 3;

                    if (splitLine[i].Length == 0 && string.Join("", splitLine).Trim().Length == 0)  //and empty line used to denote a dash separator
                    {
                        current = current.Substring(0, current.Length - 1) + new string('-', sectionLengths[i] + 2) + "| ";
                    }
                    else if (i < splitLine.Length)
                    {
                        if (splitLine[i].Length + currentLoc > currentLoc + sectionLengths[i]) //content for this section and overflows
                        {
                            current += splitLine[i].Trim();
                            currentLoc += current.Length;
                        }
                        else //there is content for this section
                        {
                            current += splitLine[i].Trim().PadRight(sectionLengths[i]) + " | ";
                            currentLoc += current.Length;
                        }
                    }
                    else if (splitLine.Length > 3)
                    {
                        current += new string(' ', sectionLengths[i]) + " | ";
                        currentLoc += current.Length;
                    }
                    else
                    {
                        if (current.Length < endLength) //no content for this section and isn't an overflow
                        {
                            current = current.PadRight(endLength - 1) + " | ";
                        }
                    }

                }
                sb.Append(current + Environment.NewLine);
            }
            sb.AppendLine("|" + new string('-', total) + "|");
            return sb.ToString().Trim();
        }

        internal static void DecryptSettingsFile(CommandLineArgs cmdLine, string settingsfilekey)
        {
            (var success, cmdLine) = Init(cmdLine);
            var serialized = JsonSerializer.Serialize<CommandLineArgs>(cmdLine, new JsonSerializerOptions() { WriteIndented = true });
            System.Console.WriteLine();
            System.Console.WriteLine(serialized);
            System.Console.WriteLine();

        }

        internal static int ShowCommands(bool markdown)
        {
            var filledCmdDocs = CommandLineBuilder.ListCommands_ForDocs();

            var output = JsonSerializer.Serialize<List<CommandDoc>>(filledCmdDocs, new JsonSerializerOptions() { WriteIndented = true, MaxDepth = 10 });

            if (!markdown)
            {
                System.Console.WriteLine("Command list for SQL Build Manager, `sbm` command line tool (with associated sub-commands)");
                System.Console.WriteLine();

                foreach (var parent in filledCmdDocs)
                {
                    System.Console.ForegroundColor = ConsoleColor.Blue;
                    System.Console.WriteLine($"{parent.ParentCommand.PadRight(23, ' ')}{parent.ParentCommandDescription}");
                    System.Console.ForegroundColor = ConsoleColor.White;
                    foreach (var sub in parent.SubCommands)
                    {
                        System.Console.WriteLine($"\t{sub.Name.PadRight(15, ' ')}{sub.Description}");
                    }
                    if (parent.SubCommands.Count > 0)
                    {
                        System.Console.WriteLine();
                    }
                }
                System.Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# Command list for SQL Build Manager, `sbm` command line tool (with associated sub-commands)");
                sb.AppendLine();
                foreach (var parent in filledCmdDocs)
                {
                    sb.AppendLine($"## `{parent.ParentCommand}`");
                    sb.AppendLine();
                    sb.AppendLine(parent.ParentCommandDescription);
                    sb.AppendLine();
                    foreach (var sub in parent.SubCommands)
                    {
                        sb.AppendLine($"  - `{sub.Name}` - {sub.Description}");
                    }
                    sb.AppendLine();

                }
                System.Console.WriteLine(sb.ToString());

            }

   

            return 0;
        }
    }
}
