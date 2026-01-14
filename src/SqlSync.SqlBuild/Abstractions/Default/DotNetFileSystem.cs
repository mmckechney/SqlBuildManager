using System.IO;

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
    }
}
