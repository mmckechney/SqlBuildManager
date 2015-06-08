using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlBuildManager.Interfaces.Console;
namespace SqlBuildManager.Services.History
{
    [Serializable]
    public class BuildRecord
    {
        private string requestedBy = string.Empty;

        public string RequestedBy
        {
            get { return requestedBy; }
            set { requestedBy = value; }
        }
        private string rootLogPath = string.Empty;

        public string RootLogPath
        {
            get { return rootLogPath; }
            set { rootLogPath = value; }
        }
        private ExecutionReturn returnValue = ExecutionReturn.Waiting;

        public ExecutionReturn ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        private string buildPackageName = string.Empty;

        public string BuildPackageName
        {
            get { return buildPackageName; }
            set { buildPackageName = value; }
        }

        private string platinumDacPacName = string.Empty;
        public string PlatinumDacPacName
        {
            get { return platinumDacPacName; }
            set { platinumDacPacName = value; }
        }

        private DateTime submissionDate = DateTime.MinValue;

        public DateTime SubmissionDate
        {
            get { return submissionDate; }
            set { submissionDate = value; }
        }



    }
}