namespace SqlSync.SqlBuild
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        void AppendAllText(string path, string contents);
        void WriteAllText(string path, string contents);
        string ReadAllText(string path);
        System.IO.Stream OpenRead(string path);
        void CreateDirectory(string path);

        System.Threading.Tasks.Task AppendAllTextAsync(string path, string contents, System.Threading.CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task WriteAllTextAsync(string path, string contents, System.Threading.CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Threading.CancellationToken cancellationToken = default);
    }
}
