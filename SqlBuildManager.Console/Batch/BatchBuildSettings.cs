using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using SqlSync.Connection;

namespace SqlBuildManager.Console.Batch
{
    public class BuildSettings
    {

        public string Description { get; set; } = string.Empty;
        public string LocalRootLoggingPath { get; set; } = string.Empty;
        public bool IsTrialBuild { get; set; } = false;
        public bool IsTransactional { get; set; } = true;
        public string[] MultiDbTextConfig { get; set; } = null;
        public string SqlBuildManagerProjectFileName { get; set; } = null;
        public System.Collections.ObjectModel.Collection<string> TargetServers { get; set; } = new System.Collections.ObjectModel.Collection<string>();
        public string AlternateLoggingDatabase { get; set; } = string.Empty;
        public string BuildRequestFrom { get; set; } = string.Empty;
        public int TimeoutRetryCount { get; set; } = 0;
        public string BuildRunGuid { get; set; } = string.Empty;
        public string DbUserName { get; set; } = string.Empty;
        public string DbPassword { get; set; } = string.Empty;
        public string PlatinumDacpacFileName { get; set; } = null;
        public string BuildRevision { get; set; } = null;
        public AuthenticationType AuthenticationType { get; set; } = SqlSync.Connection.AuthenticationType.UserNamePassword;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("\tSBM Project File Name: " + (this.SqlBuildManagerProjectFileName != null ? this.SqlBuildManagerProjectFileName : "null"));
            sb.AppendLine("\tDescription: " + (this.Description != null ? this.Description : "null"));
            sb.AppendLine("\tLocal Logging Path: " + (this.LocalRootLoggingPath != null ? this.LocalRootLoggingPath : "null"));
            int count = 0;
            if (this.TargetServers != null)
                count = this.TargetServers.Count;
            else if (this.MultiDbTextConfig != null)
                count = this.MultiDbTextConfig.Length;
            sb.AppendLine("\tTarget Server Count: " + count.ToString());
            sb.AppendLine("\tIs Trial Mode: " + this.IsTrialBuild.ToString());
            sb.AppendLine("\tIs Transactional: " + this.IsTransactional.ToString());
            sb.AppendLine("\tTimeout Retry Count: " + this.TimeoutRetryCount.ToString());
            sb.AppendLine("\tBuild Request From: " + this.BuildRequestFrom);
            return sb.ToString();
        }
    }
}
