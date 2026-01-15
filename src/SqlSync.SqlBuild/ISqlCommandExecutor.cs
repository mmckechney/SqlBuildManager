using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild
{
    internal interface ISqlCommandExecutor
    {
        SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional);
        Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default);
    }
}
