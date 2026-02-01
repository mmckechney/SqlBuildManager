using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Utilities;
using System.Threading.Tasks;
using System.Threading;
using SqlSync.SqlBuild.Services;

#nullable enable

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for SqlBuildFileHelper.
    /// </summary>
    public class SqlBuildFileHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string FileMissing = "File Missing";
        public const string Sha1HashError = "SHA1 Hash Error";
        public static string DefaultScriptXmlFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Default Scripts", "DefaultScriptRegistry.xml");
        internal static readonly ISqlBuildFileHelper fileHelper = new DefaultSqlBuildFileHelper();
        internal static readonly IScriptBatcher scriptBatcher = new DefaultScriptBatcher();

        public SqlBuildFileHelper()
        {

        }
        #region .: Loading Zip and Project Files :.
        public static bool ValidateAgainstSchema(string fileName, out string validationErrorMessage)
        {
            //Going to obsolete this....
            validationErrorMessage = string.Empty;
            return true;

        }
        [Obsolete("Use ExtractSqlBuildZipFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool ExtractSqlBuildZipFile(string fileName, ref string workingDirectory, ref string projectFilePath, ref string projectFileName, out string result)
        {
            return ExtractSqlBuildZipFile(fileName, ref workingDirectory, ref projectFilePath, ref projectFileName, true, false, out result);
        }
        [Obsolete("Use ExtractSqlBuildZipFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool ExtractSqlBuildZipFile(string fileName, ref string workingDirectory, ref string projectFilePath, ref string projectFileName, bool resetWorkingDirectory, bool overwriteExistingProjectFiles, out string result)
        {
            result = "";
            try
            {
                if (resetWorkingDirectory)
                {
                    if (SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName) == false)
                    {
                        result = "Unable to initialize working directory.";
                        log.LogError($"ExtractSqlBuildZipFile error: {result}");
                        return false;
                    }
                }
                else
                {
                    if (!workingDirectory.EndsWith(@"\") && !workingDirectory.EndsWith(@"/"))
                    {
                        workingDirectory = workingDirectory + @"/";
                    }
                    projectFilePath = workingDirectory;
                    log.LogDebug($"ExtractSqlBuildZipFile projectFilePath set to: {projectFilePath}");
                }

                //Unpack the zip contents into the working directory
                if (!ZipHelper.UnpackZipPackage(workingDirectory, fileName, overwriteExistingProjectFiles))
                {
                    result = "Unable to unpack Sql Build Project File [" + fileName + "]";
                    log.LogError($"ExtractSqlBuildZipFile error: {result}");
                    return false;
                }
                log.LogDebug($"Successfully UnZipped Sql Build Project file {fileName}");

                var mainProjectFilePath = Path.Combine(workingDirectory, XmlFileNames.MainProjectFile);
                if (File.Exists(mainProjectFilePath))
                {
                    log.LogDebug($"Found MainProjectFile at: {mainProjectFilePath}");
                    string valErrorMessage;
                    if (SqlBuildFileHelper.ValidateAgainstSchema(mainProjectFilePath, out valErrorMessage))
                    {
                        projectFileName = mainProjectFilePath;
                        log.LogDebug("MainProjectFile successfully validated against schema");
                        return true;
                    }
                    else
                    {
                        SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                        result = "Unable to validate the schema for: " + mainProjectFilePath;
                        log.LogError($"ExtractSqlBuildZipFile error: {result}");
                        return false;
                    }
                }
                else
                {
                    log.LogWarning($"The MainProjectFile not found at {mainProjectFilePath}");
                    string[] files = Directory.GetFiles(workingDirectory, "*.xml");
                    for (int i = 0; i < files.Length; i++)
                    {
                        log.LogDebug($"Attempting to validate {files[i]} against schema.");
                        string valErrorMessage;
                        if (SqlBuildFileHelper.ValidateAgainstSchema(files[i], out valErrorMessage))
                        {
                            log.LogDebug($"Project file found at {files[i]}. Using as main project metadata file.");
                            projectFileName = files[i];
                            return true;
                        }
                    }

                    SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                    result = "Unable to validate the schema for any XML file in " + workingDirectory;
                    log.LogError($"ExtractSqlBuildZipFile error: {result}");
                    return false;
                }

            }
            catch (Exception exe)
            {
                result = exe.Message;
                log.LogError($"ExtractSqlBuildZipFile exception: {result}");
                SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                return false;
            }
        }

        /// <summary>
        /// Async version of ExtractSqlBuildZipFile. Extracts and validates a SQL Build zip file.
        /// </summary>
        public static async Task<(bool success, string workingDirectory, string projectFilePath, string projectFileName, string result)> ExtractSqlBuildZipFileAsync(
            string fileName, 
            string? workingDirectory = null,
            bool resetWorkingDirectory = true, 
            bool overwriteExistingProjectFiles = false,
            CancellationToken cancellationToken = default)
        {
            string result = "";
            string projFilePath = workingDirectory ?? string.Empty;
            string projFileName = string.Empty;
            string workDir = workingDirectory ?? string.Empty;

            try
            {
                if (resetWorkingDirectory)
                {
                    var initResult = await InitializeWorkingDirectoryAsync(cancellationToken).ConfigureAwait(false);
                    if (!initResult.success)
                    {
                        result = "Unable to initialize working directory.";
                        log.LogError($"ExtractSqlBuildZipFileAsync error: {result}");
                        return (false, workDir, projFilePath, projFileName, result);
                    }
                    workDir = initResult.workingDirectory;
                    projFilePath = initResult.projectFilePath;
                    projFileName = initResult.projectFileName;
                }
                else
                {
                    if (!workDir.EndsWith(@"\") && !workDir.EndsWith(@"/"))
                    {
                        workDir = workDir + @"/";
                    }
                    projFilePath = workDir;
                    log.LogDebug($"ExtractSqlBuildZipFileAsync projectFilePath set to: {projFilePath}");
                }

                // Unpack the zip contents into the working directory
                if (!await ZipHelper.UnpackZipPackageAsync(workDir, fileName, overwriteExistingProjectFiles, cancellationToken).ConfigureAwait(false))
                {
                    result = "Unable to unpack Sql Build Project File [" + fileName + "]";
                    log.LogError($"ExtractSqlBuildZipFileAsync error: {result}");
                    return (false, workDir, projFilePath, projFileName, result);
                }
                log.LogDebug($"Successfully UnZipped Sql Build Project file {fileName}");

                var mainProjectFilePath = Path.Combine(workDir, XmlFileNames.MainProjectFile);
                if (File.Exists(mainProjectFilePath))
                {
                    log.LogDebug($"Found MainProjectFile at: {mainProjectFilePath}");
                    if (SqlBuildFileHelper.ValidateAgainstSchema(mainProjectFilePath, out string valErrorMessage))
                    {
                        projFileName = mainProjectFilePath;
                        log.LogDebug("MainProjectFile successfully validated against schema");
                        return (true, workDir, projFilePath, projFileName, result);
                    }
                    else
                    {
                        await CleanUpAndDeleteWorkingDirectoryAsync(workDir, cancellationToken).ConfigureAwait(false);
                        result = "Unable to validate the schema for: " + mainProjectFilePath;
                        log.LogError($"ExtractSqlBuildZipFileAsync error: {result}");
                        return (false, workDir, projFilePath, projFileName, result);
                    }
                }
                else
                {
                    log.LogWarning($"The MainProjectFile not found at {mainProjectFilePath}");
                    string[] files = Directory.GetFiles(workDir, "*.xml");
                    for (int i = 0; i < files.Length; i++)
                    {
                        log.LogDebug($"Attempting to validate {files[i]} against schema.");
                        if (SqlBuildFileHelper.ValidateAgainstSchema(files[i], out string valErrorMessage))
                        {
                            log.LogDebug($"Project file found at {files[i]}. Using as main project metadata file.");
                            projFileName = files[i];
                            return (true, workDir, projFilePath, projFileName, result);
                        }
                    }

                    await CleanUpAndDeleteWorkingDirectoryAsync(workDir, cancellationToken).ConfigureAwait(false);
                    result = "Unable to validate the schema for any XML file in " + workDir;
                    log.LogError($"ExtractSqlBuildZipFileAsync error: {result}");
                    return (false, workDir, projFilePath, projFileName, result);
                }
            }
            catch (Exception exe)
            {
                result = exe.Message;
                log.LogError($"ExtractSqlBuildZipFileAsync exception: {result}");
                await CleanUpAndDeleteWorkingDirectoryAsync(workDir, cancellationToken).ConfigureAwait(false);
                return (false, workDir, projFilePath, projFileName, result);
            }
        }


        [Obsolete("Use LoadSqlBuildProjectFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool LoadSqlBuildProjectFile(out SqlSyncBuildDataModel model, string projFileName, bool validateSchema)
        {
            if (File.Exists(projFileName))
            {
                model = SqlSyncBuildDataXmlSerializer.Load(projFileName);
                return true;
            }
            else
            {
                log.LogInformation($"LoadSqlBuildProjectFile: unable to find projectFile at {projFileName}. Creating shell.");
                model = CreateShellSqlSyncBuildDataModel();
                SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model).GetAwaiter().GetResult();
                return false;
            }
        }

        /// <summary>
        /// Async version of LoadSqlBuildProjectFile. Loads or creates a SQL Build project file.
        /// </summary>
        public static async Task<(bool success, SqlSyncBuildDataModel model)> LoadSqlBuildProjectFileAsync(string projFileName, bool validateSchema, CancellationToken cancellationToken = default)
        {
            if (File.Exists(projFileName))
            {
                var model = await SqlSyncBuildDataXmlSerializer.LoadAsync(projFileName, cancellationToken).ConfigureAwait(false);
                return (true, model);
            }
            else
            {
                log.LogInformation($"LoadSqlBuildProjectFileAsync: unable to find projectFile at {projFileName}. Creating shell.");
                var model = CreateShellSqlSyncBuildDataModel();
                await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model).ConfigureAwait(false);
                return (false, model);
            }
        }

        public static SqlSyncBuildDataModel LoadSqlBuildProjectModel(string projFileName, bool validateSchema)
        {
            LoadSqlBuildProjectFile(out SqlSyncBuildDataModel model, projFileName, validateSchema);
            return model;
        }

        /// <summary>
        /// Async version of LoadSqlBuildProjectModel. Loads a SQL Build project model.
        /// </summary>
        public static async Task<SqlSyncBuildDataModel> LoadSqlBuildProjectModelAsync(string projFileName, bool validateSchema, CancellationToken cancellationToken = default)
        {
            var (_, model) = await LoadSqlBuildProjectFileAsync(projFileName, validateSchema, cancellationToken).ConfigureAwait(false);
            return model;
        }

        #endregion

        [Obsolete("Use InferOverridesFromPackageAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static string InferOverridesFromPackage(string sbmFileName, string suppliedDbName)
        {
            string tempWorkingDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string projectFilePath = string.Empty;
            string projectFileName = string.Empty;
            string result;
            SqlSyncBuildDataModel model;
            StringBuilder ovr = new StringBuilder();
            try
            {
                ExtractSqlBuildZipFile(sbmFileName, ref tempWorkingDir, ref projectFilePath, ref projectFileName, out result);
                LoadSqlBuildProjectFile(out model, projectFileName, false);
                if (model != null)
                {
                    var targets = model.Script.Select(s => s.Database).Distinct();
                    if (targets != null && targets.Count() > 0)
                    {
                        foreach (var t in targets)
                        {
                            if (!string.IsNullOrWhiteSpace(suppliedDbName))
                            {
                                ovr.Append($"{t},{suppliedDbName};");
                            }
                            else
                            {
                                ovr.Append($"{t},{t};");
                            }
                        }
                        ovr.Length = ovr.Length - 1;
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (Directory.Exists(tempWorkingDir))
                {
                    Directory.Delete(tempWorkingDir, true);
                }
            }

            return ovr.ToString();
        }

        /// <summary>
        /// Async version of InferOverridesFromPackage. Infers database overrides from a build package.
        /// </summary>
        public static async Task<string> InferOverridesFromPackageAsync(string sbmFileName, string suppliedDbName, CancellationToken cancellationToken = default)
        {
            string tempWorkingDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            StringBuilder ovr = new StringBuilder();
            try
            {
                var extractResult = await ExtractSqlBuildZipFileAsync(sbmFileName, tempWorkingDir, resetWorkingDirectory: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!extractResult.success)
                    return string.Empty;

                var (_, model) = await LoadSqlBuildProjectFileAsync(extractResult.projectFileName, false, cancellationToken).ConfigureAwait(false);
                if (model != null)
                {
                    var targets = model.Script.Select(s => s.Database).Distinct();
                    if (targets != null && targets.Count() > 0)
                    {
                        foreach (var t in targets)
                        {
                            if (!string.IsNullOrWhiteSpace(suppliedDbName))
                            {
                                ovr.Append($"{t},{suppliedDbName};");
                            }
                            else
                            {
                                ovr.Append($"{t},{t};");
                            }
                        }
                        ovr.Length = ovr.Length - 1;
                    }
                }
            }
            catch
            {
                // Swallow exceptions like sync version
            }
            finally
            {
                await CleanUpAndDeleteWorkingDirectoryAsync(tempWorkingDir, cancellationToken).ConfigureAwait(false);
            }

            return ovr.ToString();
        }


        public static bool PackageProjectFileIntoZip(SqlSyncBuildDataModel model, string projFilePath, string zipFileName, bool includeHistoryAndLogs)
        {
            if (String.IsNullOrEmpty(zipFileName))
                return true;

            if (model == null)
                return false;

            ArrayList alFiles = new ArrayList();

            // Write the latest project file
            SqlSyncBuildDataXmlSerializer.SaveAsync(Path.Combine(projFilePath, XmlFileNames.MainProjectFile), model).GetAwaiter().GetResult();

            // Get the file list from the model
            for (int i = 0; i < model.Script.Count; i++)
                alFiles.Add(model.Script[i].FileName);

            // Add the project file 
            alFiles.Add(XmlFileNames.MainProjectFile);

            if (includeHistoryAndLogs)
            {
                // Add the history file
                if (File.Exists(Path.Combine(projFilePath, XmlFileNames.HistoryFile)))
                {
                    alFiles.Add(XmlFileNames.HistoryFile);
                }

                // Add any log files
                string[] logFiles = Directory.GetFiles(projFilePath, "*.log");
                for (int j = 0; j < logFiles.Length; j++)
                    alFiles.Add(Path.GetFileName(logFiles[j]));
            }

            // Put all files into a string array
            string[] fileList = new string[alFiles.Count];
            alFiles.CopyTo(fileList);

            // Create the Zip file
            bool val = ZipHelper.CreateZipPackage(fileList, projFilePath, zipFileName);
            return val;
        }

        public static async Task<bool> PackageProjectFileIntoZipAsync(SqlSyncBuildDataModel model, string projFilePath, string zipFileName, bool includeHistoryAndLogs, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(zipFileName))
                return true;
            if (model == null)
                return false;

            ArrayList alFiles = new ArrayList();
            await SqlSyncBuildDataXmlSerializer.SaveAsync(Path.Combine(projFilePath, XmlFileNames.MainProjectFile), model);

            for (int i = 0; i < model.Script.Count; i++)
                alFiles.Add(model.Script[i].FileName);

            alFiles.Add(XmlFileNames.MainProjectFile);

            if (includeHistoryAndLogs)
            {
                if (File.Exists(Path.Combine(projFilePath, XmlFileNames.HistoryFile)))
                    alFiles.Add(XmlFileNames.HistoryFile);

                string[] logFiles = Directory.GetFiles(projFilePath, "*.log");
                for (int j = 0; j < logFiles.Length; j++)
                    alFiles.Add(Path.GetFileName(logFiles[j]));
            }

            string[] fileList = new string[alFiles.Count];
            alFiles.CopyTo(fileList);

            bool val = await ZipHelper.CreateZipPackageAsync(fileList, projFilePath, zipFileName, keepPathInfo: true, cancellationToken).ConfigureAwait(false);
            return val;
        }


        public static byte[] CleanProjectFileForRemoteExecution(string fileName, out SqlSyncBuildDataModel cleanedBuildData)
        {
            cleanedBuildData = CreateShellSqlSyncBuildDataModel();
            if (!File.Exists(fileName))
                return Array.Empty<byte>();

            string tmpDir = string.Empty;
            try
            {
                tmpDir = Path.Combine(Path.GetDirectoryName(fileName) ?? Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tmpDir);

                string tmpZipShortName = "~" + Path.GetFileName(fileName);
                string tmpProjectFileName = Path.Combine(tmpDir, XmlFileNames.MainProjectFile);

                string tmpZipFullName = Path.Combine(tmpDir, tmpZipShortName);
                File.Copy(fileName, tmpZipFullName, true);

                string result;
                if (ExtractSqlBuildZipFile(tmpZipFullName, ref tmpDir, ref tmpDir, ref tmpProjectFileName, false, false, out result))
                {
                    LoadSqlBuildProjectFile(out cleanedBuildData, tmpProjectFileName, false);
                    cleanedBuildData.ScriptRun = new List<ScriptRun>();
                    cleanedBuildData.Build = new List<Build>();
                    SqlSyncBuildDataXmlSerializer.SaveAsync(tmpProjectFileName, cleanedBuildData).GetAwaiter().GetResult();

                    if (PackageProjectFileIntoZip(cleanedBuildData, tmpDir, tmpZipFullName, includeHistoryAndLogs: false))
                    {
                        return File.ReadAllBytes(tmpZipFullName);
                    }
                }

                // can't clean for some reason, so just get the raw file...
                return File.ReadAllBytes(fileName);
            }
            catch
            {
                return File.ReadAllBytes(fileName);
            }
            finally
            {
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
            }
        }


        public static SqlSyncBuildDataModel CreateShellSqlSyncBuildDataModel()
        {
            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject> { new SqlSyncBuildProject(0, string.Empty, false) },
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());
        }



        public static void SaveSqlBuildProjectFile(SqlSyncBuildDataModel model, string projFileName, string buildZipFileName, bool includeHistoryAndLogs = true)
        {
            SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model).GetAwaiter().GetResult();
            PackageProjectFileIntoZip(model, Path.GetDirectoryName(projFileName), buildZipFileName, includeHistoryAndLogs);
        }

        public static async Task SaveSqlBuildProjectFileAsync(SqlSyncBuildDataModel model, string projFileName, string buildZipFileName, bool includeHistoryAndLogs = true, CancellationToken cancellationToken = default)
        {
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model).ConfigureAwait(false);
            await PackageProjectFileIntoZipAsync(model, Path.GetDirectoryName(projFileName), buildZipFileName, includeHistoryAndLogs, cancellationToken).ConfigureAwait(false);
        }

        [Obsolete("Use SaveSqlFilesToNewBuildFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, List<string> fileNames, string targetDatabaseName, int defaultScriptTimeout, bool includeHistoryAndLogs = true)
        {
            return SaveSqlFilesToNewBuildFile(buildFileName, fileNames, targetDatabaseName, false, defaultScriptTimeout, false);
        }

        [Obsolete("Use SaveSqlFilesToNewBuildFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, List<string> fileNames, string targetDatabaseName, bool overwritePreExistingFile, int defaultScriptTimeout, bool includeHistoryAndLogs = true)
        {
            if (File.Exists(buildFileName) && !overwritePreExistingFile)
            {
                return false;
            }
            string directory = Path.GetDirectoryName(buildFileName);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = System.IO.Directory.GetCurrentDirectory();
            }
            try
            {
                SqlSyncBuildDataModel model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                string projFileName = Path.Combine(directory, XmlFileNames.MainProjectFile);
                int i = 0;
                foreach (string file in fileNames)
                {
                    string shortFileName = Path.GetFileName(file);
                    if (shortFileName == XmlFileNames.MainProjectFile ||
                        shortFileName == XmlFileNames.ExportFile)
                        continue;

                    i++;
                    model = SqlBuildFileHelper.AddScriptFileToBuild(
                        model,
                        projFileName,
                        shortFileName,
                        i,
                        "",
                        rollBackScript: true,
                        rollBackBuild: true,
                        databaseName: targetDatabaseName,
                        stripTransactions: false,
                        buildZipFileName: buildFileName,
                        saveToZip: false,
                        allowMultipleRuns: true,
                        addedBy: System.Environment.UserName,
                        scriptTimeOut: defaultScriptTimeout,
                        scriptId: Guid.NewGuid(),
                        tag: string.Empty);

                }
                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFileName, buildFileName, includeHistoryAndLogs);

                //Clean up project file (not needed, it is now in the package)
                try
                {
                    File.Delete(projFileName);
                }
                catch { log.LogWarning($"Unable to clean up temporary project file '{projFileName}'. Please remove manually."); }

                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to package scripts");
                return false;
            }

        }
        [Obsolete("Use SaveSqlFilesToNewBuildFileAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, string directory, string targetDatabaseName, int defaultScriptTimeout)
        {
            string[] files = Directory.GetFiles(directory);
            return SaveSqlFilesToNewBuildFile(buildFileName, files.ToList(), targetDatabaseName, defaultScriptTimeout);
        }

        /// <summary>
        /// Async version of SaveSqlFilesToNewBuildFile. Creates a new build file from a list of SQL files.
        /// </summary>
        public static Task<bool> SaveSqlFilesToNewBuildFileAsync(string buildFileName, List<string> fileNames, string targetDatabaseName, int defaultScriptTimeout, bool includeHistoryAndLogs = true, CancellationToken cancellationToken = default)
        {
            return SaveSqlFilesToNewBuildFileAsync(buildFileName, fileNames, targetDatabaseName, false, defaultScriptTimeout, includeHistoryAndLogs, cancellationToken);
        }

        /// <summary>
        /// Async version of SaveSqlFilesToNewBuildFile. Creates a new build file from a list of SQL files.
        /// </summary>
        public static async Task<bool> SaveSqlFilesToNewBuildFileAsync(string buildFileName, List<string> fileNames, string targetDatabaseName, bool overwritePreExistingFile, int defaultScriptTimeout, bool includeHistoryAndLogs = true, CancellationToken cancellationToken = default)
        {
            if (File.Exists(buildFileName) && !overwritePreExistingFile)
            {
                return false;
            }
            string directory = Path.GetDirectoryName(buildFileName);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = System.IO.Directory.GetCurrentDirectory();
            }
            try
            {
                SqlSyncBuildDataModel model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                string projFileName = Path.Combine(directory, XmlFileNames.MainProjectFile);
                int i = 0;
                foreach (string file in fileNames)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    string shortFileName = Path.GetFileName(file);
                    if (shortFileName == XmlFileNames.MainProjectFile ||
                        shortFileName == XmlFileNames.ExportFile)
                        continue;

                    i++;
                    model = await AddScriptFileToBuildAsync(
                        model,
                        projFileName,
                        shortFileName,
                        i,
                        "",
                        rollBackScript: true,
                        rollBackBuild: true,
                        databaseName: targetDatabaseName,
                        stripTransactions: false,
                        buildZipFileName: buildFileName,
                        saveToZip: false,
                        allowMultipleRuns: true,
                        addedBy: System.Environment.UserName,
                        scriptTimeOut: defaultScriptTimeout,
                        scriptId: Guid.NewGuid(),
                        tag: string.Empty,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                await SaveSqlBuildProjectFileAsync(model, projFileName, buildFileName, includeHistoryAndLogs, cancellationToken).ConfigureAwait(false);

                // Clean up project file (not needed, it is now in the package)
                try
                {
                    File.Delete(projFileName);
                }
                catch { log.LogWarning($"Unable to clean up temporary project file '{projFileName}'. Please remove manually."); }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to package scripts");
                return false;
            }
        }

        /// <summary>
        /// Async version of SaveSqlFilesToNewBuildFile. Creates a new build file from all SQL files in a directory.
        /// </summary>
        public static async Task<bool> SaveSqlFilesToNewBuildFileAsync(string buildFileName, string directory, string targetDatabaseName, int defaultScriptTimeout, CancellationToken cancellationToken = default)
        {
            string[] files = Directory.GetFiles(directory);
            return await SaveSqlFilesToNewBuildFileAsync(buildFileName, files.ToList(), targetDatabaseName, defaultScriptTimeout, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #region .: Packaging SBX into SBM :.
        public static List<string> PackageSbxFilesIntoSbmFiles(string directoryName, out string message)
        {
            if (directoryName == null || directoryName.Length == 0)
            {
                message = "Unable to package SBX files. Source directory parameter is empty";
                log.LogWarning(message);
                return new List<string>();
            }

            if (!Directory.Exists(directoryName))
            {
                message = String.Format("Unable to package SBX files. The specified source directory '{0}' does not exist.", directoryName);
                log.LogWarning(message);
                return new List<string>();
            }

            message = string.Empty;
            List<string> sbmFiles = new List<string>();
            string[] sbxFiles = Directory.GetFiles(directoryName, "*.sbx", SearchOption.AllDirectories);
            if (sbxFiles.Length == 0)
            {
                message = String.Format("No SBX files found in source directory '{0}' or any of it's subdirectories", directoryName);
                log.LogWarning(message);
                return new List<string>();
            }

            string tmpSbmFile = string.Empty;
            foreach (string sbx in sbxFiles)
            {
                tmpSbmFile = PackageSbxFileIntoSbmFile(sbx);
                if (tmpSbmFile.Length > 0)
                    sbmFiles.Add(tmpSbmFile);
                else
                {
                    string logFileName = SqlBuildManager.Logging.ApplicationLogging.LogFileName;
                    message = String.Format("Error packaging SBX file '{0}' - please check application log at \"{1}\"", sbx, logFileName);
                    log.LogError(message);
                    return new List<string>();
                }
            }

            return sbmFiles;
        }
        public static string PackageSbxFileIntoSbmFile(string sbxBuildControlFileName)
        {
            string sbmProjectFileName = Path.Combine(Path.GetDirectoryName(sbxBuildControlFileName), Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm");
            if (PackageSbxFileIntoSbmFile(sbxBuildControlFileName, sbmProjectFileName))
                return sbmProjectFileName;
            else
                return string.Empty;
        }
        public static bool PackageSbxFileIntoSbmFile(string sbxBuildControlFileName, string sbmProjectFileName)
        {
            try
            {
                if (sbxBuildControlFileName == null || sbxBuildControlFileName.Trim().Length == 0)
                {
                    log.LogWarning("Can't package SBX file into SBM package - SBX file name is empty");
                    return false;
                }

                if (File.Exists(sbmProjectFileName))
                {
                    log.LogWarning($"Deleting a pre-existing SBM file: {sbmProjectFileName}");
                    File.Delete(sbmProjectFileName);
                }

                SqlSyncBuildDataModel model = null;
                bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out model, sbxBuildControlFileName, true);
                if (!successfulLoad)
                {
                    log.LogError($"Problem loading SBX file: {sbxBuildControlFileName}. ");
                    return false;
                }

                bool copied = false;
                string path = Path.GetDirectoryName(sbxBuildControlFileName);
                string mainProjectFileFullPath = Path.Combine(path, XmlFileNames.MainProjectFile);
                string tmpMainProjectFileFullPath = Path.Combine(path, "~~" + XmlFileNames.MainProjectFile);

                //Just in case that file already exists...
                if (File.Exists(mainProjectFileFullPath))
                {
                    log.LogWarning($"Renaming pre-existing XML \"main project file\": {mainProjectFileFullPath}");
                    File.Copy(mainProjectFileFullPath, tmpMainProjectFileFullPath, true);
                    copied = true;
                }

                //Copy the SBX to the main project file name
                File.Copy(sbxBuildControlFileName, mainProjectFileFullPath, true);

                //Validate that all of the script files are present...
                foreach (var script in model.Script)
                {
                    if (!string.IsNullOrEmpty(script.FileName) && !File.Exists(Path.Combine(path, script.FileName)))
                    {
                        log.LogError($"A script file configured in the SBX file was not found: '{Path.Combine(path + script.FileName)}'. Unable to create SBM package.");
                        return false;
                    }
                }
                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, mainProjectFileFullPath, sbmProjectFileName);


                if (copied)
                {
                    log.LogWarning($"Moving pre-existing XML \"main project file\" back to  {mainProjectFileFullPath}");
                    File.Copy(tmpMainProjectFileFullPath, mainProjectFileFullPath, true);
                    File.Delete(tmpMainProjectFileFullPath);
                }
                else
                {
                    log.LogDebug($"Deleting the temporary file {mainProjectFileFullPath}");
                    File.Delete(mainProjectFileFullPath);
                }

                log.LogDebug($"Successfully packaged SBX file '{sbxBuildControlFileName}' into SBM file '{sbmProjectFileName}'");
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error creating SBM package for {sbxBuildControlFileName}");
                return false;
            }


        }

        public static async Task<List<string>> PackageSbxFilesIntoSbmFilesAsync(string directoryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(directoryName) || !Directory.Exists(directoryName))
                return new List<string>();

            var sbmFiles = new List<string>();
            var sbxFiles = Directory.GetFiles(directoryName, "*.sbx", SearchOption.AllDirectories);
            foreach (var sbx in sbxFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var tmp = await PackageSbxFileIntoSbmFileAsync(sbx, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(tmp)) sbmFiles.Add(tmp); else return new List<string>();
            }
            return sbmFiles;
        }

        public static async Task<string> PackageSbxFileIntoSbmFileAsync(string sbxBuildControlFileName, CancellationToken cancellationToken = default)
        {
            string sbmProjectFileName = Path.Combine(Path.GetDirectoryName(sbxBuildControlFileName), Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm");
            var ok = await PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, sbmProjectFileName, cancellationToken).ConfigureAwait(false);
            return ok ? sbmProjectFileName : string.Empty;
        }

        public static async Task<bool> PackageSbxFileIntoSbmFileAsync(string sbxBuildControlFileName, string sbmProjectFileName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sbxBuildControlFileName))
                {
                    log.LogWarning("Can't package SBX file into SBM package - SBX file name is empty");
                    return false;
                }

                if (File.Exists(sbmProjectFileName))
                {
                    log.LogWarning($"Deleting a pre-existing SBM file: {sbmProjectFileName}");
                    File.Delete(sbmProjectFileName);
                }

                SqlSyncBuildDataModel model = null;
                bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out model, sbxBuildControlFileName, true);
                if (!successfulLoad)
                {
                    log.LogError($"Problem loading SBX file: {sbxBuildControlFileName}. ");
                    return false;
                }

                bool copied = false;
                string path = Path.GetDirectoryName(sbxBuildControlFileName);
                string mainProjectFileFullPath = Path.Combine(path, XmlFileNames.MainProjectFile);
                string tmpMainProjectFileFullPath = Path.Combine(path, "~~" + XmlFileNames.MainProjectFile);

                if (File.Exists(mainProjectFileFullPath))
                {
                    log.LogWarning($"Renaming pre-existing XML \"main project file\": {mainProjectFileFullPath}");
                    File.Copy(mainProjectFileFullPath, tmpMainProjectFileFullPath, true);
                    copied = true;
                }

                File.Copy(sbxBuildControlFileName, mainProjectFileFullPath, true);

                foreach (var script in model.Script)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(script.FileName) && !File.Exists(Path.Combine(path, script.FileName)))
                    {
                        log.LogError($"A script file configured in the SBX file was not found: '{Path.Combine(path + script.FileName)}'. Unable to create SBM package.");
                        return false;
                    }
                }

                await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, mainProjectFileFullPath, sbmProjectFileName, includeHistoryAndLogs: true, cancellationToken).ConfigureAwait(false);

                if (copied)
                {
                    log.LogWarning($"Moving pre-existing XML \"main project file\" back to  {mainProjectFileFullPath}");
                    File.Copy(tmpMainProjectFileFullPath, mainProjectFileFullPath, true);
                    File.Delete(tmpMainProjectFileFullPath);
                }
                else
                {
                    log.LogDebug($"Deleting the temporary file {mainProjectFileFullPath}");
                    File.Delete(mainProjectFileFullPath);
                }

                log.LogDebug($"Successfully packaged SBX file '{sbxBuildControlFileName}' into SBM file '{sbmProjectFileName}'");
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error creating SBM package for {sbxBuildControlFileName}");
                return false;
            }
        }

        #endregion

        #region .: Add / Remove scripts from build :.



        [Obsolete("Use AddScriptFileToBuildAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static SqlSyncBuildDataModel AddScriptFileToBuild(SqlSyncBuildDataModel model, string projFileName, string fileName, double buildOrder, string description, bool rollBackScript, bool rollBackBuild, string databaseName, bool stripTransactions, string buildZipFileName, bool saveToZip, bool allowMultipleRuns, string addedBy, int scriptTimeOut, Guid scriptId, string tag)
        {
            var newScript = new Script(
                fileName: fileName,
                buildOrder: buildOrder,
                description: description,
                rollBackOnError: rollBackScript,
                causesBuildFailure: rollBackBuild,
                dateAdded: DateTime.Now,
                scriptId: (scriptId == Guid.Empty ? Guid.NewGuid() : scriptId).ToString(),
                database: databaseName,
                stripTransactionText: stripTransactions,
                allowMultipleRuns: allowMultipleRuns,
                addedBy: addedBy,
                scriptTimeOut: scriptTimeOut,
                dateModified: DateTime.MinValue,
                modifiedBy: string.Empty,
                tag: tag);

            var updatedScripts = model.Script.Concat(new[] { newScript }).ToList();
            var updatedModel = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: updatedScripts,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: model.CommittedScript);
            if (saveToZip)
            {
                SaveSqlBuildProjectFile(updatedModel, projFileName, buildZipFileName);
            }
            return updatedModel;
        }

        /// <summary>
        /// Async version of AddScriptFileToBuild. Adds a script file to the build model.
        /// </summary>
        public static async Task<SqlSyncBuildDataModel> AddScriptFileToBuildAsync(
            SqlSyncBuildDataModel model, 
            string projFileName, 
            string fileName, 
            double buildOrder, 
            string description, 
            bool rollBackScript, 
            bool rollBackBuild, 
            string databaseName, 
            bool stripTransactions, 
            string buildZipFileName, 
            bool saveToZip, 
            bool allowMultipleRuns, 
            string addedBy, 
            int scriptTimeOut, 
            Guid scriptId, 
            string tag,
            CancellationToken cancellationToken = default)
        {
            var newScript = new Script(
                fileName: fileName,
                buildOrder: buildOrder,
                description: description,
                rollBackOnError: rollBackScript,
                causesBuildFailure: rollBackBuild,
                dateAdded: DateTime.Now,
                scriptId: (scriptId == Guid.Empty ? Guid.NewGuid() : scriptId).ToString(),
                database: databaseName,
                stripTransactionText: stripTransactions,
                allowMultipleRuns: allowMultipleRuns,
                addedBy: addedBy,
                scriptTimeOut: scriptTimeOut,
                dateModified: DateTime.MinValue,
                modifiedBy: string.Empty,
                tag: tag);

            var updatedScripts = model.Script.Concat(new[] { newScript }).ToList();
            var updatedModel = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: updatedScripts,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: model.CommittedScript);
            
            if (saveToZip)
            {
                await SaveSqlBuildProjectFileAsync(updatedModel, projFileName, buildZipFileName, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            return updatedModel;
        }
        #endregion

        #region .: Default Script Handling :.
        public static DefaultScripts.DefaultScriptRegistry GetDefaultScriptRegistry()
        {
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string defaultScriptPath = Path.GetDirectoryName(SqlBuildFileHelper.DefaultScriptXmlFile);
            string defaultScriptXmlFile = SqlBuildFileHelper.DefaultScriptXmlFile;

            if (File.Exists(defaultScriptXmlFile) == false)
            {
                log.LogWarning($"Unable to load Default Script Registry at {defaultScriptXmlFile}. File does not exist");
                return null;
            }

            DefaultScripts.DefaultScriptRegistry registry = null;
            using (StreamReader sr = new StreamReader(defaultScriptXmlFile))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(DefaultScripts.DefaultScriptRegistry));
                object obj = serializer.Deserialize(sr);
                registry = (DefaultScripts.DefaultScriptRegistry)obj;
                sr.Close();
            }
            return registry;
        }

        /// <summary>
        /// Remove script files from the build using POCO model
        /// </summary>
        public static SqlSyncBuildDataModel RemoveScriptFilesFromBuild(SqlSyncBuildDataModel model, string projFileName, string buildZipFileName, IEnumerable<Script> scriptsToRemove, bool deleteFiles)
        {
            var scriptsToRemoveIds = scriptsToRemove.Select(s => s.ScriptId).ToHashSet();
            
            foreach (var script in scriptsToRemove)
            {
                try
                {
                    if (deleteFiles)
                    {
                        var fileName = Path.Combine(Path.GetDirectoryName(projFileName) ?? "", script.FileName);
                        if (File.Exists(fileName))
                            File.Delete(fileName);
                    }
                }
                catch (IOException ioExe)
                {
                    log.LogError(ioExe, $"Unable to delete file {script.FileName}");
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Unable to remove file {script.FileName}");
                    throw;
                }
            }

            model.Script = model.Script.Where(s => !scriptsToRemoveIds.Contains(s.ScriptId)).ToList();
            SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFileName, buildZipFileName);
            return model;
        }

        /// <summary>
        /// Add a default script to the build using POCO model
        /// </summary>
        public static (DefaultScriptCopyStatus status, SqlSyncBuildDataModel model) AddDefaultScriptToBuild(SqlSyncBuildDataModel model, DefaultScripts.DefaultScript defaultScript, DefaultScriptCopyAction copyAction, string projFileName, string buildZipFileName)
        {
            DefaultScriptCopyStatus status = DefaultScriptCopyStatus.Success;
            string defaultScriptPath = Path.GetDirectoryName(SqlBuildFileHelper.DefaultScriptXmlFile) ?? "";
            string fullScriptPath = Path.Combine(defaultScriptPath, defaultScript.ScriptName);
            
            if (!File.Exists(fullScriptPath))
                return (DefaultScriptCopyStatus.DefaultNotFound, model);

            string newLocalFile = Path.Combine(Path.GetDirectoryName(projFileName) ?? "", defaultScript.ScriptName);

            if (File.Exists(newLocalFile))
            {
                bool isReadOnly = ((new FileInfo(newLocalFile).Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                if ((DefaultScriptCopyAction.OverwriteExisting == copyAction) && !isReadOnly)
                {
                    File.Copy(fullScriptPath, newLocalFile, true);
                }
                else if ((File.ReadAllText(newLocalFile) != File.ReadAllText(fullScriptPath)) &&
                                (copyAction != DefaultScriptCopyAction.LeaveExisting))
                {
                    if (isReadOnly)
                        status = DefaultScriptCopyStatus.PreexistingDifferentReadOnly;
                    else
                        return (DefaultScriptCopyStatus.PreexistingDifferent, model);
                }
                else if (isReadOnly)
                {
                    status = DefaultScriptCopyStatus.PreexistingReadOnly;
                }
            }
            else
            {
                File.Copy(fullScriptPath, newLocalFile, true);
            }

            model = AddScriptFileToBuild(
                model,
                projFileName,
                defaultScript.ScriptName,
                defaultScript.BuildOrder,
                defaultScript.Description,
                defaultScript.RollBackScript,
                defaultScript.RollBackBuild,
                defaultScript.DatabaseName,
                defaultScript.StripTransactions,
                buildZipFileName,
                saveToZip: false,
                allowMultipleRuns: defaultScript.AllowMultipleRuns,
                addedBy: System.Environment.UserName,
                scriptTimeOut: defaultScript.ScriptTimeout,
                scriptId: Guid.Empty,
                tag: defaultScript.ScriptTag);

            SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFileName, buildZipFileName);
            return (status, model);
        }
        #endregion

        #region .: Object/ Populate Script Update settings :.

        public static SqlBuild.CodeTable.ScriptUpdates GetFileDataForCodeTableUpdates(string baseFileName, string projFileName)
        {
            string line = string.Empty;

            //Open the populate script file
            string localFile = Path.Combine(Path.GetDirectoryName(projFileName), Path.GetFileName(baseFileName));
            if (File.Exists(localFile) == false)
                return null;

            SqlBuild.CodeTable.ScriptUpdates codeTableUpdate = new SqlSync.SqlBuild.CodeTable.ScriptUpdates();
            codeTableUpdate.ShortFileName = baseFileName;

            using (StreamReader sr = File.OpenText(localFile))
            {
                bool keepReading = true;
                while ((line = sr.ReadLine()) != null && keepReading == true)
                {
                    if (line.Trim().StartsWith("Source Server:"))
                        codeTableUpdate.SourceServer = line.Replace("Source Server:", "").Trim();

                    if (line.Trim().StartsWith("Source Db:"))
                        codeTableUpdate.SourceDatabase = line.Replace("Source Db:", "").Trim();

                    if (line.Trim().StartsWith("Table Scripted:"))
                        codeTableUpdate.SourceTable = line.Replace("Table Scripted:", "").Trim();

                    if (line.Trim().StartsWith("Key Check Columns:"))
                        codeTableUpdate.KeyCheckColumns = line.Replace("Key Check Columns:", "").Trim();

                    //This is a multi-line element
                    if (line.Trim().StartsWith("Query Used:"))
                    {
                        string queryLine = string.Empty;
                        string fullquery = string.Empty;
                        while ((queryLine = sr.ReadLine().Trim()) != "*/")
                        {
                            fullquery += queryLine + System.Environment.NewLine;
                        }
                        codeTableUpdate.Query = fullquery;
                        keepReading = false;
                    }
                }
                sr.Close();
            }
            return codeTableUpdate;
        }

        /// <summary>
        /// Get file data for code table updates using POCO model
        /// </summary>
        public static SqlBuild.CodeTable.ScriptUpdates[]? GetFileDataForCodeTableUpdates(SqlSyncBuildDataModel model, string projFileName)
        {
            if (model?.Script == null)
                return null;

            var scriptFiles = new List<SqlBuild.CodeTable.ScriptUpdates>();
            foreach (var script in model.Script)
            {
                // Find the ".pop" populate scripts
                if (!Path.GetExtension(script.FileName).Equals(SqlSync.Constants.DbObjectType.PopulateScript, StringComparison.OrdinalIgnoreCase))
                    continue;

                var obj = GetFileDataForCodeTableUpdates(script.FileName, projFileName);
                if (obj != null)
                    scriptFiles.Add(obj);
            }

            return scriptFiles.ToArray();
        }


        public static SqlBuild.Objects.ObjectUpdates[]? GetFileDataForObjectUpdates(SqlSyncBuildDataModel model, string projFileName)
        {
            GetFileDataForObjectUpdates(model, projFileName, out List<SqlBuild.Objects.ObjectUpdates>? canUpdate, out List<string>? canNotUpdate);
            return canUpdate?.ToArray();
        }

        public static void GetFileDataForObjectUpdates(SqlSyncBuildDataModel model, string projFileName, out List<SqlBuild.Objects.ObjectUpdates>? canUpdate, out List<string>? canNotUpdate)
        {
            if (model == null)
            {
                canUpdate = null;
                canNotUpdate = null;
                return;
            }

            canUpdate = new List<Objects.ObjectUpdates>();
            canNotUpdate = new List<string>();

            foreach (var row in model.Script)
            {
                if (row.FileName == null)
                {
                    canNotUpdate.Add(string.Empty);
                    continue;
                }

                //Find the database objects that can be updated...SP, View, UDF, Trigger
                var ext = Path.GetExtension(row.FileName).ToUpper();
                if (ext != SqlSync.Constants.DbObjectType.StoredProcedure &&
                    ext != SqlSync.Constants.DbObjectType.View &&
                    ext != SqlSync.Constants.DbObjectType.UserDefinedFunction &&
                    ext != SqlSync.Constants.DbObjectType.Trigger)
                {
                    canNotUpdate.Add(row.FileName);
                }
                else
                {
                    SqlBuild.Objects.ObjectUpdates obj = GetFileDataForObjectUpdates(row.FileName, projFileName);
                    if (obj != null)
                    {
                        canUpdate.Add(obj);
                    }
                }
            }
        }

        public static SqlBuild.Objects.ObjectUpdates GetFileDataForObjectUpdates(string baseFilename, string projFileName)
        {
            string line = string.Empty;
            //Open the populate script file
            string localFile = Path.Combine(Path.GetDirectoryName(projFileName), Path.GetFileName(baseFilename));
            if (File.Exists(localFile) == false)
                return null;

            SqlBuild.Objects.ObjectUpdates objectUpdate = new SqlBuild.Objects.ObjectUpdates();
            objectUpdate.ShortFileName = baseFilename;

            using (StreamReader sr = File.OpenText(localFile))
            {

                bool keepReading = true;
                while ((line = sr.ReadLine()) != null && keepReading == true)
                {
                    if (line.Trim().StartsWith("Source Server:"))
                        objectUpdate.SourceServer = line.Replace("Source Server:", "").Trim();

                    if (line.Trim().StartsWith("Source Db:"))
                    {
                        objectUpdate.SourceDatabase = line.Replace("Source Db:", "").Trim();
                    }

                    if (line.Trim().StartsWith("Object Scripted:"))
                        objectUpdate.SourceObject = line.Replace("Object Scripted:", "").Trim();

                    if (line.Trim().StartsWith("Object Type:"))
                    {
                        objectUpdate.ObjectType = line.Replace("Object Type:", "").Trim();
                    }
                    if (line.Trim().StartsWith("Include Permissions:"))
                    {
                        objectUpdate.IncludePermissions = Boolean.Parse(line.Replace("Include Permissions:", "").Trim());
                    }
                    if (line.Trim().StartsWith("Script as ALTER:"))
                    {
                        objectUpdate.ScriptAsAlter = Boolean.Parse(line.Replace("Script as ALTER:", "").Trim());
                    }
                    if (line.Trim().StartsWith("Script PK with Table:"))
                    {
                        objectUpdate.ScriptPkWithTable = Boolean.Parse(line.Replace("Script PK with Table:", "").Trim());
                    }
                    if (line.Trim().StartsWith("*/"))
                        keepReading = false;
                }
                sr.Close();
            }
            return objectUpdate;
        }

        #endregion

        #region .: Log File Handling :.
        public static bool ArchiveLogFiles(string[] logFileName, string basePath, string destinationArchiveName)
        {
            if (ZipHelper.AppendZipPackage(logFileName, basePath, destinationArchiveName, false))
            {
                for (int i = 0; i < logFileName.Length; i++)
                {
                    try
                    {
                        File.Delete(Path.Combine(basePath, logFileName[i]));
                    }
                    catch { }
                }
                return true;
            }
            else
                return false;

        }
        public static Int64 GetTotalLogFilesSize(string projFilePath)
        {
            Int64 sum = 0;
            if (Directory.Exists(projFilePath))
            {
                string[] logFiles = Directory.GetFiles(projFilePath, "*.log");
                for (int i = 0; i < logFiles.Length; i++)
                    sum += new FileInfo(logFiles[i]).Length;
            }
            return sum;
        }
        #endregion

        #region .: SHA1 Hashing related :.

        [Obsolete("Use CalculateSha1HashFromPackageAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static string CalculateSha1HashFromPackage(string buildPackageName)
        {
            SqlSyncBuildDataModel model = null;

            if (String.IsNullOrEmpty(buildPackageName))
                return string.Empty;

            string projFileName = string.Empty;
            string projectFilePath = string.Empty;
            string workingDirectory = string.Empty;

            string extension = Path.GetExtension(buildPackageName).ToLower();
            switch ((extension))
            {
                case ".sbm":
                    string result;
                    ExtractSqlBuildZipFile(buildPackageName, ref workingDirectory, ref projectFilePath,
                                           ref projFileName,
                                           out result);
                    LoadSqlBuildProjectFile(out model, projFileName, false);
                    break;
                case ".sbx":
                    projectFilePath = Path.GetDirectoryName(buildPackageName);
                    LoadSqlBuildProjectFile(out model, buildPackageName, false);
                    break;
                default:
                    return string.Empty;
            }

            if (model != null)
            {
                string hash = CalculateBuildPackageSHA1SignatureFromPath(projectFilePath, model);
                if (extension == ".sbm")
                    CleanUpAndDeleteWorkingDirectory(projectFilePath);

                return hash;

            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Async version of CalculateSha1HashFromPackage. Calculates the SHA1 hash of a build package.
        /// </summary>
        public static async Task<string> CalculateSha1HashFromPackageAsync(string buildPackageName, CancellationToken cancellationToken = default)
        {
            SqlSyncBuildDataModel? model = null;

            if (String.IsNullOrEmpty(buildPackageName))
                return string.Empty;

            string projectFilePath = string.Empty;

            string extension = Path.GetExtension(buildPackageName).ToLower();
            switch (extension)
            {
                case ".sbm":
                    var extractResult = await ExtractSqlBuildZipFileAsync(buildPackageName, cancellationToken: cancellationToken).ConfigureAwait(false);
                    if (extractResult.success)
                    {
                        projectFilePath = extractResult.projectFilePath;
                        var loadResult = await LoadSqlBuildProjectFileAsync(extractResult.projectFileName, false, cancellationToken).ConfigureAwait(false);
                        model = loadResult.model;
                    }
                    break;
                case ".sbx":
                    projectFilePath = Path.GetDirectoryName(buildPackageName) ?? string.Empty;
                    var sbxLoadResult = await LoadSqlBuildProjectFileAsync(buildPackageName, false, cancellationToken).ConfigureAwait(false);
                    model = sbxLoadResult.model;
                    break;
                default:
                    return string.Empty;
            }

            if (model != null)
            {
                string hash = await CalculateBuildPackageSHA1SignatureFromPathAsync(projectFilePath, model, cancellationToken).ConfigureAwait(false);
                if (extension == ".sbm")
                    await CleanUpAndDeleteWorkingDirectoryAsync(projectFilePath, cancellationToken).ConfigureAwait(false);

                return hash;
            }
            else
            {
                return string.Empty;
            }
        }




        /// <summary>
        /// Calculated the SHA1 hash of the script package. Takes the build order into account 
        /// </summary>

        [Obsolete("Use CalculateBuildPackageSHA1SignatureFromPathAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static string CalculateBuildPackageSHA1SignatureFromPath(string projectFileExtractionPath, SqlSyncBuildDataModel model)
        {
            if (model != null && !string.IsNullOrEmpty(projectFileExtractionPath))
            {
                var scripts = model.Script.OrderBy(s => s.BuildOrder);

                StringBuilder sb = new StringBuilder();
                string fileHash, textHash;
                foreach (var script in scripts)
                {
                    GetSHA1Hash(Path.Combine(projectFileExtractionPath, script.FileName), out fileHash, out textHash, script.StripTransactionText ?? false);
                    sb.AppendLine(textHash);
                }

                string strHashData = GetSHA1Hash(sb.ToString());
                return strHashData;
            }
            else
            {
                return "Error calculating hash";
            }
        }

        /// <summary>
        /// Async version of CalculateBuildPackageSHA1SignatureFromPath. Calculates the SHA1 hash of a script package.
        /// </summary>
        public static async Task<string> CalculateBuildPackageSHA1SignatureFromPathAsync(string projectFileExtractionPath, SqlSyncBuildDataModel model, CancellationToken cancellationToken = default)
        {
            if (model != null && !string.IsNullOrEmpty(projectFileExtractionPath))
            {
                var scripts = model.Script.OrderBy(s => s.BuildOrder);

                StringBuilder sb = new StringBuilder();
                foreach (var script in scripts)
                {
                    var (_, textHash) = await GetSHA1HashAsync(Path.Combine(projectFileExtractionPath, script.FileName), script.StripTransactionText ?? false, cancellationToken).ConfigureAwait(false);
                    sb.AppendLine(textHash);
                }

                string strHashData = GetSHA1Hash(sb.ToString());
                return strHashData;
            }
            else
            {
                return "Error calculating hash";
            }
        }

        /// <summary>
        /// Used to calculate the hash of the build package via the collection of batched scripts.
        /// The batch should have accounted for execution run order and the strip transactions as per SqlBuildHelper.LoadAndBatchSqlScripts()
        /// </summary>
        /// <param name="scriptBatchColl">Collection of pre-batched SQL scripts</param>
        /// <returns>The SHA1 hash signature of the package</returns>
        internal static string CalculateBuildPackageSHA1SignatureFromBatchCollection(ScriptBatchCollection scriptBatchColl)
        {
            StringBuilder sb = new StringBuilder();
            string textHash;
            foreach (ScriptBatch batch in scriptBatchColl)
            {
                textHash=  fileHelper.GetSHA1Hash(batch.ScriptBatchContents);
                sb.AppendLine(textHash);
            }

            string strHashData = GetSHA1Hash(sb.ToString());
            return strHashData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="fileHash">Hash for the file itself. This takes into account creation times, etc. Needed for backward compataibility</param>
        /// <param name="textHash">Hash for just the file contents. This is the better hash to use</param>
        public static void GetSHA1Hash(string pathName, out string fileHash, out string textHash, bool stripTransactions)
        {
            string strHashData = "";

            byte[] arrbytHashValue;

            var oSHA1Hasher = System.Security.Cryptography.SHA1.Create();


            try
            {
                using (FileStream fs = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                          System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    arrbytHashValue = oSHA1Hasher.ComputeHash(fs);
                    fs.Close();

                    strHashData = System.BitConverter.ToString(arrbytHashValue);
                    strHashData = strHashData.Replace("-", "");
                    fileHash = strHashData;
                }

                //We need to process the file the same as when it was run.
                string[] split = scriptBatcher.ReadBatchFromScriptFile(pathName, stripTransactions, false);
                textHash = fileHelper.GetSHA1Hash(split);
                //string scriptText = String.Join("\r\n" + BatchParsing.Delimiter + "\r\n", split);

                //arrbytHashValue = oSHA1Hasher.ComputeHash(new ASCIIEncoding().GetBytes(scriptText));
                //strHashData = System.BitConverter.ToString(arrbytHashValue);
                //textHash = strHashData.Replace("-", "");
            }
            catch (FileNotFoundException)
            {
                fileHash = SqlBuildFileHelper.FileMissing;
                textHash = SqlBuildFileHelper.FileMissing;
            }
            catch
            {
                fileHash = SqlBuildFileHelper.Sha1HashError;
                textHash = SqlBuildFileHelper.Sha1HashError;
            }

            //return(strResult);
        }

        public static async Task<(string fileHash, string textHash)> GetSHA1HashAsync(string pathName, bool stripTransactions, CancellationToken cancellationToken = default)
        {
            string fileHash = string.Empty;
            string textHash = string.Empty;
            try
            {
                await using var fs = new System.IO.FileStream(pathName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                var oSHA1Hasher = System.Security.Cryptography.SHA1.Create();
                var arrbytHashValue = await oSHA1Hasher.ComputeHashAsync(fs, cancellationToken).ConfigureAwait(false);
                fileHash = System.BitConverter.ToString(arrbytHashValue).Replace("-", "");

                var split = await scriptBatcher.ReadBatchFromScriptFileAsync(pathName, stripTransactions, false, cancellationToken).ConfigureAwait(false);
                textHash = fileHelper.GetSHA1Hash(split);
            }
            catch (FileNotFoundException)
            {
                fileHash = SqlBuildFileHelper.FileMissing;
                textHash = SqlBuildFileHelper.FileMissing;
            }
            catch
            {
                fileHash = SqlBuildFileHelper.Sha1HashError;
                textHash = SqlBuildFileHelper.Sha1HashError;
            }

            return (fileHash, textHash);
        }

        public static string JoinBatchedScripts(string[] batchedScripts) => fileHelper.JoinBatchedScripts(batchedScripts);
        /// <summary>
        /// Gets the file hash of a file that has been split into batch scripts using the SqlBuildHelper.ReadBatchFromScriptFile or SqlBuildHelper.ReadBatchFromScriptText method
        /// </summary>
        /// <param name="batchedScriptLines">Batched Sql script</param>
        /// <param name="textHash">SHA1 hash of the batched script.</param>
        public static void GetSHA1Hash(string[] batchedScriptLines) => fileHelper.GetSHA1Hash(batchedScriptLines);
        /// <summary>
        /// Computes the SHA1 hash of a string. Should only be used on string recomposed from batching
        /// </summary>
        /// <param name="textContents"></param>
        /// <returns></returns>
        internal static string GetSHA1Hash(string textContents) => fileHelper.GetSHA1Hash(textContents);
       
        #endregion

        #region .: Renumbering/ Resorting :.


        /// <summary>
        /// Renumber the build sequence using POCO model
        /// </summary>
        public static SqlSyncBuildDataModel RenumberBuildSequence(SqlSyncBuildDataModel model, string projectFileName, string buildZipFileName)
        {
            return RenumberBuildSequence(model, projectFileName, buildZipFileName, (int)ResequenceIgnore.StartNumber);
        }

        /// <summary>
        /// Renumber the build sequence using POCO model with a custom ignore start number
        /// </summary>
        internal static SqlSyncBuildDataModel RenumberBuildSequence(SqlSyncBuildDataModel model, string projectFileName, string buildZipFileName, int renumberIgnoreStart)
        {
            try
            {
                var scriptsToRenumber = model.Script
                    .Where(s => s.BuildOrder < renumberIgnoreStart)
                    .OrderBy(s => s.BuildOrder)
                    .ToList();

                for (int i = 0; i < scriptsToRenumber.Count; i++)
                {
                    scriptsToRenumber[i].BuildOrder = i + 1;
                }

                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projectFileName, buildZipFileName);
                return model;
            }
            catch (Exception e)
            {
                log.LogError(e, "Error renumbering build sequence");
                throw;
            }
        }

        /// <summary>
        /// Resort the build by file type using POCO model
        /// </summary>
        public static SqlSyncBuildDataModel ResortBuildByFileType(SqlSyncBuildDataModel model, string projectFileName, string buildZipFileName)
        {
            // First move out the "reserve" items (renumber starting at 20000)
            int reservedIndex = 20000;
            var reservedScripts = model.Script
                .Where(s => s.BuildOrder >= (int)ResequenceIgnore.StartNumber)
                .OrderBy(s => s.BuildOrder)
                .ToList();
            
            foreach (var script in reservedScripts)
                script.BuildOrder = reservedIndex++;

            // Renumber the standard items by file extension
            for (int i = 0; i < ResortBuildType.SortOrder.Length; i++)
            {
                string extension = "." + ResortBuildType.SortOrder[i];
                var matchingScripts = model.Script
                    .Where(s => s.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) && s.BuildOrder < 20000)
                    .OrderBy(s => s.BuildOrder)
                    .ToList();

                for (int j = 0; j < matchingScripts.Count; j++)
                    matchingScripts[j].BuildOrder = (1000 * (i + 1)) + j;
            }

            // Take any of the remaining items (files with extensions not in SortOrder)
            int leftOverStart = 19000;
            var knownExtensions = ResortBuildType.SortOrder.Select(e => "." + e).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var leftOverScripts = model.Script
                .Where(s => !knownExtensions.Any(ext => s.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) && s.BuildOrder < 20000)
                .OrderBy(s => s.BuildOrder)
                .ToList();
            
            foreach (var script in leftOverScripts)
                script.BuildOrder = leftOverStart++;

            SaveSqlBuildProjectFile(model, projectFileName, buildZipFileName);

            // Renumber twice as the legacy code did
            model = RenumberBuildSequence(model, projectFileName, buildZipFileName, 19999);
            model = RenumberBuildSequence(model, projectFileName, buildZipFileName);
            return model;
        }
        #endregion



        public static bool ScriptRequiresBuildDescription(string scriptContents)
        {
            if (string.IsNullOrEmpty(scriptContents))
                return false;

            return scriptContents.IndexOf(SqlBuild.ScriptTokens.BuildDescription, 1, StringComparison.CurrentCultureIgnoreCase) > -1;
        }

        #region .: Updating from Legacy code :.

        public static bool UpdateObsoleteXmlNamespace(string fileName)
        {
            log.LogDebug($"Updating XmlNamespace from legacy file '{fileName}'");
            bool replaced = false;
            //valid namespace settings
            string xmlns = "xmlns=\"http://schemas.mckechney.com/";
            Regex xmlnsX = new Regex("xmlns=\"http://.+?/", RegexOptions.IgnoreCase);

            string contents = File.ReadAllText(fileName);
            if (xmlnsX.Match(contents).Success)
            {
                if (!contents.Contains(xmlns))
                {
                    contents = xmlnsX.Replace(contents, xmlns);
                    replaced = true;
                    File.WriteAllText(fileName, contents);
                }
            }

            if (replaced)
                log.LogInformation($"Successfully updated the XmlNamespace in file {fileName}");
            else
                log.LogInformation($"Unable to update the XmlNamespace in file {fileName}");
            return replaced;

        }
        #endregion

        #region .: Copying scripts out to plain files :.



        /// <summary>
        /// Copy scripts to individual files using POCO model
        /// </summary>
        public static bool CopyIndividualScriptsToFolder(SqlSyncBuildDataModel model, string destinationFolder, string projectFilePath, bool includeUSE, bool includeSequence)
        {
            if (model.Script == null || model.Script.Count == 0)
                return false;

            StringBuilder sb = new StringBuilder();
            string[] batch;
            string fileName = string.Empty;

            try
            {
                var sortedScripts = model.Script.OrderBy(s => s.BuildOrder).ToList();
                for (int i = 0; i < sortedScripts.Count; i++)
                {
                    var script = sortedScripts[i];
                    if (!File.Exists(Path.Combine(projectFilePath, script.FileName)))
                        continue;

                    if (includeUSE)
                        sb.Append("USE " + script.Database + "\r\nGO\r\n");

                    batch = scriptBatcher.ReadBatchFromScriptFile(Path.Combine(projectFilePath, script.FileName), script.StripTransactionText ?? false, true);
                    for (int j = 0; j < batch.Length; j++)
                        sb.Append(batch[j] + "\r\n");

                    if (includeSequence)
                        fileName = Path.Combine(destinationFolder, (i + 1).ToString().PadLeft(3, '0') + " " + script.FileName);
                    else
                        fileName = Path.Combine(destinationFolder, script.FileName);

                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Flush();
                    }
                    sb.Length = 0;
                }
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to export script {fileName} to destination folder {destinationFolder}");
                return false;
            }
        }

        /// <summary>
        /// Copy scripts to a single file using POCO model
        /// </summary>
        public static bool CopyScriptsToSingleFile(SqlSyncBuildDataModel model, string destinationFile, string projectFilePath, string buildFileName, bool includeUSE)
        {
            if (model.Script == null || model.Script.Count == 0)
                return false;

            StringBuilder sb = new StringBuilder();
            string[] batch;

            try
            {
                sb.Append("-- Scripts Consolidated from: " + Path.GetFileName(buildFileName) + "\r\n");
                var sortedScripts = model.Script.OrderBy(s => s.BuildOrder).ToList();
                for (int i = 0; i < sortedScripts.Count; i++)
                {
                    var script = sortedScripts[i];
                    if (!File.Exists(Path.Combine(projectFilePath, script.FileName)))
                        continue;

                    sb.Append("\r\n-- Source File: " + script.FileName + "\r\n");
                    if (includeUSE)
                        sb.Append("USE " + script.Database + "\r\nGO\r\n");
                    batch = scriptBatcher.ReadBatchFromScriptFile(Path.Combine(projectFilePath, script.FileName), script.StripTransactionText ?? false, true);
                    for (int j = 0; j < batch.Length; j++)
                        sb.Append(batch[j] + "\r\n");
                }

                using (StreamWriter sw = File.CreateText(destinationFile))
                {
                    sw.WriteLine(sb.ToString());
                    sw.Flush();
                }
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to copy scripts to {destinationFile}");
                return false;
            }
        }

        /// <summary>
        /// Copy scripts to individual files using POCO model (async)
        /// </summary>
        public static async Task<bool> CopyIndividualScriptsToFolderAsync(SqlSyncBuildDataModel model, string destinationFolder, string projectFilePath, bool includeUSE, bool includeSequence, CancellationToken cancellationToken = default)
        {
            if (model.Script == null || model.Script.Count == 0)
                return false;

            StringBuilder sb = new StringBuilder();
            string[] batch;
            string fileName = string.Empty;

            try
            {
                var sortedScripts = model.Script.OrderBy(s => s.BuildOrder).ToList();
                for (int i = 0; i < sortedScripts.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var script = sortedScripts[i];
                    if (!File.Exists(Path.Combine(projectFilePath, script.FileName)))
                        continue;

                    if (includeUSE)
                        sb.Append("USE " + script.Database + "\r\nGO\r\n");

                    batch = await scriptBatcher.ReadBatchFromScriptFileAsync(Path.Combine(projectFilePath, script.FileName), script.StripTransactionText ?? false, true, cancellationToken).ConfigureAwait(false);
                    for (int j = 0; j < batch.Length; j++)
                        sb.Append(batch[j] + "\r\n");

                    if (includeSequence)
                        fileName = Path.Combine(destinationFolder, (i + 1).ToString().PadLeft(3, '0') + " " + script.FileName);
                    else
                        fileName = Path.Combine(destinationFolder, script.FileName);

                    await File.WriteAllTextAsync(fileName, sb.ToString(), cancellationToken).ConfigureAwait(false);
                    sb.Length = 0;
                }
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to export script {fileName} to destination folder {destinationFolder}");
                return false;
            }
        }

        /// <summary>
        /// Copy scripts to a single file using POCO model (async)
        /// </summary>
        public static async Task<bool> CopyScriptsToSingleFileAsync(SqlSyncBuildDataModel model, string destinationFile, string projectFilePath, string buildFileName, bool includeUSE, CancellationToken cancellationToken = default)
        {
            if (model.Script == null || model.Script.Count == 0)
                return false;

            StringBuilder sb = new StringBuilder();
            string[] batch;

            try
            {
                sb.Append("-- Scripts Consolidated from: " + Path.GetFileName(buildFileName) + "\r\n");
                var sortedScripts = model.Script.OrderBy(s => s.BuildOrder).ToList();
                for (int i = 0; i < sortedScripts.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var script = sortedScripts[i];
                    if (!File.Exists(Path.Combine(projectFilePath, script.FileName)))
                        continue;

                    sb.Append("\r\n-- Source File: " + script.FileName + "\r\n");
                    if (includeUSE)
                        sb.Append("USE " + script.Database + "\r\nGO\r\n");
                    batch = await scriptBatcher.ReadBatchFromScriptFileAsync(Path.Combine(projectFilePath, script.FileName), script.StripTransactionText ?? false, true, cancellationToken).ConfigureAwait(false);
                    for (int j = 0; j < batch.Length; j++)
                        sb.Append(batch[j] + "\r\n");
                }

                await File.WriteAllTextAsync(destinationFile, sb.ToString(), cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to copy scripts to {destinationFile}");
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Import scripts from another build file using POCO model
        /// </summary>
        public static (double buildNumber, SqlSyncBuildDataModel model, string[] addedFileNames) ImportSqlScriptFile(
            SqlSyncBuildDataModel model, 
            SqlSyncBuildDataModel importModel,
            string importWorkingDirectory, 
            double lastBuildNumber, 
            string projectFilePath, 
            string projectFileName, 
            string buildZipFileName, 
            bool cleanUp)
        {
            var addedFileNames = new List<string>();
            double startBuildNumber = lastBuildNumber + 1;
            
            try
            {
                var sortedImportScripts = importModel.Script?.OrderBy(s => s.BuildOrder).ToList() ?? new List<Script>();
                
                if (sortedImportScripts.Count == 0)
                    return ((double)ImportFileStatus.NoRowsImported, model, Array.Empty<string>());

                int increment = 0;
                foreach (var importScript in sortedImportScripts)
                {
                    var newScript = new Script(
                        importScript.FileName,
                        startBuildNumber + increment,
                        importScript.Description,
                        importScript.RollBackOnError,
                        importScript.CausesBuildFailure,
                        importScript.DateAdded,
                        string.IsNullOrEmpty(importScript.ScriptId) ? Guid.NewGuid().ToString() : importScript.ScriptId,
                        importScript.Database,
                        importScript.StripTransactionText,
                        importScript.AllowMultipleRuns,
                        importScript.AddedBy,
                        importScript.ScriptTimeOut,
                        importScript.DateModified,
                        importScript.ModifiedBy,
                        importScript.Tag);
                    
                    addedFileNames.Add(importScript.FileName ?? "");
                    model.Script.Add(newScript);
                    
                    var destPath = Path.Combine(projectFilePath, importScript.FileName);
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    
                    try
                    {
                        File.Copy(Path.Combine(importWorkingDirectory, importScript.FileName), destPath);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(200);
                        File.Copy(Path.Combine(importWorkingDirectory, importScript.FileName), destPath);
                    }
                    increment++;
                }

                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projectFileName, buildZipFileName);

                if (cleanUp)
                {
                    try
                    {
                        var files = Directory.GetFiles(importWorkingDirectory);
                        foreach (var file in files)
                            File.Delete(file);
                        Directory.Delete(importWorkingDirectory);
                    }
                    catch { }
                }

                return (startBuildNumber, model, addedFileNames.ToArray());
            }
            catch (Exception e)
            {
                log.LogError(e, "Error importing script file");
                return (-1, model, Array.Empty<string>());
            }
        }


        public static bool MakeFileWriteable(string fileName)
        {
            try
            {
                FileInfo inf = new FileInfo(fileName);
                inf.Attributes = FileAttributes.Normal;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<double> GetInsertedIndexValues(double floorIndex, double ceilingIndex, int insertCount)
        {
            List<double> insertIndexes = new List<double>();
            //Get the closest integer values if possible...
            if (Math.Ceiling(floorIndex) < Math.Floor(ceilingIndex))
            {
                floorIndex = Math.Ceiling(floorIndex);
                ceilingIndex = Math.Floor(ceilingIndex);
            }

            //We can use whole values
            if (floorIndex + insertCount < ceilingIndex)
            {
                for (int i = 1; i <= insertCount; i++)
                    insertIndexes.Add(floorIndex + i);

                return insertIndexes;
            }

            //Can we use simple tenth values?
            if (floorIndex + Convert.ToDouble(insertCount) * 0.1 < ceilingIndex)
            {
                for (double i = 1; i <= insertCount; i++)
                    insertIndexes.Add(floorIndex + i / 10.0);

                return insertIndexes;
            }

            //Can't use whole values, so let's do some math!!
            double step = (ceilingIndex - floorIndex) / (insertCount + 2);
            for (double i = 1; i <= insertCount; i++)
            {
                double tmp = Math.Round(floorIndex + (i * step), 2);
                insertIndexes.Add(tmp);
            }

            return insertIndexes;
        }

        #region .: Files/Path Initilization Methods :.
        [Obsolete("Use CleanUpAndDeleteWorkingDirectoryAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool CleanUpAndDeleteWorkingDirectory(string workingDir)
        {
            try
            {
                if (Directory.Exists(workingDir))
                    Directory.Delete(workingDir, true);
                return true;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to clean up working directory '{workingDir}'");
                return false;
            }
        }

        /// <summary>
        /// Async version of CleanUpAndDeleteWorkingDirectory. Cleans up and deletes a working directory.
        /// </summary>
        public static Task<bool> CleanUpAndDeleteWorkingDirectoryAsync(string workingDir, CancellationToken cancellationToken = default)
        {
            // Directory operations are not truly async in .NET, but we wrap for consistency
            return Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(workingDir))
                        Directory.Delete(workingDir, true);
                    return true;
                }
                catch (Exception exe)
                {
                    log.LogWarning(exe, $"Unable to clean up working directory '{workingDir}'");
                    return false;
                }
            }, cancellationToken);
        }

        [Obsolete("Use InitializeWorkingDirectoryAsync instead for better performance. This synchronous method will be removed in a future version.")]
        public static bool InitilizeWorkingDirectory(ref string workingDirectory, ref string projectFilePath, ref string projectFileName)
        {
            try
            {
                if (workingDirectory != null)
                {
                    CleanUpAndDeleteWorkingDirectory(workingDirectory);
                    string tmpDir = System.IO.Path.GetTempPath();
                    workingDirectory = Path.Combine(tmpDir, @"Sqlsync-" + System.Guid.NewGuid().ToString().Replace("-", ""));
                }

                projectFilePath = workingDirectory;

                if (projectFileName != null && projectFileName.Length > 0)
                    projectFileName = Path.Combine(workingDirectory, Path.GetFileName(projectFileName));

                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);

                log.LogDebug($"Successfully created working directory at '{workingDirectory}'");

                return true;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to clean up working directory '{workingDirectory}'");
                return false;
            }
        }

        /// <summary>
        /// Async version of InitilizeWorkingDirectory. Initializes a working directory.
        /// </summary>
        public static Task<(bool success, string workingDirectory, string projectFilePath, string projectFileName)> InitializeWorkingDirectoryAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                string workingDirectory = string.Empty;
                string projectFilePath = string.Empty;
                string projectFileName = string.Empty;
                
                bool success = InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);
                return (success, workingDirectory, projectFilePath, projectFileName);
            }, cancellationToken);
        }

        #endregion


    }
}

