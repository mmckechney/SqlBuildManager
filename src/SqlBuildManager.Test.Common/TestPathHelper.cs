using System;
using System.IO;
using System.Linq;

namespace SqlBuildManager.Test.Common
{
    /// <summary>
    /// Cross-platform path helpers for test log directory resolution.
    /// </summary>
    public static class TestPathHelper
    {
        /// <summary>
        /// Find the "Working" directory under loggingRoot, case-insensitively.
        /// Production code creates "Working" (capital W) but Linux is case-sensitive.
        /// </summary>
        public static string FindWorkingDirectory(string loggingRoot)
        {
            if (!Directory.Exists(loggingRoot))
                throw new DirectoryNotFoundException($"Logging root not found: {loggingRoot}");

            string? workingDir = Directory.GetDirectories(loggingRoot)
                .FirstOrDefault(d => Path.GetFileName(d).Equals("working", StringComparison.OrdinalIgnoreCase));

            if (workingDir == null)
            {
                string found = string.Join(", ", Directory.GetDirectories(loggingRoot).Select(Path.GetFileName));
                throw new DirectoryNotFoundException($"Unable to find working directory at root: {loggingRoot}. Found: [{found}]");
            }
            return workingDir;
        }

        /// <summary>
        /// Find the server log directory under the working directory.
        /// Handles named instances (e.g. "localhost\SQLEXPRESS") and plain server names.
        /// Falls back to single-directory heuristic.
        /// </summary>
        public static string FindServerLogDirectory(string loggingRoot, string serverName)
        {
            string workingDir = FindWorkingDirectory(loggingRoot);

            // Try exact match based on server name segments
            string[] serverParts = serverName.Split('\\');
            string candidatePath = Path.Combine(new[] { workingDir }.Concat(serverParts).ToArray());
            if (Directory.Exists(candidatePath))
                return candidatePath;

            // Fallback: if exactly one server directory exists, use it
            var serverDirs = Directory.GetDirectories(workingDir);
            if (serverDirs.Length == 1)
                return serverDirs[0];

            // Well-known fallbacks
            foreach (var candidate in new[] {
                Path.Combine(workingDir, "(local)", "SQLEXPRESS"),
                Path.Combine(workingDir, "localhost", "SQLEXPRESS"),
                Path.Combine(workingDir, "localhost") })
            {
                if (Directory.Exists(candidate))
                    return candidate;
            }

            string dirs = string.Join(", ", serverDirs.Select(Path.GetFileName));
            throw new DirectoryNotFoundException($"Unable to find server log directory at root: {loggingRoot}. Found under working/: [{dirs}]");
        }
    }
}
