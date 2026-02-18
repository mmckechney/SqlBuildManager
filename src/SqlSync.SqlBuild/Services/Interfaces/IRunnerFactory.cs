namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Factory for creating SqlBuildRunner instances.
    /// </summary>
    internal interface IRunnerFactory
    {
        SqlBuildRunner Create(
            IConnectionsService connectionsService,
            ISqlBuildRunnerContext context,
            IBuildFinalizerContext finalizerContext,
            ISqlCommandExecutor executor = null,
            ITransactionManager transactionManager = null,
            IBuildFinalizer buildFinalizer = null,
            ISqlLoggingService sqlLoggingService = null);
    }
}
