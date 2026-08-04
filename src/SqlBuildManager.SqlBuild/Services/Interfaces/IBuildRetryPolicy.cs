using SqlBuildManager.SqlBuild.Models;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface IBuildRetryPolicy
    {
        bool ShouldRetry(Build result, int attemptIndex);
    }
}
