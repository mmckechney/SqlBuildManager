using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.DbInformation.ChangeDates;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.SqlLogging;
using SqlSync.Constants;
using System.Linq;
using SqlBuildManager.Interfaces.Console;
namespace SqlSync.SqlBuild
{
	/// <summary>
	/// Summary description for SqlBuildHelper.
	/// </summary>
	public class SqlBuildHelper
	{
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool isTransactional = true;
        private List<DatabaseOverride> targetDatabaseOverrides = null;
        public List<DatabaseOverride> TargetDatabaseOverrides
        {
            get { return targetDatabaseOverrides; }
            set { targetDatabaseOverrides = value; }
        }
        private BackgroundWorker bgWorker;
		/// <summary>
		/// Indicator for external callers that an error occurred. Can also subscribe to ScriptingErrorEvent
		/// </summary>
		public bool ErrorOccured;
		/// <summary>
		/// DataSet holding build configuration data
		/// </summary>
		private SqlSyncBuildData buildData;
		/// <summary>
		/// Data collection used to pass connection data
		/// </summary>
		private ConnectionData connData;
		/// <summary>
		/// 
		/// </summary>
		private string buildType;
		/// <summary>
		/// User description of the build
		/// </summary>
		private string buildDescription;
		/// <summary>
		/// Index where the build should start
		/// </summary>
		private double startIndex;
		/// <summary>
        /// Dictionary containing the BuildConnectData objects with the server/database as key
		/// </summary>
        private Dictionary<string, BuildConnectData> connectDictionary = new Dictionary<string,BuildConnectData>();
		/// <summary>
		/// Name of the Sql Build project (sbm) file
		/// </summary>
		private string projectFileName; 
		/// <summary>
		/// Holding string for the last message from Sql server
		/// </summary>
		private string lastSqlMessage = string.Empty;
		/// <summary>
		/// Path to the Sql Build project (sbm) file
		/// </summary>
		private string projectFilePath;
		/// <summary>
		/// Current run for 
		/// </summary>
		private SqlSyncBuildData.ScriptRunRow myRunRow;
		/// <summary>
		/// Flag for determining if this is a trial (rollback)
		/// </summary>
		private bool isTrialBuild = false;
		/// <summary>
		/// Name of the file being used to store scripts
		/// </summary>
		private string scriptLogFileName;
		/// <summary>
		/// String to hold the Sql event messages
		/// </summary>
		private string sqlInfoMessage = string.Empty;
		/// <summary>
		/// Index of the current script of the batch
		/// </summary>
		private int currentBatchScriptIndex;
		/// <summary>
		/// Index of the single script file to run (if single file run)
		/// </summary>
		private double[] runItemIndexes = new double[0];
		/// <summary>
		/// Id of the current build
		/// </summary>
		private System.Guid currentBuildId = System.Guid.Empty;
		/// <summary>
		/// Flag use to set "script only" setting
		/// </summary>
		private bool runScriptOnly = false;
		/// <summary>
		/// Name of the build file (.sbm)
		/// </summary>
		private string buildFileName = string.Empty;
		/// <summary>
		/// Names of scripts selected in the GUI
		/// </summary>
		private string[] selectedScriptIds = null;
		/// <summary>
		/// Public accessor to get the GUID of the current build
		/// </summary>
		public System.Guid CurrentBuildId
		{
			get
			{
				return currentBuildId;
			}
		}
        /// <summary>
        /// The hash signature of the build package scripts
        /// </summary>
        private string buildPackageHash = string.Empty;
        /// <summary>
        /// Database to write commits to if an alternate logging database has been specified
        /// </summary>
        private string logToDatabaseName = string.Empty;
        /// <summary>
        /// Database to write commits to if an alternate logging database has been specified
        /// </summary>
        public string LogToDatabaseName
        {
            get { return logToDatabaseName; }
            set { logToDatabaseName = value; }
        }
        /// <summary>
        /// List of the scripts that have been run and will be committed when the build is committed.
        /// </summary>
        private List<CommittedScript> committedScripts = new List<CommittedScript>();
        /// <summary>
        /// Used by the remote service to record the user that requested the build (vs. the user id used in execution) since they may be different
        /// </summary>
        private string buildRequestedBy = string.Empty;

		private string buildHistoryXmlFile = string.Empty;
		private SqlSyncBuildData buildHistoryData= null;
        private string externalScriptLogFileName = string.Empty;
		public SqlBuildHelper(ConnectionData data) :this(data, true, string.Empty, true)
		{
		}
        public SqlBuildHelper(ConnectionData data, bool createScriptRunLogFile, string externalScriptLogFileName,bool isTransactional)
        {
            this.connData = data;
            this.isTransactional = isTransactional;
            if(createScriptRunLogFile)
                this.ScriptLogWriteEvent += new ScriptLogWriteEventHandler(SqlBuildHelper_ScriptLogWriteEvent);

            if (externalScriptLogFileName != null && externalScriptLogFileName.Length > 0)
                this.externalScriptLogFileName = externalScriptLogFileName;

        }
        /// <summary>
        /// Used when the user wants to run the build across multiple databases and/or servers
        /// </summary>
        /// <param name="multiDbRunData"></param>
        /// <param name="bgWorker"></param>
        /// <param name="e"></param>
        public void ProcessMultiDbBuild(MultiDbData multiDbRunData, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            int intSequence;
            string strSequence;
            this.committedScripts.Clear();
            this.connectDictionary.Clear();

            List<SqlSyncBuildData.BuildRow> buildResults = new List<SqlSyncBuildData.BuildRow>();
            foreach (ServerData srvData in multiDbRunData)
            {
                srvData.OverrideSequence.Sort();
                foreach(KeyValuePair<string, List<DatabaseOverride>> sequence in srvData.OverrideSequence)
                {
                    if (int.TryParse(sequence.Key, out intSequence))
                        strSequence = intSequence.ToString().PadLeft(3, '0');
                    else
                        strSequence = sequence.Key;

                    SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
                    runData.BuildData = multiDbRunData.BuildData;
                    runData.BuildType = "Other";

                    if (multiDbRunData.BuildDescription == null || multiDbRunData.BuildDescription.Length == 0)
                        runData.BuildDescription = "Multi-Database Run ID:" + multiDbRunData.MultiRunId + ". Server:" + srvData.ServerName + "; Sequence: " + strSequence;
                    else
                        runData.BuildDescription = multiDbRunData.BuildDescription;

                    runData.StartIndex = 0;
                    runData.ProjectFileName = multiDbRunData.ProjectFileName;
                    runData.IsTrial =multiDbRunData.RunAsTrial;
                    runData.BuildFileName = multiDbRunData.BuildFileName;
                    runData.TargetDatabaseOverrides = sequence.Value;
                    this.targetDatabaseOverrides = sequence.Value;
                    SqlSyncBuildData.BuildRow buildResult = ProcessBuild(runData, bgWorker, e, srvData.ServerName, true, null, multiDbRunData.AllowableTimeoutRetries);
                    buildResults.Add(buildResult);
                    
                    if(buildResult.FinalStatus == BuildItemStatus.PendingRollBack || buildResult.FinalStatus == BuildItemStatus.FailedNoTransaction)
                    {
                        PerformRunScriptFinalization(true,buildResults,multiDbRunData, ref e);
                        return;
                    }
                }
            }

            PerformRunScriptFinalization(false, buildResults, multiDbRunData, ref e);
        }


        /// <summary>
        /// Private method that kicks off a build. 
        /// This overload does NOT clear out the committedScripts list collection and should be used for MultiDb runs
        /// </summary>
        /// <param name="runData">Run configuration data</param>
        /// <param name="bgWorker">Worker object that will report status</param>
        /// <param name="e">DoWorkEventArgs that will return state</param>
        /// <param name="serverName">Server to execute against</param>
        /// <param name="isMultiDbRun">Whether or not this is a UI based multi-database execution run</param>
        /// <param name="scriptBatchColl">Pre-batched script collection. Provided by console based threaded execution</param>
        /// <returns>BuildRow object with the result data.</returns>
        private SqlSyncBuildData.BuildRow ProcessBuild(SqlBuildRunData runData, BackgroundWorker bgWorker, DoWorkEventArgs e, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, int allowableTimeoutRetries)
        {
            this.bgWorker = bgWorker;
            this.ErrorOccured = false;
            this.buildData = runData.BuildData;
            this.buildType = runData.BuildType;
            this.buildDescription = runData.BuildDescription;
            this.startIndex = runData.StartIndex;
            this.projectFileName = runData.ProjectFileName;
            this.projectFilePath = Path.GetDirectoryName(this.projectFileName) + @"\";
            this.isTrialBuild = runData.IsTrial;
            this.runItemIndexes = runData.RunItemIndexes;
            this.runScriptOnly = runData.RunScriptOnly;
            this.buildFileName = Path.GetFileName(runData.BuildFileName);
            this.targetDatabaseOverrides = runData.TargetDatabaseOverrides;
            this.logToDatabaseName = runData.LogToDatabaseName;
            int buildRetries = 0;

            if (bgWorker != null)
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Starting Build Process"));

            DataView filteredScripts;
            SqlSyncBuildData.BuildRow myBuild;

            PrepareBuildForRun(serverName, isMultiDbRun, scriptBatchColl, ref e, out filteredScripts,out myBuild);
            SqlSyncBuildData.BuildRow buildResults = null;

            //Run the build... retry as needed until we exceed the retry count.
            while (buildRetries <= allowableTimeoutRetries &&
                (buildResults == null || buildResults.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout))
            {
                if (buildRetries > 0)
                {
                    if (ScriptLogWriteEvent != null)
                        this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));

                }
                buildResults = RunBuildScripts(filteredScripts, myBuild, serverName, isMultiDbRun, scriptBatchColl, ref e);

                if (buildRetries > 0 && buildResults.FinalStatus == BuildItemStatus.Committed)
                    buildResults.FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries;

