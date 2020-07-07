using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for SqlBuildFileHelper.
    /// </summary>
    public class SqlBuildFileHelper
	{
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string FileMissing = "File Missing";
        public const string Sha1HashError = "SHA1 Hash Error";
        public static string DefaultScriptXmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Default Scripts\DefaultScriptRegistry.xml";
		public SqlBuildFileHelper()
		{
			
		}
        #region .: Loading Zip and Project Files :.
        public static bool ValidateAgainstSchema(string fileName,out string validationErrorMessage)
		{
            //Going to obsolete this....
            validationErrorMessage = string.Empty;
            return true;

            //string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6); //get rid of the file:\

            //string schemaFile = path + @"\" + SchemaEnums.SqlBuildManagerProjectXsd;

            //if (!File.Exists(schemaFile))
            //{
            //    log.DebugFormat("The schema file not found at '{0}'. Returning 'true' for validation so processing can continue.",  schemaFile);
            //    validationErrorMessage = string.Empty;
            //    return true;
            //}

            //log.DebugFormat("Validating '{0}' against schema file '{1}'", fileName, schemaFile);

            //SchemaValidator validator = new SchemaValidator();
            //bool isValid =  validator.ValidateAgainstSchema(fileName,
            //    schemaFile,
            //    SchemaEnums.SqlBuildManagerProjectNamespace);

            //validationErrorMessage = validator.ValidityErrorMessage;

            ////may be an obsolete xml namespace.. try to change it...
            //if (!isValid)
            //{
            //    if(UpdateObsoleteXmlNamespace(fileName))
            //        isValid = ValidateAgainstSchema(fileName, out validationErrorMessage);
            //}
            //if (!isValid)
            //    log.ErrorFormat("Unable to validate {0} against schema definition {1}. Error message: {2}", fileName, schemaFile, validationErrorMessage);
            //else
            //    log.DebugFormat("ValidateAgainstSchema results: isValid = {0}; validationErrorMessage = '{1}'", isValid.ToString(), validationErrorMessage);

            //return isValid;
		}
        public static bool ExtractSqlBuildZipFile(string fileName, ref string workingDirectory, ref string projectFilePath, ref string projectFileName, out string result)
        {
            return ExtractSqlBuildZipFile(fileName, ref workingDirectory, ref projectFilePath, ref projectFileName, true, false, out result);
        }
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
                        log.ErrorFormat("ExtractSqlBuildZipFile error: {0}", result);
                        return false;
                    }
                }
                else
                {
                    if (!workingDirectory.EndsWith(@"\")) workingDirectory = workingDirectory + @"\";
                    projectFilePath = workingDirectory;
                    log.DebugFormat("ExtractSqlBuildZipFile projectFilePath set to: {0}", projectFilePath);
                }

                //Unpack the zip contents into the working directory
                if (!ZipHelper.UnpackZipPackage(workingDirectory, fileName, overwriteExistingProjectFiles))
                {
                    result = "Unable to unpack Sql Build Project File [" + fileName + "]";
                    log.ErrorFormat("ExtractSqlBuildZipFile error: {0}", result);
                    return false;
                }
                log.DebugFormat("Successfully UnZipped Sql Build Project file {0}", fileName);

                if (File.Exists(workingDirectory + XmlFileNames.MainProjectFile))
                {
                    log.DebugFormat("Found MainProjectFile at: {0}", workingDirectory + XmlFileNames.MainProjectFile);
                    string valErrorMessage;
                    if (SqlBuildFileHelper.ValidateAgainstSchema(workingDirectory + XmlFileNames.MainProjectFile, out valErrorMessage))
                    {
                        projectFileName = workingDirectory + XmlFileNames.MainProjectFile;
                        log.Debug("MainProjectFile successfully validated against schema");
                        return true;
                    }
                    else
                    {
                        SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                        result = "Unable to validate the schema for: "+workingDirectory + XmlFileNames.MainProjectFile;
                        log.ErrorFormat("ExtractSqlBuildZipFile error: {0}", result);
                        return false;
                    }
                }
                else
                {
                    log.WarnFormat("The MainProjectFile not found at {0}", workingDirectory + XmlFileNames.MainProjectFile);
                    string[] files = Directory.GetFiles(workingDirectory, "*.xml");
                    for (int i = 0; i < files.Length; i++)
                    {
                        log.DebugFormat("Attempting to validate {0} against schema.", files[i]);
                        string valErrorMessage;
                        if (SqlBuildFileHelper.ValidateAgainstSchema(files[i], out valErrorMessage))
                        {
                            log.DebugFormat("Project file found at {0}. Using as main project metadata file.", files[i]);
                            projectFileName = files[i];
                            return true;
                        }
                    }

                    SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                    result = "Unable to validate the schema for any XML file in "+ workingDirectory;
                    log.ErrorFormat("ExtractSqlBuildZipFile error: {0}", result);
                    return false;
                }

           }
            catch (Exception exe)
            {
                result = exe.Message;
                log.ErrorFormat("ExtractSqlBuildZipFile exception: {0}", result);
                SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);
                return false;
            }
        }

        public static bool LoadSqlBuildProjectFile(out SqlSyncBuildData buildData, string projFileName, bool validateSchema)
        {
            bool successfulLoad = true;
            string returnName = string.Empty;

            buildData = new SqlSyncBuildData();
            if (File.Exists(projFileName))
            {
                log.DebugFormat("LoadSqlBuildProjectFile: found projectFile at {0}", projFileName);
                //Read the table list
                try
                {
                    bool isValid = true;
                    string valErrorMessage;
                    if (validateSchema)
                    {

                        isValid = ValidateAgainstSchema(projFileName, out valErrorMessage);
                    }

                    if (isValid)
                    {
                        buildData.ReadXml(projFileName);
                    }
                    else
                    {
                        log.Error("Unable to load Sql Project file. XML file is not valid");
                        successfulLoad = false;
                    }
                }
                catch(Exception exe)
                {
                    log.Error("Unable to load Sql Project file", exe);
                    successfulLoad = false;
                }
            }
            else
            {
                log.InfoFormat("LoadSqlBuildProjectFile: unable to find projectFile at {0}. Creating shell.", projFileName);
                buildData = CreateShellSqlSyncBuildDataObject();
                buildData.WriteXml(projFileName);
            }
            log.DebugFormat("LoadSqlBuildProjectFile: Returning successfulLoad =  {0}.", successfulLoad.ToString());
            return successfulLoad;
        }
        #endregion

        public static string InferOverridesFromPackage(string sbmFileName)
        {
            string tempWorkingDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string projectFilePath = string.Empty;
            string projectFileName = string.Empty;
            string result;
            SqlSyncBuildData buildData;
            StringBuilder ovr = new StringBuilder();
            try
            {
                ExtractSqlBuildZipFile(sbmFileName, ref tempWorkingDir, ref projectFilePath, ref projectFileName, out result);
                LoadSqlBuildProjectFile(out buildData, projectFileName, false);
                if(buildData != null)
                {
                   var targets =  buildData.Script.Select(s => s.Database).Distinct();
                   if(targets != null && targets.Count() > 0)
                    {
                        foreach(var t in targets)
                        {
                            ovr.Append($"{t},{t};");
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
                if(Directory.Exists(tempWorkingDir))
                {
                    Directory.Delete(tempWorkingDir, true);
                }
            }

            return ovr.ToString();
        }


        //#region .: Tfs Source Control integration :.
        //[Obsolete("Removing integration with TFS here. Will untangle the rest of the code later!")]
        //public static IFileStatus CheckoutFilesFromSourceControl(string sourceControlURL, List<string> files)
        //{
        //    try
        //    {
        //        //ISourceControl sc = new SourceControl(sourceControlURL);
        //        //return sc.UpdateSourceControl(files);
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }

        //}
        //[Obsolete("Removing integration with TFS here. Will untangle the rest of the code later!")]
        //public static SourceControlStatus CheckoutFileFromSourceControl(string sourceControlURL, string fileName)
        //{
        //    try
        //    {
        //        //ISourceControl sc = new SourceControl(sourceControlURL);
        //        //return sc.UpdateSourceControl(fileName);
        //        return SourceControlStatus.Unknown;
        //    }
        //    catch
        //    {
        //        return SourceControlStatus.Unknown;
        //    }
        //}

       // #endregion

        public static bool PackageProjectFileIntoZip(SqlSyncBuildData projData, string projFilePath, string zipFileName)
        {
            return PackageProjectFileIntoZip(projData, projFilePath, zipFileName, true);
        }
        public static bool PackageProjectFileIntoZip(SqlSyncBuildData projData, string projFilePath, string zipFileName, bool includeHistoryAndLogs)
        {
            if (String.IsNullOrEmpty(zipFileName))
                return true;

            ArrayList alFiles = new ArrayList();
            //Write the latest to the project dataset file
            if(!projFilePath.EndsWith(@"\")) projFilePath = projFilePath + @"\";
            if (projData != null)
                projData.WriteXml(projFilePath + XmlFileNames.MainProjectFile);
            else
                return false;

            //Get the file list from the dataset
            for (int i = 0; i < projData.Script.Rows.Count; i++)
                alFiles.Add(((SqlSyncBuildData.ScriptRow)projData.Script.Rows[i]).FileName);

            //Add the project file 
            alFiles.Add(XmlFileNames.MainProjectFile);

            
            if (includeHistoryAndLogs)
            {
                //Add the history file
                if (File.Exists(projFilePath + XmlFileNames.HistoryFile))
                {
                    alFiles.Add(XmlFileNames.HistoryFile);
                }

                //Add any log files
                string[] logFiles = Directory.GetFiles(projFilePath, "*.log");
                for (int j = 0; j < logFiles.Length; j++)
                    alFiles.Add(Path.GetFileName(logFiles[j]));
            }

            //Put all files into a string array
            string[] fileList = new string[alFiles.Count];
            alFiles.CopyTo(fileList);

            //Create the Zip file
            bool val = ZipHelper.CreateZipPackage(fileList, projFilePath, zipFileName);
            return val;
        }
         /// <summary>
        /// Minimize the size of the package by cleaing out the logs and the code review items..
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] CleanProjectFileForRemoteExecution(string fileName)
        {
            SqlSyncBuildData cleanedBuildData;
            return CleanProjectFileForRemoteExecution(fileName, out cleanedBuildData);
        }
        /// <summary>
        /// Minimize the size of the package by cleaing out the logs and the code review items..
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] CleanProjectFileForRemoteExecution(string fileName, out SqlSyncBuildData cleanedBuildData)
        {
            cleanedBuildData = new SqlSyncBuildData();
            if(!File.Exists(fileName))
                return new byte[0];

            string tmpDir = string.Empty;
            try
            {
                tmpDir = Path.GetDirectoryName(fileName)+@"\" + Guid.NewGuid().ToString();
                Directory.CreateDirectory(tmpDir);

                string tmpZipShortName = "~" + Path.GetFileName(fileName);
                string tmpProjectFileName = tmpDir + @"\" + XmlFileNames.MainProjectFile;

                string tmpZipFullName = tmpDir + @"\" + tmpZipShortName;
                File.Copy(fileName, tmpZipFullName);

                string result;
                if (ExtractSqlBuildZipFile(tmpZipFullName, ref tmpDir, ref tmpDir, ref tmpProjectFileName,false, false, out result))
                {
                    LoadSqlBuildProjectFile(out cleanedBuildData, tmpProjectFileName, false);
                    cleanedBuildData.CodeReview.Rows.Clear();
                    cleanedBuildData.ScriptRun.Rows.Clear();
                    cleanedBuildData.Build.Rows.Clear();
                    //buildData.Builds.Rows.Clear();
                    cleanedBuildData.AcceptChanges();
                    cleanedBuildData.WriteXml(tmpProjectFileName);

                    if (PackageProjectFileIntoZip(cleanedBuildData, tmpDir, tmpZipFullName, false))
                    {
                        return File.ReadAllBytes(tmpZipFullName);
                    }
                }

                //can't clean for some reason, so just get the raw file...
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
        public static SqlSyncBuildData CreateShellSqlSyncBuildDataObject()
        {
            SqlSyncBuildData newData = new SqlSyncBuildData();
            //Create new project row if needed 
            if (newData.SqlSyncBuildProject.Rows.Count == 0)
            {
                newData.SqlSyncBuildProject.AddSqlSyncBuildProjectRow("", false);
            }
            //Create the new script row if needed
            if (newData.Scripts.Rows.Count == 0)
            {
                newData.Scripts.AddScriptsRow((SqlSyncBuildData.SqlSyncBuildProjectRow)newData.SqlSyncBuildProject.Rows[0]);
            }
            if (newData.Builds.Rows.Count == 0)
            {
                newData.Builds.AddBuildsRow((SqlSyncBuildData.SqlSyncBuildProjectRow)newData.SqlSyncBuildProject.Rows[0]);
            }
            return newData;
        }


		public static void SaveSqlBuildProjectFile(ref SqlSyncBuildData buildData, string projFileName, string buildZipFileName)
		{
			buildData.WriteXml(projFileName);
			PackageProjectFileIntoZip(buildData,Path.GetDirectoryName(projFileName)+@"\", buildZipFileName);
		}

        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, List<string> fileNames, string targetDatabaseName, int defaultScriptTimeout)
        {
            return SaveSqlFilesToNewBuildFile(buildFileName, fileNames, targetDatabaseName, false, defaultScriptTimeout);
        }

        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, List<string> fileNames, string targetDatabaseName, bool overwritePreExistingFile, int defaultScriptTimeout)
        {
            if (File.Exists(buildFileName) && !overwritePreExistingFile)
            {
                return false;
            }
            string directory = Path.GetDirectoryName(buildFileName);
            try
            {
                SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                string projFileName = directory + @"\" + XmlFileNames.MainProjectFile;
                int i = 0;
                foreach (string file in fileNames)
                {
                    string shortFileName = Path.GetFileName(file);
                    if (shortFileName == XmlFileNames.MainProjectFile ||
                        shortFileName == XmlFileNames.ExportFile)
                        continue;

                    i++;
                    SqlBuildFileHelper.AddScriptFileToBuild(
                        ref buildData,
                        projFileName,
                        shortFileName,
                        i + 1,
                        "",
                        true,
                        true,
                        targetDatabaseName,
                        false,
                        buildFileName,
                        false,
                        true,
                        System.Environment.UserName,
                        defaultScriptTimeout, "");

                }
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildFileName);
                return true;
            }
            catch(Exception exe)
            {
                log.Error("Unable to package scripts", exe);
                return false;
            }

        }
        public static bool SaveSqlFilesToNewBuildFile(string buildFileName, string directory, string targetDatabaseName, int defaultScriptTimeout)
        {
            string[] files = Directory.GetFiles(directory);
            return SaveSqlFilesToNewBuildFile(buildFileName, files.ToList(), targetDatabaseName, defaultScriptTimeout);
        }

        #region .: Packaging SBX into SBM :.
        public static List<string> PackageSbxFilesIntoSbmFiles(string directoryName, out string message)
        {
            if (directoryName == null || directoryName.Length == 0)
            {
                message = "Unable to package SBX files. Source directory parameter is empty";
                log.Warn(message);
                return new List<string>();
            }

            if (!Directory.Exists(directoryName))
            {
                message = String.Format("Unable to package SBX files. The specified source directory '{0}' does not exist.", directoryName);
                log.Warn(message);
                return new List<string>();
            }

            message = string.Empty;
            List<string> sbmFiles = new List<string>();
            string[] sbxFiles = Directory.GetFiles(directoryName, "*.sbx", SearchOption.AllDirectories);
            if (sbxFiles.Length == 0)
            {
                message = String.Format("No SBX files found in source directory '{0}' or any of it's subdirectories", directoryName);
                log.Warn(message);
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
                    message = String.Format("Error packaging SBX file '{0}' - please check application log at \"{1}\"", sbx, GetLogFileName());
                    log.Error(message);
                    return new List<string>();
                }
            }

            return sbmFiles;
        }
        public static string PackageSbxFileIntoSbmFile(string sbxBuildControlFileName)
        {
            string sbmProjectFileName = Path.GetDirectoryName(sbxBuildControlFileName) + @"\"+ Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm";
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
                    log.Warn("Can't package SBX file into SBM package - SBX file name is empty");
                    return false;
                }
                
                if (File.Exists(sbmProjectFileName))
                {
                    log.WarnFormat("Deleting a pre-existing SBM file: {0}", sbmProjectFileName);
                    File.Delete(sbmProjectFileName);
                }

                SqlSyncBuildData buildData = null;
                bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, sbxBuildControlFileName, true);
                if (!successfulLoad)
                {
                    log.ErrorFormat("Problem loading SBX file: {0}. ", sbxBuildControlFileName);
                    return false;
                }
              
                bool copied = false;
                string path = Path.GetDirectoryName(sbxBuildControlFileName) + @"\";

                //Just in case that file already exists...
                if (File.Exists(path + XmlFileNames.MainProjectFile))
                {
                    log.WarnFormat("Renaming pre-existing XML \"main project file\": {0}", path + XmlFileNames.MainProjectFile);
                    File.Copy(path + XmlFileNames.MainProjectFile, path + "~~" + XmlFileNames.MainProjectFile, true);
                    copied = true;
                }

                //Copy the SBX to the main project file name
                File.Copy(sbxBuildControlFileName, path + XmlFileNames.MainProjectFile, true);
               
                //Validate that all of the script files are present...
                foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                {
                    if (!File.Exists(path + row.FileName))
                    {
                        log.ErrorFormat("A script file configured in the SBX file was not found: '{0}'. Unable to create SBM package.", path + row.FileName);
                        return false;
                    }
                }


                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, path + XmlFileNames.MainProjectFile, sbmProjectFileName);


                if (copied)
                {
                    log.WarnFormat("Moving pre-existing XML \"main project file\" back to  {0}", path + XmlFileNames.MainProjectFile);
                    File.Copy(path + "~~" + XmlFileNames.MainProjectFile, path + XmlFileNames.MainProjectFile, true);
                    File.Delete(path + "~~" + XmlFileNames.MainProjectFile);
                }
                else
                {
                    log.DebugFormat("Deleting the temporary file {0}", path + XmlFileNames.MainProjectFile);
                    File.Delete(path + XmlFileNames.MainProjectFile);
                }

                log.DebugFormat("Successfully packaged SBX file '{0}' into SBM file '{1}'", sbxBuildControlFileName, sbmProjectFileName);
                return true;
            }
            catch (Exception exe)
            {
                log.Error(String.Format("Error creating SBM package for {0}", sbxBuildControlFileName), exe);
                return false;
            }

            
        }
        
        #endregion

        public static string GetLogFileName()
        {
            log4net.Appender.IAppender[] appenders = log.Logger.Repository.GetAppenders();
            foreach (log4net.Appender.IAppender appender in appenders)
            {
                if (appender is log4net.Appender.RollingFileAppender)
                {
                    string file = ((log4net.Appender.RollingFileAppender)appender).File;
                    if (File.Exists(file))
                        return file;
                }
            }
            return string.Empty;
        }

        #region .: Add / Remove scripts from build :.
        public static bool RemoveScriptFilesFromBuild(ref SqlSyncBuildData buildData, string projFileName, string buildZipFileName, SqlSyncBuildData.ScriptRow[] rows, bool deleteFiles)
        {
            string fileName;
            buildData.ScriptRun.AcceptChanges();
            for (int i = 0; i < rows.Length; i++)
            {
                try
                {
                    fileName = Path.GetDirectoryName(projFileName) + @"\" + rows[i].FileName;
                    buildData.Script.RemoveScriptRow(rows[i]);
                    if (deleteFiles)
                        File.Delete(fileName);

                }
                catch (IOException ioExe)
                {
                    log.Error(String.Format("Unable to delete file {0}", rows[i].FileName), ioExe);
                    string message = String.Format("Unable to delete file {0}. Error message: {1}", rows[i].FileName, ioExe.Message);
                }
                catch (IndexOutOfRangeException iExe)
                {
                    log.Error(String.Format("Unable to delete file at index {0}", i.ToString()), iExe);
                    string message = String.Format("Unable to delete file at index {0}. Error message: {1}", i.ToString(), iExe.Message);
                    return false;
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Unable to remove file file {0}", rows[i].FileName), e);
                    string message = String.Format("Unable to delete file {0}. Error message: {1}", rows[i].FileName, e.Message);
                    return false;
                }
            }

            buildData.Script.AcceptChanges();
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildZipFileName);
            return true;

        }
        public static void AddScriptFileToBuild(ref SqlSyncBuildData buildData, string projFileName, string fileName, double buildOrder, string description, bool rollBackScript, bool rollBackBuild, string databaseName, bool stripTransactions, string buildZipFileName, bool saveToZip, bool allowMultipleRuns, string addedBy, int scriptTimeOut, System.Guid scriptId, string tag)
        {
            //Create new project row if needed 
            if (buildData.SqlSyncBuildProject.Rows.Count == 0)
                buildData.SqlSyncBuildProject.AddSqlSyncBuildProjectRow("", false);
            //Create the new script row if needed
            if (buildData.Scripts.Rows.Count == 0)
                buildData.Scripts.AddScriptsRow((SqlSyncBuildData.SqlSyncBuildProjectRow)buildData.SqlSyncBuildProject.Rows[0]);

            SqlSyncBuildData.ScriptsRow row = (SqlSyncBuildData.ScriptsRow)buildData.Scripts.Rows[0];

            DateTime now = DateTime.Now;
            //Add the new script file
            buildData.Script.AddScriptRow(
                fileName,
                buildOrder,
                description,
                rollBackScript,
                rollBackBuild,
                now,
                scriptId.ToString(),
                databaseName,
                stripTransactions,
                allowMultipleRuns,
                addedBy,
                scriptTimeOut,
                DateTime.MinValue,
                "",
                row,
                tag);
            buildData.Script.AcceptChanges();

            //Save the changes
            if (saveToZip)
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildZipFileName);
        }
        public static void AddScriptFileToBuild(ref SqlSyncBuildData buildData, string projFileName, string fileName, double buildOrder, string description, bool rollBackScript, bool rollBackBuild, string databaseName, bool stripTransactions, string buildZipFileName, bool saveToZip, bool allowMultipleRuns, string addedBy, int scriptTimeOut, string tag)
        {
            AddScriptFileToBuild(ref buildData, projFileName, fileName, buildOrder, description, rollBackScript, rollBackBuild, databaseName, stripTransactions, buildZipFileName, saveToZip, allowMultipleRuns, addedBy, scriptTimeOut, System.Guid.NewGuid(), tag);
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
                log.WarnFormat("Unable to load Default Script Registry at {0}. File does not exist", defaultScriptXmlFile);
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
        public static DefaultScriptCopyStatus AddDefaultScriptToBuild(ref SqlSyncBuildData buildData, DefaultScripts.DefaultScript defaultScript, DefaultScriptCopyAction copyAction, string projFileName, string buildZipFileName)
        {
            DefaultScriptCopyStatus status = DefaultScriptCopyStatus.Success;
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string defaultScriptPath = Path.GetDirectoryName(SqlBuildFileHelper.DefaultScriptXmlFile);

            string fullScriptPath = defaultScriptPath + @"\" + defaultScript.ScriptName;
            if (File.Exists(fullScriptPath) == false)
                return DefaultScriptCopyStatus.DefaultNotFound;

            string newLocalFile = Path.GetDirectoryName(projFileName) + @"\" + defaultScript.ScriptName;

            if (File.Exists(newLocalFile))
            {
                bool isReadOnly = ((new FileInfo(newLocalFile).Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                if ( (DefaultScriptCopyAction.OverwriteExisting == copyAction) && !isReadOnly)
                {
                    File.Copy(fullScriptPath, newLocalFile, true);
                }
                else if ( (File.ReadAllText(newLocalFile) != File.ReadAllText(fullScriptPath)) && 
                                (copyAction != DefaultScriptCopyAction.LeaveExisting))
                {
                    if (isReadOnly)
                        status = DefaultScriptCopyStatus.PreexistingDifferentReadOnly;
                    else
                        return DefaultScriptCopyStatus.PreexistingDifferent;
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

            AddScriptFileToBuild(ref buildData,
                projFileName,
                defaultScript.ScriptName,
                defaultScript.BuildOrder,
                defaultScript.Description,
                defaultScript.RollBackScript,
                defaultScript.RollBackBuild,
                defaultScript.DatabaseName,
                defaultScript.StripTransactions,
                buildZipFileName,
                false,
                defaultScript.AllowMultipleRuns,
                System.Environment.UserName, defaultScript.ScriptTimeout, defaultScript.ScriptTag);

            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildZipFileName);

            return status;
        }
        #endregion 

        #region .: Object/ Populate Script Update settings :.
        public static SqlBuild.CodeTable.ScriptUpdates[] GetFileDataForCodeTableUpdates(ref SqlSyncBuildData buildData, string projFileName)
		{
			if(buildData == null)
				return null;
			
			ArrayList scriptFiles = new ArrayList();
			foreach(SqlBuild.SqlSyncBuildData.ScriptRow row in buildData.Script)
			{
				//Find the ".pop" populate scripts
				if(Path.GetExtension(row.FileName).ToLower() != SqlSync.Constants.DbObjectType.PopulateScript.ToLower())
					continue;

				SqlBuild.CodeTable.ScriptUpdates obj = GetFileDataForCodeTableUpdates(row.FileName,projFileName);
				if(obj != null)
                    scriptFiles.Add(obj);
 			}

			SqlBuild.CodeTable.ScriptUpdates[] updates = new SqlSync.SqlBuild.CodeTable.ScriptUpdates[scriptFiles.Count];
			scriptFiles.CopyTo(updates);
			return updates;
		}
		public static SqlBuild.CodeTable.ScriptUpdates GetFileDataForCodeTableUpdates(string baseFileName, string projFileName)
		{
			string line = string.Empty;

			//Open the populate script file
			string localFile = Path.GetDirectoryName(projFileName)+@"\"+Path.GetFileName(baseFileName);
			if(File.Exists(localFile) == false)
				return null;

			SqlBuild.CodeTable.ScriptUpdates codeTableUpdate = new SqlSync.SqlBuild.CodeTable.ScriptUpdates();
			codeTableUpdate.ShortFileName = baseFileName;

			using(StreamReader sr = File.OpenText(localFile))
			{
				bool keepReading = true;
				while((line = sr.ReadLine()) != null && keepReading == true)
				{
					if(line.Trim().StartsWith("Source Server:"))
						codeTableUpdate.SourceServer = line.Replace("Source Server:","").Trim();

					if(line.Trim().StartsWith("Source Db:"))
						codeTableUpdate.SourceDatabase = line.Replace("Source Db:","").Trim();

					if(line.Trim().StartsWith("Table Scripted:"))
						codeTableUpdate.SourceTable = line.Replace("Table Scripted:","").Trim();
						
					if(line.Trim().StartsWith("Key Check Columns:"))
						codeTableUpdate.KeyCheckColumns = line.Replace("Key Check Columns:","").Trim();

					//This is a multi-line element
					if(line.Trim().StartsWith("Query Used:"))
					{
						string queryLine = string.Empty;
						string fullquery = string.Empty;
						while((queryLine = sr.ReadLine().Trim()) != "*/")
						{
							fullquery += queryLine+"\r\n";
						}
						codeTableUpdate.Query = fullquery;
						keepReading = false;
					}
				}
				sr.Close();
			}
			return codeTableUpdate;
		}

        public static SqlBuild.Objects.ObjectUpdates[] GetFileDataForObjectUpdates(ref SqlSyncBuildData buildData, string projFileName)
		{
            List<SqlBuild.Objects.ObjectUpdates> canUpdate;
            List<string> canNotUpdate;

            GetFileDataForObjectUpdates(ref buildData, projFileName, out canUpdate, out canNotUpdate);
            if (canUpdate != null)
                return canUpdate.ToArray();
            else
                return null;
		}
        public static void GetFileDataForObjectUpdates(ref SqlSyncBuildData buildData, string projFileName, out List<SqlBuild.Objects.ObjectUpdates> canUpdate, out  List<string> canNotUpdate)
        {
            if (buildData == null)
            {
                canUpdate = null;
                canNotUpdate = null;
                return;
            }

            canUpdate = new List<Objects.ObjectUpdates>();
            canNotUpdate = new List<string>();

            foreach (SqlBuild.SqlSyncBuildData.ScriptRow row in buildData.Script)
            {
                //Find the database objects that can be updated...SP, View, UDF, Trigger
                if (Path.GetExtension(row.FileName).ToUpper() != SqlSync.Constants.DbObjectType.StoredProcedure &&
                    Path.GetExtension(row.FileName).ToUpper() != SqlSync.Constants.DbObjectType.View &&
                    Path.GetExtension(row.FileName).ToUpper() != SqlSync.Constants.DbObjectType.UserDefinedFunction &&
                    Path.GetExtension(row.FileName).ToUpper() != SqlSync.Constants.DbObjectType.Trigger)
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
			string localFile = Path.GetDirectoryName(projFileName)+@"\"+Path.GetFileName(baseFilename);
			if(File.Exists(localFile) == false)
				return null;

			SqlBuild.Objects.ObjectUpdates objectUpdate = new SqlBuild.Objects.ObjectUpdates();
			objectUpdate.ShortFileName = baseFilename;

			using(StreamReader sr = File.OpenText(localFile))
			{
				
				bool keepReading = true;
				while((line = sr.ReadLine()) != null && keepReading == true)
				{
					if(line.Trim().StartsWith("Source Server:"))
						objectUpdate.SourceServer = line.Replace("Source Server:","").Trim();

                    if (line.Trim().StartsWith("Source Db:"))
                    {
                        objectUpdate.SourceDatabase = line.Replace("Source Db:", "").Trim();
                    }

					if(line.Trim().StartsWith("Object Scripted:"))
						objectUpdate.SourceObject = line.Replace("Object Scripted:","").Trim();
						
					if(line.Trim().StartsWith("Object Type:"))
					{
						objectUpdate.ObjectType = line.Replace("Object Type:","").Trim();
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
			if( ZipHelper.AppendZipPackage(logFileName,basePath,destinationArchiveName,false))
			{
				for(int i=0;i<logFileName.Length;i++)
				{
					try
					{
						File.Delete(basePath +@"\"+logFileName[i]);
					}
					catch{}
				}
				return true;
			}else
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

	    public static string CalculateSha1HashFromPackage(string buildPackageName)
	    {
	        SqlSyncBuildData buildData = null;

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
	                LoadSqlBuildProjectFile(out buildData, projFileName, false);
	                break;
	            case ".sbx":
	                projectFilePath = Path.GetDirectoryName(buildPackageName);
	                LoadSqlBuildProjectFile(out buildData, buildPackageName, false);
	                break;
	            default:
	                return string.Empty;
	        }

	        if (buildData != null)
	        {
	            string hash = CalculateBuildPackageSHA1SignatureFromPath(projectFilePath, buildData);
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
        /// Calculated the SHA1 hash of the script package. Takes the build order into account 
        /// </summary>
        /// <param name="projectFileExtractionPath">Path where all of the script files are to be founw</param>
        /// <param name="buildData">The SqlSyncBuildData build config data the contains the script names and build order</param>
        /// <returns></returns>
        public static string CalculateBuildPackageSHA1SignatureFromPath(string projectFileExtractionPath, SqlSyncBuildData buildData)
	    {
	        
            if (buildData != null && !string.IsNullOrEmpty((projectFileExtractionPath)))
            {
                if (!projectFileExtractionPath.EndsWith(@"\'")) projectFileExtractionPath = projectFileExtractionPath + @"\"; 

                IEnumerable<SqlSyncBuildData.ScriptRow> row = from x in buildData.Script.AsEnumerable()
                                                              orderby x.BuildOrder ascending
                                                              select x;



                StringBuilder sb = new StringBuilder();
                string fileHash, textHash;
                foreach (SqlSyncBuildData.ScriptRow r in row)
                {
                    GetSHA1Hash(projectFileExtractionPath + r.FileName, out fileHash, out textHash, r.StripTransactionText);
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
                GetSHA1Hash(batch.ScriptBatchContents, out textHash);
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
		public static void GetSHA1Hash(string pathName, out string fileHash, out string textHash,bool stripTransactions)
		{
			string strHashData = "";

			byte[] arrbytHashValue;

			System.Security.Cryptography.SHA1CryptoServiceProvider oSHA1Hasher=
				new System.Security.Cryptography.SHA1CryptoServiceProvider();

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
                string[] split = SqlBuildHelper.ReadBatchFromScriptFile(pathName, stripTransactions, false);
                GetSHA1Hash(split, out textHash);
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

        public static string JoinBatchedScripts(string[] batchedScripts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < batchedScripts.Length - 1; i++)
            {
                if (batchedScripts[i].EndsWith("\r\n"))
                {
                    sb.Append(batchedScripts[i] + BatchParsing.Delimiter + "\r\n");
                }
                else
                {
                    sb.Append(batchedScripts[i] + "\r\n" + BatchParsing.Delimiter + "\r\n");
                }
            }
            sb.Append(batchedScripts[batchedScripts.Length - 1]);
            return sb.ToString();
        }
        /// <summary>
        /// Gets the file hash of a file that has been split into batch scripts using the SqlBuildHelper.ReadBatchFromScriptFile or SqlBuildHelper.ReadBatchFromScriptText method
        /// </summary>
        /// <param name="batchedScriptLines">Batched Sql script</param>
        /// <param name="textHash">SHA1 hash of the batched script.</param>
        public static void GetSHA1Hash(string[] batchedScriptLines, out string textHash)
        {
            string scriptText = JoinBatchedScripts(batchedScriptLines);
            textHash = GetSHA1Hash(scriptText);
        }
        /// <summary>
        /// Computes the SHA1 hash of a string. Should only be used on string recomposed from batching
        /// </summary>
        /// <param name="textContents"></param>
        /// <returns></returns>
        internal static string GetSHA1Hash(string textContents)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider oSHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            byte[] textBytes =  new ASCIIEncoding().GetBytes(textContents);
            byte[] arrbytHashValue = oSHA1Hasher.ComputeHash(textBytes);
            string textHash = System.BitConverter.ToString(arrbytHashValue);
            textHash =  textHash.Replace("-", "");
            return textHash;
        }
        #endregion

        #region .: Renumbering/ Resorting :.
        public static bool RenumberBuildSequence(ref SqlSyncBuildData buildData,string projectFileName,string buildZipFileName)
		{
			return RenumberBuildSequence(ref buildData, projectFileName, buildZipFileName,(int)ResequenceIgnore.StartNumber);
		}
		internal static bool RenumberBuildSequence(ref SqlSyncBuildData buildData,string projectFileName,string buildZipFileName, int renumberIgnoreStart)
		{
			try
			{
				buildData.Script.AcceptChanges();
				DataView view = buildData.Script.DefaultView;
				view.Sort = buildData.Script.BuildOrderColumn.ColumnName +" ASC";
				view.RowFilter = buildData.Script.BuildOrderColumn.ColumnName +" < "+renumberIgnoreStart.ToString();
				view.RowStateFilter = DataViewRowState.OriginalRows;
				for(int i=0;i<view.Count;i++)
				{
					((SqlSyncBuildData.ScriptRow)view[i].Row).BuildOrder = i+1;
				}
				view.Table.AcceptChanges();
				SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData,projectFileName, buildZipFileName);
				return true;
			}
			catch(Exception e)
			{
				string error = e.ToString();
				return false;
			}

		}
		public static bool ResortBuildByFileType(ref SqlSyncBuildData buildData, string projectFileName,string buildZipFileName)
		{

			// first move out the "reserve" items (renumber starting at 20000)
			buildData.Script.AcceptChanges();
			int reservedIndex = 20000;
			DataView reserved =  buildData.Script.DefaultView;
			reserved.RowFilter = buildData.Script.BuildOrderColumn.ColumnName +" >= "+((int)ResequenceIgnore.StartNumber).ToString();
			reserved.Sort = buildData.Script.BuildOrderColumn.ColumnName +" ASC";
			reserved.RowStateFilter = DataViewRowState.OriginalRows;
			for(int i=0;i<reserved.Count;i++)
				((SqlSyncBuildData.ScriptRow)reserved[i].Row).BuildOrder = reservedIndex++;
			reserved.Table.AcceptChanges();
			buildData.AcceptChanges();
			
			//renumber the standard items
			string generalFilter = buildData.Script.FileNameColumn.ColumnName +" LIKE '%{0}' AND "+ buildData.Script.BuildOrderColumn.ColumnName+"< 20000";
			string notLikeFilter = " "+buildData.Script.FileNameColumn.ColumnName +" NOT LIKE '%{0}' AND";
			System.Text.StringBuilder sbNotLike = new System.Text.StringBuilder();
			for(int i=0;i<ResortBuildType.SortOrder.Length;i++)
			{
				string extension = "."+ResortBuildType.SortOrder[i];
				sbNotLike.Append(string.Format(notLikeFilter,extension));
				
				DataView general =  buildData.Script.DefaultView;
				general.RowFilter = string.Format(generalFilter,extension);
				general.Sort = buildData.Script.BuildOrderColumn.ColumnName +" ASC";
				general.RowStateFilter = DataViewRowState.OriginalRows;

				for(int j=0;j<general.Count;j++)
					((SqlSyncBuildData.ScriptRow)general[j].Row).BuildOrder = (1000 *(i+1))+j;

				general.Table.AcceptChanges();
			}
			

			//Take any of the remaining items
			sbNotLike.Length = sbNotLike.Length - 3;
			int leftOverStart = 19000;
			DataView leftOver =  buildData.Script.DefaultView;
			leftOver.RowFilter = sbNotLike.ToString();
			leftOver.Sort = buildData.Script.BuildOrderColumn.ColumnName +" ASC";
			leftOver.RowStateFilter = DataViewRowState.OriginalRows;
			for(int i=0;i<leftOver.Count;i++)
				((SqlSyncBuildData.ScriptRow)leftOver[i].Row).BuildOrder = leftOverStart++;

			buildData.Script.AcceptChanges();
			SaveSqlBuildProjectFile(ref buildData,projectFileName,buildZipFileName);

			RenumberBuildSequence(ref buildData,projectFileName,buildZipFileName,19999);
			RenumberBuildSequence(ref buildData,projectFileName,buildZipFileName);
			return true;
		}
        #endregion

     

        public static bool ScriptRequiresBuildDescription(string scriptContents)
        {
            if (scriptContents == null || scriptContents.Length == 0)
                return false;


            if (scriptContents.IndexOf(SqlBuild.ScriptTokens.BuildDescription, 1, StringComparison.CurrentCultureIgnoreCase) > -1)
                return true;
            else
                return false;

            //try
            //{
            //    if (File.Exists(pathName))
            //    {
            //        if (File.ReadAllText(pathName).IndexOf(SqlBuild.ScriptTokens.BuildDescription, 1, StringComparison.CurrentCultureIgnoreCase) > -1)
            //            return true;

            //    }
            //}
            //catch
            //{
            //}
            //return false;
        }


        #region .: Updating from Legacy code :.
        public static void ConvertLegacyProjectHistory(ref SqlSyncBuildData buildData, string projFilePath, string zipFileName)
		{
			if(buildData.Builds.Count == 0 || buildData.Build.Count == 0)
				return;

			string buildHistoryXmlFileName = projFilePath + XmlFileNames.HistoryFile;
			if(File.Exists(buildHistoryXmlFileName))
				return;
		
			//Save project file as the history file
			buildData.WriteXml(buildHistoryXmlFileName);

			//Load up as new data object
			SqlSyncBuildData buildHistData = new SqlSyncBuildData();
			buildHistData.ReadXml(buildHistoryXmlFileName);

			//Remove the committed scripts element
			for(int i=0;i<buildHistData.CommittedScript.Count;i++)
			{
				buildHistData.CommittedScript[i].Delete();
				buildHistData.AcceptChanges();
			}

			//remove the build data element
			for(int i=0;i<buildHistData.Scripts.Count;i++)
			{
				buildHistData.Scripts[i].Delete();
				buildHistData.AcceptChanges();
			}
			buildHistData.WriteXml(buildHistoryXmlFileName);

			for(int i=0;i<buildData.Builds.Count;i++)
			{
				buildData.Builds[i].Delete();
				buildData.AcceptChanges();
			}

			PackageProjectFileIntoZip(buildData,projFilePath,zipFileName);

			
		}
        public static bool UpdateObsoleteXmlNamespace(string fileName)
        {
            log.DebugFormat("Updating XmlNamespace from legacy file '{0}'", fileName);

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
                log.InfoFormat("Successfully updated the XmlNamespace in file {0}", fileName);
            else
                log.InfoFormat("Unable to update the XmlNamespace in file {0}", fileName);
            return replaced;

        }
        #endregion

        #region .: Copying scripts out to plain files :.
        public static bool CopyIndividualScriptsToFolder(ref SqlSyncBuildData buildData, string destinationFolder, string projectFilePath,bool includeUSE,bool includeSequence)
		{
			if(buildData.Script == null || buildData.Script.Count == 0)
				return false;

			StringBuilder sb = new StringBuilder();
			string[] batch;
			string fileName = string.Empty;
			
			try
			{
				DataView view =  buildData.Script.DefaultView;
				view.Sort = buildData.Script.BuildOrderColumn.ColumnName + " ASC";
				for(int i=0;i<view.Count;i++)
				{
					SqlSync.SqlBuild.SqlSyncBuildData.ScriptRow row = (SqlSync.SqlBuild.SqlSyncBuildData.ScriptRow)view[i].Row;
					if(!File.Exists(projectFilePath+row.FileName))
						continue;

					if(includeUSE)
						sb.Append("USE "+row.Database+"\r\nGO\r\n");

					batch = SqlBuildHelper.ReadBatchFromScriptFile(projectFilePath+row.FileName,row.StripTransactionText,true);
					for(int j=0;j<batch.Length;j++)
						sb.Append(batch[j]+"\r\n");
	
					if(includeSequence)
						fileName = destinationFolder+@"\"+(i+1).ToString().PadLeft(3,'0')+" "+row.FileName;
					else
						fileName = destinationFolder+@"\"+row.FileName;

					
					using(StreamWriter sw = File.CreateText(fileName))
					{
						sw.WriteLine(sb.ToString());
						sw.Flush();
						sw.Close();
					}
					sb.Length = 0;
				}
				return true;
			}
			catch(Exception e)
			{
                log.Error(String.Format("Unable to export script {0} to destination folder {1}", fileName, destinationFolder), e);
				return false;
			}
		}
		public static bool CopyScriptsToSingleFile(ref SqlSyncBuildData buildData, string destinationFile, string projectFilePath, string buildFileName, bool includeUSE)
		{
			if(buildData.Script == null || buildData.Script.Count == 0)
				return false;

			StringBuilder sb = new StringBuilder();
			string[] batch;
		
			try
			{
				sb.Append("-- Scripts Consolidated from: "+ Path.GetFileName(buildFileName)+"\r\n");
				DataView view =  buildData.Script.DefaultView;
				view.Sort = buildData.Script.BuildOrderColumn.ColumnName + " ASC";
				for(int i=0;i<view.Count;i++)
				{
					SqlSync.SqlBuild.SqlSyncBuildData.ScriptRow row = (SqlSync.SqlBuild.SqlSyncBuildData.ScriptRow)view[i].Row;
					if(!File.Exists(projectFilePath+row.FileName))
						continue;

					sb.Append("\r\n-- Source File: "+row.FileName+"\r\n");
					if(includeUSE)
						sb.Append("USE "+row.Database+"\r\nGO\r\n");
					batch = SqlBuildHelper.ReadBatchFromScriptFile(projectFilePath+row.FileName,row.StripTransactionText,true);
					for(int j=0;j<batch.Length;j++)
						sb.Append(batch[j]+"\r\n");
				}

					
				using(StreamWriter sw = File.CreateText(destinationFile))
				{
					sw.WriteLine(sb.ToString());
					sw.Flush();
					sw.Close();
				}
				sb.Length = 0;
				return true;
			}
			catch(Exception e)
			{
				string debug = e.ToString();
				return false;
			}
		}
        #endregion

        //public static double ImportSqlScriptFile
        public static double ImportSqlScriptFile(ref SqlSyncBuildData buildData, SqlSyncBuildData importData, string importWorkingDirectory, double lastBuildNumber, string projectFilePath, string projectFileName, string buildZipFileName, bool cleanUp, out string[] addedFileNames)
        {
            bool haveImportedRows = false;
            double startBuildNumber = lastBuildNumber + 1;
            ArrayList list = new ArrayList();
            try
            {
                importData.AcceptChanges();
                int increment = 0;
                DataView importView = importData.Script.DefaultView;
                importView.Sort = "BuildOrder  ASC";
                importView.RowStateFilter = DataViewRowState.OriginalRows;

                if (importView.Count > 0)
                    haveImportedRows = true;

                for (int i = 0; i < importView.Count; i++)
                {
                    SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)importView[i].Row;

                    row.BuildOrder = startBuildNumber + increment;
                    list.Add(row.FileName);
                    buildData.Script.ImportRow(row);
                    if (File.Exists(projectFilePath + row.FileName))
                        File.Delete(projectFilePath + row.FileName);
                    try
                    {
                        File.Copy(importWorkingDirectory + row.FileName, projectFilePath + row.FileName);
                    }
                    catch(Exception)
                    {
                        System.Threading.Thread.Sleep(200);
                        File.Copy(importWorkingDirectory + row.FileName, projectFilePath + row.FileName);
                    }
                    increment++;
                }

                buildData.AcceptChanges();
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
                addedFileNames = new string[list.Count];
                list.CopyTo(addedFileNames);
                if (cleanUp)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(importWorkingDirectory);
                        for (int i = 0; i < files.Length; i++)
                            File.Delete(files[i]);

                        Directory.Delete(importWorkingDirectory);
                    }
                    catch
                    {
                    }
                }

                if (haveImportedRows)
                    return startBuildNumber;
                else
                    return (double)ImportFileStatus.NoRowsImported;

            }
            catch (Exception e)
            {
                string error = e.ToString();
                buildData.RejectChanges();
            }

        

            addedFileNames = new string[0];
            return -1;
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

        public static List<double>  GetInsertedIndexValues(double floorIndex, double ceilingIndex, int insertCount)
        {
            List<double> insertIndexes = new List<double>();
            //Get the closest integer values if possible...
            if(Math.Ceiling(floorIndex) < Math.Floor(ceilingIndex))
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
            if(floorIndex + Convert.ToDouble(insertCount)*0.1 < ceilingIndex)
            {
                for (double i = 1; i <= insertCount; i++)
                    insertIndexes.Add(floorIndex + i/10.0);

                return insertIndexes;
            }

            //Can't use whole values, so let's do some math!!
            double step = (ceilingIndex - floorIndex) / (insertCount+2);
            for (double i = 1; i <= insertCount; i++)
            {
                double tmp = Math.Round(floorIndex + (i * step),2);
                insertIndexes.Add(tmp);
            }

            return insertIndexes;
        }
       
        #region .: Files/Path Initilization Methods :.
        public static bool CleanUpAndDeleteWorkingDirectory(string workingDir)
		{
			try
			{
                if(Directory.Exists(workingDir))
                    Directory.Delete(workingDir, true);
				return true;
			}
			catch(Exception exe)
			{
                log.Warn("Unable to clean up working directory '" + workingDir + "'", exe);
				return false;
			}
		}
        public static bool InitilizeWorkingDirectory(ref string workingDirectory,ref string projectFilePath, ref string projectFileName)
        {
            try
            {
                if (workingDirectory != null)
                {
                    CleanUpAndDeleteWorkingDirectory(workingDirectory);
                    string tmpDir = System.IO.Path.GetTempPath();
                    workingDirectory = tmpDir + @"Sqlsync-" + System.Guid.NewGuid().ToString().Replace("-", "") + @"\";
                }

                projectFilePath = workingDirectory;

                if (projectFileName != null && projectFileName.Length > 0)
                    projectFileName = workingDirectory + Path.GetFileName(projectFileName);
                
                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);

                log.DebugFormat("Successfully created working directory at '{0}'", workingDirectory);

                return true;
            }
            catch(Exception exe)
            {
                log.Warn("Unable to clean up working directory '" + workingDirectory + "'", exe);
                return false;
            }
        }
        #endregion
    }
}
