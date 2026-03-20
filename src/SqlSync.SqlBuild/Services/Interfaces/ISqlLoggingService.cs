using Microsoft.Data.SqlClient;
using Serilog.Debugging;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    public interface ISqlLoggingService
    {
        Task<string> EnsureLogTablePresence(Dictionary<string, BuildConnectData> connectDictionary, string logToDatabaseName);
        Task<bool> LogCommittedScriptsToDatabase(List<SqlSync.SqlBuild.SqlLogging.CommittedScript> committedScripts, ISqlBuildRunnerProperties runnerProperties, MultiDbData multiDbRunData);
        Task<bool> LogTableExists(DbConnection conn);
    }
}