                buildRetries++;
                if (buildResults.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout)
                {
                    log.WarnFormat("Timeout encountered. Incrementing retries to {0}", buildRetries);
                }
            }

            //Do we need to try to update the target using the Platinum Dacpac?
            if (buildResults.FinalStatus != BuildItemStatus.Committed && buildResults.FinalStatus != BuildItemStatus.TrialRolledBack &&
                !string.IsNullOrEmpty(runData.PlatinumDacPacFileName) && File.Exists(runData.PlatinumDacPacFileName))
            {
                var database = ((SqlSyncBuildData.ScriptRow)filteredScripts[0].Row).Database;
                string targetDatabase = GetTargetDatabase(database);
                log.WarnFormat("Custom dacpac required for {0} : {1}. Generating file.", serverName, targetDatabase);
                var stat = DacPacHelper.UpdateBuildRunDataForDacPacSync(ref runData, serverName, targetDatabase, this.connData.UserId, this.connData.Password, projectFilePath);

                if(stat == DacpacDeltasStatus.Success)
                {
                    runData.PlatinumDacPacFileName = string.Empty; //Keep this from becoming an infinite loop by taking out the dacpac name
                    var dacStat =  ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                    if(dacStat.FinalStatus ==  BuildItemStatus.Committed || dacStat.FinalStatus == BuildItemStatus.CommittedWithTimeoutRetries)
                    {
                        dacStat.FinalStatus = BuildItemStatus.CommittedWithCustomDacpac;
                        if (this.BuildCommittedEvent != null)
                            this.BuildCommittedEvent(this, RunnerReturn.CommittedWithCustomDacpac);
                    }
                }
                else if(stat == DacpacDeltasStatus.InSync) 
                {
                    buildResults.FinalStatus = BuildItemStatus.AlreadyInSync;
                    if (this.BuildCommittedEvent != null)
                        this.BuildCommittedEvent(this, RunnerReturn.DacpacDatabasesInSync);
                }

            }



            //If a timeout gets here.. need to decide how to label the rollback
            if (buildResults.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout && buildRetries > 1) //will always be at least 1..
                buildResults.FinalStatus = BuildItemStatus.RolledBackAfterRetries;
            else if (buildResults.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout)
                buildResults.FinalStatus = BuildItemStatus.RolledBack;

