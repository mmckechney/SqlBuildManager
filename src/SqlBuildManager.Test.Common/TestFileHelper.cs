using System;
using System.IO;

namespace SqlBuildManager.Test.Common
{
    /// <summary>
    /// Centralized temp file management for all test projects.
    /// </summary>
    public static class TestFileHelper
    {
        /// <summary>
        /// Creates a unique temp file with the given extension.
        /// Uses Path.GetTempFileName for atomic creation, then renames with a GUID-based name.
        /// Caller is responsible for tracking and cleaning up the returned path.
        /// </summary>
        public static string GetTrulyUniqueFile(string extension = "tmp")
        {
            if (extension.StartsWith(".")) extension = extension.TrimStart('.');
            string tmpName = Path.GetTempFileName();
            string dir = Path.GetDirectoryName(tmpName) ?? Path.GetTempPath();
            string newName = Path.Combine(dir, $"SqlBuildManagerTest-{Guid.NewGuid()}.{extension}");
            File.Move(tmpName, newName);
            return newName;
        }

        /// <summary>
        /// Best-effort cleanup of a list of temp files.
        /// </summary>
        public static void CleanupTempFiles(System.Collections.Generic.IEnumerable<string> files)
        {
            if (files == null) return;
            foreach (string file in files)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }
        }
    }
}
