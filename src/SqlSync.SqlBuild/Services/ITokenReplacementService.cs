using System.Threading;
using System.Threading.Tasks;
namespace SqlSync.SqlBuild.Services
{
    internal interface ITokenReplacementService
    {
        string ReplaceTokens(string script, SqlBuildHelper ctx);
        Task<string> ReplaceTokensAsync(string script, SqlBuildHelper ctx, CancellationToken cancellationToken = default);
    }
}
