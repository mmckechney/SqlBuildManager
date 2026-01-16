using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild
{
    internal sealed class DefaultBuildRetryPolicy : IBuildRetryPolicy
    {
        public bool ShouldRetry(Build result, int attemptIndex)
        {
            return result?.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout;
        }
    }
}
