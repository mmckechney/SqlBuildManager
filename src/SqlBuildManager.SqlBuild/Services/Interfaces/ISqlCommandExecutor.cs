using System.Threading;
using System.Threading.Tasks;
using SqlBuildManager.SqlBuild.Models;

namespace SqlBuildManager.SqlBuild.Services
{
    internal interface ISqlCommandExecutor
    {
        SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional);
        Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default);
    }
}
