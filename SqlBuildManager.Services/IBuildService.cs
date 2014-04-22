using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SqlSync.Connection;
using SqlBuildManager.Services.History;
namespace SqlBuildManager.Services
{
    [ServiceContract(Namespace = "http://schemas.mckechney.com/SqlBuildManager.Services/BuildService")]
    public interface IBuildService
    {
        [OperationContract]
        bool SubmitBuildPackage(BuildSettings settings);

        [OperationContract]
        IList<ConnectionTestResult> TestDatabaseConnectivity(ConnectionTestSettings settings);

        [OperationContract]
        ServiceStatus GetServiceStatus();

        [OperationContract]
        string GetLastExecutionCommitsLog();

        [OperationContract]
        string GetLastExecutionErrorsLog();

        [OperationContract]
        string GetServiceLogFile();

        [OperationContract]
        string GetDetailedDatabaseExecutionLog(string serverAndDatabase);

        [OperationContract]
        string GetServiceVersion();

        [OperationContract]
        IList<BuildRecord> GetServiceBuildHistory();

        [OperationContract]
        string GetSpecificCommitsLog(DateTime submittedDate);

        [OperationContract]
        string GetSpecificErrorsLog(DateTime submittedDate);

        [OperationContract]
        string GetSpecificDatabaseExecutionLog(DateTime submittedDate, string serverAndDatabase);

        [OperationContract]
        byte[] GetAllErrorLogsForExecution(DateTime submittedDate);

        [OperationContract]
        string GetLastFailuresDatabaseConfig();
    }
}
