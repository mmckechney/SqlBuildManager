namespace SqlBuildManager.Console.Kubernetes
{
    internal enum PodStatus
    {
        Unknown,
        Creating,
        Running,
        Completed,
        Error,
        Pending,
        KubectlError
    }
}
