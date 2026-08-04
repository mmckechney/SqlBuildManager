using SqlBuildManager.SqlBuild.Models;

namespace SqlBuildManager.SqlBuild.Services
{
    internal sealed class DefaultBuildRetryPolicy : IBuildRetryPolicy
    {
        public bool ShouldRetry(Build result, int attemptIndex)
        {
            return result?.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout;
        }
    }
}
