using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.Connection;
namespace SqlSync.SqlBuild
{
    public class SqlBuildRunData
    {
        SqlSyncBuildData buildData = null;
        public SqlSyncBuildData BuildData
        {
            get { return buildData; }
            set { buildData = value; }
        }
        
        string buildType = string.Empty;
        public string BuildType
        {
            get { return buildType; }
            set { buildType = value; }
        }
        string server = string.Empty;
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        string buildDescription = string.Empty;
        public string BuildDescription
        {
            get { return buildDescription; }
            set { buildDescription = value; }
        }

        double startIndex = 0;
        public double StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; }
        }
       
        string projectFileName;
        public string ProjectFileName
        {
            get { return projectFileName; }
            set { projectFileName = value; }
        }
        
        bool isTrial = false;
        public bool IsTrial
        {
            get { return isTrial; }
            set { isTrial = value; }
        }
       
        double[] runItemIndexes = new double[0];
        public double[] RunItemIndexes
        {
            get { return runItemIndexes; }
            set { runItemIndexes = value; }
        }
       
        bool runScriptOnly = false;
        public bool RunScriptOnly
        {
            get { return runScriptOnly; }
            set { runScriptOnly = value; }
        }
       
        string buildFileName;
        public string BuildFileName
        {
            get { return buildFileName; }
            set { buildFileName = value; }
        }

        string logToDatabaseName = string.Empty;

        public string LogToDatabaseName
        {
            get { return logToDatabaseName; }
            set { logToDatabaseName = value; }
        }

        bool isTransactional = true;

        public bool IsTransactional
        {
            get { return isTransactional; }
            set { isTransactional = value; }
        }
        
        List<DatabaseOverride> targetDatabaseOverrides;

        public List<DatabaseOverride> TargetDatabaseOverrides
        {
            get { return targetDatabaseOverrides; }
            set { targetDatabaseOverrides = value; }
        }
    }
}
