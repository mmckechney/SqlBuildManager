using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlBuildManager.Console
{
    internal static class ExecutionOptions
    {
        internal const int DefaultConcurrency = 10;
        internal const int DefaultScriptTimeoutSeconds = 500;
        internal const int DefaultTimeoutRetryCount = 0;
        internal const int DefaultBatchNodeCount = 10;
        internal const int DefaultKubernetesPodCount = 10;
        internal const int DefaultContainerAppMaxCount = 10;
        internal const int DefaultBatchJobMonitorTimeoutMinutes = 30;

        internal const string DefaultBatchPoolName = "SqlBuildManagerPoolLinux";
        internal const string BatchJobIdFormat = "SqlBuildManagerJob{0}_{1}";
        internal const string BatchTargetFileFormat = "target_{0}.cfg";

        internal const int BatchJobNameMinLength = 3;
        internal const int BatchJobNameMaxLength = 41;
        internal const string BatchJobNamePattern = @"^[a-z0-9]+(-[a-z0-9]+)*$";

        internal const int SasClockSkewHours = 1;
        internal const int SasWriteDurationHours = 4;
        internal const int SasReadDurationHours = 7;
        internal const int QueueVisibilityRetryCount = 4;

        internal static readonly TimeSpan FastPollingInterval = TimeSpan.FromSeconds(1);
        internal static readonly TimeSpan StorageDeletionRetryInterval = TimeSpan.FromSeconds(3);
        internal static readonly TimeSpan MessagePollingInterval = TimeSpan.FromMilliseconds(500);
        internal static readonly TimeSpan DistributedPollingInterval = TimeSpan.FromSeconds(2);
        internal static readonly TimeSpan QueueWorkerPollingInterval = TimeSpan.FromSeconds(5);
        internal static readonly TimeSpan ResourceProvisioningPollingInterval = TimeSpan.FromSeconds(10);
        internal static readonly TimeSpan BatchNodePollingInterval = TimeSpan.FromSeconds(15);
        internal static readonly TimeSpan AciPollingInterval = TimeSpan.FromSeconds(15);
        internal static readonly TimeSpan StatusHeartbeatInterval = TimeSpan.FromSeconds(30);
        internal static readonly TimeSpan CleanupTimeout = TimeSpan.FromSeconds(30);
        internal static readonly TimeSpan CredentialProcessTimeout = TimeSpan.FromSeconds(60);
    }

    internal static class ExecutionOptionValidator
    {
        private static readonly Regex BatchJobNameRegex = new(
            ExecutionOptions.BatchJobNamePattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static IReadOnlyList<string> Validate(CommandLineArgs commandLine)
        {
            var errors = new List<string>();

            AddPositiveIntegerError(errors, commandLine.Concurrency, "--concurrency");
            AddPositiveIntegerError(errors, commandLine.DefaultScriptTimeout, "--defaultscripttimeout");

            return errors;
        }

        internal static IReadOnlyList<string> ValidateBatch(CommandLineArgs commandLine)
        {
            var errors = new List<string>();

            AddPositiveIntegerError(errors, commandLine.BatchArgs.BatchNodeCount, "--batchnodecount");
            AddPositiveIntegerError(errors, commandLine.BatchArgs.JobMonitorTimeout, "--batchjobmonitortimeout");

            return errors;
        }

        internal static IReadOnlyList<string> ValidateKubernetes(CommandLineArgs commandLine)
        {
            var errors = new List<string>();

            AddPositiveIntegerError(errors, commandLine.KubernetesArgs.PodCount, "--podcount");

            return errors;
        }

        internal static IReadOnlyList<string> ValidateContainerApp(CommandLineArgs commandLine)
        {
            var errors = new List<string>();

            AddPositiveIntegerError(errors, commandLine.ContainerAppArgs.MaxContainerCount, "--maxcontainers");

            return errors;
        }

        internal static bool TryValidateBatchJobName(string? batchJobName, bool required, out string error)
        {
            if (string.IsNullOrWhiteSpace(batchJobName))
            {
                error = required
                    ? $"The job name is required and must be {ExecutionOptions.BatchJobNameMinLength} to {ExecutionOptions.BatchJobNameMaxLength} lowercase alphanumeric or dash characters."
                    : string.Empty;
                return !required;
            }

            if (batchJobName.Length < ExecutionOptions.BatchJobNameMinLength ||
                batchJobName.Length > ExecutionOptions.BatchJobNameMaxLength ||
                !BatchJobNameRegex.IsMatch(batchJobName))
            {
                error = $"The job name must be lowercase, between {ExecutionOptions.BatchJobNameMinLength} and {ExecutionOptions.BatchJobNameMaxLength} characters, and contain only letters, numbers, or single dashes. Value provided: '{batchJobName}'.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static void AddPositiveIntegerError(List<string> errors, int value, string optionName)
        {
            if (value <= 0)
            {
                errors.Add($"{optionName} must be greater than zero. Value provided: {value}.");
            }
        }
    }
}
