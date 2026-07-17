using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SqlBuildManager.RelayProxy;

internal sealed class DacpacRelayExtractor : IAsyncDisposable
{
    private const int MaximumJobs = 20;
    private static readonly TimeSpan CompletedJobRetention = TimeSpan.FromMinutes(10);

    private readonly string managedIdentityClientId;
    private readonly ConcurrentDictionary<Guid, DacpacJob> jobs = new();
    private readonly SemaphoreSlim extractionGate = new(1, 1);
    private readonly CancellationTokenSource shutdown = new();
    private readonly Task cleanupTask;

    public DacpacRelayExtractor(string managedIdentityClientId)
    {
        this.managedIdentityClientId = managedIdentityClientId;
        cleanupTask = CleanupLoopAsync();
    }

    public Guid Start(string server, string database)
    {
        RemoveExpiredJobs();
        if (jobs.Count >= MaximumJobs)
        {
            throw new InvalidOperationException("Too many DACPAC extraction jobs are active.");
        }

        var jobId = Guid.NewGuid();
        var job = new DacpacJob(Path.Combine(Path.GetTempPath(), $"{jobId:N}.dacpac"));
        job.Extraction = Task.Run(() => ExtractAsync(job, server, database));
        if (!jobs.TryAdd(jobId, job))
        {
            throw new InvalidOperationException("Unable to register the DACPAC extraction job.");
        }
        return jobId;
    }

    public bool IsReady(Guid jobId)
    {
        var job = GetJob(jobId);
        job.Touch();
        return job.Extraction.IsCompleted;
    }

    public async Task CopyResultAndRemoveAsync(Guid jobId, Stream destination)
    {
        var job = GetJob(jobId);
        job.Touch();
        if (!job.Extraction.IsCompleted)
        {
            throw new InvalidOperationException("The DACPAC extraction job is not complete.");
        }

        try
        {
            await job.Extraction;
            await using var source = File.OpenRead(job.FilePath);
            await source.CopyToAsync(destination);
        }
        finally
        {
            RemoveJob(jobId, job);
        }
    }

    private DacpacJob GetJob(Guid jobId) =>
        jobs.TryGetValue(jobId, out var job)
            ? job
            : throw new KeyNotFoundException("DACPAC extraction job was not found.");

    private async Task ExtractAsync(DacpacJob job, string server, string database)
    {
        await extractionGate.WaitAsync(shutdown.Token);
        var timer = Stopwatch.StartNew();
        try
        {
            Console.WriteLine($"Starting DACPAC extraction for {server}/{database}.");
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                Encrypt = true,
                TrustServerCertificate = false,
                ConnectTimeout = 30,
                Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                UserID = managedIdentityClientId
            }.ConnectionString;
            var options = new DacExtractOptions
            {
                IgnoreExtendedProperties = true,
                IgnoreUserLoginMappings = true,
                LongRunningCommandTimeout = 300,
                CommandTimeout = 300,
                DatabaseLockTimeout = 300
            };
            var service = new DacServices(connectionString);
            service.Extract(
                job.FilePath,
                database,
                "Sql Build Manager",
                typeof(DacpacRelayExtractor).Assembly.GetName().Version!,
                "Sql Build Manager",
                null,
                options);
            Console.WriteLine(
                $"Completed DACPAC extraction for {server}/{database} in {timer.Elapsed.TotalSeconds:F1} seconds.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"DACPAC extraction failed for {server}/{database}: {exception}");
            throw;
        }
        finally
        {
            extractionGate.Release();
        }
    }

    private async Task CleanupLoopAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(shutdown.Token))
            {
                RemoveExpiredJobs();
            }
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
        }
    }

    private void RemoveExpiredJobs()
    {
        var cutoff = DateTimeOffset.UtcNow - CompletedJobRetention;
        foreach (var pair in jobs)
        {
            if (pair.Value.Extraction.IsCompleted &&
                pair.Value.LastAccessUtc < cutoff)
            {
                RemoveJob(pair.Key, pair.Value);
            }
        }
    }

    private void RemoveJob(Guid jobId, DacpacJob job)
    {
        if (jobs.TryRemove(new KeyValuePair<Guid, DacpacJob>(jobId, job)))
        {
            File.Delete(job.FilePath);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await shutdown.CancelAsync();
        await cleanupTask;
        foreach (var pair in jobs)
        {
            try
            {
                await pair.Value.Extraction;
            }
            catch
            {
            }
            RemoveJob(pair.Key, pair.Value);
        }
        extractionGate.Dispose();
        shutdown.Dispose();
    }

    private sealed class DacpacJob
    {
        public DacpacJob(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }
        public Task Extraction { get; set; } = Task.CompletedTask;
        public DateTimeOffset LastAccessUtc { get; private set; } = DateTimeOffset.UtcNow;

        public void Touch() => LastAccessUtc = DateTimeOffset.UtcNow;
    }
}
