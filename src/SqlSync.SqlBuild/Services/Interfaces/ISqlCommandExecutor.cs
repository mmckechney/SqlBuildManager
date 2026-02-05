using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface ISqlCommandExecutor
    {
        SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional);
        Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default);
    }
}
