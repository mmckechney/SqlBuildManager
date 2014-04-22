using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;
using SqlBuildManager.Interfaces.Console;
namespace SqlBuildManager.Services
{
    [DataContract()]
    public class ServiceStatus
    {
        public ServiceStatus(ServiceReadiness readiness, ExecutionReturn executionStatus, string currentVersion)
        {
            this.Readiness = readiness;
            this.ExecutionStatus = executionStatus;
            this.currentVersion = currentVersion;
        }

        private ServiceReadiness readiness;

        [DataMember()]
        public ServiceReadiness Readiness
        {
            get { return readiness; }
            set { readiness = value; }
        }
        private ExecutionReturn executionStatus;

        [DataMember()]
        public ExecutionReturn ExecutionStatus
        {
            get { return executionStatus; }
            set { executionStatus = value; }
        }

        private string currentVersion;

        [DataMember()]
        public string CurrentVersion
        {
            get { return currentVersion; }
            set { currentVersion = value; }
        }
    }


    [DataContract()]
    public enum ServiceReadiness
    {
        [EnumMember]
        ReadyToAccept,
        [EnumMember]
        PackageAccepted,
        [EnumMember]
        PackageValidationError,
        [EnumMember]
        Processing,
        [EnumMember]
        Error,
        [EnumMember]
        Unknown,
        [EnumMember]
        Unreachable,
        [EnumMember]
        ProcessingCompletedSuccessfully

    }
}
