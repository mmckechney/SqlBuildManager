using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultTokenReplacementService : ITokenReplacementService
    {
        public string ReplaceTokens(string script, ISqlBuildRunnerProperties ctx)
        {
            if (ScriptTokens.regBuildDescription.Match(script).Success)
            {
                script = ScriptTokens.regBuildDescription.Replace(script, ctx.BuildDescription ?? string.Empty);
            }

            if (ScriptTokens.regBuildPackageHash.Match(script).Success)
            {
                script = ScriptTokens.regBuildPackageHash.Replace(script, ctx.BuildPackageHash ?? string.Empty);
            }

            if (ScriptTokens.regBuildFileName.Match(script).Success)
            {
                script = ScriptTokens.regBuildFileName.Replace(script, ctx.BuildFileName ?? "sbx file");
            }
            return script;
        }

        public Task<string> ReplaceTokensAsync(string script, ISqlBuildRunnerProperties ctx, CancellationToken cancellationToken = default)
            => Task.FromResult(ReplaceTokens(script, ctx));
    }
}
