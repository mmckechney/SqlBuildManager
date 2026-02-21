using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.Status
{
    class StatusReportRunner
    {
        SqlSyncBuildDataModel buildDataModel;
        string serverName;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }
        private string baseDatabase = string.Empty;

        public string BaseDatabase
        {
            get { return baseDatabase = null!; }
            set { baseDatabase = value; }
        }

        List<DatabaseOverride> dbOverrides;
        private StatusDataCollection status = new StatusDataCollection();
        private string projectFilePath;
        public StatusDataCollection Status
        {
            get { return status; }
            set { status = value; }
        }

        private IDatabaseUtility dbUtil { get; }
        public StatusReportRunner(IDatabaseUtility dbUtil, SqlSyncBuildDataModel buildDataModel, string serverName, List<DatabaseOverride> dbOverrides, string projectFilePath)
        {
            this.buildDataModel = buildDataModel;
            this.serverName = serverName;
            this.dbOverrides = dbOverrides;
            this.projectFilePath = projectFilePath;
            this.dbUtil = dbUtil;
        }
        public void RetrieveStatus()
        {
            DateTime commitDate;
            DateTime serverChangeDate;
            Connection.ConnectionData connData;
            string databaseName;
            foreach (Script script in buildDataModel.Script)
            {
                databaseName = ConnectionHelper.GetTargetDatabase(script.Database ?? string.Empty, dbOverrides);

                if (baseDatabase == null)
                    baseDatabase = databaseName;

                ScriptStatusData dat = new ScriptStatusData();
                connData = new SqlSync.Connection.ConnectionData(serverName, databaseName);

                ScriptStatusType stat = StatusHelper.DetermineScriptRunStatus(dbUtil, script, connData, projectFilePath, true, dbOverrides, out commitDate, out serverChangeDate);
                dat.Fill(script);
                dat.DatabaseName = databaseName;
                dat.ServerName = serverName;
                dat.ServerChangeDate = serverChangeDate;
                dat.LastCommitDate = commitDate;
                dat.ScriptStatus = stat;

                status.Add(dat);

            }
        }

    }
}
