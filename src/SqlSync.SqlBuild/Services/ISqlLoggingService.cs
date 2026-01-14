namespace SqlSync.SqlBuild.Services
{
    internal interface ISqlLoggingService
    {
        void EnsureLogTablePresence();
        bool LogCommittedScriptsToDatabase(System.Collections.Generic.List<SqlLogging.CommittedScript> committedScripts, MultiDb.MultiDbData multiDbRunData);
    }
}
