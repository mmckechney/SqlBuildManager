using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.Connection;
using System.IO;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Objects;
using SqlSync;
using System.ComponentModel;
using SqlSync.Constants;
namespace SqlSync.ObjectScript
{
    public class BackoutPackage
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool CreateBackoutPackage(ConnectionData connData, List<SqlBuild.Objects.ObjectUpdates> objectUpdates, List<SqlBuild.Objects.ObjectUpdates> dontUpdate, List<string> manualScriptsCanNotUpdate, string sourceBuildZipFileName, string destinationBuildZipFileName, string sourceServer, string sourceDb, bool removeNewObjectsFromPackage, bool markManualScriptsAsRunOnce, bool dropNewRoutines, ref BackgroundWorker bg)
        {
            //Copy the source straight over...
            bool reportProgress = false;
            if (bg != null && bg.WorkerReportsProgress)
            {
                reportProgress = true;
                bg.ReportProgress(-1, "Copying package to destination...");
            }

            log.DebugFormat("Copying SBM from '{0}' to '{1}'", sourceBuildZipFileName, destinationBuildZipFileName);
            try
            {
                File.Copy(sourceBuildZipFileName, destinationBuildZipFileName, true);
            }
            catch (Exception exe)
            {
                log.Error(String.Format("Unable to copy SBM from '{0}' to '{1}'", sourceBuildZipFileName, destinationBuildZipFileName), exe);
                return false;
            }

            //init working location for destination backout package
            string workingDir = string.Empty;
            string projectPath = string.Empty;
            string projectFileName = string.Empty;

            SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDir, ref projectPath, ref projectFileName);
            string message = "Initialized working directory {0}";
            log.DebugFormat(message, workingDir);
            if (reportProgress) bg.ReportProgress(-1, String.Format(message, ""));


            //Extract destination package into working folder
            string result;
            bool success = SqlBuildFileHelper.ExtractSqlBuildZipFile(destinationBuildZipFileName, ref workingDir, ref projectPath, ref projectFileName, out result);
            if (success)
            {
                log.DebugFormat("Successfully extracted build file {0} to {1}", destinationBuildZipFileName, workingDir);
            }
            else
            {
                log.Error("Unable to proceed with Backout package. See previous errors");
                return false;
            }

            //Load the build data 
            SqlSyncBuildData buildData;
            if (reportProgress) bg.ReportProgress(-1, "Loading project file for modification.");
            bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projectFileName, false);
            if (!successfulLoad)
            {
                log.Error("Unable to load SBM project data");
                return false;
            }

            ConnectionData tmpData = new ConnectionData();
            tmpData.DatabaseName = sourceDb;
            tmpData.SQLServerName = sourceServer;
            tmpData.Password = connData.Password;
            tmpData.UserId = connData.UserId;
            tmpData.UseWindowAuthentication = connData.UseWindowAuthentication;
            ObjectScriptHelper helper = new ObjectScriptHelper(tmpData);


            //Get the updated scripts...
            if (reportProgress) bg.ReportProgress(-1, "Generating updated scripts.");
            List<UpdatedObject> lstScripts = ObjectScriptHelper.ScriptDatabaseObjects(objectUpdates, tmpData, ref bg);

            //Log if some scripts were not updated properly...
            var notUpdated = from s in objectUpdates
                             where !(from u in lstScripts select s.ShortFileName).Contains(s.ShortFileName)
                             select s.ShortFileName;

            if (notUpdated.Count() > 0)
            {
                foreach (string file in notUpdated)
                    log.ErrorFormat("Unable to create new script for {0}", file);

                return false;
            }

            if(lstScripts.Count() != objectUpdates.Count())
            {
                log.ErrorFormat("Not all scripts were updated. Expected {0}, only {1} were updated", lstScripts.Count().ToString(), objectUpdates.Count().ToString());
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

                        if (sr.Count() > 0)
                        {
                            SqlSyncBuildData.ScriptRow row = sr.First();
                            row.DateModified = updateTime;
                            row.ModifiedBy = System.Environment.UserName;
                        }

                    }
                    catch (Exception exe)
                    {
                        errorWriting = true;
                        log.Error("Unable to save updated script file to " + obj.ScriptName, exe);
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

                        if (sr.Count() > 0)
                        {
                            SqlSyncBuildData.ScriptRow row = sr.First();
                            if (obj.ObjectType == DbScriptDescription.StoredProcedure || obj.ObjectType == DbScriptDescription.UserDefinedFunction ||
                                obj.ObjectType == DbScriptDescription.Trigger || obj.ObjectType == DbScriptDescription.View)
                            {
                                if (dropNewRoutines)
                                {
                                    string schema, objName;
                                    string[] arr = obj.SourceObject.Split(new char[] { '.' });
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
                            log.Error(String.Format("Unable to remove new object script '{0}' from package", obj.ShortFileName, exe));
                        }
                        else
                        {
                            log.Error(String.Format("Unable to mark new object script {0} as run once", obj.ShortFileName, exe));
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

                        if (sr.Count() > 0)
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
                        log.Error(String.Format("Unable to mark script {0} as run once", scr, exe));
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



        



    }
}
