using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild
{
    public interface IBuildRetryPolicy
    {
        bool ShouldRetry(Build result, int attemptIndex);
    }
}
