namespace SqlBuildManager.Console.Kubernetes
{
    public class KubernetesFiles
    {
        public string RuntimeConfigMapFile { get; set; } = string.Empty;
        public string SecretsFile { get; set; } = string.Empty;
        public string JobFileName { get; set; } = string.Empty;
    }
}
