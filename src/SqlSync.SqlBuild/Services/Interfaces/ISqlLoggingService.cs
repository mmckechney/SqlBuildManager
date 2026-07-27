using Microsoft.Data.SqlClient;
using Serilog.Debugging;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.MultiDb;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface ISqlLoggingService
    {
        Task<string> EnsureLogTablePresence(Dictionary<string, BuildConnectData> connectDictionary, string logToDatabaseName);
        Task<bool> LogCommittedScriptsToDatabase(List<SqlBuildManager.SqlBuild.SqlLogging.CommittedScript> committedScripts, ISqlBuildRunnerProperties runnerProperties, MultiDbData multiDbRunData);
        Task<bool> LogTableExists(DbConnection conn);
    }
}
