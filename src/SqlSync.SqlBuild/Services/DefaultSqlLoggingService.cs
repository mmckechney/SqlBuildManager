using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.SqlLogging;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultSqlLoggingService : ISqlLoggingService
    {
        private readonly SqlBuildHelper _helper;
        public DefaultSqlLoggingService(SqlBuildHelper helper) => _helper = helper;

        public void EnsureLogTablePresence() => _helper.EnsureLogTablePresence();
        public bool LogCommittedScriptsToDatabase(List<CommittedScript> committedScripts, MultiDbData multiDbRunData)
            => _helper.LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);
    }
}
