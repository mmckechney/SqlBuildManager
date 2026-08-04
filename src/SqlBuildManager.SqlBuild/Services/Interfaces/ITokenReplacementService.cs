using System.Threading;
using System.Threading.Tasks;
namespace SqlBuildManager.SqlBuild.Services
{
    internal interface ITokenReplacementService
    {
        string ReplaceTokens(string script, ISqlBuildRunnerProperties ctx);
        Task<string> ReplaceTokensAsync(string script, ISqlBuildRunnerProperties ctx, CancellationToken cancellationToken = default);
    }
}
