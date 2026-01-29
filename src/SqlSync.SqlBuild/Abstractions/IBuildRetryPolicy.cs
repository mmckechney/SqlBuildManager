using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Abstractions
{
    public interface IBuildRetryPolicy
    {
        bool ShouldRetry(Build result, int attemptIndex);
    }
}
