using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultTokenReplacementService : ITokenReplacementService
    {
        public string ReplaceTokens(string script, SqlBuildHelper ctx) => ctx.PerformScriptTokenReplacement(script);

        public Task<string> ReplaceTokensAsync(string script, SqlBuildHelper ctx, CancellationToken cancellationToken = default)
            => Task.FromResult(ctx.PerformScriptTokenReplacement(script));
    }
}
