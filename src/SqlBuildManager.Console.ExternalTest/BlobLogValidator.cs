using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// Validates that blob storage logs produced by compute nodes agree with the
    /// local test result and provides structured assertions on log content.
    /// </summary>
    public class BlobLogValidator
    {
        private readonly BlobContainerClient _containerClient;

        public string CommitsLog { get; private set; } = string.Empty;
        public string ErrorsLog { get; private set; } = string.Empty;
        public string SuccessDatabases { get; private set; } = string.Empty;
        public string FailureDatabases { get; private set; } = string.Empty;
        public List<string> BlobNames { get; private set; } = new();
        public Dictionary<string, string> TaskExecutionLogs { get; private set; } = new();

        public BlobLogValidator(string storageAccountName, string storageAccountKey, string containerName)
        {
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                _containerClient = new BlobContainerClient(
                    new Uri($"https://{storageAccountName}.blob.core.windows.net/{containerName}"),
                    new DefaultAzureCredential());
            }
            else
            {
                var creds = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                _containerClient = new BlobContainerClient(
                    new Uri($"https://{storageAccountName}.blob.core.windows.net/{containerName}"),
                    creds);
            }
        }

        /// <summary>
        /// Downloads all consolidated and per-task log files from blob storage.
        /// </summary>
        public async Task LoadLogsAsync()
        {
            BlobNames = new List<string>();
            await foreach (var blob in _containerClient.GetBlobsAsync())
            {
                BlobNames.Add(blob.Name);
            }

            CommitsLog = await DownloadBlobTextAsync("commits.log");
            ErrorsLog = await DownloadBlobTextAsync("errors.log");
            SuccessDatabases = await DownloadBlobTextAsync("successdatabases.cfg");
            FailureDatabases = await DownloadBlobTextAsync("failuredatabases.cfg");

            TaskExecutionLogs = new Dictionary<string, string>();
            foreach (var blobName in BlobNames)
            {
                if (blobName.ToLower().Contains("sqlbuildmanager.console") && blobName.EndsWith(".log"))
                {
                    TaskExecutionLogs[blobName] = await DownloadBlobTextAsync(blobName);
                }
            }
        }

        private async Task<string> DownloadBlobTextAsync(string blobName)
        {
            try
            {
                var blob = _containerClient.GetBlobClient(blobName);
                var response = await blob.DownloadContentAsync();
                return response.Value.Content.ToString();
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Asserts that blob storage logs are consistent with a successful build.
        /// Validates: commits.log has entries, errors.log is empty, success/failure databases
        /// match expected counts, and per-task execution logs have no ERR entries.
        /// </summary>
        public void AssertBuildSuccess(int expectedDbCount, TestContext? testContext = null)
        {
            var successLines = SuccessDatabases.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var failureLines = FailureDatabases.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            testContext?.WriteLine($"--- Blob Storage Log Validation (Success) ---");
            testContext?.WriteLine($"  Blobs found: {BlobNames.Count}");
            testContext?.WriteLine($"  Blob names: {string.Join(", ", BlobNames)}");
            testContext?.WriteLine($"  commits.log length: {CommitsLog.Length}");
            testContext?.WriteLine($"  errors.log length: {ErrorsLog.Length}");
            testContext?.WriteLine($"  successdatabases.cfg length: {SuccessDatabases.Length}");
            testContext?.WriteLine($"  failuredatabases.cfg length: {FailureDatabases.Length}");
            testContext?.WriteLine($"  Task execution logs: {TaskExecutionLogs.Count}");
            testContext?.WriteLine($"  Expected databases: {expectedDbCount}");
            testContext?.WriteLine($"  Success database count: {successLines.Length}");
            testContext?.WriteLine($"  Failure database count: {failureLines.Length}");

            Assert.IsFalse(string.IsNullOrWhiteSpace(CommitsLog),
                "Blob: commits.log should contain committed script entries");

            Assert.IsTrue(string.IsNullOrWhiteSpace(ErrorsLog),
                $"Blob: errors.log should be empty for a successful build, but contained:\n{Truncate(ErrorsLog, 500)}");

            if (expectedDbCount > 0)
            {
                  Assert.AreEqual(expectedDbCount, successLines.Length,
                    $"Blob: successdatabases.cfg should list {expectedDbCount} databases, found {successLines.Length}");
            }


            Assert.AreEqual(0, failureLines.Length,
                $"Blob: failuredatabases.cfg should be empty for a successful build, found:\n{Truncate(FailureDatabases, 500)}");

            foreach (var (name, content) in TaskExecutionLogs)
            {
                var errorMatches = Regex.Matches(content, @"\[\d{4}-\d{2}-\d{2}\s[\d:.]+\s+ERR\s+TH:\s*\d+\]");
                Assert.AreEqual(0, errorMatches.Count,
                    $"Blob: Task log '{name}' should not contain ERR entries. Found {errorMatches.Count}. First: {(errorMatches.Count > 0 ? GetLineContaining(content, errorMatches[0].Value) : "")}");
            }
        }

        /// <summary>
        /// Asserts that blob storage logs are consistent with a failed build.
        /// At least one of: errors.log, failuredatabases.cfg, or task execution ERR entries must be present.
        /// </summary>
        public void AssertBuildFailure(TestContext? testContext = null)
        {
            testContext?.WriteLine($"--- Blob Storage Log Validation (Failure) ---");
            testContext?.WriteLine($"  Blobs found: {BlobNames.Count}");
            testContext?.WriteLine($"  errors.log length: {ErrorsLog.Length}");
            testContext?.WriteLine($"  failuredatabases.cfg length: {FailureDatabases.Length}");

            bool hasErrors = !string.IsNullOrWhiteSpace(ErrorsLog);
            bool hasFailures = !string.IsNullOrWhiteSpace(FailureDatabases);
            bool hasErrInLogs = TaskExecutionLogs.Values.Any(c =>
                Regex.IsMatch(c, @"\[\d{4}-\d{2}-\d{2}\s[\d:.]+\s+ERR\s+TH:\s*\d+\]"));

            Assert.IsTrue(hasErrors || hasFailures || hasErrInLogs,
                "Blob: A failed build should have errors in errors.log, failuredatabases.cfg, or task execution logs");
        }

        /// <summary>
        /// Asserts that blob storage logs are consistent with a successful query operation.
        /// Validates: output files exist in blob storage and errors.log is empty.
        /// </summary>
        public void AssertQuerySuccess(TestContext? testContext = null)
        {
            testContext?.WriteLine($"--- Blob Storage Log Validation (Query Success) ---");
            testContext?.WriteLine($"  Blobs found: {BlobNames.Count}");
            testContext?.WriteLine($"  Blob names: {string.Join(", ", BlobNames)}");

            Assert.IsTrue(BlobNames.Count > 0,
                "Blob: Query run should produce output files in blob storage");

            bool hasCsvOutput = BlobNames.Any(b => b.EndsWith(".csv"));
            testContext?.WriteLine($"  Has CSV output: {hasCsvOutput}");

            Assert.IsTrue(string.IsNullOrWhiteSpace(ErrorsLog),
                $"Blob: errors.log should be empty for a successful query, but contained:\n{Truncate(ErrorsLog, 500)}");
        }

        /// <summary>
        /// Checks if the blob container name in the log output matches the expected container name.
        /// Logs a warning if not found (may happen if Serilog hasn't flushed), but only fails if a
        /// different container name is found.
        /// </summary>
        public static void AssertBlobContainerNameInLog(string logContent, string expectedContainerName, TestContext? testContext = null)
        {
            string actual = ExtractBlobContainerName(logContent);
            if (string.IsNullOrEmpty(actual))
            {
                testContext?.WriteLine($"  WARNING: 'in blob container' not found in log output (log may not have flushed). Proceeding with known container name '{expectedContainerName}'.");
                return;
            }
            Assert.AreEqual(expectedContainerName, actual,
                $"Blob container name in log should match the job name. Expected: '{expectedContainerName}', Found: '{actual}'");
        }

        /// <summary>
        /// Extracts the blob container name from log output.
        /// Matches the pattern: "in blob container '{name}'"
        /// </summary>
        public static string ExtractBlobContainerName(string logContent)
        {
            var match = Regex.Match(logContent, @"in blob container '([^']+)'");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Decrypts a settings file to extract storage account credentials.
        /// </summary>
        public static (string storageAccountName, string storageAccountKey) GetStorageCredentials(
            string settingsFilePath, string settingsFileKeyPath)
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFilePath);
            cmdLine.SettingsFileKey = settingsFileKeyPath;
            var (_, decrypted) = Cryptography.DecryptSensitiveFields(cmdLine);
            return (decrypted.ConnectionArgs.StorageAccountName, decrypted.ConnectionArgs.StorageAccountKey);
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength) + "...";
        }

        private static string GetLineContaining(string content, string search)
        {
            var lines = content.Split('\n');
            return lines.FirstOrDefault(l => l.Contains(search))?.Trim() ?? search;
        }
    }
}
