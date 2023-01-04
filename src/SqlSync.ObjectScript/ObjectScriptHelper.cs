using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.ObjectScript.Hash;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
namespace SqlSync.ObjectScript
{
    /// <summary>
    /// Summary description for ObjectScriptHelper.
    /// </summary>
    public class ObjectScriptHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private BackgroundWorker bgWorker;
        /// <summary>
        /// SMO server interface variable
        /// </summary>
        Microsoft.SqlServer.Management.Smo.Server smoServer = null;
        /// <summary>
        /// SMO database object
        /// </summary>
        private Microsoft.SqlServer.Management.Smo.Database smoDatabase = null;
        /// <summary>
        /// Used for comparison of database objects to scripted files. 
        /// </summary>
        Hashtable dbObjects;
        /// <summary>
        /// Flag on whether or not to delete any pre-existing script files in the 
        /// selected path for a initial script
        /// </summary>
        private bool fullScriptWithDelete = false;
        /// <summary>
        /// flag on whether or not to combine all table objects into one file
        /// (inc. FK, PK, indexes as well as table definition)
        /// </summary>
        private bool combineTableObjects = true;
        /// <summary>
        /// Flag on whether or not to zip results when scripting is complete
        /// </summary>
        private bool zipScripts = false;
        private bool scriptPkWithTable = true;

        public string ScriptHeader(DateTime processedDate, string objectSchemaAndName, string objectTypeDesc, bool includePermissions, bool scriptAsAlter, bool scriptPkWithTable)
        {
            string[] split = objectSchemaAndName.Split(new char[] { '.' });
            return ScriptHeader(ConnData.SQLServerName, ConnData.DatabaseName, processedDate, split[0], (split.Length == 1) ? "" : split[1], objectTypeDesc, System.Environment.UserName, includePermissions, scriptAsAlter, scriptPkWithTable);
        }
        public string ScriptHeader(string objectName, string schemaOwner, string objectTypeDesc, bool includePermissions, bool scriptAsAlter, bool scriptPkWithTable)
        {
            return ScriptHeader(ConnData.SQLServerName, ConnData.DatabaseName, DateTime.Now, schemaOwner, objectName, objectTypeDesc, System.Environment.UserName, includePermissions, scriptAsAlter, scriptPkWithTable);
        }
        public static string ScriptHeader(string sourceServer, string sourceDatabase, DateTime processedDate, string schemaOwner, string objectName, string objectTypeDesc, string scriptedBy, bool includePermissions, bool scriptAsAlter, bool scriptPkWithTable)
        {
            string scriptHeader =
                     "/* \r\n" +
                     "Source Server:\t{0}\r\n" +
                     "Source Db:\t{1}\r\n" +
                     "Process Date:\t{2}\r\n" +
                     "Object Scripted:{3}.{4}\r\n" +
                     "Object Type:\t{5}\r\n" +
                     "Scripted By:\t{6}\r\n" +
                     "Include Permissions: {7}\r\n" +
                     "Script as ALTER: {8}\r\n" +
                     "Script PK with Table:{9}\r\n*/\r\n";

            return string.Format(scriptHeader, sourceServer, sourceDatabase, processedDate.ToString(), schemaOwner, objectName, objectTypeDesc, scriptedBy, includePermissions.ToString(), scriptAsAlter.ToString(), scriptPkWithTable.ToString());

        }
        /// <summary>
        /// Data Transfer object of required data
        /// </summary>
        private ConnectionData data = null;
        /// <summary>
        /// Whether or not to include the informational file header
        /// </summary>
        private bool includeFileHeader = true;
        private bool scriptAsAlter = false;

        public bool ScriptAsAlter
        {
            get { return scriptAsAlter; }
            set { scriptAsAlter = value; }
        }
        private bool includePermissions = false;

        public bool IncludePermissions
        {
            get { return includePermissions; }
            set { includePermissions = value; }
        }
        public ConnectionData ConnData
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
        public ObjectScriptHelper(ConnectionData data)
        {
            ConnData = data;
        }
        public ObjectScriptHelper(ConnectionData data, bool scriptAsAlter, bool includePermissions, bool scriptPkWithTable) : this(data)
        {
            this.includePermissions = includePermissions;
            this.scriptAsAlter = scriptAsAlter;
            this.scriptPkWithTable = scriptPkWithTable;
        }


