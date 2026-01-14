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
    }
}
