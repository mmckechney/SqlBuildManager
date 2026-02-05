using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    public interface IBuildRetryPolicy
    {
        bool ShouldRetry(Build result, int attemptIndex);
    }
}