        #region ## Processing Existing Scripts in File System ##
        public void ProcessScripts(BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            this.bgWorker = bgWorker;
            bgWorker.ReportProgress(0, new StatusEventArgs("Starting Script Processing"));

            //Connect to server or quit
            if (ConnectToServer() == false)
                return;

            //Get reference to starting directory
            DirectoryInfo dirInf = new DirectoryInfo(data.StartingDirectory);

            //Make sure directory exists. If not, send message and quit.
            if (dirInf.Exists == false)
            {
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs("Invalid Starting Directory", "Error", "", true));
                return;
            }
            ProcessDirectory(dirInf);
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Complete. Ready"));
            DisconnectServer();
        }

        private void ProcessScriptsThreaded()
        {


        }
        public void ProcessDirectory(DirectoryInfo dirInf)
        {
            //Send up a status message
            string dirMessage = (dirInf.FullName.Length > 100) ? ".." + dirInf.FullName.Substring(dirInf.FullName.Length - 98, 98) : dirInf.FullName;
            bgWorker.ReportProgress(0, new StatusEventArgs("Processing Directory: " + dirMessage));

            //Get the list of files in this directory
            FileInfo[] fileInf = dirInf.GetFiles();
            string script = string.Empty;
            string objectType = string.Empty;
            string message;
            bool success = false;
            for (int i = 0; i < fileInf.Length; i++)
            {
                string schemaOwner;
                string name = Path.GetFileNameWithoutExtension(fileInf[i].Name);
                InfoHelper.ExtractNameAndSchema(name, out name, out schemaOwner);

                //Send message update
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileInf[i].Name, "Searching", fileInf[i].FullName, true));

                success = ScriptDatabaseObject(fileInf[i].Extension.ToUpper(), name, schemaOwner, ref script, ref objectType, out message);

                //Save if found
                if (success)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileInf[i].Name, "Scripted", "", false));
                    SaveScriptToFile(script, name, objectType, fileInf[i].Name, fileInf[i].FullName, true, true);
                }
                else
                {
                    //Send failure message
                    switch (fileInf[i].Extension.ToUpper())
                    {
                        case DbObjectType.ForeignKey:
                        case DbObjectType.KeysAndIndexes:
                        case DbObjectType.StoredProcedure:
                        case DbObjectType.Table:
                        case DbObjectType.Trigger:
                        case DbObjectType.UserDefinedFunction:
                        case DbObjectType.View:
                        case DbObjectType.ServerLogin:
                        case DbObjectType.DatabaseUser:
                            bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileInf[i].Name, "Object not in Db", "", false));
                            break;
                        default:
                            bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileInf[i].Name, "Not a Db script type", "", false));
                            break;
                    }
                }
                success = false;
                objectType = string.Empty;
            }

            DirectoryInfo[] subDirs = dirInf.GetDirectories();
            for (int i = 0; i < subDirs.Length; i++)
            {
                ProcessDirectory(subDirs[i]);
            }

        }

        #endregion

        public static List<SqlSync.SqlBuild.Objects.UpdatedObject> ScriptDatabaseObjects(List<SqlSync.SqlBuild.Objects.ObjectUpdates> objectsToUpdate, ConnectionData basicConnData)
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            return ScriptDatabaseObjects(objectsToUpdate, basicConnData, ref bg);
        }
        public static List<SqlSync.SqlBuild.Objects.UpdatedObject> ScriptDatabaseObjects(List<SqlSync.SqlBuild.Objects.ObjectUpdates> objectsToUpdate, ConnectionData basicConnData, ref BackgroundWorker bg)
        {
            ObjectScriptHelper helper = new ObjectScriptHelper(basicConnData);
            List<SqlSync.SqlBuild.Objects.UpdatedObject> lstScripts = new List<SqlBuild.Objects.UpdatedObject>();
            foreach (SqlSync.SqlBuild.Objects.ObjectUpdates objUpdate in objectsToUpdate)
            {
                bg.ReportProgress(-1, String.Format("Updating {0} from {1}", objUpdate.SourceObject, objUpdate.SourceServer));

                ConnectionData tmpData = new ConnectionData();
                tmpData.DatabaseName = objUpdate.SourceDatabase;
                tmpData.SQLServerName = objUpdate.SourceServer;
                tmpData.Password = basicConnData.Password;
                tmpData.UserId = basicConnData.UserId;
                tmpData.AuthenticationType = basicConnData.AuthenticationType;
                helper.ConnData = tmpData;
                helper.ScriptAsAlter = objUpdate.ScriptAsAlter;
                helper.IncludePermissions = objUpdate.IncludePermissions;
                string script = string.Empty;

                string name = objUpdate.SourceObject;
                string schemaOwner; ;
                InfoHelper.ExtractNameAndSchema(name, out name, out schemaOwner);

                string objectType = objUpdate.ObjectType;
                log.LogDebug($"Updating object: {objUpdate.ShortFileName} from {tmpData.SQLServerName}.{tmpData.DatabaseName}");

                bool success = helper.ScriptDatabaseObjectWithHeader(Path.GetExtension(objUpdate.ShortFileName).ToUpper(), name, schemaOwner, ref script, ref objectType);

                if (script.Length > 0 && success)
                {
                    SqlBuild.Objects.UpdatedObject scr = new SqlBuild.Objects.UpdatedObject(objUpdate.ShortFileName, script);
                    lstScripts.Add(scr);
                }
            }
            return lstScripts;
        }

        public bool ScriptDatabaseObject(string dbObjectType, string objectName, string schemaOwner, ref string script, ref string objectTypeDesc, out string message)
        {

            //Try to connect to the server, if already connected, it's handled ok
            if (!ConnectToServer())
            {
                log.LogError($"ScriptDatabaseObject - Failed to Connect to SQL Server '{ConnData.SQLServerName}' / Database '{ConnData.DatabaseName}'");
                message = "Failed to Connect to SQL Server.";
                return false;
            }

            //Search for file type
            bool success = false;
            if (CheckForExclusion(objectName, DbObjectType.DatabaseUser, ref script))
            {
                message = string.Empty;
                return false;
            }
            switch (dbObjectType)
            {
                case DbObjectType.StoredProcedure: //Stored Proc
                    success = ScriptStoredProcedure(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.StoredProcedure;
                    break;
                case DbObjectType.View: //View
                    success = ScriptView(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.View;
                    break;
                case DbObjectType.Table: //Table
                    success = ScriptTable(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.Table;
                    break;
                case DbObjectType.KeysAndIndexes: //Key Constraint, default and Index
                    success = ScriptKeysAndIndex(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.KeysAndIndexes;
                    break;
                case DbObjectType.ForeignKey: //Foreign Key
                    success = ScriptForeignKeys(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.ForeignKey;
                    break;
                case DbObjectType.UserDefinedFunction:
                    success = ScriptUserDefinedFunctions(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.UserDefinedFunction;
                    break;
                case DbObjectType.DatabaseUser:
                    success = ScriptDatabaseUsers(objectName, ref script, out message);
                    objectTypeDesc = DbScriptDescription.DatabaseUser;
                    break;
                case DbObjectType.ServerLogin:
                    success = ScriptServerLogin(objectName, ref script, out message);
                    objectTypeDesc = DbScriptDescription.ServerLogin;
                    break;
                case DbObjectType.DatabaseRole:
                    success = ScriptDatabaseRole(objectName, ref script, out message);
                    objectTypeDesc = DbScriptDescription.DatabaseRole;
                    break;
                case DbObjectType.DatabaseSchema:
                    success = ScriptDatabaseSchema(objectName, ref script, out message);
                    objectTypeDesc = DbScriptDescription.DatabaseSchema;
                    break;
                case DbObjectType.Trigger:
                    success = ScriptTrigger(objectName, schemaOwner, ref script, out message);
                    objectTypeDesc = DbScriptDescription.Trigger;
                    break;
                default:
                    message = "Unable to find object type " + dbObjectType.ToString();
                    log.LogWarning($"ScriptDatabaseObject problem: {message}");
                    success = false;
                    break;

            }

            return success;
        }

        public bool ScriptDatabaseObjectWithHeader(string dbObjectType, string objectName, string schemaOwner, ref string script, ref string objectTypeDesc)
        {

            string message;
            bool success = ScriptDatabaseObject(dbObjectType, objectName, schemaOwner, ref script, ref objectTypeDesc, out message);

            if (script.Length == 0)
                return false;

            string header = ObjectScriptHelper.ScriptHeader(ConnData.SQLServerName, ConnData.DatabaseName, DateTime.Now, schemaOwner, objectName, objectTypeDesc, System.Environment.UserName, includePermissions, scriptAsAlter, scriptPkWithTable);
            StringBuilder sb = new StringBuilder(header);
            sb.Append(script);
            script = sb.ToString();
            return success;
        }
        public void SaveScriptToFile(string script, string objectName, string objectType, string shortFileName, string fullFileName, bool includeHeader, bool reportObjectStatus)
        {
            FileData data = new FileData(script, objectName, objectType, shortFileName, fullFileName, includeHeader, reportObjectStatus);
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveScriptToFileThreaded), data);
        }

        private void SaveScriptToFileThreaded(object fileData)
        {
            FileData data = (FileData)fileData;
            try
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(data.FullFileName, false))
                    {
                        if (data.IncludeFileHeader)
                            writer.Write(ScriptHeader(DateTime.Now, data.ObjectName, data.ObjectType, includePermissions, scriptAsAlter, scriptPkWithTable));
                        writer.Write(data.Script);
                        writer.Flush();
                        writer.Close();
                    }

                    if (data.ReportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(data.ShortFileName, "New Script Saved", "", false));
                    //return true;

                }
                catch
                {
                    if (data.ReportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(data.ShortFileName, "Failed to save", "", false));
                    //return false;
                }
            }
            catch (InvalidOperationException opE)
            {
                System.Diagnostics.EventLog.WriteEntry("SqlSync", "Failed to report progress in 'SaveScriptToFileThreaded' for " + data.ShortFileName + "(" + data.FullFileName + ")\r\n" + opE.ToString(), System.Diagnostics.EventLogEntryType.Error, 328);
            }
        }
        private class FileData
        {
            public readonly string Script;
            public readonly string ObjectName;
            public readonly string ObjectType;
            public readonly string ShortFileName;
            public readonly string FullFileName;
            public readonly bool IncludeFileHeader;
            public readonly bool ReportObjectStatus;
            public FileData(string script, string objectName, string objectType, string shortFileName, string fullFileName, bool includeFileHeader, bool reportObjectStatus)
            {
                Script = script;
                ObjectName = objectName;
                ObjectType = objectType;
                ShortFileName = shortFileName;
                FullFileName = fullFileName;
                IncludeFileHeader = includeFileHeader;
                ReportObjectStatus = reportObjectStatus;
            }
        }
        private bool CheckForExclusion(string objectName, string objectType, ref string script)
        {
            switch (objectType)
            {
                case DbObjectType.DatabaseUser:
                    if (objectName == "dbo")
                    {
                        script = string.Empty;
                        return true;
                    }
                    break;
                case DbObjectType.ServerLogin:
                    if (objectName == "sa" || objectName.IndexOf("BUILTIN", StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        script = string.Empty;
                        return true;
                    }
                    break;
            }
            return false;
        }

        #region ## Full Scripting ## 
        //public void ProcessFullScripting(bool withDelete,bool combineTableObjects,bool zipScripts, bool includeFileHeader)
        public void ProcessFullScripting(ObjectScriptingConfigData cfgData, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {

            bool reportObjectStatus = cfgData.ReportStatusPerObject;
            fullScriptWithDelete = cfgData.WithDelete;
            combineTableObjects = cfgData.CombineTableObjects;
            includeFileHeader = cfgData.IncludeFileHeader;
            zipScripts = cfgData.ZipScripts;
            this.bgWorker = bgWorker;
            bgWorker.ReportProgress(0, new StatusEventArgs("Starting Script Processing"));

            //Connect to server or quit
            if (ConnectToServer() == false)
            {
                return;
            }

            //Create the needed directories
            InitializeDirectories(fullScriptWithDelete);


            string tmpScript = string.Empty;
            string fileName = string.Empty;
            string fullPath = string.Empty;
            string unused = string.Empty;
            string message;

            //Script Tables
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Tables"));
            string dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.Table);
            for (int i = 1; i < smoDatabase.Tables.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Table obj = smoDatabase.Tables[i];
                fileName = obj.Schema + "." + obj.Name + DbObjectType.Table;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }

                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.Table, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.Table, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

                if (combineTableObjects == false)
                {
                    fileName = obj.Schema + "." + obj.Name + DbObjectType.KeysAndIndexes;
                    fullPath = Path.Combine(dir, fileName);
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                    if (ScriptDatabaseObject(DbObjectType.KeysAndIndexes, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                    {
                        SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.KeysAndIndexes, fileName, fullPath, includeFileHeader, reportObjectStatus);
                    }
                    else
                    {
                        if (reportObjectStatus)
                            bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                        if (message.Length > 0)
                            bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                    }


                    fileName = obj.Schema + "." + obj.Name + DbObjectType.ForeignKey;
                    fullPath = Path.Combine(dir, fileName);
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                    if (ScriptDatabaseObject(DbObjectType.ForeignKey, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                    {
                        SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.ForeignKey, fileName, fullPath, includeFileHeader, reportObjectStatus);
                    }
                    else
                    {
                        if (reportObjectStatus)
                            bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                        if (message.Length > 0)
                            bgWorker.ReportProgress(-1, new StatusEventArgs(message));

                    }
                }
            }

            //Script user defined functions
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting User Defined Functions"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.UserDefinedFunction);
            for (int i = 1; i < smoDatabase.UserDefinedFunctions.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                UserDefinedFunction obj = smoDatabase.UserDefinedFunctions[i];
                fileName = obj.Schema + "." + obj.Name + DbObjectType.UserDefinedFunction;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.UserDefinedFunction, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.UserDefinedFunction, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));


                }

            }

            //Script Stored Procedures
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Stored Procedures"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.StoredProcedure);
            for (int i = 1; i < smoDatabase.StoredProcedures.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                StoredProcedure obj = smoDatabase.StoredProcedures[i];
                fileName = obj.Schema + "." + obj.Name + DbObjectType.StoredProcedure;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.StoredProcedure, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.StoredProcedure, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));

                }

            }

            //Script views
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Views"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.View);
            for (int i = 1; i < smoDatabase.Views.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                View obj = smoDatabase.Views[i];
                fileName = obj.Schema + "." + obj.Name + DbObjectType.View;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.View, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.View, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

            }

            //Script database users
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Database Users"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseUser);
            for (int i = 1; i < smoDatabase.Users.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                User obj = smoDatabase.Users[i];
                fileName = obj.Name.Replace(@"\", "-") + DbObjectType.DatabaseUser;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.DatabaseUser, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.DatabaseUser, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

            }

            //Script server logins
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Server Logins"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.ServerLogin);
            for (int i = 1; i < smoServer.Logins.Count; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Login obj = smoServer.Logins[i];
                fileName = obj.Name.Replace(@"\", "-") + DbObjectType.ServerLogin;
                fullPath = Path.Combine(dir, fileName);
                if (obj.IsSystemObject)
                {
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(obj.Name, "Skipping SysObj", fullPath, true));
                    continue;
                }
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.ServerLogin, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.ServerLogin, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

            }

            //Script database roles
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Database Roles"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseRoles);
            for (int i = 0; i < smoDatabase.Roles.Count; i++)
            {
                DatabaseRole obj = smoDatabase.Roles[i];
                fileName = obj.Name.Replace(@"\", "-") + DbObjectType.DatabaseRole;
                fullPath = Path.Combine(dir, fileName);
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.DatabaseRole, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.DatabaseRole, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

            }

            //Script database schema
            bgWorker.ReportProgress(0, new StatusEventArgs("Scripting Database Schemas"));
            dir = Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseSchemas);
            for (int i = 0; i < smoDatabase.Schemas.Count; i++)
            {
                Schema obj = smoDatabase.Schemas[i];
                fileName = obj.Name.Replace(@"\", "-") + DbObjectType.DatabaseSchema;
                fullPath = Path.Combine(dir, fileName);
                bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Scripting", fullPath, true));
                if (ScriptDatabaseObject(DbObjectType.DatabaseSchema, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    SaveScriptToFile(tmpScript, obj.Name, DbScriptDescription.DatabaseSchema, fileName, fullPath, includeFileHeader, reportObjectStatus);
                }
                else
                {
                    if (reportObjectStatus)
                        bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(fileName, "Excluded", string.Empty, false));

                    if (message.Length > 0)
                        bgWorker.ReportProgress(-1, new StatusEventArgs(message));
                }

            }

            if (zipScripts)
            {
                ZipScripts();
            }

            bgWorker.ReportProgress(100, new StatusEventArgs("Scripting Complete. Ready."));
        }

        private void ZipScripts()
        {
            bgWorker.ReportProgress(0, new StatusEventArgs("Zipping up Script Files."));

            //Put together the base file name
            StringBuilder sb = new StringBuilder(data.DatabaseName + "-" + data.SQLServerName.Replace(@"\", "_") + " ");
            sb.Append(DateTime.Now.Year.ToString() + ".");
            sb.Append(DateTime.Now.Month.ToString().PadLeft(2, '0') + ".");
            sb.Append(DateTime.Now.Day.ToString().PadLeft(2, '0') + " at ");
            sb.Append(DateTime.Now.Hour.ToString().PadLeft(2, '0') + ".");
            sb.Append(DateTime.Now.Minute.ToString().PadLeft(2, '0') + ".zip");

            ArrayList fileList = new ArrayList();
            GetRecursiveFiles(data.StartingDirectory, ref fileList);
            string[] files = new string[fileList.Count];
            fileList.CopyTo(files);

            SqlBuild.ZipHelper.CreateZipPackage(files, data.StartingDirectory, Path.Combine(data.StartingDirectory, sb.ToString()));


        }
        private void GetRecursiveFiles(string startingDirectory, ref ArrayList filesList)
        {
            string[] files = Directory.GetFiles(startingDirectory);
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]).IndexOf("zip", StringComparison.CurrentCultureIgnoreCase) == -1)
                {
                    filesList.Add(files[i].Replace(data.StartingDirectory, ""));
                }
            }
            string[] directories = Directory.GetDirectories(startingDirectory);
            for (int i = 0; i < directories.Length; i++)
            {
                GetRecursiveFiles(directories[i], ref filesList);
            }
        }
        private void InitializeDirectories(bool withDelete)
        {
            bgWorker.ReportProgress(0, new StatusEventArgs("Initializing Directories"));
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.Table)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.Table));
            }

            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.View)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.View));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.StoredProcedure)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.StoredProcedure));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.UserDefinedFunction)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.UserDefinedFunction));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseUser)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseUser));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.ServerLogin)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.ServerLogin));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseRoles)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseRoles));
            }
            if (Directory.Exists(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseSchemas)) == false)
            {
                Directory.CreateDirectory(Path.Combine(data.StartingDirectory, DbObjectFilePath.DatabaseSchemas));
            }
            if (withDelete)
            {
                DeletePreExistingScriptFiles(data.StartingDirectory);
            }
        }
        private void DeletePreExistingScriptFiles(string rootPath)
        {
            string path;
            bgWorker.ReportProgress(0, new StatusEventArgs("Removing pre-existing files"));
            System.Reflection.FieldInfo[] extensionInfo = typeof(DbObjectType).GetFields();
            System.Reflection.FieldInfo[] pathInfo = typeof(DbObjectFilePath).GetFields();
            for (int x = 0; x < pathInfo.Length; x++)
            {
                path = Path.Combine(rootPath, pathInfo[x].GetValue(null).ToString());
                bgWorker.ReportProgress(0, new StatusEventArgs("Removing pre-existing files in " + path));
                for (int j = 0; j < extensionInfo.Length; j++)
                {
                    string[] files = Directory.GetFiles(path, "*" + extensionInfo[j].GetValue(null));
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            File.Delete(files[i]);
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion

        #region ## Script Hashing ##
        private string GetHash(ref MD5 oMD5Hasher, string script)
        {
            string textHash;
            byte[] arrbytHashValue;
            byte[] textBytes = new ASCIIEncoding().GetBytes(script);
            arrbytHashValue = oMD5Hasher.ComputeHash(textBytes);
            textHash = System.BitConverter.ToString(arrbytHashValue);
            return textHash.Replace("-", "");
        }
        public ObjectScriptHashData GetDatabaseObjectHashes()
        {

            var oMD5Hasher = System.Security.Cryptography.MD5.Create();
            ObjectScriptHashData hashData = new ObjectScriptHashData();

            combineTableObjects = false;
            includeFileHeader = false;
            //Connect to server or quit
            if (ConnectToServer() == false)
            {
                return null;
            }

            string tmpScript = string.Empty;
            string objectName = string.Empty;
            string unused = string.Empty;
            string message;
            string scriptHash;
            //Script Tables

            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Tables"));

            for (int i = 1; i < smoDatabase.Tables.Count; i++)
            {
                Table obj = smoDatabase.Tables[i];
                objectName = obj.Schema + "." + obj.Name + DbObjectType.Table;
                if (obj.IsSystemObject)
                    continue;

                if (ScriptDatabaseObject(DbObjectType.Table, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Tables.Add(obj.Schema + "." + obj.Name, scriptHash, "Added");
                }


                if (combineTableObjects == false)
                {
                    objectName = obj.Schema + "." + obj.Name + DbObjectType.KeysAndIndexes;
                    if (ScriptDatabaseObject(DbObjectType.KeysAndIndexes, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                    {
                        //TODO: Add to collection
                        scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    }
                    else


                        objectName = obj.Schema + "." + obj.Name + DbObjectType.ForeignKey;
                    if (ScriptDatabaseObject(DbObjectType.ForeignKey, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                    {
                        //TODO: Add to collection
                        scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    }

                }
            }

            //Script user defined functions
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Functions"));


            for (int i = 1; i < smoDatabase.UserDefinedFunctions.Count; i++)
            {
                UserDefinedFunction obj = smoDatabase.UserDefinedFunctions[i];
                if (obj.IsSystemObject)
                    continue;
                if (ScriptDatabaseObject(DbObjectType.UserDefinedFunction, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Functions.Add(obj.Schema + "." + obj.Name, scriptHash, "Added");
                }
            }

            //Script Stored Procedures
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Stored Procedures"));

            for (int i = 1; i < smoDatabase.StoredProcedures.Count; i++)
            {
                StoredProcedure obj = smoDatabase.StoredProcedures[i];
                if (obj.IsSystemObject)
                    continue;
                if (ScriptDatabaseObject(DbObjectType.StoredProcedure, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.StoredProcedures.Add(obj.Schema + "." + obj.Name, scriptHash, "Added");
                }
            }

            //Script views
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Views"));

            for (int i = 1; i < smoDatabase.Views.Count; i++)
            {
                View obj = smoDatabase.Views[i];
                if (obj.IsSystemObject)
                    continue;
                if (ScriptDatabaseObject(DbObjectType.View, obj.Name, obj.Schema, ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Views.Add(obj.Schema + "." + obj.Name, scriptHash, "Added");
                }
            }

            //Script database users
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Users"));

            for (int i = 1; i < smoDatabase.Users.Count; i++)
            {
                User obj = smoDatabase.Users[i];
                if (obj.IsSystemObject)
                    continue;
                if (ScriptDatabaseObject(DbObjectType.DatabaseUser, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Users.Add(obj.Name, scriptHash, "Added");
                }
            }

            //Script server logins
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Logins"));


            for (int i = 1; i < smoServer.Logins.Count; i++)
            {
                Login obj = smoServer.Logins[i];
                if (obj.IsSystemObject)
                    continue;
                if (ScriptDatabaseObject(DbObjectType.ServerLogin, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Logins.Add(obj.Name, scriptHash, "Added");
                }
            }

            //Script database roles
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Roles"));

            for (int i = 0; i < smoDatabase.Roles.Count; i++)
            {
                DatabaseRole obj = smoDatabase.Roles[i];
                if (ScriptDatabaseObject(DbObjectType.DatabaseRole, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Roles.Add(obj.Name, scriptHash, "Added");
                }
            }

            //Script database schema
            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Schemas"));

            for (int i = 0; i < smoDatabase.Schemas.Count; i++)
            {
                Schema obj = smoDatabase.Schemas[i];
                if (ScriptDatabaseObject(DbObjectType.DatabaseSchema, obj.Name, "", ref tmpScript, ref unused, out message))
                {
                    scriptHash = GetHash(ref oMD5Hasher, tmpScript);
                    hashData.Schemas.Add(obj.Name, scriptHash, "Added");
                }
            }

            if (HashScriptingEvent != null)
                HashScriptingEvent(this, new HashScriptingEventArgs("Scripting Complete"));

            return hashData;
        }
        #endregion

        #region ## Server Connect/Disconnect ##
        private bool allowConnectRetry = true;
        private bool ConnectToServer()
        {
            try
            {
                if (smoServer == null)
                {
                    if (data.AuthenticationType == Connection.AuthenticationType.Windows)
                        smoServer = new Microsoft.SqlServer.Management.Smo.Server(ConnData.SQLServerName);
                    else
                        smoServer = new Server(new ServerConnection(ConnData.SQLServerName, ConnData.UserId, ConnData.Password));

                    //To help improve performance, try retrieving all object properties at first retrieval vs. lazy retrieval when read.
                    smoServer.SetDefaultInitFields(true);
                }

                if (smoServer == null)
                    throw new ApplicationException(String.Format("Unable to create SMO connection to Server: {0}", ConnData.SQLServerName));

                if (!smoServer.ConnectionContext.IsOpen)
                    smoServer.ConnectionContext.Connect();




                if (smoDatabase == null || smoDatabase.Name != ConnData.DatabaseName)
                {
                    smoDatabase = smoServer.Databases[ConnData.DatabaseName];
                    //for (int i = 0; i < this.smoServer.Databases.Count; i++)
                    //{
                    //    if (this.smoServer.Databases[i].Name == this.ConnData.DatabaseName)
                    //    {
                    //        this.smoDatabase = this.smoServer.Databases[i];
                    //        break;
                    //    }
                    //}

                    //var d = from Database db in this.smoServer.Databases
                    //        where db.Name.Equals(this.ConnData.DatabaseName, StringComparison.CurrentCultureIgnoreCase)
                    //        select 1;

                    //if (d == null || d.Count() == 0 || d.First() != 1)
                    //{
                    //    log.LogError($"The selected database {this.ConnData.DatabaseName} is not included in the SMO.Databases collection for SMO server {this.smoServer.Name}");
                    //}
                    //else
                    //{
                    //    this.smoDatabase = this.smoServer.Databases[this.ConnData.DatabaseName];
                    //}
                }

                if (smoDatabase == null)
                    throw new ApplicationException(String.Format("Unable to create SMO connection to Database {1} on Server: {0}", ConnData.SQLServerName, ConnData.DatabaseName));

                allowConnectRetry = true;
                return true;
            }
            catch (Microsoft.SqlServer.Management.Common.ExecutionFailureException efe)
            {
                if (allowConnectRetry)
                {
                    log.LogInformation($"Connection retry required for {ConnData.SQLServerName}.{ConnData.DatabaseName}. Message: {efe.Message}");
                    return ConnectToServer();
                }
                else
                {
                    log.LogError(efe, $"SMO Connection failure for {ConnData.SQLServerName}.{ConnData.DatabaseName}");
                    return false;
                }
            }
            catch (Exception exe)
            {

                if (bgWorker != null)
                    bgWorker.ReportProgress(0, new DatabaseScriptEventArgs(string.Format("Failed to Connect to SQL Server '{0}' / Database '{1}'\r\n{2}", ConnData.SQLServerName, ConnData.DatabaseName, exe.ToString()), "Error", "", true));

                log.LogError(exe, $"Failed to Connect to SQL Server '{ConnData.SQLServerName}' / Database '{ConnData.DatabaseName}'");
                return false;
                //exceptions thrown due to inability to connect.
            }
        }

        public bool DisconnectServer()
        {
            try
            {
                if (smoServer != null && smoServer.ConnectionContext.IsOpen)
                    smoServer.ConnectionContext.Disconnect();

                smoServer = null;
                return true;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, "Error disconnecting from SQL Server SMO connection");
                return false;
            }
        }

        #endregion

        #region ## Database to FileSystem Comparison ##
        public ObjectSyncData[] CompareTableObjects(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            bgWorker.ReportProgress(0, new StatusEventArgs("Checking Table Scripts"));
            CompareTableTypes(startingPath, DbObjectType.Table);
            ObjectSyncData[] tables = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(tables, 0);

            //			StatusEvent(this,new StatusEventArgs("Checking Key and Index Scripts"));
            //			CompareTableTypes(startingPath,".KCI");
            //			ObjectSyncData[] keys = new ObjectSyncData[dbObjects.Count];
            //			dbObjects.Values.CopyTo(keys,0);
            //
            //			StatusEvent(this,new StatusEventArgs("Checking Foreign Key Scripts"));
            //			CompareTableTypes(startingPath,DbObjectType.ForeignKey);
            //			ObjectSyncData[] fk = new ObjectSyncData[dbObjects.Count];
            //			dbObjects.Values.CopyTo(fk,0);

            bgWorker.ReportProgress(0, new StatusEventArgs("Combining Data"));
            ArrayList arrList = new ArrayList();
            arrList.AddRange(tables);
            //			arrList.AddRange(keys);
            //			arrList.AddRange(fk);
            ObjectSyncData[] combined = new ObjectSyncData[arrList.Count];
            arrList.CopyTo(combined);

            bgWorker.ReportProgress(0, new StatusEventArgs("Comparison Complete. Ready"));
            return combined;
        }
        private void CompareTableTypes(string startingPath, string extension)
        {
            dbObjects = new Hashtable();
            for (int i = 1; i < smoDatabase.Tables.Count + 1; i++)
            {
                Table tbl = smoDatabase.Tables[i];
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = tbl.Name;
                data.ObjectType = extension;
                data.IsInDatabase = true;
                data.SchemaOwner = tbl.Schema;
                dbObjects.Add(tbl.Schema + "." + tbl.Name + extension, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, extension);


        }
        public ObjectSyncData[] CompareStoredProcs(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            dbObjects = new Hashtable();
            for (int i = 1; i < smoDatabase.StoredProcedures.Count + 1; i++)
            {
                StoredProcedure obj = smoDatabase.StoredProcedures[i];
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = obj.Name;
                data.IsInDatabase = true;
                data.ObjectType = DbObjectType.StoredProcedure;
                data.SchemaOwner = obj.Schema;
                if (!dbObjects.ContainsKey(obj.Schema + "." + obj.Name + DbObjectType.StoredProcedure))
                    dbObjects.Add(obj.Schema + "." + obj.Name + DbObjectType.StoredProcedure, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, DbObjectType.StoredProcedure);

            ObjectSyncData[] myData = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(myData, 0);
            return myData;
        }
        public ObjectSyncData[] CompareViews(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            dbObjects = new Hashtable();
            for (int i = 1; i < smoDatabase.Views.Count + 1; i++)
            {
                View obj = smoDatabase.Views[i];
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = obj.Name;
                data.IsInDatabase = true;
                data.ObjectType = DbObjectType.View;
                data.SchemaOwner = obj.Schema;
                dbObjects.Add(obj.Schema + "." + obj.Name + DbObjectType.View, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, DbObjectType.View);

            ObjectSyncData[] myData = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(myData, 0);
            return myData;

        }
        public ObjectSyncData[] CompareDatabaseUsers(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            dbObjects = new Hashtable();
            for (int i = 1; i < smoDatabase.Users.Count + 1; i++)
            {
                User obj = smoDatabase.Users[i];
                if (obj.Name == "dbo") continue;
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = obj.Name;
                data.IsInDatabase = true;
                data.ObjectType = DbObjectType.DatabaseUser;
                dbObjects.Add(obj.Name + DbObjectType.DatabaseUser, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, DbObjectType.DatabaseUser);

            ObjectSyncData[] myData = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(myData, 0);
            return myData;

        }

        public ObjectSyncData[] CompareUserDefinedFunctions(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            dbObjects = new Hashtable();
            for (int i = 1; i < smoDatabase.UserDefinedFunctions.Count + 1; i++)
            {
                UserDefinedFunction obj = smoDatabase.UserDefinedFunctions[i];
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = obj.Name;
                data.IsInDatabase = true;
                data.ObjectType = DbObjectType.UserDefinedFunction;
                data.SchemaOwner = obj.Schema;
                dbObjects.Add(obj.Schema + "." + obj.Name + DbObjectType.UserDefinedFunction, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, DbObjectType.UserDefinedFunction);

            ObjectSyncData[] myData = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(myData, 0);
            return myData;

        }
        public ObjectSyncData[] CompareServerLogins(string startingPath)
        {
            if (ConnectToServer() == false)
            {
                return null;
            }
            dbObjects = new Hashtable();
            for (int i = 1; i < smoServer.Logins.Count + 1; i++)
            {
                Login obj = smoServer.Logins[i];
                if (obj.Name == "sa" || obj.Name.IndexOf("BUILTIN", StringComparison.CurrentCultureIgnoreCase) > -1) continue;
                ObjectSyncData data = new ObjectSyncData();
                data.ObjectName = obj.Name;
                data.IsInDatabase = true;
                data.ObjectType = DbObjectType.ServerLogin;
                dbObjects.Add(obj.Name + DbObjectType.ServerLogin, data);
            }
            DirectoryInfo dirInf = new DirectoryInfo(startingPath);
            ScanFileSystemForTypes(dirInf, DbObjectType.ServerLogin);

            ObjectSyncData[] myData = new ObjectSyncData[dbObjects.Count];
            dbObjects.Values.CopyTo(myData, 0);
            return myData;

        }


        private void ScanFileSystemForTypes(DirectoryInfo dirInf, string typeExtension)
        {
            FileInfo[] files = dirInf.GetFiles();
            string fileName;
            for (int i = 0; i < files.Length; i++)
            {

                fileName = files[i].Name;
                if (typeExtension == DbObjectType.ServerLogin ||
                    typeExtension == DbObjectType.DatabaseUser)
                {
                    fileName = fileName.Replace("-", @"\");
                }
                if (typeExtension.ToUpper() == files[i].Extension.ToUpper())
                {
                    if (dbObjects.ContainsKey(fileName))
                    {
                        ((ObjectSyncData)dbObjects[fileName]).IsInFileSystem = true;
                        ((ObjectSyncData)dbObjects[fileName]).FileName = files[i].Name;
                        ((ObjectSyncData)dbObjects[fileName]).FullPath = files[i].FullName;
                    }
                    else
                    {
                        ObjectSyncData data = new ObjectSyncData();
                        data.ObjectName = fileName;
                        data.FullPath = files[i].FullName;
                        data.FileName = files[i].Name;
                        data.IsInFileSystem = true;
                        data.ObjectType = typeExtension;
                        dbObjects.Add(fileName, data);
                    }
                }
            }

            DirectoryInfo[] subDirs = dirInf.GetDirectories();
            for (int i = 0; i < subDirs.Length; i++)
            {
                ScanFileSystemForTypes(subDirs[i], typeExtension);
            }
        }



        #endregion

        #region ##Scripting Methods ##

        private bool ScriptTableObjects(string name, string schemaOwner, string dbOjbectType, ref string script, out string message)
        {
            Microsoft.SqlServer.Management.Smo.Table smoTable = smoDatabase.Tables[name, schemaOwner];
            if (smoTable == null)
            {
                message = "Unable to find table '" + schemaOwner + "." + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }
            try
            {
                StringCollection coll;
                StringBuilder sb = new StringBuilder();
                ScriptingOptions options = new ScriptingOptions();
                options.IncludeIfNotExists = true;
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                switch (dbOjbectType)
                {
                    case DbObjectType.Table:
                        //script just the table first
                        options.PrimaryObject = true;
                        coll = smoTable.Script(options);
                        CollateScriptWithSchemaCheck(coll, schemaOwner, ref sb);

                        //now add in the indexes, defaults, etc...
                        if (combineTableObjects)
                        {

                            options.PrimaryObject = false;
                            options.Triggers = false;
                            options.DriIndexes = true;
                            if (scriptPkWithTable)
                            {
                                options.DriPrimaryKey = true;
                            }
                            else
                            {
                                options.DriPrimaryKey = false;
                            }
                            options.DriUniqueKeys = true;
                            options.DriDefaults = true;
                            options.DriForeignKeys = true;
                            options.DriNonClustered = true;
                            options.DriClustered = true;
                            options.ClusteredIndexes = true;
                            options.NonClusteredIndexes = true;
                        }
                        break;
                    case DbObjectType.KeysAndIndexes:
                        options.PrimaryObject = false;
                        options.DriPrimaryKey = true;
                        options.Indexes = true;
                        options.DriUniqueKeys = true;
                        options.DriDefaults = true;
                        options.DriNonClustered = true;
                        options.DriClustered = true;
                        options.ClusteredIndexes = true;
                        options.NonClusteredIndexes = true;
                        break;
                    case DbObjectType.ForeignKey:
                        options.PrimaryObject = false;
                        options.DriForeignKeys = true;
                        break;


                }
                options.AnsiFile = true;
                coll = smoTable.Script(options);
                CollateScriptWithSchemaCheck(coll, schemaOwner, ref sb);
                script = sb.ToString();
                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script table {name}");
                return false;
            }
        }
        private bool ScriptTable(string name, string schemaOwner, ref string script, out string message)
        {
            return ScriptTableObjects(name, schemaOwner, DbObjectType.Table, ref script, out message);
        }
        private bool ScriptTrigger(string name, string schemaOwner, ref string script, out string message)
        {
            string[] tableAndTrigger = name.Split('-');
            if (tableAndTrigger.Length != 2)
            {
                message = String.Format("Unable to determine trigger name from {0}", tableAndTrigger);
                log.LogWarning(message);
                return false;
            }

            string table = tableAndTrigger[0].Trim();
            string trigger = tableAndTrigger[1].Trim();

            Microsoft.SqlServer.Management.Smo.Table smoTable = smoDatabase.Tables[table, schemaOwner];
            if (smoTable == null)
            {
                message = String.Format("Unable to find table '{0}.{1}' for trigger {2} in database  '{3} ' on server '{4}'",
                    schemaOwner,
                    table,
                    trigger,
                    smoServer,
                    smoDatabase);
                log.LogWarning(message);
                return false;
            }
            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.IncludeIfNotExists = true;
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.PrimaryObject = false;
                options.Triggers = true;
                options.AnsiFile = true;
                StringCollection coll = smoTable.Script(options);

                int commandStartIndex = 0;
                int triggerIndex = -1;
                //Find the one trigger that we're looking for in the collection...but we also want all of the setup commands as well
                for (int i = 0; i < coll.Count; i++)
                {

                    if (coll[i].IndexOf(trigger, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                        triggerIndex = i;

                    if (coll[i].IndexOf("IF NOT EXISTS") > -1 && triggerIndex == -1 && i + 1 <= coll.Count)
                        commandStartIndex = i + 1;

                    if (triggerIndex != -1)
                        break;
                }

                StringCollection trigColl = new StringCollection();


                for (int i = commandStartIndex; i < triggerIndex + 1; i++)
                {
                    trigColl.Add(coll[i]);
                }

                //Now we have our script, we need to piece it together with the ALTER/CREATE
                StringBuilder sb = new StringBuilder();
                string trigScript = trigColl[trigColl.Count - 1];
                trigScript = new System.Text.RegularExpressions.Regex(@"CREATE\s+TRIGGER", RegexOptions.IgnoreCase).Replace(trigScript, "ALTER TRIGGER", 1);
                trigScript = new System.Text.RegularExpressions.Regex("IF NOT EXISTS", RegexOptions.IgnoreCase).Replace(trigScript, "IF EXISTS", 1);


                //Start putting it together, first the "SET" directives...
                for (int i = 0; i < trigColl.Count - 1; i++)
                    sb.AppendLine(trigColl[i] + "\r\nGO");

                //Need to insert an extra "BEGIN" after the IF EXISTS, but only the first one - in case the script has them as well..
                string[] trigLines = trigScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool foundOne = false;
                foreach (string line in trigLines)
                {
                    if (line.StartsWith("IF EXISTS") && !foundOne)
                    {
                        sb.AppendLine(line);
                        sb.AppendLine("BEGIN");
                        foundOne = true;
                    }
                    else
                        TabJustifyScript(line, ref sb);
                }

                //sb.AppendLine(trigScript);
                //sb.Length = sb.Length - 5; //Remove "END\r\n"
                sb.AppendLine(String.Format("\tPRINT 'ALTERED Trigger: [{0}].[{1}]'", schemaOwner, trigger));
                sb.AppendLine("END");
                //Script the ELSE ALter
                sb.Append("ELSE\r\nBEGIN\r\n");
                //sb.AppendLine("GO");
                //Next the create
                trigLines = trigColl[trigColl.Count - 1].Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foundOne = false;
                foreach (string line in trigLines)
                {
                    if (line.StartsWith("IF NOT EXISTS") && !foundOne)
                    {
                        sb.AppendLine(line);
                        sb.AppendLine("BEGIN");
                        foundOne = true;
                    }
                    else
                        TabJustifyScript(line, ref sb);
                }
                //sb.AppendLine(coll[coll.Count - 1]);
                //sb.AppendLine("GO");

                //sb.Length = sb.Length - 5;  //remove "END\r\n"
                sb.AppendLine(String.Format("\tPRINT 'CREATED Trigger: [{0}].[{1}]'", schemaOwner, trigger));

                sb.AppendLine("END");
                sb.AppendLine("END");
                sb.AppendLine("GO");

                script = sb.ToString();
                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script trigger {name}");
                return false;
            }
        }
        private bool ScriptKeysAndIndex(string name, string schemaOwner, ref string script, out string message)
        {
            return ScriptTableObjects(name, schemaOwner, DbObjectType.KeysAndIndexes, ref script, out message);
        }
        private bool ScriptForeignKeys(string name, string schemaOwner, ref string script, out string message)
        {
            return ScriptTableObjects(name, schemaOwner, DbObjectType.ForeignKey, ref script, out message);
        }
        internal bool ScriptStoredProcedure(string name, string schemaOwner, ref string script, out string message)
        {
            Microsoft.SqlServer.Management.Smo.StoredProcedure smoSp = smoDatabase.StoredProcedures[name, schemaOwner];
            if (smoSp == null)
            {
                message = "Unable to find Stored Procedure '" + schemaOwner + "." + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.AnsiFile = true;
                StringCollection coll;
                StringBuilder sb = new StringBuilder();
                options.IncludeIfNotExists = true;

                if (!scriptAsAlter)
                {
                    options.ScriptDrops = true;
                    coll = smoSp.Script(options);
                    CollateScript(coll, ref sb);

                    options.IncludeIfNotExists = false;
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    coll = smoSp.Script(options);
                    CollateScript(coll, ref sb);

                }
                else
                {
                    //Script the CREATE if NOT EXISTS
                    Regex regExists = new Regex("^IF NOT EXISTS", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    coll = smoSp.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (regExists.Match(coll[i]).Success) //This should be the actual script
                            TabJustifyScript(coll[i].TrimEnd(), ref sb);
                        else
                            sb.AppendLine(coll[i]); //This shoud be the "SET ..." precursors
                    }

                    sb.Length = sb.Length - 5; //Remove "END\r\n"
                    sb.AppendLine("\tPRINT 'CREATED Stored Procedure: " + schemaOwner + "." + name + "'");
                    sb.AppendLine("END");
                    //Script the ELSE ALter
                    sb.Append("ELSE\r\nBEGIN\r\n");

                    //coll = smoSp.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].IndexOf("CREATE", 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            coll[i] = new System.Text.RegularExpressions.Regex(@"CREATE\s+PROCEDURE", RegexOptions.IgnoreCase).Replace(coll[i], "ALTER PROCEDURE", 1);
                            coll[i] = new System.Text.RegularExpressions.Regex("IF NOT EXISTS", RegexOptions.IgnoreCase).Replace(coll[i], "IF EXISTS", 1);
                            TabJustifyScript(coll[i].TrimEnd(), ref sb);
                        }
                    }
                    sb.Length = sb.Length - 5;  //remove "END\r\n"
                    sb.AppendLine("\tPRINT 'ALTERED Stored Procedure: " + schemaOwner + "." + name + "'\r\n\tEND");

                    sb.AppendLine("END");
                    sb.AppendLine("GO");


                }

                if (includePermissions)
                {
                    sb.AppendLine();
                    options = new ScriptingOptions();
                    options.SchemaQualifyForeignKeysReferences = true;
                    options.SchemaQualify = true;
                    options.PrimaryObject = false;
                    options.Permissions = true;
                    coll = smoSp.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].StartsWith("GRANT", StringComparison.CurrentCultureIgnoreCase))
                            sb.AppendLine(coll[i] + "\r\nGO\r\n");
                    }

                }

                script = sb.ToString();

                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script stored procedure {name}");
                return false;
            }
        }
        internal bool ScriptView(string name, string schemaOwner, ref string script, out string message)
        {
            Microsoft.SqlServer.Management.Smo.View smoView = smoDatabase.Views[name, schemaOwner];
            if (smoView == null)
            {
                message = "Unable to find View '" + schemaOwner + "." + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                options.AnsiFile = true;
                StringCollection coll;
                StringBuilder sb = new StringBuilder();
                if (!scriptAsAlter)
                {
                    options.ScriptDrops = true;
                    coll = smoView.Script(options);

                    CollateScript(coll, ref sb);


                    options.IncludeIfNotExists = false;
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    options.Indexes = true;
                    options.Triggers = true;
                    coll = smoView.Script(options);
                    CollateScript(coll, ref sb);

                    sb.AppendLine("\r\nGO\r\n");
                }
                else
                {
                    Regex regExec = new Regex("^EXEC", RegexOptions.IgnoreCase | RegexOptions.Multiline); ///VIEW scripts don't have a BEGIN / END, so we need to add it...
                    Regex regExists = new Regex("^IF NOT EXISTS", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    coll = smoView.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {

                        if (regExists.Match(coll[i]).Success)
                        {
                            coll[i] = regExec.Replace(coll[i], "BEGIN\r\nEXEC", 1);
                            TabJustifyScript(coll[i].TrimEnd(), ref sb);
                        }
                        else
                            sb.AppendLine(coll[i]); //This is the "SET ..." precursors
                    }

                    //sb.Length = sb.Length - 5; //View scripts don't have an "END"!
                    sb.AppendLine("\tPRINT 'CREATED View: " + schemaOwner + "." + name + "'");
                    sb.AppendLine("END");
                    //Script the ELSE ALter
                    sb.Append("ELSE\r\nBEGIN\r\n");

                    options.Statistics = false;
                    coll = smoView.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].IndexOf("CREATE", StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            coll[i] = new Regex(@"CREATE\s+VIEW", RegexOptions.IgnoreCase).Replace(coll[i], "ALTER VIEW", 1);
                            coll[i] = new Regex("IF NOT EXISTS", RegexOptions.IgnoreCase).Replace(coll[i], "IF EXISTS", 1);
                            coll[i] = regExec.Replace(coll[i], "BEGIN\r\nEXEC", 1);
                            TabJustifyScript(coll[i].TrimEnd(), ref sb);
                        }
                    }
                    //sb.Length = sb.Length - 5;  //remove "END\r\n"
                    sb.AppendLine("\tPRINT 'ALTERED View: " + schemaOwner + "." + name + "'\r\n\tEND");

                    sb.AppendLine("END");
                    sb.AppendLine("GO");


                }

                if (includePermissions)
                {
                    sb.AppendLine();
                    options = new ScriptingOptions();
                    options.SchemaQualifyForeignKeysReferences = true;
                    options.SchemaQualify = true;
                    options.PrimaryObject = false;
                    options.Permissions = true;
                    coll = smoView.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].StartsWith("GRANT", StringComparison.CurrentCultureIgnoreCase))
                            sb.AppendLine(coll[i] + "\r\nGO\r\n");
                    }

                }

                script = sb.ToString();
                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script view {name}");
                return false;
            }


        }

        internal bool ScriptUserDefinedFunctions(string name, string schemaOwner, ref string script, out string message)
        {

            Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoFunc = smoDatabase.UserDefinedFunctions[name, schemaOwner];
            if (smoFunc == null)
            {
                message = "Unable to find Function '" + schemaOwner + "." + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                StringCollection coll;
                StringBuilder sb = new StringBuilder();
                if (!scriptAsAlter)
                {
                    options.ScriptDrops = true;
                    coll = smoFunc.Script(options);

                    CollateScript(coll, ref sb);

                    options.IncludeIfNotExists = false;
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    options.AnsiFile = true;
                    coll = smoFunc.Script(options);
                    CollateScript(coll, ref sb);

                    script = sb.ToString();
                }
                else
                {
                    //Script the CREATE if NOT EXISTS
                    Regex regExists = new Regex("^IF NOT EXISTS", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    options.ScriptDrops = false;
                    options.PrimaryObject = true;
                    coll = smoFunc.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (regExists.Match(coll[i]).Success)
                            TabJustifyScript(coll[i].TrimEnd(), ref sb); // This should be the actual script
                        else
                            sb.AppendLine(coll[i]); //This should be the "SET ..." precursors
                    }
                    sb.Length = sb.Length - 5;  //remove "END\r\n"
                    sb.AppendLine("\tPRINT 'CREATED Function: " + schemaOwner + "." + name + "'");
                    sb.AppendLine("END");
                    //Script the ELSE ALter
                    sb.Append("ELSE\r\nBEGIN\r\n");

                    coll = smoFunc.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].IndexOf("CREATE", StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            coll[i] = new System.Text.RegularExpressions.Regex(@"CREATE\s+FUNCTION", RegexOptions.IgnoreCase).Replace(coll[i], "ALTER FUNCTION", 1);
                            coll[i] = new System.Text.RegularExpressions.Regex("IF NOT EXISTS", RegexOptions.IgnoreCase).Replace(coll[i], "IF EXISTS", 1);
                            TabJustifyScript(coll[i].TrimEnd(), ref sb);
                        }
                    }
                    sb.Length = sb.Length - 5; //remove "END\r\n"
                    sb.AppendLine("\tPRINT 'ALTERED Function: " + schemaOwner + "." + name + "'\r\n\tEND");

                    sb.AppendLine("END");
                    sb.AppendLine("GO");

                }

                if (includePermissions)
                {
                    sb.AppendLine();
                    options = new ScriptingOptions();
                    options.SchemaQualifyForeignKeysReferences = true;
                    options.SchemaQualify = true;
                    options.PrimaryObject = false;
                    options.Permissions = true;
                    coll = smoFunc.Script(options);
                    for (int i = 0; i < coll.Count; i++)
                    {
                        if (coll[i].StartsWith("GRANT", StringComparison.CurrentCultureIgnoreCase))
                            sb.AppendLine(coll[i] + "\r\nGO\r\n");
                    }

                }
                script = sb.ToString();
                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script user defined function {name}");
                return false;
            }



        }

        private bool ScriptDatabaseUsers(string name, ref string script, out string message)
        {

            Microsoft.SqlServer.Management.Smo.User smoUser = smoDatabase.Users[name];
            if (smoUser == null)
            {
                message = "Unable to find User '" + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                options.AnsiFile = true;
                options.ScriptDrops = true;
                StringCollection coll = smoUser.Script(options);
                StringBuilder sb = new StringBuilder();
                CollateScript(coll, ref sb);


                options.ScriptDrops = false;
                options.Permissions = true;
                options.PrimaryObject = true;
                coll = smoUser.Script(options);
                CollateScript(coll, ref sb);


                script = sb.ToString();
                message = string.Empty;
                return true;

            }
            catch (Exception e)
            {
                message = e.Message;

                log.LogError(e, $"Unable to script database user {name}");
                return false;
            }
        }
        private bool ScriptServerLogin(string name, ref string script, out string message)
        {

            Microsoft.SqlServer.Management.Smo.Login smoLogin = smoServer.Logins[name];
            if (smoLogin == null)
            {
                message = "Unable to find Login '" + name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                options.AnsiFile = true;
                options.ScriptDrops = true;
                StringCollection coll = smoLogin.Script(options);
                StringBuilder sb = new StringBuilder();
                CollateScript(coll, ref sb);



                options.ScriptDrops = false;
                options.Permissions = true;
                options.PrimaryObject = true;
                options.LoginSid = true;

                coll = smoLogin.Script(options);
                CollateScript(coll, ref sb);

                StringCollection members = smoLogin.ListMembers();
                foreach (string member in members)
                    sb.Append("exec sp_addsrvrolemember N'" + name + "'," + member + "\r\nGO\r\n\r\n");

                script = sb.ToString();
                message = string.Empty;
                return true;

            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script database login {name}");
                return false;
            }

        }
        private bool ScriptDatabaseRole(string name, ref string script, out string message)
        {
            Microsoft.SqlServer.Management.Smo.DatabaseRole smoRole = smoDatabase.Roles[name];
            if (smoRole == null)
            {
                message = "Unable to find Role '" + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                options.AnsiFile = true;
                options.ScriptDrops = true;
                StringCollection coll = smoRole.Script(options);
                StringBuilder sb = new StringBuilder();
                CollateScript(coll, ref sb);


                options.ScriptDrops = false;
                options.Permissions = true;
                options.PrimaryObject = true;
                coll = smoRole.Script(options);
                CollateScript(coll, ref sb);


                script = sb.ToString();
                message = string.Empty;
                return true;

            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script database role {name}");
                return false;
            }
        }
        private bool ScriptDatabaseSchema(string name, ref string script, out string message)
        {
            Microsoft.SqlServer.Management.Smo.Schema smoSchema = smoDatabase.Schemas[name];
            if (smoSchema == null)
            {
                message = "Unable to find Schema '" + name + "' in database '" + smoDatabase.Name + "' on server '" + smoServer.Name + "'";
                log.LogWarning(message);
                return false;
            }

            try
            {
                ScriptingOptions options = new ScriptingOptions();
                options.SchemaQualifyForeignKeysReferences = true;
                options.SchemaQualify = true;
                options.IncludeIfNotExists = true;
                options.AnsiFile = true;
                options.ScriptDrops = true;
                StringCollection coll = smoSchema.Script(options);
                StringBuilder sb = new StringBuilder();
                CollateScript(coll, ref sb);


                options.ScriptDrops = false;
                options.Permissions = true;
                options.PrimaryObject = true;
                coll = smoSchema.Script(options);
                CollateScript(coll, ref sb);


                script = sb.ToString();
                message = string.Empty;
                return true;

            }
            catch (Exception e)
            {
                message = e.Message;
                log.LogError(e, $"Unable to script database schema {name}");
                return false;
            }
        }
        private void CollateScript(StringCollection coll, ref StringBuilder sb)
        {
            foreach (string s in coll)
            {
                sb.AppendLine(s);
                sb.AppendLine("GO");
                sb.AppendLine("\r\n");
            }
        }
        internal void CollateScriptWithSchemaCheck(StringCollection coll, string schema, ref StringBuilder sb)
        {
            Regex regFindGood = new Regex(@"OBJECT_ID\(N'\[.{1,20}\]\.\[.{1,256}\]'\)", RegexOptions.IgnoreCase);
            Regex regFindMissingIndex = new Regex(@"OBJECT_ID\(N'", RegexOptions.IgnoreCase);
            for (int i = 0; i < coll.Count; i++)
            {
                string s = coll[i];
                if (s.StartsWith("IF NOT EXISTS", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (regFindGood.Matches(s).Count != regFindMissingIndex.Matches(s).Count)
                    {
                        bool keepGoing = true;
                        int startIndex = 0;
                        while (keepGoing)
                        {
                            Match gm = regFindGood.Match(s, startIndex);
                            Match mm = regFindMissingIndex.Match(s, startIndex);
                            if (mm.Index == 0)
                            {
                                keepGoing = false;
                                break;
                            }
                            if (gm.Index != mm.Index)
                            {
                                s = s.Insert(mm.Index + mm.Length, "[" + schema + "].");
                            }
                            startIndex = mm.Index + mm.Length;
                        }
                        coll[i] = s;
                    }
                }
            }
            CollateScript(coll, ref sb);
        }

        /// <summary>
        /// Adjusts the indentation for the scripts to align them better for easier reading of the nested begin/end
        /// Version 8.6.12 - removed tabbing to eliminate excessive shift from multiple scripting. Leaving in call to minimize code changes
        /// </summary>
        /// <param name="script">The script segment (multi-line)</param>
        /// <param name="sb">The StringBuilder object that is building the full script</param>
        private void TabJustifyScript(string script, ref StringBuilder sb)
        {
            bool inBegin = false;
            string[] list = script.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Trim().Equals("BEGIN", StringComparison.CurrentCultureIgnoreCase))
                {
                    sb.AppendLine(list[i]);
                    inBegin = true;
                    continue;
                }
                if (inBegin)
                {
                    if (!list[i].Trim().Equals("END", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //sb.AppendLine("\t" + list[i]);
                        sb.AppendLine(list[i]);
                    }
                    else
                    {
                        sb.AppendLine(list[i]);
                        inBegin = false;
                        continue;
                    }
                }
                else if (list[i].Trim().StartsWith("CREATE", StringComparison.CurrentCultureIgnoreCase)) //View CREATE STATISTICS
                {
                    //sb.AppendLine("\t" + list[i]);
                    sb.AppendLine(list[i]);
                }
                else
                {
                    sb.AppendLine(list[i]);
                }
            }

        }
        #endregion

        #region ## Events ##

        public event HashScriptingEventHandler HashScriptingEvent;
        public delegate void HashScriptingEventHandler(object sender, HashScriptingEventArgs e);

        #endregion
    }
    public class HashScriptingEventArgs
    {
        public readonly string Message;
        public HashScriptingEventArgs(string message)
        {
            Message = message;
        }
    }
}
