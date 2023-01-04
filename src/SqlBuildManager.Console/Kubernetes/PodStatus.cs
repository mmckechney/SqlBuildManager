namespace SqlBuildManager.Console.Kubernetes
{
    internal enum PodStatus
    {
        Unknown,
        Running,
        Completed,
        Error,
        Pending,
        KubectlError
    }
}
