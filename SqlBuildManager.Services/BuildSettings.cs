using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace SqlBuildManager.Services
{
    [DataContract]
    public class BuildSettings
    {


        private string description = string.Empty;

        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
       
        private string localRootLoggingPath = string.Empty;

        [DataMember]
        public string LocalRootLoggingPath
        {
            get { return localRootLoggingPath; }
            set { localRootLoggingPath = value; }
        }
        private bool isTrialBuild = false;
        [DataMember]
        public bool IsTrialBuild
        {
            get { return isTrialBuild; }
            set { isTrialBuild = value; }
        }
        private bool isTransactional = true;
        [DataMember]
        public bool IsTransactional
        {
            get { return isTransactional; }
            set { isTransactional = value; }
        }

        private string[] multiDbTextConfig = null;
        [DataMember]
        public string[] MultiDbTextConfig
        {
            get { return multiDbTextConfig; }
            set { multiDbTextConfig = value; }
        }
        private byte[] sqlBuildManagerProjectContents = null;
        [DataMember]
        public byte[] SqlBuildManagerProjectContents
        {
            get { return sqlBuildManagerProjectContents; }
            set { sqlBuildManagerProjectContents = value; }
        }

        private string sqlBuildManagerProjectFileName = null;
        [DataMember]
        public string SqlBuildManagerProjectFileName
        {
            get { return sqlBuildManagerProjectFileName; }
            set { sqlBuildManagerProjectFileName = value; }
        }
        private System.Collections.ObjectModel.Collection<string> targetServers = new System.Collections.ObjectModel.Collection<string>();
        [DataMember]
        public System.Collections.ObjectModel.Collection<string> TargetServers
        {
            get { return targetServers; }
            set { targetServers = value; }
        }

        private string alternateLoggingDatabase = string.Empty;
        [DataMember]
        public string AlternateLoggingDatabase
        {
            get { return alternateLoggingDatabase; }
            set { alternateLoggingDatabase = value; }
        }
        private string buildRequestFrom = string.Empty;
        [DataMember]
        public string BuildRequestFrom
        {
            get { return buildRequestFrom; }
            set { buildRequestFrom = value; }
        }

        private int timeoutRetryCount = 0;
        [DataMember]
        public int TimeoutRetryCount
        {
            get { return timeoutRetryCount; }
            set { timeoutRetryCount = value; }
        }

        private string buildRunGuid = string.Empty;
        [DataMember(IsRequired=false)]
        public string BuildRunGuid
        {
            get { return buildRunGuid; }
            set { buildRunGuid = value; }
        }

        private string dbUserName = string.Empty;
        [DataMember(IsRequired = false)]
        public string DbUserName
        {
            get { return dbUserName; }
            set { dbUserName = value; }
        }

        private string dbPassword = string.Empty;
        [DataMember(IsRequired = false)]
        public string DbPassword
        {
            get { return dbPassword; }
            set { dbPassword = value; }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("\tSBM Project File Name: " + (this.sqlBuildManagerProjectFileName != null ? this.sqlBuildManagerProjectFileName : "null"));
            sb.AppendLine("\tDescription: " + (this.description != null ? this.description : "null"));
            sb.AppendLine("\tLocal Logging Path: " + (this.localRootLoggingPath != null ? this.localRootLoggingPath : "null"));
           int count = 0;
            if (this.targetServers != null)
                count = this.targetServers.Count;
            else if (this.multiDbTextConfig != null)
                count = this.multiDbTextConfig.Length;
            sb.AppendLine("\tTarget Server Count: " + count.ToString());
            sb.AppendLine("\tIs Trial Mode: " + this.isTrialBuild.ToString());
            sb.AppendLine("\tIs Transactional: " + this.isTransactional.ToString());
            sb.AppendLine("\tTimeout Retry Count: " + this.timeoutRetryCount.ToString());
            sb.AppendLine("\tBuild Request From: " + this.buildRequestFrom);
            return sb.ToString();
        }
    }
}
