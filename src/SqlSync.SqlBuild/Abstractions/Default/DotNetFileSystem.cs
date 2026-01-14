using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild
{
    internal sealed class DotNetFileSystem : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public void AppendAllText(string path, string contents) => File.AppendAllText(path, contents);
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public Stream OpenRead(string path) => File.OpenRead(path);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(path, contents, cancellationToken);
        public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(path, contents, cancellationToken);
        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) => File.ReadAllTextAsync(path, cancellationToken);
    }
}
