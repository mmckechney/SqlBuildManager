using Microsoft.Data.SqlClient;
using Serilog.Debugging;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.Services
{
    public interface ISqlLoggingService
    {
        string EnsureLogTablePresence(Dictionary<string, BuildConnectData> connectDictionary, string logToDatabaseName);
        bool LogCommittedScriptsToDatabase(List<SqlSync.SqlBuild.SqlLogging.CommittedScript> committedScripts, ISqlBuildRunnerProperties runnerProperties, MultiDbData multiDbRunData);
        bool LogTableExists(SqlConnection conn);
    }
}
