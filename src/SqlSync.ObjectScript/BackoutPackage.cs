using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.Connection;
using System.IO;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Objects;
using System.ComponentModel;
using SqlSync.Constants;
using SqlSync.DbInformation;
using Microsoft.Extensions.Logging;
namespace SqlSync.ObjectScript
{
    public class BackoutPackage
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool CreateBackoutPackage(ConnectionData connData,
                                                List<SqlBuild.Objects.ObjectUpdates> objectUpdates,
                                                List<SqlBuild.Objects.ObjectUpdates> dontUpdate,
                                                List<string> manualScriptsCanNotUpdate, string sourceBuildZipFileName,
                                                string destinationBuildZipFileName, string sourceServer, string sourceDb,
                                                bool removeNewObjectsFromPackage, bool markManualScriptsAsRunOnce,
                                                bool dropNewRoutines, ref BackgroundWorker bg)
        {
            //Copy the source straight over...
            bool reportProgress = false;
            if (bg != null && bg.WorkerReportsProgress)
            {
                reportProgress = true;
                bg.ReportProgress(-1, "Copying package to destination...");
            }

            if (!CopyOriginalToBackout(sourceBuildZipFileName, destinationBuildZipFileName))
                return false;

            //init working location for destination backout package
            string workingDir = string.Empty;
            string projectPath = string.Empty;
            string projectFileName = string.Empty;

            SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDir, ref projectPath, ref projectFileName);
            string message = $"Initialized working directory {workingDir}";
            log.LogDebug(message);
            if (reportProgress) 
            { 
                bg.ReportProgress(-1, "Initialized working directory");
            }


            //Extract destination package into working folder
            string result;
            bool success = SqlBuildFileHelper.ExtractSqlBuildZipFile(destinationBuildZipFileName, ref workingDir,
                                                                     ref projectPath, ref projectFileName, out result);
            if (success)
            {
                log.LogDebug($"Successfully extracted build file {destinationBuildZipFileName} to {workingDir}");
            }
            else
            {
                log.LogError("Unable to proceed with Backout package. See previous errors");
                return false;
            }

