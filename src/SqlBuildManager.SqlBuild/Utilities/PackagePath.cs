using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using SqlBuildManager.SqlBuild.Models;

namespace SqlBuildManager.SqlBuild.Utilities
{
    public static class PackagePath
    {
        public static string NormalizeRelativePath(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException("Package file name must not be empty.");

            var parts = name.Replace('\\', '/').Split('/');
            foreach (var part in parts)
            {
                if (part.Length == 0 || part == "." || part == ".." ||
                    part.EndsWith(' ') || part.EndsWith('.') ||
                    part.Any(c => c < 32 || "<>:\"|?*".Contains(c)))
                    throw new InvalidDataException($"Invalid package file name '{name}'.");

                var stem = part.Split('.')[0].TrimEnd(' ').ToUpperInvariant();
                if (stem is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$" or "CLOCK$" ||
                    (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) ||
                    stem.StartsWith("LPT", StringComparison.Ordinal)) && "123456789\u00b9\u00b2\u00b3".Contains(stem[3])))
                    throw new InvalidDataException($"Reserved package file name '{name}'.");
            }
            return string.Join(Path.DirectorySeparatorChar, parts);
        }

        public static string Resolve(string root, string name)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidDataException("Package directory must not be empty.");
            var relative = NormalizeRelativePath(name);
            var fullRoot = Path.GetFullPath(root);
            var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relative));
            var prefix = Path.TrimEndingDirectorySeparator(fullRoot) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(prefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new InvalidDataException($"Package file '{name}' is outside its directory.");
            EnsureNoLinks(fullPath);
            return fullPath;
        }

        public static void EnsureNoLinks(string path)
        {
            for (var current = Path.GetFullPath(path); current != null; current = Path.GetDirectoryName(current))
            {
                try
                {
                    if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                        throw new InvalidDataException($"Package path contains a symbolic link or reparse point: '{current}'.");
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }
        }

        internal static Dictionary<string, ZipArchiveEntry> GetArchiveFiles(ZipArchive archive)
        {
            // Extraction historically flattens directories. Reject collisions on every OS.
            var files = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in archive.Entries)
            {
                var isDirectory = entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');
                var relative = NormalizeRelativePath(isDirectory ? entry.FullName[..^1] : entry.FullName);
                var unixType = (entry.ExternalAttributes >> 16) & 0xF000;
                if ((entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0 ||
                    (unixType != 0 && unixType != 0x8000 && unixType != 0x4000))
                    throw new InvalidDataException($"Package entry '{entry.FullName}' is not a regular file or directory.");
                if (isDirectory)
                    continue;
                var name = Path.GetFileName(relative);
                if (!files.TryAdd(name, entry))
                    throw new InvalidDataException($"Package entries collide on extracted file name '{name}'.");
            }
            return files;
        }

        internal static (string Name, SqlSyncBuildDataModel Model) ReadArchiveProject(Dictionary<string, ZipArchiveEntry> files)
        {
            var name = files.Keys.FirstOrDefault(n => n.Equals(XmlFileNames.MainProjectFile, StringComparison.Ordinal))
                ?? files.Keys.FirstOrDefault(n => n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException("Package does not contain project XML.");
            using var stream = files[name].Open();
            var model = SqlSyncBuildDataXmlSerializer.Load(XDocument.Load(stream));
            var members = files.Keys.ToHashSet(StringComparer.Ordinal);
            foreach (var script in model.Script)
            {
                var relative = NormalizeRelativePath(script.FileName ?? string.Empty);
                if (!members.Contains(relative))
                    throw new InvalidDataException($"Referenced script '{script.FileName}' is not a file in this package.");
            }
            return (name, model);
        }

        public static void ValidateScripts(SqlSyncBuildDataModel model, string root, bool requireFiles = true)
        {
            foreach (var script in model.Script)
            {
                var path = Resolve(root, script.FileName ?? string.Empty);
                if (requireFiles && !File.Exists(path))
                    throw new InvalidDataException($"Referenced script '{script.FileName}' does not exist in the package directory.");
            }
        }
    }
}
