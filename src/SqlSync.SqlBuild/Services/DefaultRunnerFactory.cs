namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Default implementation of IRunnerFactory that creates SqlBuildRunner instances.
    /// </summary>
    internal sealed class DefaultRunnerFactory : IRunnerFactory
    {
        public SqlBuildRunner Create(
            IConnectionsService connectionsService,
            ISqlBuildRunnerContext context,
            IBuildFinalizerContext finalizerContext,
            ISqlCommandExecutor executor = null)
        {
            return new SqlBuildRunner(connectionsService, context, finalizerContext, executor);
        }
    }
}