            //Load the build data 
            SqlSyncBuildData buildData;
            if (reportProgress) bg.ReportProgress(-1, "Loading project file for modification.");
            bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projectFileName, false);
            if (!successfulLoad)
            {
                log.LogError("Unable to load SBM project data");
                return false;
            }

            ConnectionData tmpData = new ConnectionData();
            tmpData.DatabaseName = sourceDb;
            tmpData.SQLServerName = sourceServer;
            tmpData.Password = connData.Password;
            tmpData.UserId = connData.UserId;
            tmpData.AuthenticationType = connData.AuthenticationType;
            ObjectScriptHelper helper = new ObjectScriptHelper(tmpData);


            //Get the updated scripts...
            if (reportProgress) bg.ReportProgress(-1, "Generating updated scripts.");
            List<UpdatedObject> lstScripts = ObjectScriptHelper.ScriptDatabaseObjects(objectUpdates, tmpData, ref bg);

            //Log if some scripts were not updated properly...
            var notUpdated = from s in objectUpdates
                             where !(from u in lstScripts select s.ShortFileName).Contains(s.ShortFileName)
                             select s.ShortFileName;

            if (notUpdated.Any())
            {
                foreach (string file in notUpdated)
                    log.LogError($"Unable to create new script for {file}");

                return false;
            }

            if (lstScripts.Count() != objectUpdates.Count())
            {
                log.LogError($"Not all scripts were updated. Expected {lstScripts.Count().ToString()}, only {objectUpdates.Count().ToString()} were updated");
                return false;
            }

            //Save the updated scripts...
            DateTime updateTime = DateTime.Now;
            bool errorWriting = false;
            if (lstScripts != null)
            {
                foreach (UpdatedObject obj in lstScripts)
                {
                    try
                    {
                        File.WriteAllText(projectPath + obj.ScriptName, obj.ScriptContents);

                        //Update the buildData object with the update date/time and user;
                        var sr = from r in buildData.Script
                                 where r.FileName == obj.ScriptName
                                 select r;

                        if (sr.Any())
                        {
                            SqlSyncBuildData.ScriptRow row = sr.First();
                            row.DateModified = updateTime;
                            row.ModifiedBy = System.Environment.UserName;
                        }

                    }
                    catch (Exception exe)
                    {
                        errorWriting = true;
                        log.LogError(exe, $"Unable to save updated script file to {obj.ScriptName}");
                    }
                }
            }

            //Update new object scripts: either remove or mark as run once
            if (dontUpdate != null)
            {
                foreach (SqlBuild.Objects.ObjectUpdates obj in dontUpdate)
                {
                    try
                    {
                        //Update the buildData object with the update date/time and user;
                        var sr = from r in buildData.Script
                                 where r.FileName == obj.ShortFileName
                                 select r;

                        if (sr.Any())
                        {
                            SqlSyncBuildData.ScriptRow row = sr.First();
                            if (obj.ObjectType == DbScriptDescription.StoredProcedure ||
                                obj.ObjectType == DbScriptDescription.UserDefinedFunction ||
                                obj.ObjectType == DbScriptDescription.Trigger ||
                                obj.ObjectType == DbScriptDescription.View)
                            {
                                if (dropNewRoutines)
                                {
                                    string schema, objName;
                                    string[] arr = obj.SourceObject.Split(new char[] {'.'});
                                    schema = arr[0];
                                    objName = arr[1];

                                    string str = CreateRoutineDropScript(schema, objName, obj.ObjectType);
                                    File.WriteAllText(projectPath + "DROP " + row.FileName, str);
                                    row.FileName = "DROP " + row.FileName;
                                    row.DateModified = updateTime;
                                    row.ModifiedBy = System.Environment.UserName;
                                }
                                else
                                {
                                    row.AllowMultipleRuns = false;
                                    row.DateModified = updateTime;
                                    row.ModifiedBy = System.Environment.UserName;
                                }
                            }
                            else if (removeNewObjectsFromPackage)
                            {
                                buildData.Script.RemoveScriptRow(row);
                            }
                            else
                            {
                                row.AllowMultipleRuns = false;
                                row.DateModified = updateTime;
                                row.ModifiedBy = System.Environment.UserName;
                            }
                        }

                    }
                    catch (Exception exe)
                    {
                        errorWriting = true;
                        if (removeNewObjectsFromPackage)
                        {
                            log.LogError(exe, $"Unable to remove new object script '{obj.ShortFileName}' from package");
                        }
                        else
                        {
                            log.LogError(exe, $"Unable to mark new object script {obj.ShortFileName} as run once");
                        }
                    }
                }
            }

            //Mark non-updated scripts as run-once
            if (manualScriptsCanNotUpdate != null)
            {
                foreach (string scr in manualScriptsCanNotUpdate)
                {
                    try
                    {
                        //Update the buildData object with the update date/time and user;
                        var sr = from r in buildData.Script
                                 where r.FileName == scr
                                 select r;

                        if (sr.Any())
                        {
                            if (markManualScriptsAsRunOnce)
                            {
                                SqlSyncBuildData.ScriptRow row = sr.First();
                                if (row.BuildOrder < 1000)
                                {
                                    row.AllowMultipleRuns = false;
                                    row.DateModified = updateTime;
                                    row.ModifiedBy = System.Environment.UserName;
                                }
                            }
                        }

                    }
                    catch (Exception exe)
                    {
                        errorWriting = true;
                        log.LogError(exe, $"Unable to mark script {scr} as run once");
                    }
                }
            }


            if (errorWriting)
                return false;
            if (reportProgress) bg.ReportProgress(-1, "Saving backout package.");

            buildData.AcceptChanges();
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, destinationBuildZipFileName);


            return true;
        }



        /// <summary>
        /// This method should only really be used with a command line, unattended execution.
        /// </summary>
        /// <param name="connData"></param>
        /// <param name="objectUpdates"></param>
        /// <param name="dontUpdate"></param>
        /// <param name="manualScriptsCanNotUpdate"></param>
        /// <param name="sourceBuildZipFileName"></param>
        /// <param name="sourceServer"></param>
        /// <param name="sourceDb"></param>
        /// <returns></returns>
        public static string CreateDefaultBackoutPackage(ConnectionData connData, string sourceBuildZipFileName, string sourceServer, string sourceDb)
        {
            /*How to create a backout package:
             * 
             * 1. Extract the package and load the BuildData
             * 2. Get the list of sriptable objects and manually created scripts
             * 3. Targeting your "old" source, and see what scriptable objects are not there (i.e. are "new" in the package)
             * 
             */
            List<string> manualScriptsCanNotUpdate;
            List<ObjectUpdates> initialCanUpdateList;
            string workingDirectory = string.Empty;
            string projectFilePath = string.Empty;
            string projectFileName = string.Empty;
            string result;
            SqlSyncBuildData buildData;
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;

            //Extract and load the build data...
            log.LogDebug($"Extracting SBM zip file for {sourceBuildZipFileName}");
            bool success = SqlBuildFileHelper.ExtractSqlBuildZipFile(sourceBuildZipFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, out result);
            if (success)
            {
                log.LogDebug($"Loading SqlSyncBuldData object from {projectFileName}");
                success = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projectFileName, false);
                if (!success)
                    return string.Empty;
            }
            else
            {
                return string.Empty;
            }

            //Get the scriptable objects
            log.LogDebug($"Getting the scriptable objects from {sourceBuildZipFileName}");
            SqlBuildFileHelper.GetFileDataForObjectUpdates(ref buildData, projectFileName, out initialCanUpdateList, out manualScriptsCanNotUpdate);

            //Get object that are also on the target (ie are "existing") -- only these will be updated
            log.LogDebug($"Getting list of objects can be rolled back from {sourceServer}:{sourceDb}");
            List<ObjectUpdates> canUpdate = GetObjectThatCanBeUpdated(initialCanUpdateList, connData, sourceServer, sourceDb);
            SetBackoutSourceDatabaseAndServer(ref canUpdate,sourceServer,sourceDb);

            //Get the scriptable objects that are not found on the target (i.e. are "new") -- these will be dropped
            log.LogDebug($"Getting list of objects can not be rolled back from {sourceServer}:{sourceDb}");
            List<ObjectUpdates> notPresentOnTarget = GetObjectsNotPresentTargetDatabase(initialCanUpdateList, connData, sourceServer, sourceDb);

            //Get the name of the new package
            string backoutPackageName = GetDefaultPackageName(sourceBuildZipFileName);
          
            //Create the package!!
            log.LogDebug($"Creating backout package {backoutPackageName} from source package {sourceBuildZipFileName}");
            success = CreateBackoutPackage(connData, canUpdate, notPresentOnTarget, manualScriptsCanNotUpdate,
                                                sourceBuildZipFileName, backoutPackageName,
                                                sourceServer, sourceDb, true, true, true, ref bg);

            //Cleanup all the temp files created
            SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory);

            if (!success)
            {
                log.LogError("Unable to create backout package!");
                return string.Empty;
            }

            return backoutPackageName;
        }

        #region Helper Methods
        /// <summary>
        /// Creates the default name of the backout package based on the provided intial package name
        /// </summary>
        /// <param name="sourceSbmFullFileName">Full name for the SBM package, including its path</param>
        /// <returns>Backout package name</returns>
        public static string GetDefaultPackageName(string sourceSbmFullFileName)
        {
            return Path.Combine(Path.GetDirectoryName(sourceSbmFullFileName) , "Backout_" + Path.GetFileName(sourceSbmFullFileName));
        }

        /// <summary>
        /// Creates a list of ObjectUpdate objects that are present in the incoming "master list" that are not present in the target database and server.
        /// i.e. Finds the "new objects" that are in the package
        /// </summary>
        /// <param name="masterList">A list of scriptable objects that are found in the base package</param>
        /// <param name="connData">The connection data used for the intial package</param>
        /// <param name="newServer">The database server name that we are going to check for the presence of these scriptable objects</param>
        /// <param name="newDatabase">The database name that we are going to check for the presence of these scriptable objects</param>
        /// <returns>A List of Objects that are in the master list, but not in the target database (i.e. are "new" objects)</returns>
        public static List<SqlBuild.Objects.ObjectUpdates> GetObjectsNotPresentTargetDatabase(List<SqlBuild.Objects.ObjectUpdates> masterList, ConnectionData connData, string newServer,string newDatabase)
        {
            List<SqlBuild.Objects.ObjectUpdates> notPresent = new List<SqlBuild.Objects.ObjectUpdates>();
            ConnectionData tmp = new ConnectionData();
            tmp.Fill(connData);
            tmp.DatabaseName = newDatabase;
            tmp.SQLServerName = newServer;

            List<ObjectData> lstAllScriptAble = new List<ObjectData>();
            lstAllScriptAble.AddRange(InfoHelper.GetStoredProcedureList(tmp));
            lstAllScriptAble.AddRange(InfoHelper.GetViewList(tmp));
            lstAllScriptAble.AddRange(InfoHelper.GetFunctionList(tmp));
            lstAllScriptAble.AddRange(InfoHelper.GetTriggerObjectList(tmp));

            var x = from m in masterList
                    where
                        !(from s in lstAllScriptAble select s.SchemaOwner + "." + s.ObjectName).Contains(m.SourceObject)
                    select m;

            return x.ToList();
        }

        public static List<SqlBuild.Objects.ObjectUpdates> GetObjectThatCanBeUpdated(List<SqlBuild.Objects.ObjectUpdates> masterList, ConnectionData connData, string newServer, string newDatabase)
        {
            List<ObjectUpdates> currentTargetCanUpdateList = new List<ObjectUpdates>();
            List<ObjectUpdates> notPresentOnTarget = BackoutPackage.GetObjectsNotPresentTargetDatabase(masterList,connData,newServer, newDatabase);
            if (notPresentOnTarget.Count > 0)
            {
                var cur = from i in masterList
                          where !(from n in notPresentOnTarget select n.SourceObject).Contains(i.SourceObject)
                          select i;

                if (cur.Any())
                    currentTargetCanUpdateList = cur.ToList();
            }
            else
            {
                currentTargetCanUpdateList.AddRange(masterList);
            }

            return currentTargetCanUpdateList;
        }

        private static bool CopyOriginalToBackout(string sourceBuildZipFileName, string destinationBuildZipFileName)
        {
            log.LogDebug($"Copying SBM from '{sourceBuildZipFileName}' to '{destinationBuildZipFileName}'");
            try
            {
                File.Copy(sourceBuildZipFileName, destinationBuildZipFileName, true);
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Unable to copy SBM from '{sourceBuildZipFileName}' to '{destinationBuildZipFileName}'");
                return false;
            }
            return true;
        }

        public static void SetBackoutSourceDatabaseAndServer(ref List<ObjectUpdates> scriptableObjectList, string serverName, string databaseName)
        {
            foreach (ObjectUpdates script in scriptableObjectList)
            {
                script.SourceDatabase = databaseName;
                script.SourceServer = serverName;
            }
        }
        #endregion

        #region Drop script scripting methods
        public static string CreateRoutineDropScript(string schemaName, string objectName, string objectDecsription)
        {
            switch (objectDecsription)
            {
                case DbScriptDescription.StoredProcedure:
                    return CreateStoredProcedureDropScript(schemaName, objectName);
                case DbScriptDescription.UserDefinedFunction:
                    return CreateFunctionDropScript(schemaName, objectName);
                case DbScriptDescription.Trigger:
                    return CreateTriggerDropScript(schemaName, objectName);
                case DbScriptDescription.View:
                    return CreateViewDropScript(schemaName, objectName);
                default:
                    return string.Empty;
            }
        }
        private static string CreateStoredProcedureDropScript(string schemaName, string storedProcName)
        {
            string tmp = @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [{0}].[{1}]
GO";
            return string.Format(tmp, schemaName, storedProcName);

        }
        private static string CreateFunctionDropScript(string schemaName, string functionName)
        {
            string tmp = @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
    DROP FUNCTION [{0}].[{1}]
GO";

            return string.Format(tmp, schemaName, functionName);
        }
        private static string CreateTriggerDropScript(string schemaName, string tableAndTriggerName)
        {
            string triggerName = tableAndTriggerName.Split('-')[1].Trim();
            string tmp = @"IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}]'))
    DROP TRIGGER [{0}].[{1}]
GO";

            return string.Format(tmp, schemaName, triggerName);
        }
        private static string CreateViewDropScript(string schemaName, string viewName)
        {
            string tmp = @"IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[{0}].[{1}]'))
    DROP VIEW [{0}].[{1}]
GO
";

            return string.Format(tmp, schemaName, viewName);
        }
        #endregion
    }


}