            return buildResults;
        }
        /// <summary>
       /// Entry method to kick off a build.
       /// This overload should be used for single server runs as it clears out the committedScript List collection
       /// </summary>
       /// <param name="runData"></param>
       /// <param name="bgWorker"></param>
       /// <param name="e"></param>
        public void ProcessBuild(SqlBuildRunData runData, int allowableTimeoutRetries, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            this.connectDictionary.Clear();
            this.committedScripts.Clear();

            this.ProcessBuild(runData, bgWorker, e, this.connData.SQLServerName, false,null, allowableTimeoutRetries);

        }
        /// <summary>
        /// Entry method to kick off a build.
        /// This overload should be used for the console based threaded execution of a build.
        /// </summary>
        /// <param name="runData"></param>
        /// <param name="bgWorker"></param>
        /// <param name="e"></param>
        /// <param name="scriptBatchColl"></param>
        public void ProcessBuild(SqlBuildRunData runData, BackgroundWorker bgWorker, DoWorkEventArgs e, ScriptBatchCollection scriptBatchColl, string buildRequestedBy, int allowableTimeoutRetries)
        {
            this.connectDictionary.Clear();
            this.committedScripts.Clear();
            this.buildRequestedBy = buildRequestedBy;

            this.ProcessBuild(runData, bgWorker, e, this.connData.SQLServerName, false, scriptBatchColl, allowableTimeoutRetries);
        }
        /// <summary>
        /// Sets the parameters for the build run 
        /// </summary>
        /// <param name="workEventArgs"></param>
        private void PrepareBuildForRun(string serverName, bool isMultiDbRun,ScriptBatchCollection scriptBatchColl, ref DoWorkEventArgs workEventArgs, out DataView filteredScripts, out SqlSyncBuildData.BuildRow myBuild)
		{

			//Make sure the project file is not read-only
            if(File.Exists(this.projectFileName))
			    File.SetAttributes(this.projectFileName, System.IO.FileAttributes.Normal);

			//Set the file name for the script log
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Creating Script Log File"));
			this.scriptLogFileName = this.projectFilePath+"LogFile-"+DateTime.Now.Year.ToString()+"-"+DateTime.Now.Month.ToString().PadLeft(2,'0')+"-"+DateTime.Now.Day.ToString().PadLeft(2,'0')+" at "+DateTime.Now.Hour.ToString().PadLeft(2,'0')+"_"+DateTime.Now.Minute.ToString().PadLeft(2,'0')+"_"+DateTime.Now.Second.ToString().PadLeft(2,'0')+".log";
			
			//Get a new build row
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Generating Build Record"));

			this.buildHistoryXmlFile = this.projectFilePath+SqlBuild.XmlFileNames.HistoryFile;
			myBuild = GetNewBuildRow(serverName);
			myBuild.UserId = System.Environment.UserName;

			//Read scripting configuration
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Reading Scripting configuration"));
			SqlSyncBuildData.ScriptDataTable scriptTable = GetScriptSourceTable(this.buildData);

			if(scriptTable == null)
			{
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("ERROR Reading <script> element in config template"));
                filteredScripts = null;
                if (isMultiDbRun)
                    myBuild.FinalStatus = BuildItemStatus.PendingRollBack;
                else
                    myBuild.FinalStatus = BuildItemStatus.RolledBack;

                return;
			}

			//Get View
            filteredScripts = scriptTable.DefaultView;
			//Sort by Build order column
            filteredScripts.Sort = scriptTable.BuildOrderColumn.ColumnName + " ASC ";
			//Filter by BuildOrder >= start index
			if(this.runItemIndexes.Length > 0)
			{
				//get the values
				string list = "";
				for(int i=0;i<this.runItemIndexes.Length;i++)
					list +=this.runItemIndexes[i].ToString()+",";
				list = list.Substring(0,list.Length-1);
                filteredScripts.RowFilter = scriptTable.BuildOrderColumn.ColumnName + " IN (" + list + ")";
			}
			else
			{
                filteredScripts.RowFilter = scriptTable.BuildOrderColumn.ColumnName + " >=" + startIndex.ToString();
			}

            //Get the build hash value for use in logging to the databae
            if (scriptBatchColl == null)
                this.buildPackageHash = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(this.projectFilePath, this.buildData);
            else
                this.buildPackageHash = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(scriptBatchColl);

            log.InfoFormat("Prepared build for run. Build Package hash = {0}", this.buildPackageHash);
		}
        /// <summary>
        /// Method that performs the splitting of the scripts into their batch and then executes the scripts
        /// </summary>
        /// <param name="view">The filtered list of scripts to run</param>
        /// <param name="myBuild">The build row that has been prepared and is used to contain the build history data</param>
        /// <param name="serverName">The name of the server that will be used for the build</param>
        /// <param name="workEventArgs"></param>
        private SqlSyncBuildData.BuildRow RunBuildScripts(DataView view, SqlSyncBuildData.BuildRow myBuild, string serverName, bool isMultiDbRun,  ScriptBatchCollection scriptBatchColl, ref DoWorkEventArgs workEventArgs)
		{
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Proceeding with Build"));
            log.DebugFormat("Processing with build for build Package hash = {0}", this.buildPackageHash);
			int overallIndex = 0;
			int runSequence = 0;
			bool buildFailure = false;
            bool failureDueToScriptTimeout = false;


			try
			{
                if (view == null)
                {
                    log.Error("No scripts selected for execution.");
                    throw new ApplicationException("No scripts selected for execution.");
                }

				for(int i=0;i<view.Count;i++)
				{
                    string[] batchScripts = null;
					//Get the script file row
					SqlSyncBuildData.ScriptRow myRow = (SqlSyncBuildData.ScriptRow)view[i].Row;
                    //Retrieve the pre-batched file contents if available...
                    if (scriptBatchColl != null)
                    {
                        ScriptBatch b = scriptBatchColl.GetScriptBatch(myRow.ScriptId);
                        if (b != null)
                            batchScripts = b.ScriptBatchContents;
                    }
                        
                    //If not pre-batched script found, read the script file contents as a batch array
                    if (batchScripts == null || batchScripts.Length == 0)
                        batchScripts = ReadBatchFromScriptFile(this.projectFilePath + myRow.FileName, myRow.StripTransactionText, false);

                    //Check for database override setting...
                    string targetDatabase = GetTargetDatabase(myRow.Database);

                    //Get Script Run Row id
                    System.Guid scriptRunRowId = System.Guid.NewGuid();
                    //Send Notification
                    bgWorker.ReportProgress(0, new BuildScriptEventArgs(i + 1,
                            myRow.FileName,
                            ((isMultiDbRun)? serverName +"."+targetDatabase : targetDatabase),
                            myRow.BuildOrder,
                            "Running Script",
                            myRow.ScriptId,
                            myRow.StripTransactionText,
                            scriptRunRowId));

                    //Create a new ScriptRun row to record build
                    this.myRunRow = this.buildHistoryData.ScriptRun.NewScriptRunRow();

                    //Set basic run row values
                    this.myRunRow.BuildRow = myBuild;
                    this.myRunRow.Database = targetDatabase;
                    this.myRunRow.RunOrder = i + 1;
                    this.myRunRow.RunStart = DateTime.Now;
                    this.myRunRow.FileName = myRow.FileName;
                    this.myRunRow.ScriptRunId = scriptRunRowId.ToString();

					//Get the connection objects for the row
                    BuildConnectData cData = null;
                    try
                    {
                        cData = GetConnectionDataClass(serverName, targetDatabase);
                    }
                    catch (Exception e)
                    {
                        log.Error(String.Format("Database connection to {0}.{1} failed", serverName, targetDatabase), e);
                        this.myRunRow.Results += "Database connection to " + targetDatabase + " failed\r\n" + e.Message;
                        this.myRunRow.Success = false;
                       	this.myRunRow.RunEnd = DateTime.Now;
					    this.buildHistoryData.ScriptRun.AddScriptRunRow(this.myRunRow);
					    this.buildHistoryData.AcceptChanges();


                        //Write the error to the log...
                        if (ScriptLogWriteEvent != null)
                            this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(overallIndex, "Database connection to " + targetDatabase +" failed", targetDatabase, myRow.FileName, e.Message + "\r\n" + this.sqlInfoMessage));
                        //Roll back the whole build
                        this.RollbackBuild();
                        //Send Notification
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                        //Set build failure flag
                        buildFailure = true;
                        break;

                    }

					//Check to see if this row can be committed multiple times,if not, check for a commited entry
					if(myRow.AllowMultipleRuns == false)
					{
						if(this.buildData.CommittedScript != null)
						{

							bool hasBlockingSqlLog = false;
							if(cData.HasLoggingTable)
								hasBlockingSqlLog = GetBlockingSqlLog(new Guid(myRow.ScriptId),ref cData);

                            if(hasBlockingSqlLog)
							{
                                log.InfoFormat("Skipping pre-run script {0}", myRow.FileName);
								//don't allow the script to be run again
                                bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Skipped Pre-Run script", TimeSpan.Zero));
								continue;
							}
						}
					}



                    string textHash;
                    SqlBuildFileHelper.GetSHA1Hash(batchScripts, out textHash);
                    this.myRunRow.FileHash = textHash;

					//Add script Id to the arraylist for use later
				    string scriptText = SqlBuildFileHelper.JoinBatchedScripts(batchScripts); // String.Join("\r\n"+BatchParsing.Delimiter+"\r\n",batchScripts);
                    CommittedScript tmpCommmittedScr = new CommittedScript(new Guid(myRow.ScriptId), this.myRunRow.FileHash, runSequence++, scriptText, myRow.Tag,cData.ServerName,cData.DatabaseName);


					//Create a transaction save point
					string savePointName = myRow.ScriptId.Replace("-","");
					try
					{
                        if(this.isTransactional)
						    cData.Transaction.Save(savePointName);
					}
					catch(Exception mye)
					{
                        log.Warn(String.Format("Error creating Transaction save point on {0}.{1} for script {2}", serverName, targetDatabase, myRow.FileName), mye);
					}

                    DateTime start = DateTime.Now;
					//Loop through the batch statements
					for(int x=0;x<batchScripts.Length;x++)
					{
                        //Check to see if there is a pending cancellation. If so, break out.
                        if(this.bgWorker.CancellationPending)
                        {
                            log.InfoFormat("Encountered cancellation pending directive. Breaking out of build");
                            workEventArgs.Cancel = true;
                            break;
                        }

                        //Make any necessary token replacements
                        batchScripts[x] = PerformScriptTokenReplacement(batchScripts[x]);
						this.currentBatchScriptIndex = x;
						overallIndex++;

						//Create the SqlCommand object - with or without a transaction...
						SqlCommand cmd;
                        if (this.isTransactional)
                            cmd = new SqlCommand(batchScripts[x], cData.Connection, cData.Transaction);
                        else
                            cmd = new SqlCommand(batchScripts[x], cData.Connection);

						//cmd.CommandTimeout = this.connData.ScriptTimeout;
						cmd.CommandTimeout = myRow.ScriptTimeOut;

						//Clear the info message string
						this.sqlInfoMessage = string.Empty;

						//Execute the SqlCommand
						try
						{
							if(this.runScriptOnly)
							{
                                if(ScriptLogWriteEvent != null)
                                    this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(overallIndex, batchScripts[x], targetDatabase, myRow.FileName, "Scripted"));
								continue;
							}

							StringBuilder sb = new StringBuilder();
								using(SqlDataReader reader = cmd.ExecuteReader())
								{
									bool more = true;
									while(more)
									{
										sb.Append("Records Affected: "+reader.RecordsAffected.ToString()+"\r\n");
										while(reader.Read())
										{
											for(int j=0;j<reader.FieldCount;j++)
											{
												sb.Append(reader[j]);
												if(j<reader.FieldCount-1)
													sb.Append("|");
												else
													sb.Append("\r\n");
											}
										}
										more = reader.NextResult();
									}
									reader.Close();
								}
							

							//
							this.myRunRow.Success = true;
							this.myRunRow.Results += sb.ToString();
						
							//Write to the log...
                            if (ScriptLogWriteEvent != null)
                                this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(overallIndex, batchScripts[x], targetDatabase, myRow.FileName, sb.ToString() + this.sqlInfoMessage));
						
						}
						catch(SqlException e)
						{
                            if (e.Message.Trim().ToLower().StartsWith("timeout expired."))
                            {
                                log.WarnFormat("Encountered a Timeout exception for script: \"{0}\"", cmd.CommandText);
                                failureDueToScriptTimeout = true;
                            }

							StringBuilder sb = new StringBuilder("Script File: "+myRow.FileName+"\r\n");
							foreach(SqlError error in e.Errors)
							{
								sb.Append("Line Number: "+error.LineNumber+"\r\n");
								sb.Append("Error Message: "+error.Message+"\r\n");
								sb.Append("Offending Script:\r\n"+batchScripts[currentBatchScriptIndex]);
								sb.Append("----------------");
							}
							this.myRunRow.Success = false;
							this.myRunRow.Results += sb.ToString();
						    log.Debug(sb.ToString(),e);

							//Write the error to the log...
                            if(ScriptLogWriteEvent != null)
                                this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(overallIndex, batchScripts[x], targetDatabase, myRow.FileName, sb.ToString() + this.sqlInfoMessage));
						
							//Roll back save point?
							if(myRow.RollBackOnError)
							{
                                try
                                {
                                    DateTime end = DateTime.Now;
                                    TimeSpan span = new TimeSpan(end.Ticks - start.Ticks);
                                    if (this.isTransactional)
                                    {
                                        cData.Transaction.Rollback(savePointName);
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Script Rolled Back", span));
                                    }
                                    else
                                    {
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Script Error. No Rollback Available.", span));
                                    }
                                }
                                catch (SqlException sqle)
                                {
                                    sb.Length = 0;
                                    foreach (SqlError err in sqle.Errors)
                                        sb.Append(err.Message + "\r\n");

                                    this.myRunRow.Results += sb.ToString();
                                    if (this.isTransactional)
                                    {
                                        this.RollbackBuild();
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));

                                    }
                                    else
                                    {
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                                    }
                                    buildFailure = true;
                                }
                                catch (InvalidOperationException invalExe)
                                {
                                    sb.Length = 0;
                                    sb.Append(invalExe.Message + "\r\n");

                                    this.myRunRow.Results += sb.ToString();
                                    if (this.isTransactional)
                                    {
                                        this.RollbackBuild();
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                                    }
                                    else
                                    {
                                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                                    }
                                    buildFailure = true;

                                }
								//Send Notification
							}
							else
							{
								//Send Notification;
                                DateTime end = DateTime.Now;
                                TimeSpan span = new TimeSpan(end.Ticks - start.Ticks);
                                if(this.isTransactional)
                                    bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error, Save Point Not Rolled Back", TimeSpan.Zero));
                                else
                                    bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                            
							}

							//Roll back entire build?
							if(myRow.CausesBuildFailure || buildFailure)
							{
                                DateTime end = DateTime.Now;
                                TimeSpan span = new TimeSpan(end.Ticks - start.Ticks);
                                if (this.isTransactional)
                                {
                                    //Roll back the whole build
                                    this.RollbackBuild();
                                    //Send Notification
                                    bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                                }
                                else
                                {
                                    bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                                }

								//Set build failure flag
								buildFailure = true;
							}
						}
						if(buildFailure)
						{
							this.ErrorOccured = true;
							break;
						}

                        //If one of the batch scripts for a file failed, we need to break out and fail the whole file.
                        if (myRow.RollBackOnError && myRunRow.Success == false)
                            break;
					}
                    if (workEventArgs.Cancel)
                    {
                        DateTime end = DateTime.Now;
                        TimeSpan span = new TimeSpan(end.Ticks - start.Ticks);
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Cancelled", TimeSpan.Zero));
                        buildFailure = true; 
                    }
                    else if (this.myRunRow.Success)//Only add to the committed script list if marked successful
                    {
                        DateTime end = DateTime.Now;
                        TimeSpan span = new TimeSpan(end.Ticks - start.Ticks);
                        tmpCommmittedScr.RunStart = this.myRunRow.RunStart;
                        tmpCommmittedScr.RunEnd = DateTime.Now;
                        committedScripts.Add(tmpCommmittedScr);
                        //Send Notification
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Script Successful",span));

                    }

					this.myRunRow.RunEnd = DateTime.Now;
					this.buildHistoryData.ScriptRun.AddScriptRunRow(this.myRunRow);
					this.buildHistoryData.AcceptChanges();

					if(buildFailure)
					{
						this.ErrorOccured = true;
						break;
					}

					if(this.runScriptOnly)
					{
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Scripted", TimeSpan.Zero));
					}


				}
			}
			catch(Exception e)
			{
                log.Error("General build failure", e);
				this.ErrorOccured = true;
                buildFailure = true;
                bgWorker.ReportProgress(100, e);
			}
			finally
			{
				this.buildData.AcceptChanges();
				this.buildHistoryData.AcceptChanges();
				this.SaveBuildDataSet(false);
                if (this.ScriptLogWriteEvent != null)
                {
                    if (this.isTransactional)
                    {
                        if (!buildFailure)
                        {
                            if (this.isTrialBuild)
                            {
                                this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(-10000, "ROLLBACK TRANSACTION", "", "-- Trial Run: Complete Transaction with a rollback--", "Scripted"));
                            }
                            else
                            {
                                this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(-10000, "COMMIT TRANSACTION", "", "-- Complete Transaction --", "Scripted"));
                            }
                        }
                        else
                            this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(-10000, "ROLLBACK TRANSACTION", "", "-- ERROR: Rollback Transaction --", "Scripted"));
                    }
                    else
                    {
                        if (!buildFailure)
                        {
                            this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(-10000, "", "", "-- Completed: No Transaction Set --", "Scripted"));
                        }
                        else
                        {
                            this.ScriptLogWriteEvent(null, new ScriptLogEventArgs(-10000, "", "", "-- ERROR: No Transaction Set --", "Scripted"));
                        }
                    }

                }

			}

            if (buildFailure)
            {
               
                log.Error("Build failure. Check execution logs for details");
                if (!isMultiDbRun)
                {
                    PerformRunScriptFinalization(buildFailure, myBuild, ref workEventArgs);
                }
                else
                {
                    if (this.isTransactional)
                        myBuild.FinalStatus = BuildItemStatus.PendingRollBack;
                    else
                        myBuild.FinalStatus = BuildItemStatus.FailedNoTransaction;
                }

                if (this.isTransactional && failureDueToScriptTimeout)
                {
                    myBuild.FinalStatus = BuildItemStatus.FailedDueToScriptTimeout;
                    //TODO: need to run "this.connectDictionary.Clear();" here for multiDbRun???
                }
            }
            else
            {
                
                if (isMultiDbRun)
                    myBuild.FinalStatus = BuildItemStatus.Pending;
                else
                    PerformRunScriptFinalization(buildFailure, myBuild, ref workEventArgs);
                log.DebugFormat("Build Successful!");
            }

            return myBuild;
		}
        private string PerformScriptTokenReplacement(string script)
        {
            if (ScriptTokens.regBuildDescription.Match(script).Success)
            {
                if (this.buildDescription == null)
                    this.buildDescription = "";
                script = ScriptTokens.regBuildDescription.Replace(script,this.buildDescription);
            }

            if (ScriptTokens.regBuildPackageHash.Match(script).Success)
            {
                if (this.buildPackageHash == null)
                    this.buildPackageHash = "";
                script = ScriptTokens.regBuildDescription.Replace(script, this.buildPackageHash);
            }

            if (ScriptTokens.regBuildFileName.Match(script).Success)
            {
                if (this.buildFileName != null)
                    script = ScriptTokens.regBuildFileName.Replace(script, this.buildFileName);
                else
                    script = ScriptTokens.regBuildFileName.Replace(script, "sbx file");
            }
            return script;

        }
        private void PerformRunScriptFinalization(bool buildFailure, SqlSyncBuildData.BuildRow myBuild, ref DoWorkEventArgs workEventArgs)
        {
            List<SqlSyncBuildData.BuildRow> tmp = new List<SqlSyncBuildData.BuildRow>();
            tmp.Add(myBuild);
            PerformRunScriptFinalization(buildFailure, tmp, null,ref workEventArgs);
        }
        private void PerformRunScriptFinalization(bool buildFailure, List<SqlSyncBuildData.BuildRow> myBuilds,MultiDbData multiDbRunData, ref DoWorkEventArgs workEventArgs)
        {
            DateTime end = DateTime.Now;
            for(int i=0;i<myBuilds.Count;i++)
                myBuilds[i].BuildEnd = DateTime.Now;

            //Set the stautus in the XML file
            if (buildFailure) //Cancelled is considered a failure too. 
            {
                this.ErrorOccured = true;
                for (int i = 0; i < myBuilds.Count; i++)
                {
                    if (this.isTransactional)
                        myBuilds[i].FinalStatus = BuildItemStatus.RolledBack;
                    else
                        myBuilds[i].FinalStatus = BuildItemStatus.FailedNoTransaction;
                }

                if (!this.isTransactional)
                {
                    RecordCommittedScripts(committedScripts);
                    this.LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);

                }
            }
            else
            {
                if (!this.isTrialBuild)
                {
                    if (this.isTransactional)
                    {
                        bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Attempting to Commit Build"));

                        bool commitSuccess = this.CommitBuild();

                        if (commitSuccess)
                            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Commit Successful"));
                    }

                    for (int i = 0; i < myBuilds.Count; i++)
                        myBuilds[i].FinalStatus = BuildItemStatus.Committed;

                    //Add the committed scriptIds to the commited scripts section
                    RecordCommittedScripts(committedScripts);
                    this.LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);


                    if (this.BuildCommittedEvent != null)
                        this.BuildCommittedEvent(this, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (this.isTransactional)
                    {
                        this.RollbackBuild();

                        for (int i = 0; i < myBuilds.Count; i++)
                            myBuilds[i].FinalStatus = BuildItemStatus.TrialRolledBack;

                        if (this.BuildSuccessTrialRolledBackEvent != null)
                            this.BuildSuccessTrialRolledBackEvent(this, EventArgs.Empty);
                    }
                    else //should never really get here, but just in case :-)
                    {
                        for (int i = 0; i < myBuilds.Count; i++)
                            myBuilds[i].FinalStatus = BuildItemStatus.Committed;
                    }
                }
            }

            if (buildFailure)
            {
                if (this.BuildErrorRollBackEvent != null)
                    this.BuildErrorRollBackEvent(this, EventArgs.Empty);
            }

            this.SaveBuildDataSet(true);


            //Set final status
            if (buildFailure)
            {
                if (workEventArgs.Cancel)
                {
                    if (this.isTransactional)
                    {
                        bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK;
                    }
                    else
                    {
                        bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION;

                    }
                }
                else
                {
                    if (this.isTransactional)
                    {
                        bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK;
                    }
                    else 
                    {
                        bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                    }
                }
            }
            else
            {
                if (this.runScriptOnly)
                {
                    bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Script Generation Complete"));
                    workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.SCRIPT_GENERATION_COMPLETE;
                }
                else
                {
                    if (this.isTrialBuild == false)
                    {
                        bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Committed"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;
                    }
                    else
                    {
                        if (this.isTransactional)
                        {
                            bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Rolled back for Trial Build"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
                        }
                        else //should never really get here
                        {
                            bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Committed with no transaction"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;

                        }
                    }
                }
            }

            //All connections committed...clear out dictionary
            this.connectDictionary.Clear();
        }
        /// <summary>
        /// Adds the record of the scripts that have been executed into the CommittedScript list.
        /// Also calls LogCommittedScriptsToDatabase to update the SqlBuild_Logging table
        /// </summary>
        /// <param name="committedScriptIds"></param>
        /// <returns></returns>
        private bool RecordCommittedScripts(List<CommittedScript> committedScripts)
		{
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Recording Commited Scripts to Log"));
			
			for(int i=0;i<committedScripts.Count;i++)
			{
                this.buildData.CommittedScript.AddCommittedScriptRow(committedScripts[i].ScriptId.ToString(), committedScripts[i].ServerName, DateTime.Now, true, committedScripts[i].FileHash, this.buildData.SqlSyncBuildProject[0]);
			}
			return true;

		}
		
		public void ClearScriptBlocks(ClearScriptData scrData , BackgroundWorker bgWorker,DoWorkEventArgs e)
		{
            this.projectFileName = scrData.ProjectFileName;
            this.buildData = scrData.BuildData;
            this.buildFileName = scrData.BuildZipFileName;
            this.selectedScriptIds = scrData.SelectedScriptIds;
            this.bgWorker = bgWorker;

			bgWorker.ReportProgress(0,new GeneralStatusEventArgs("Clearing Script Blocks"));

            this.EnsureLogTablePresence();

            SqlCommand cmd = new SqlCommand("UPDATE SqlBuild_Logging SET AllowScriptBlock = 0, AllowBlockUpdateId = @UserId WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1");
            cmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            cmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            for (int i = 0; i < this.selectedScriptIds.Length; i++)
            {
                DataRow[] rows = this.buildData.Script.Select(this.buildData.Script.ScriptIdColumn.ColumnName + "='" + this.selectedScriptIds[i] + "'");
                if (rows != null && rows.Length > 0)
                {
                    SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)rows[0];
                    bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Clearing " + row.FileName));

                    //Update local XML Log
                    DataRow[] commitRows = this.buildData.CommittedScript.Select(
                        this.buildData.CommittedScript.ServerNameColumn.ColumnName + "='" + this.connData.SQLServerName + "' AND " +
                        this.buildData.CommittedScript.ScriptIdColumn.ColumnName + "='" + this.selectedScriptIds[i] + "' AND (" +
                        this.buildData.CommittedScript.AllowScriptBlockColumn.ColumnName + "= 1 OR " +
                        this.buildData.CommittedScript.AllowScriptBlockColumn.ColumnName + "='true' OR " +
                        this.buildData.CommittedScript.AllowScriptBlockColumn.ColumnName + " IS NULL)");

                    if (commitRows != null && commitRows.Length > 0)
                    {
                        for (int j = 0; j < commitRows.Length; j++)
                        {
                            ((SqlSyncBuildData.CommittedScriptRow)commitRows[j]).AllowScriptBlock = false;
                        }
                        this.buildData.CommittedScript.AcceptChanges();
                    }

                    //Update Sql server log
                    string targetDatabase = GetTargetDatabase(row.Database);
                    BuildConnectData cData = GetConnectionDataClass(this.connData.SQLServerName,targetDatabase);
                    this.EnsureLogTablePresence();
                    cmd.Connection = cData.Connection;
                    cmd.Transaction = cData.Transaction;
                    cmd.Parameters["@ScriptId"].Value = new System.Guid(this.selectedScriptIds[i]);
                    cmd.ExecuteNonQuery();
                }
            }

            this.CommitBuild();
            this.SaveBuildDataSet(true);

            bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Selected Script Blocks Cleared"));
		}

        /// <summary>
        /// Gets the database to actually execute against based on the set default and any matching override
        /// </summary>
        /// <param name="defaultDatabase">The default database set up in the SBM configuration</param>
        /// <returns>Actual database to execute against</returns>coo
        private string GetTargetDatabase(string defaultDatabase)
        {
            if (this.targetDatabaseOverrides != null)
            {
                return ConnectionHelper.GetTargetDatabase(defaultDatabase, this.targetDatabaseOverrides);
            }
            else if (OverrideData.TargetDatabaseOverrides != null)
            {
                return ConnectionHelper.GetTargetDatabase(defaultDatabase, OverrideData.TargetDatabaseOverrides);
            }
            else
            {
                return defaultDatabase;
            }
            
        }
        public static string GetTargetDatabase(string serverName, string defaultDatabase, MultiDbData multiDbRunData)
        {
            foreach (ServerData srvData in multiDbRunData)
            {
                if (serverName.ToUpper() != srvData.ServerName.ToUpper())
                    continue;

                foreach (KeyValuePair<string, List<DatabaseOverride>> sequence in srvData.OverrideSequence)
                {
                    List<DatabaseOverride> tmp = sequence.Value;
                    for (int i = 0; i < tmp.Count; i++)
                        if (tmp[i].DefaultDbTarget.ToUpper() == defaultDatabase.ToUpper())
                            return tmp[i].OverrideDbTarget;
                }
            }
            return defaultDatabase;
        }
		
		#region ## SQL table logging ##
        /// <summary>
        /// Ensures that the SqlBuild_Logging table exists and that it is setup properly. Self-heals if it is not. 
        /// </summary>
		private void EnsureLogTablePresence()
		{
            this.sqlInfoMessage = string.Empty;
			//Self healing: add the table if needed
            SqlCommand createTableCmd = new SqlCommand(Properties.Resources.LoggingTable);
            Dictionary<string, BuildConnectData>.KeyCollection keys = this.connectDictionary.Keys;
			foreach(string key in keys)
			{
                this.sqlInfoMessage = string.Empty;
				try
				{

                    createTableCmd.Connection = ((BuildConnectData)this.connectDictionary[key]).Connection;
                    
                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (this.logToDatabaseName.Length > 0 && !createTableCmd.Connection.Database.Equals(this.logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;

					if(createTableCmd.Connection.State == ConnectionState.Closed)
						createTableCmd.Connection.Open();

                    if (((BuildConnectData)this.connectDictionary[key]).Transaction != null)
                        createTableCmd.Transaction = ((BuildConnectData)this.connectDictionary[key]).Transaction;

					createTableCmd.ExecuteNonQuery();
                    log.DebugFormat("EnsureLogTablePresence Table Sql Messages for {0}.{1}:\r\n{2}",createTableCmd.Connection.DataSource, createTableCmd.Connection.Database,this.sqlInfoMessage);
				}
				catch(Exception e)
				{
                    log.Error(String.Format("Error ensuring log table presence for {0}.{1}", createTableCmd.Connection.DataSource, createTableCmd.Connection.Database), e);
	    		}
			}
            //SqlCommand createCommitIndex = new SqlCommand(GetFromResources("SqlSync.SqlBuild.SqlLogging.LoggingTableCommitCheckIndex.sql"));
            SqlCommand createCommitIndex = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex);
            foreach (string key in keys)
            {
                this.sqlInfoMessage = string.Empty;
                try
                {
                    createCommitIndex.Connection = ((BuildConnectData)this.connectDictionary[key]).Connection;

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (this.logToDatabaseName.Length > 0 && !createCommitIndex.Connection.Database.Equals(this.logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;


                    if (createCommitIndex.Connection.State == ConnectionState.Closed)
                        createCommitIndex.Connection.Open();

                    if (((BuildConnectData)this.connectDictionary[key]).Transaction != null)
                        createCommitIndex.Transaction = ((BuildConnectData)this.connectDictionary[key]).Transaction;

                    createCommitIndex.ExecuteNonQuery();
                    log.DebugFormat("EnsureLogTablePresence Index Sql Messages for {0}.{1}:\r\n{2}",createTableCmd.Connection.DataSource, createTableCmd.Connection.Database,this.sqlInfoMessage);

                }
                catch (Exception e)
                {
                    log.Error(String.Format("Error ensuring log table commit check index for {0}.{1}",createTableCmd.Connection.DataSource, createTableCmd.Connection.Database) , e);
                }
            }
            log.DebugFormat("sqlInfoMessage value: {0}", this.sqlInfoMessage);
            log.Debug("Exiting EnsureLogPresence method");
            this.sqlInfoMessage = string.Empty;
		}
        /// <summary>
        /// Checks to see that the logging table exists or not.
        /// </summary>
        /// <param name="conn">Connection object to the target database</param>
        /// <returns></returns>
        private bool LogTableExists(SqlConnection conn)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT 1 FROM sys.objects WITH (NOLOCK) WHERE name = 'SqlBuild_Logging' AND type = 'U'", conn);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                object result = cmd.ExecuteScalar();
                if (result == null || result == System.DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Adds the list of scripts that were committed with the run to the SqlBuild_Logging table
        /// </summary>
        /// <param name="committedScripts">List of CommittedScript objects</param>
        /// <param name="multiDbRunData">The MultiDbRun data for the run</param>
        /// <returns>True if the commit was successful</returns>
        private bool LogCommittedScriptsToDatabase(List<CommittedScript> committedScripts, MultiDbData multiDbRunData)
		{
            bool returnValue = true;
            //If using an alternate database to log the commits to, we need to initiate the connection objects 
            //so that the EnsureLogTablePresence method catches them and creates the tables as needed.
			if(this.logToDatabaseName.Length > 0 && this.committedScripts.Count > 0)
            {
                List<string> servers = new List<string>();
                for (int i = 0; i < this.committedScripts.Count; i++)
                    if (!servers.Contains(this.committedScripts[i].ServerName))
                        servers.Add(this.committedScripts[i].ServerName);

                for (int i = 0; i < servers.Count; i++)
                {
                    BuildConnectData tmp = GetConnectionDataClass(servers[i], this.logToDatabaseName);
                }
            }
			this.EnsureLogTablePresence();

            //Get date from the server
            DateTime commitDate;
            SqlCommand cmd = null;
            string oldDb = this.connData.DatabaseName;
            this.connData.DatabaseName = "master";
            try
            {
                cmd = new SqlCommand("SELECT getdate()", SqlSync.Connection.ConnectionHelper.GetConnection(this.connData));
                cmd.Connection.Open();
                commitDate = (DateTime)cmd.ExecuteScalar();
            }
            catch
            {
                commitDate = DateTime.Now;
                log.InfoFormat("Unable to getdate() from server/database {0}-{1}", this.connData.SQLServerName, this.connData.DatabaseName);
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                    cmd.Connection.Close();

                this.connData.DatabaseName = oldDb;
            }

			//Add the commited scripts to the log
            SqlCommand logCmd = new SqlCommand(Properties.Resources.LogScript);
            if(!String.IsNullOrEmpty(this.buildFileName))
			    logCmd.Parameters.AddWithValue("@BuildFileName",this.buildFileName);
            else
                logCmd.Parameters.AddWithValue("@BuildFileName", Path.GetFileName(this.projectFileName));

            logCmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            logCmd.Parameters.AddWithValue("@CommitDate", commitDate);
            logCmd.Parameters.AddWithValue("@RunWithVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            logCmd.Parameters.AddWithValue("@BuildProjectHash", this.buildPackageHash);
            if(this.buildRequestedBy == string.Empty)
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", System.Environment.UserDomainName + "\\" + System.Environment.UserName);
            else
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", this.buildRequestedBy);

            if(this.buildDescription == null)
                logCmd.Parameters.AddWithValue("@Description", string.Empty);    
            else
                logCmd.Parameters.AddWithValue("@Description", this.buildDescription);  

			logCmd.Parameters.Add("@ScriptFileName",SqlDbType.VarChar);
			logCmd.Parameters.Add("@ScriptId",SqlDbType.UniqueIdentifier);
			logCmd.Parameters.Add("@ScriptFileHash",SqlDbType.VarChar);
			logCmd.Parameters.Add("@Sequence",SqlDbType.Int);
			logCmd.Parameters.Add("@ScriptText",SqlDbType.Text);
            logCmd.Parameters.Add("@Tag", SqlDbType.VarChar);
            logCmd.Parameters.Add("@TargetDatabase", SqlDbType.VarChar);
            logCmd.Parameters.Add("@ScriptRunStart", SqlDbType.DateTime);
            logCmd.Parameters.Add("@ScriptRunEnd", SqlDbType.DateTime);
			
			
			for(int i=0;i<committedScripts.Count;i++)
			{
				try
				{

					CommittedScript script = committedScripts[i];
					DataRow[] rows = this.buildData.Script.Select(this.buildData.Script.ScriptIdColumn.ColumnName +" ='"+script.ScriptId.ToString()+"'");
					if(rows.Length > 0)
					{
						SqlBuild.SqlSyncBuildData.ScriptRow row = (SqlBuild.SqlSyncBuildData.ScriptRow)rows[0];
					
						bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Recording Commited Script: "+row.FileName));

                        BuildConnectData tmpConnDat;
                        if (this.logToDatabaseName.Length > 0)
                            tmpConnDat = GetConnectionDataClass(script.ServerName, this.logToDatabaseName);
                        else
                            tmpConnDat = GetConnectionDataClass(script.ServerName, script.DatabaseTarget);

                        logCmd.Connection = tmpConnDat.Connection;
						logCmd.Parameters["@ScriptFileName"].Value = row.FileName;
						logCmd.Parameters["@ScriptId"].Value = script.ScriptId;
						logCmd.Parameters["@ScriptFileHash"].Value = script.FileHash;
						logCmd.Parameters["@Sequence"].Value = script.Sequence;
						logCmd.Parameters["@ScriptText"].Value = script.ScriptText;
                        logCmd.Parameters["@Tag"].Value = script.Tag;
                        logCmd.Parameters["@TargetDatabase"].Value = script.DatabaseTarget;
                        logCmd.Parameters["@ScriptRunStart"].Value = script.RunStart;
                        logCmd.Parameters["@ScriptRunEnd"].Value = script.RunEnd;
						if(logCmd.Connection.State == ConnectionState.Closed)
							logCmd.Connection.Open();

						logCmd.ExecuteNonQuery();
					}
				}
				catch(Exception sqlexe)
				{
                    log.Error(String.Format("Unable to log full text value for script {0}. Inserting \"Error\" instead",logCmd.Parameters["@ScriptFileName"].Value.ToString()),sqlexe);
					try
					{
						logCmd.Parameters["@ScriptText"].Value = "Error";
						if(logCmd.Connection.State == ConnectionState.Closed)
							logCmd.Connection.Open();

						logCmd.ExecuteNonQuery();
					}
					catch(Exception exe)
					{
                        log.Error(String.Format("Unable to log commit for script {0}.", logCmd.Parameters["@ScriptFileName"].Value.ToString()), exe);
                        returnValue = false;
					}
				}
			}
			return returnValue;
		}

        /// <summary>
        /// Checks to see if the specified script has a block against running more than once. If so, returns some data about it
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="cData">The ConnectionData object for the target database</param>
        /// <param name="databaseName">The name of the database that needs to be checked</param>
        /// <param name="scriptHash">out string for the hash of the script</param>
        /// <param name="scriptTextHash">out string for the hash of the parsed script</param>
        /// <param name="commitDate">out DateTime for the commit date that is blocking the re-run</param>
        /// <returns>True if there is a script block in place</returns>
        public static bool HasBlockingSqlLog(System.Guid scriptId ,ConnectionData cData,string databaseName, out string scriptHash, out string scriptTextHash,out DateTime commitDate)
		{
			bool hasBlock = false;
			scriptHash = string.Empty;
            scriptTextHash = string.Empty;
            commitDate = DateTime.MinValue;
            SqlCommand cmd = new SqlCommand("SELECT AllowScriptBlock,ScriptFileHash,CommitDate,ScriptText FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
			cmd.Parameters.AddWithValue("@ScriptId",scriptId);
			cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName,cData.SQLServerName,cData.UserId,cData.Password,cData.UseWindowAuthentication,2);
            try
            {
                cmd.Connection.Open();
                int i = 0;
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        if (i == 0)
                        {
                            scriptHash = (reader[1] == DBNull.Value) ? string.Empty : reader[1].ToString();
                            commitDate = (reader[2] == DBNull.Value) ? DateTime.MinValue : DateTime.Parse(reader[2].ToString());
                            scriptTextHash = (reader[3] == DBNull.Value) ? string.Empty : SqlBuildFileHelper.GetSHA1Hash(reader[3].ToString());
                            i++;
                        }

                        if (reader.GetSqlBoolean(0) == true)
                        {
                            hasBlock = true;
                            break;
                        }
                    }
                    reader.Close();
                }
                return hasBlock;
            }
            catch (SqlException)
            {
                //swallow the exception
                return false;
            }
            catch (Exception exe)
            {
                log.Warn(string.Format("Unable to check for blocking SQL for script {0} on database {1}.{2}", scriptId.ToString(), cmd.Connection.DataSource, cmd.Connection.Database), exe);
                return false;
            }
			finally
			{
				cmd.Connection.Close();
			}
		}
		/// <summary>
		/// Quick check to see if the specicified script has a block against it.
		/// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The BuildConnectData for the target database</param>
		/// <returns>True if there is a block</returns>
        private bool GetBlockingSqlLog(System.Guid scriptId ,ref BuildConnectData connData)
		{
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1", connData.Connection, connData.Transaction);
                cmd.Parameters.AddWithValue("@ScriptId", scriptId);
                object has = cmd.ExecuteScalar();
                if (has == null || has == DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch // most likely get here because the table doesn't exist?
            {
                return false;
            }
		}
        
        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public static ScriptRunLog GetScriptRunLog(System.Guid scriptId, ConnectionData connData)
		{
			try
			{
				SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
                ScriptRunLog log = new ScriptRunLog();
				cmd.Parameters.AddWithValue("@ScriptId",scriptId);
				cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
				SqlDataAdapter adapt = new SqlDataAdapter(cmd);
				DataSet ds = new DataSet();
                adapt.Fill(log);
                return log;
			}
			catch(Exception e)
			{
                log.Error(String.Format("Unable to retrieve script run log for {0} on database {1}.{2}", scriptId.ToString(), connData.SQLServerName, connData.DatabaseName), e);
				throw new ApplicationException("Error retrieving Script Run Log",e);
			}
		}

        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public static ScriptRunLog GetObjectRunHistoryLog(string objectFileName, ConnectionData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE [ScriptFileName] = @ScriptFileName ORDER BY CommitDate DESC");
                ScriptRunLog log = new ScriptRunLog();
                cmd.Parameters.AddWithValue("@ScriptFileName", objectFileName);
                cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(log);
                return log;
            }
            catch (Exception e)
            {
                log.Error(String.Format("Unable to retrieve object history for {0} on database {1}.{2}", objectFileName, connData.SQLServerName, connData.DatabaseName), e);
                throw new ApplicationException("Error retrieving Script Run Log", e);
            }
        }


		#endregion

		#region ## Script File Batching & IO Helper Methods ##
        #region New File Batching with Regex - honors text in comments
        public static List<string> ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter)
        {
            List<string> list = new List<string>();

            //Convert \n to \r\n
            scriptContents = scriptContents.ConvertNewLinetoCarriageReturnNewLine();

            //Find the "GO" delimiters that are not commented out or embedded in scripts
            List<KeyValuePair<Match, int>> activeDelimiters = FindActiveBatchDelimiters(scriptContents);
            if(activeDelimiters.Count == 0)
            {
                //A trim hack for backward compatability...
                //if(!scriptContents.EndsWith("\r\n\r\n"))
                scriptContents = scriptContents.ClearTrailingCarriageReturn();
                list.Add(scriptContents);
                return list;
            }
           
           
            //Needed when we are going to be removing the batch delimiter text.
            Regex regBackwardEndOfLine = null;
            int previousEndOfLine = 0;
            if (!maintainBatchDelimiter)
                regBackwardEndOfLine = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

            
            
            int startIndex = 0;
            int modStartIndex = 0;
            string scriptSubstring = string.Empty;
            foreach (KeyValuePair<Match, int> m in activeDelimiters)
            {
                if (maintainBatchDelimiter)
                {
                    //Want to include any whitespace after the delimiter up to the end of line...
                    if (m.Value <= 0)
                    {
                        scriptSubstring = scriptContents.Substring(startIndex, m.Key.Index + m.Key.Length - startIndex);
                        list.Add(scriptSubstring);
                        startIndex = m.Key.Index + m.Key.Length;
                    }
                    else
                    {
                        list.Add(scriptContents.Substring(startIndex, m.Value + 2 - startIndex));
                        startIndex = m.Value + 2;
                    }
                }
                else
                {
                    previousEndOfLine = regBackwardEndOfLine.Match(scriptContents, m.Key.Index).Index;
                    if (previousEndOfLine > 0)
                    {
                        if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                            startIndex = startIndex - 2;

                        modStartIndex = (startIndex == 0) ? startIndex : startIndex + Environment.NewLine.Length;
                        scriptSubstring = scriptContents.Substring(modStartIndex, m.Key.Index - modStartIndex);
                        scriptSubstring = scriptSubstring.ClearTrailingSpacesAndTabs();
                        list.Add(scriptSubstring);
                        startIndex = m.Key.Index + m.Key.Length;
                    }
                    else
                    {
                        scriptSubstring = scriptContents.Substring(startIndex, m.Key.Index - startIndex);
                        list.Add(scriptSubstring);
                        startIndex = m.Value + 2;
                    }

                }
            }

            //Get the last item into the collection...
            if (maintainBatchDelimiter)
            {
                list.Add(scriptContents.Substring(startIndex));
            }
            else
            {
                previousEndOfLine = regBackwardEndOfLine.Match(scriptContents, startIndex).Index;

                if (previousEndOfLine > 0)
                {
                    if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                        startIndex = startIndex - 2;

                    modStartIndex = (startIndex == 0) ? startIndex : startIndex + Environment.NewLine.Length;
                    modStartIndex = (modStartIndex > scriptContents.Length) ? scriptContents.Length : modStartIndex;
                    string lastItem = scriptContents.Substring(modStartIndex);
                    lastItem = lastItem.ClearTrailingCarriageReturn();
                    list.Add(lastItem);
                }
                else
                {
                    string lastItem = scriptContents.Substring(startIndex).ClearTrailingCarriageReturn();
                    list.Add(lastItem);
                }
            }

            //If the last item is actually just whitespace, remove it..
            if (list[list.Count - 1].Trim().Length == 0)
                list.RemoveAt(list.Count - 1);

            //Remove trailing \r\n in the last item...(can't remember why, but the old methods do this so it needs to be done for backward compatability
            if(scriptContents.Trim().EndsWith("GO",StringComparison.CurrentCultureIgnoreCase))
                list[list.Count - 1] = list[list.Count - 1].ClearTrailingCarriageReturn();

            //Remove transaction references if applicable
            if (stripTransaction)
            {
                for (int i = 0; i < list.Count; i++)
                    list[i] = RemoveTransactionReferences(list[i]);
            }

            //Remove any "USE" statements
            for (int i = 0; i < list.Count; i++)
                list[i] = RemoveUseStatement(list[i]);


            log.DebugFormat("Batched build package into {0} scripts", list.Count.ToString());
            return list;

        }
        
        private static List<KeyValuePair<Match, int>> FindActiveBatchDelimiters(string scriptContents)
        {
             //Regex for delimiter
            Regex regDelimiter= new Regex(Properties.Resources.RegexBatchParsingDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //Regex for seeing if delimiter is the only thing on the line
            Regex regNonWhiteSpace = new Regex(Properties.Resources.RegexNonWhiteSpace, RegexOptions.IgnoreCase);
            Regex regEndOfLine = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase);
  
            //First, identify all of the delimiters...
            MatchCollection collDelimiter =  regDelimiter.Matches(scriptContents);

            List<KeyValuePair<Match, int>> activeDelimiters = new List<KeyValuePair<Match, int>>();
            
            if(collDelimiter.Count == 0)
                return activeDelimiters;
                      

            //Find the delimiters that are "real"
            
            foreach (Match delim in collDelimiter)
            {
                if (!IsInComment(scriptContents, delim.Index))
                {
                    //at the end of the string.
                    if (delim.Index + delim.Length == scriptContents.Length)
                    {
                        activeDelimiters.Add(new KeyValuePair<Match,int>(delim,-1));
                        continue;
                    }


                    int nextChar = regNonWhiteSpace.Match(scriptContents, delim.Index + delim.Length).Index;
                    int endOfLine = regEndOfLine.Match(scriptContents, delim.Index + delim.Length).Index;
                    
                    if(endOfLine < nextChar || nextChar == 0 || endOfLine == 0)
                        activeDelimiters.Add(new KeyValuePair<Match,int>(delim,endOfLine));
                        //activeDelimiters.Add(delim);
                }
            }

            return activeDelimiters;
        }
        public static string RemoveUseStatement(string script)
        {
            Regex regUse = new Regex(Properties.Resources.RegexUseStatement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            int startAt = 0;
            while (regUse.Match(script, startAt).Success)
            {
                Match m = regUse.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regUse.Replace(script, "", 1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }
        public static string RemoveTransactionReferences(string script)
        {
            ////Regex for Transactions
            //Regex regTransaction = new Regex(Properties.Resources.RegexTransaction, RegexOptions.IgnoreCase);
            //Regex regTran = new Regex(Properties.Resources.RegexTran, RegexOptions.IgnoreCase);
            //Regex regCommit = new Regex(Properties.Resources.RegexCommit, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //Regex regTranLevel = new Regex(Properties.Resources.RegexTransactionLevel, RegexOptions.IgnoreCase);

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransaction, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTran, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexCommit, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransactionLevel, script, options);
          
            return script;
        }
        private static string RegexRemoveIfNotInComments(string regexExpression, string script, RegexOptions options)
        {
            Regex regRemoveTag = new Regex(regexExpression, options);
            int startAt = 0;
            while (regRemoveTag.Match(script, startAt).Success)
            {
                Match m = regRemoveTag.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regRemoveTag.Replace(script, "",1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }
       
        #endregion

        #region Old File Batching with string replace
        /// <summary>
		/// Breaks Sql file into batches
		/// </summary>
		/// <param name="fileName">Name of the file to parse</param>
		/// <param name="stripTransaction">Flag to strip out references to a transaction</param>
		/// <param name="maintainBatchDelimiter">Flag to keep or remove the batch delimiter ("GO")</param>
		/// <returns>String array for each command to run in batch</returns>
        public static string[] ReadBatchFromScriptFile(string fileName, bool stripTransaction, bool maintainBatchDelimiter)
        {

            //Procedured and functions should never have transaction text stripped..they may need it as part of their definition
            if (fileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                stripTransaction = false;


            //List<string> list = new List<string>();
            //string[] scriptLines = File.ReadAllLines(fileName);
            //string[] batch = ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
            //return batch;

            string scriptContents = File.ReadAllText(fileName);
            string[] batchNew = ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            return batchNew;
           
        }
        [Obsolete("This method is only meant for use in backward compatability testing",false)]
        public static string[] ReadBatchFromScript(bool stripTransaction, bool maintainBatchDelimiter, string scriptContents)
        {
            string[] scriptLines = scriptContents.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
        }
        [Obsolete("This method has been replaced with ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter)", false)]
        public static string[] ReadBatchFromScriptText(bool stripTransaction, bool maintainBatchDelimiter, string[] scriptLines)
        {
            StringBuilder sb = new StringBuilder();
            string completeScript = string.Empty;
            string currentLine = string.Empty;
            List<string> list = new List<string>();
            Regex usingStatement = new Regex("use \\S+", RegexOptions.IgnoreCase);
            for (int i = 0; i < scriptLines.Length; i++)
            {
                currentLine = scriptLines[i];
                if (currentLine.Trim().ToUpper() == BatchParsing.Delimiter && sb.Length != 0)
                {
                    if (maintainBatchDelimiter)
                        sb.Append(currentLine + "\r\n");

                    list.Add(sb.ToString());
                    sb.Length = 0;
                }
                else
                {

                    if (stripTransaction)
                    {
                        
                        currentLine = currentLine.Replace("BEGIN TRANSACTION", "");
                        currentLine = currentLine.Replace("BEGIN TRAN", "");
                        currentLine = currentLine.Replace("ROLLBACK TRANSACTION", "");
                        currentLine = currentLine.Replace("ROLLBACK TRAN", "");
                        currentLine = currentLine.Replace("COMMIT TRANSACTION", "");
                        currentLine = currentLine.Replace("COMMIT TRAN", "");
                        currentLine = currentLine.Replace("COMMIT", "");
                        currentLine = currentLine.Replace("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE", "");
                    }

                    //remove any "using" directives that could change the target database for the script. The build definition should be used instead
                    if (usingStatement.Match(currentLine).Success)
                    {
                        // if the match is the only thing on this line, then is is a true "using" statement, so remove it...
                        if (currentLine.Trim().Length == usingStatement.Match(currentLine).Value.Length)
                            currentLine = "";
                    }
                    sb.Append(currentLine + "\r\n");
                }
                
            }
            if (sb.ToString().Trim().Length > 0)
            {
                list.Add(sb.ToString());
            }
            string[] batch = list.ToArray();
            if (batch.Length > 0 && batch[batch.Length - 1].Length >= 2)
            {
                //Trim the last carriage return off the last item
                batch[batch.Length - 1] = batch[batch.Length - 1].Substring(0, batch[batch.Length - 1].Length - 2);
            }
            return batch;
        }
       
        #endregion
        public static ScriptBatchCollection LoadAndBatchSqlScripts(SqlSyncBuildData buildData, string projectFilePath)
        {
            ScriptBatchCollection coll = new ScriptBatchCollection();
            SqlSyncBuildData.ScriptDataTable scriptTable = SqlBuildHelper.GetScriptSourceTable(buildData);
            DataView view = scriptTable.DefaultView;
            view.Sort = scriptTable.BuildOrderColumn.ColumnName + " ASC ";
            for (int i = 0; i < view.Count; i++)
            {
                SqlSyncBuildData.ScriptRow myRow = (SqlSyncBuildData.ScriptRow)view[i].Row;
                string[] batchScripts = SqlBuildHelper.ReadBatchFromScriptFile(projectFilePath + myRow.FileName, myRow.StripTransactionText, false);

                ScriptBatch batch = new ScriptBatch(myRow.FileName, batchScripts, myRow.ScriptId);
                coll.Add(batch);
            }
            return coll;
        }
        #endregion

        #region ## SQL Connection Helper Methods ##
        private BuildConnectData GetConnectionDataClass(string serverName, string databaseName)
		{
			//always Upper case the database name
            string databaseKey = serverName.ToUpper() + ":" + databaseName.ToUpper();
            if (this.connectDictionary.ContainsKey(databaseKey) == false)
			{
				BuildConnectData cData = new BuildConnectData();
                cData.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName, serverName, this.connData.UserId, this.connData.Password, connData.UseWindowAuthentication, this.connData.ScriptTimeout);
				cData.Connection.Open();
				cData.HasLoggingTable = LogTableExists(cData.Connection);
				cData.Connection.InfoMessage +=new SqlInfoMessageEventHandler(Connection_InfoMessage);
                if(!databaseName.Equals(this.logToDatabaseName) && this.isTransactional) //we don't want a transaction for the logging database
				    cData.Transaction = cData.Connection.BeginTransaction(BuildTransaction.TransactionName);
				cData.DatabaseName = databaseName;
                cData.ServerName = serverName;

                this.connectDictionary.Add(databaseKey, cData);
				return cData;
			}
			else
			{
                return (BuildConnectData)this.connectDictionary[databaseKey];
			}
		}
		private bool CommitBuild()
		{
            //If we're not in a transaction, they've already committed.
            if (!this.isTransactional)
                return true;

            Dictionary<string, BuildConnectData>.KeyCollection keys = this.connectDictionary.Keys;
			bool success = true;
            foreach (string key in keys)
            {
				try
				{
                    log.InfoFormat("Committings transaction for {0}", key);
                    ((BuildConnectData)this.connectDictionary[key]).Transaction.Commit();
				}
				catch(Exception e)
				{
                    log.Error(String.Format("Error in CommitBuild Transaction.Commit() for database '{0}'", key), e);
					bgWorker.ReportProgress(100, new CommitFailureEventArgs(e.Message));
					success = false;
				}
				try
				{
                    log.DebugFormat("Closing connection for {0}", key);
                    ((BuildConnectData)this.connectDictionary[key]).Connection.Close();
				}
				catch(Exception e)
				{
                    log.WarnFormat(String.Format("Error in CommitBuild Connection.Close() for database '{0}'", key), e);
                    bgWorker.ReportProgress(100, new CommitFailureEventArgs(e.Message));
					success = false;
				}
			}

			return success;
		}
		private bool RollbackBuild()
		{
            //If we're not in a transaction, we can't roll back...
            if (!this.isTransactional)
            {
                log.Warn("Build is non-transactional. Unable to rollback");
                return false;
            }

            Dictionary<string, BuildConnectData>.KeyCollection keys = this.connectDictionary.Keys;
            foreach (string key in keys)
			{
				try
				{
                    log.InfoFormat("Rolling back transaction for {0}", key);
                    ((BuildConnectData)this.connectDictionary[key]).Transaction.Rollback();
				}
				catch(Exception e)
				{
                    log.Error(String.Format("Error in RollbackBuild Transaction.Rollback() for database '{0}'", key), e);
				}
				try
				{
                    log.DebugFormat("Closing connection for {0}", key);
                    ((BuildConnectData)this.connectDictionary[key]).Connection.Close();
				}
				catch(Exception e)
				{
                    log.Error(String.Format("Error in RollbackBuild Connection.Close() for database '{0}'", key), e);
				}
			}
           
			return true;
		}
		private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			StringBuilder messages = new StringBuilder();
			foreach(SqlError err in e.Errors)
			{
				messages.Append(err.Message+"\r\n");
			}
			if(this.myRunRow != null)
				this.myRunRow.Results += messages.ToString();

			this.sqlInfoMessage += messages.ToString();
		}
        //public bool TestDatabaseConnection(string databaseName)
        //{
        //    BuildConnectData cData = new BuildConnectData();
        //    try
        //    {
        //        cData.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName,this.connData.SQLServerName,this.connData.UserId,this.connData.Password,connData.UseWindowAuthentication,this.connData.ScriptTimeout);
        //        cData.Connection.Open();

        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        string error = ex.Message;
        //    }
        //    finally
        //    {
        //        if( cData != null )
        //            cData.Connection.Close();
        //        cData = null;
        //    }

        //    return false;
        //}
		#endregion

		#region ## DataSet Handling Helper Methods ##
		private SqlSyncBuildData.BuildRow GetNewBuildRow(string serverName)
		{
			if(File.Exists(this.buildHistoryXmlFile) == false)
				this.buildHistoryData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
			else
			{
				this.buildHistoryData = new SqlSyncBuildData();
				this.buildHistoryData.ReadXml(this.buildHistoryXmlFile);
			}


			//Create a root Project row if needed
			if(this.buildHistoryData.SqlSyncBuildProject.Rows.Count == 0)
                this.buildHistoryData.SqlSyncBuildProject.AddSqlSyncBuildProjectRow("", false);
			
			SqlSyncBuildData.SqlSyncBuildProjectRow projRow = (SqlSyncBuildData.SqlSyncBuildProjectRow)this.buildHistoryData.SqlSyncBuildProject.Rows[0];
			
			//Create a builds row if needed
			if(this.buildHistoryData.Builds.Rows.Count == 0)
				this.buildHistoryData.Builds.AddBuildsRow(projRow);
			
			SqlSyncBuildData.BuildsRow buildsRow = (SqlSyncBuildData.BuildsRow)this.buildHistoryData.Builds.Rows[0];

			//Set build id
			this.currentBuildId = System.Guid.NewGuid();

			//Create a build row for the new build
			SqlSyncBuildData.BuildDataTable buildTable = this.buildHistoryData.Build;
			SqlSyncBuildData.BuildRow myBuild =  buildTable.NewBuildRow();
			myBuild.BuildsRow = buildsRow;
			myBuild.Name = this.buildDescription;
			myBuild.BuildType = this.buildType;
			myBuild.BuildStart = DateTime.Now;
			myBuild.ServerName = serverName;
			myBuild.BuildId = this.currentBuildId.ToString();
			
			buildTable.AddBuildRow(myBuild);
			return myBuild;
		}

        /// <summary>
        /// Retrieves the ScriptRows from the SqlSyncBuildData object and imports them into a new ScriptDataTable
        /// This leaves the original object in tact.
        /// </summary>
        /// <returns>SqlSyncBuildData.ScriptDataTable populated with the ScriptRows from the SqlSyncBuildData object</returns>
		public static SqlSyncBuildData.ScriptDataTable GetScriptSourceTable(SqlSyncBuildData buildData)
		{
            if (buildData == null)
            {
                log.Warn("The SqlSyncBuildData object passed into \"GetScriptSourceTable\" was null. Unable to process build");
                return null;
            }
            if (buildData.Script == null)
            {
                log.Warn("The SqlSyncBuildData.ScriptTable object passed into \"GetScriptSourceTable\" was null. Unable to process build");
                return null;
            }

            try
            {
                IEnumerable<SqlSyncBuildData.ScriptRow> scriptRows = from s in buildData.Script select s;
                if (scriptRows.Count() > 0)
                {
                    SqlSyncBuildData.ScriptDataTable scriptTable = new SqlSync.SqlBuild.SqlSyncBuildData.ScriptDataTable();
                    foreach (SqlSyncBuildData.ScriptRow r in scriptRows)
                    {
                        scriptTable.ImportRow(r);
                    }
                    return scriptTable;
                }
            }
            catch(Exception exe)
            {
                log.Error("Unable to get script rows from SqlSyncBuildData object", exe);
            }
			return null;
		}
		private void SaveBuildDataSet(bool fireSavedEvent)
		{
			bgWorker.ReportProgress(0,new GeneralStatusEventArgs("Saving Build File Updates"));

            if (this.projectFileName == null || this.projectFileName.Length == 0)
            {
                string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

			this.buildData.WriteXml(this.projectFileName);


            if (this.buildHistoryXmlFile == null || this.buildHistoryXmlFile.Length == 0)
            {
                string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

			if(this.buildHistoryData != null)
				this.buildHistoryData.WriteXml(this.buildHistoryXmlFile);

            if(fireSavedEvent)
                   bgWorker.ReportProgress(0, new ScriptRunProjectFileSavedEventArgs(true));
		}
		#endregion

		#region ## Events ##
		//public event GeneralStatusEventHandler GeneralStatusEvent;
        //public event BuildScriptEventHandler BuildScriptEvent;
        //public event ScriptRunStatusEventHandler ScriptRunStatusEvent;
        //public event ScriptRunProjectFileSavedEventHandler ScriptRunProjectFileSavedEvent;
        //public event CommitFailureEventHandler CommitFailureEvent;
        public event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        //public event ScriptingErrorEventHandler ScriptingErrorEvent;
        public event BuildCommittedEventHandler BuildCommittedEvent;
        public event EventHandler BuildSuccessTrialRolledBackEvent;
        public event EventHandler BuildErrorRollBackEvent;
        //public event EventHandler BuildProcessingComplete;
		#endregion

        public static bool IsInComment(string rawScript, int index)
        {
            Regex regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase);
            Regex regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection comment = regDoubleDash.Matches(rawScript);
            for (int i = 0; i < comment.Count; i++)
            {
                if (index > comment[i].Index && index < comment[i].Index + comment[i].Length)
                    return true;
            }

            comment = regMultiLineComment.Matches(rawScript);
            for (int i = 0; i < comment.Count; i++)
            {
                if (index > comment[i].Index && index <= comment[i].Index + comment[i].Length)
                    return true;
            }
            return false;
        }


		private void SqlBuildHelper_ScriptLogWriteEvent(object sender, ScriptLogEventArgs e)
		{
            if (this.scriptLogFileName == null)
                throw new NullReferenceException("Attempting to write to Script Log File, but \"scriptLogFileName\" field value is null");

			if(File.Exists(this.scriptLogFileName) == false || e.InsertStartTransaction)
			{
				using(StreamWriter sw = File.AppendText(this.scriptLogFileName))
				{
                    sw.WriteLine("-- Start Time: "+ DateTime.Now.ToString() +" --");
                    if (this.isTransactional)
                    {
                        sw.WriteLine("-- Start Transaction --");
                        sw.WriteLine("BEGIN TRANSACTION");
                    }
                    else
                    {
                        sw.WriteLine("-- Executed without a transaction --");
                    }
				}
			}

           
			using(StreamWriter sw = File.AppendText(this.scriptLogFileName))
			{
				sw.WriteLine("/************************************");
				sw.WriteLine("Script #"+e.ScriptIndex.ToString()+"; Source File: "+e.SourceFile);
				sw.WriteLine("Server: "+this.connData.SQLServerName+"; Run On Database:"+e.Database+"  */");
				if(e.Database.Length > 0)
					sw.WriteLine("use "+e.Database+"\r\nGO\r\n");
				sw.WriteLine(e.SqlScript+"\r\nGO\r\n");
				sw.WriteLine("/*Script #"+e.ScriptIndex.ToString()+" Result: "+e.Results.Trim()+"  */");
				sw.Flush();
				sw.Close();
			}

            if (e.ScriptIndex == -10000 && this.externalScriptLogFileName.Length > 0)
            {
                using (StreamWriter sw = File.AppendText(this.scriptLogFileName))
                {
                    sw.WriteLine("-- END Time: " + DateTime.Now.ToString() + " --");
                    sw.Flush();
                    sw.Close();
                }

                try
                {
                    string tmpPath = Path.GetDirectoryName(this.externalScriptLogFileName);
                    if (!Directory.Exists(tmpPath))
                    {
                        bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Creating External Log file directory \"" + tmpPath + "\"" ));
                        Directory.CreateDirectory(tmpPath);
                    }

                    File.Copy(this.scriptLogFileName, this.externalScriptLogFileName,true);
                    bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Copied log file to \"" + this.externalScriptLogFileName + "\""));
                }
                catch(Exception exe)
                {
                    bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Error copying results file to " + this.externalScriptLogFileName +"\r\n"+exe.ToString()));
                }
            }

		}

		public string GetFromResources(string resourceName)
		{  
			System.Reflection.Assembly assem = this.GetType().Assembly;   
			using( System.IO.Stream stream = assem.GetManifestResourceStream(resourceName) )   
			{
				try      
				{ 
					using( System.IO.StreamReader reader = new StreamReader(stream) )         
					{
						return reader.ReadToEnd();         
					}
				}     
				catch(Exception e)      
				{
					throw new ApplicationException("Error retrieving from Resources. Tried '" 
						+ resourceName+"'\r\n"+e.ToString());      
				}
			}
		}
	}
}
