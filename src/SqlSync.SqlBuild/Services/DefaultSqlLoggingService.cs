using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SqlSync.Connection;
using SqlSync.SqlBuild.Abstractions;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using sqlLog = SqlSync.SqlBuild.SqlLogging;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultSqlLoggingService : ISqlLoggingService
    {
        private string sqlInfoMessage = string.Empty;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConnectionsService connectionsService;
        private readonly IProgressReporter progressReporter;

        public DefaultSqlLoggingService(IConnectionsService connectionsService, IProgressReporter progressReporter) 
        {
            this.connectionsService = connectionsService;
            this.progressReporter = progressReporter;
        }

        /// <summary>
        /// Ensures that the SqlBuild_Logging table exists and that it is setup properly. Self-heals if it is not. 
        /// </summary>
        public string EnsureLogTablePresence(Dictionary<string, BuildConnectData> connectDictionary, string logToDatabaseName)
        {
            //Self healing: add the table if needed
            sqlInfoMessage = string.Empty;
            SqlCommand createTableCmd = new SqlCommand(Properties.Resources.LoggingTable);
            Dictionary<string, BuildConnectData>.KeyCollection keys = connectDictionary.Keys;
            foreach (string key in keys)
            {
                try
                {

                    createTableCmd.Connection = ((BuildConnectData)connectDictionary[key]).Connection;
                    createTableCmd.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createTableCmd.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (createTableCmd.Connection.State == ConnectionState.Closed)
                        createTableCmd.Connection.Open();

                    if (((BuildConnectData)connectDictionary[key]).Transaction != null)
                        createTableCmd.Transaction = ((BuildConnectData)connectDictionary[key]).Transaction;

                    createTableCmd.ExecuteNonQuery();
                    log.LogDebug($"EnsureLogTablePresence Table Sql Messages for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}:\r\n{sqlInfoMessage}");
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error ensuring log table presence for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}");
                }
                finally
                {
                    createTableCmd.Connection.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
                }
            }
            //SqlCommand createCommitIndex = new SqlCommand(GetFromResources("SqlSync.SqlBuild.SqlLogging.LoggingTableCommitCheckIndex.sql"));
            SqlCommand createCommitIndex = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex);
            foreach (string key in keys)
            {
                sqlInfoMessage = string.Empty;
                try
                {
                    createCommitIndex.Connection = ((BuildConnectData)connectDictionary[key]).Connection;
                    createCommitIndex.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createCommitIndex.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;


                    if (createCommitIndex.Connection.State == ConnectionState.Closed)
                        createCommitIndex.Connection.Open();

                    if (((BuildConnectData)connectDictionary[key]).Transaction != null)
                        createCommitIndex.Transaction = ((BuildConnectData)connectDictionary[key]).Transaction;

                    createCommitIndex.ExecuteNonQuery();
                    log.LogDebug($"EnsureLogTablePresence Index Sql Messages for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}:\r\n{sqlInfoMessage}");

                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error ensuring log table commit check index for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}");
                }finally
                {
                    createCommitIndex.Connection.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
                }
            }
            log.LogDebug($"sqlInfoMessage value: {sqlInfoMessage}");
            log.LogDebug("Exiting EnsureLogPresence method");
            return sqlInfoMessage;
        }
        /// <summary>
        /// Checks to see that the logging table exists or not.
        /// </summary>
        /// <param name="conn">Connection object to the target database</param>
        /// <returns></returns>
        public bool LogTableExists(SqlConnection conn)
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
        public bool LogCommittedScriptsToDatabase(List<sqlLog.CommittedScript> committedScripts, ISqlBuildRunnerProperties runnerProperties, MultiDbData multiDbRunData)
        {
            bool returnValue = true;
            //If using an alternate database to log the commits to, we need to initiate the connection objects 
            //so that the EnsureLogTablePresence method catches them and creates the tables as needed.
            if (runnerProperties.LogToDatabaseName.Length > 0 && committedScripts.Count > 0)
            {
                List<string> servers = new List<string>();
                for (int i = 0; i < committedScripts.Count; i++)
                    if (!servers.Contains(committedScripts[i].ServerName))
                        servers.Add(committedScripts[i].ServerName);

                for (int i = 0; i < servers.Count; i++)
                {
                    BuildConnectData tmp = connectionsService.GetOrAddBuildConnectionDataClass(runnerProperties.ConnectionData, servers[i], runnerProperties.LogToDatabaseName, runnerProperties.IsTransactional);
                }
            }
            EnsureLogTablePresence(connectionsService.Connections, runnerProperties.LogToDatabaseName);

            //Get date from the server
            DateTime commitDate;
            SqlCommand cmd = null;
            string oldDb = runnerProperties.ConnectionData.DatabaseName;
            runnerProperties.ConnectionData.DatabaseName = "master";
            try
            {
                cmd = new SqlCommand("SELECT getdate()", SqlSync.Connection.ConnectionHelper.GetConnection(runnerProperties.ConnectionData));
                cmd.Connection.Open();
                commitDate = (DateTime)cmd.ExecuteScalar();
            }
            catch
            {
                commitDate = DateTime.Now;
                log.LogInformation($"Unable to getdate() from server/database {runnerProperties.ConnectionData.SQLServerName}-{runnerProperties.ConnectionData.DatabaseName}");
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                    cmd.Connection.Close();

                runnerProperties.ConnectionData.DatabaseName = oldDb;
            }

            //Add the commited scripts to the log
            SqlCommand logCmd = new SqlCommand(Properties.Resources.LogScript);
            if (!String.IsNullOrEmpty(runnerProperties.BuildFileName))
                logCmd.Parameters.AddWithValue("@BuildFileName", runnerProperties.BuildFileName);
            else
                logCmd.Parameters.AddWithValue("@BuildFileName", Path.GetFileName(runnerProperties.ProjectFileName));

            logCmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            logCmd.Parameters.AddWithValue("@CommitDate", commitDate);
            logCmd.Parameters.AddWithValue("@RunWithVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            logCmd.Parameters.AddWithValue("@BuildProjectHash", runnerProperties.BuildPackageHash);
            if (runnerProperties.BuildRequestedBy == string.Empty)
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", System.Environment.UserDomainName + "\\" + System.Environment.UserName);
            else
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", runnerProperties.BuildRequestedBy);

            if (runnerProperties.BuildDescription == null)
                logCmd.Parameters.AddWithValue("@Description", string.Empty);
            else
                logCmd.Parameters.AddWithValue("@Description", runnerProperties.BuildDescription);

            logCmd.Parameters.Add("@ScriptFileName", SqlDbType.VarChar);
            logCmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            logCmd.Parameters.Add("@ScriptFileHash", SqlDbType.VarChar);
            logCmd.Parameters.Add("@Sequence", SqlDbType.Int);
            logCmd.Parameters.Add("@ScriptText", SqlDbType.Text);
            logCmd.Parameters.Add("@Tag", SqlDbType.VarChar);
            logCmd.Parameters.Add("@TargetDatabase", SqlDbType.VarChar);
            logCmd.Parameters.Add("@ScriptRunStart", SqlDbType.DateTime);
            logCmd.Parameters.Add("@ScriptRunEnd", SqlDbType.DateTime);


            for (int i = 0; i < committedScripts.Count; i++)
            {
                try
                {

                    var script = committedScripts[i];
                    var row = runnerProperties.BuildDataModel.Script.FirstOrDefault(s => string.Equals(s.ScriptId, script.ScriptId.ToString(), StringComparison.OrdinalIgnoreCase));
                    if (row != null)
                    {


                        //progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Recording Commited Script: " + row.FileName));

                        BuildConnectData tmpConnDat;
                        if (runnerProperties.LogToDatabaseName.Length > 0)
                            tmpConnDat = connectionsService.GetBuildConnectionDataClass(script.ServerName, runnerProperties.LogToDatabaseName, runnerProperties.IsTransactional);
                        else
                            tmpConnDat = connectionsService.GetBuildConnectionDataClass(script.ServerName, script.DatabaseTarget, runnerProperties.IsTransactional);

                        log.LogInformation($"Recording Commited Script: {row.FileName} to {tmpConnDat.ServerName}:{tmpConnDat.DatabaseName}");

                        logCmd.Connection = tmpConnDat.Connection;
                        logCmd.Parameters["@ScriptFileName"].Value = row.FileName;
                        logCmd.Parameters["@ScriptId"].Value = script.ScriptId;
                        logCmd.Parameters["@ScriptFileHash"].Value = script.FileHash;
                        logCmd.Parameters["@Sequence"].Value = script.Sequence;
                        logCmd.Parameters["@ScriptText"].Value = script.ScriptText;
                        logCmd.Parameters["@Tag"].Value = script.Tag ?? "";
                        logCmd.Parameters["@TargetDatabase"].Value = script.DatabaseTarget;
                        logCmd.Parameters["@ScriptRunStart"].Value = script.RunStart;
                        logCmd.Parameters["@ScriptRunEnd"].Value = script.RunEnd;
                        if (logCmd.Connection.State == ConnectionState.Closed)
                            logCmd.Connection.Open();

                        if (tmpConnDat.Transaction != null)
                            logCmd.Transaction = tmpConnDat.Transaction;

                        logCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception sqlexe)
                {
                    log.LogError(sqlexe, $"Unable to log full text value for script {logCmd.Parameters["@ScriptFileName"].Value?.ToString()}. Inserting \"Error\" instead");
                    try
                    {
                        logCmd.Parameters["@ScriptText"].Value = "Error";
                        if (logCmd.Connection.State == ConnectionState.Closed)
                            logCmd.Connection.Open();

                        logCmd.ExecuteNonQuery();
                    }
                    catch (Exception exe)
                    {
                        log.LogError(exe, $"Unable to log commit for script {logCmd.Parameters["@ScriptFileName"].Value?.ToString()}.");
                        returnValue = false;
                    }
                }
            }

            if (logCmd.Transaction != null)
            {
                logCmd.Transaction.Commit();
            }
            return returnValue;
        }

        private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            StringBuilder messages = new StringBuilder();
            foreach (SqlError err in e.Errors)
            {
                messages.Append(err.Message + "\r\n");
            }

            sqlInfoMessage += messages.ToString();
        }
    }
}
