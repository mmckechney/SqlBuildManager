using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.IntegrationTest;

public sealed record RuntimeContainerResult(int ExitCode, string StandardOutput, string StandardError)
{
    public string Output => StandardOutput + StandardError;
}

public static class RuntimeContainerClient
{
    public static Task<RuntimeContainerResult> RunVersionAsync(CancellationToken cancellationToken = default) =>
        RunAsync(cancellationToken, "--version");

    public static async Task<RuntimeContainerResult[]> RunManyAsync(
        int containerCount,
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        if (containerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(containerCount));
        }

        var rootLoggingPathIndex = Array.IndexOf(arguments, "--rootloggingpath");
        if (rootLoggingPathIndex < 0 || rootLoggingPathIndex + 1 >= arguments.Length)
        {
            throw new InvalidOperationException("Runtime-container arguments must include --rootloggingpath.");
        }

        var runRoot = Directory.GetParent(arguments[rootLoggingPathIndex + 1])?.FullName
            ?? throw new InvalidOperationException("Unable to determine the runtime test results directory.");
        var results = await Task.WhenAll(
            Enumerable.Range(0, containerCount)
                .Select(index => RunAsync(cancellationToken, WithContainerPaths(arguments, runRoot, index + 1))));

        for (var index = 0; index < results.Length; index++)
        {
            var containerDirectory = Path.Combine(runRoot, $"runtime-container-{index + 1}");
            Directory.CreateDirectory(containerDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(containerDirectory, "console-output.log"),
                results[index].Output,
                cancellationToken);
        }

        return results;
    }

    public static async Task<RuntimeContainerResult> RunAsync(
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        var image = RequiredEnvironment("SBM_RUNTIME_IMAGE");
        var network = RequiredEnvironment("SBM_RUNTIME_NETWORK");
        var volume = RequiredEnvironment("SBM_TEST_RESULTS_VOLUME");
        var blobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        var name = $"sbm-runtime-test-{Guid.NewGuid():N}";

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var value in new[]
        {
            "run", "--rm", "--name", name, "--network", network,
            "--mount", $"type=volume,source={volume},target=/tests/TestResults",
        })
        {
            startInfo.ArgumentList.Add(value);
        }
        if (!string.IsNullOrWhiteSpace(blobEndpoint))
        {
            startInfo.ArgumentList.Add("--env");
            startInfo.ArgumentList.Add($"SBM_BLOB_ENDPOINT={blobEndpoint}");
        }
        startInfo.ArgumentList.Add(image);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start the Docker CLI.");
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return new RuntimeContainerResult(process.ExitCode, await stdout, await stderr);
    }

    private static string RequiredEnvironment(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"{name} must be set for runtime-container tests.");

    private static string[] WithContainerPaths(string[] arguments, string runRoot, int containerNumber)
    {
        var isolatedArguments = arguments.ToArray();
        var rootLoggingPathIndex = Array.IndexOf(isolatedArguments, "--rootloggingpath");
        if (rootLoggingPathIndex < 0 || rootLoggingPathIndex + 1 >= isolatedArguments.Length)
        {
            throw new InvalidOperationException("Runtime-container arguments must include --rootloggingpath.");
        }

        isolatedArguments[rootLoggingPathIndex + 1] =
            Path.Combine(runRoot, $"runtime-container-{containerNumber}", "logs");

        var monitorIndex = Array.IndexOf(isolatedArguments, "--monitor");
        if (monitorIndex < 0 || monitorIndex + 1 >= isolatedArguments.Length)
        {
            throw new InvalidOperationException("Runtime-container arguments must include --monitor.");
        }

        isolatedArguments[monitorIndex + 1] = "false";
        return isolatedArguments;
    }
}
