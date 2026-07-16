using Azure.Core;
using Azure.Messaging.EventHubs.Consumer;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace SqlBuildManager.RelayProxy;

internal sealed class EventHubRelayMonitor : IAsyncDisposable
{
    private static readonly TimeSpan SessionIdleTimeout = TimeSpan.FromMinutes(10);
    private readonly string namespaceName;
    private readonly string eventHubName;
    private readonly TokenCredential credential;
    private readonly ConcurrentDictionary<Guid, MonitorSession> sessions = new();
    private readonly CancellationTokenSource shutdown = new();
    private readonly Task cleanupTask;

    public EventHubRelayMonitor(string namespaceName, string eventHubName, TokenCredential credential)
    {
        this.namespaceName = namespaceName;
        this.eventHubName = eventHubName;
        this.credential = credential;
        cleanupTask = CleanupIdleSessionsLoopAsync();
    }

    public async Task<Guid> StartAsync(EventMonitorStartRequest request, CancellationToken cancellationToken)
    {
        await RemoveIdleSessionsAsync();
        if (!string.Equals(request.NamespaceName, namespaceName, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(request.EventHubName, eventHubName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The requested Event Hub is not configured for this proxy.");
        }
        if (string.IsNullOrWhiteSpace(request.ConsumerGroup))
        {
            throw new ArgumentException("An Event Hub consumer group is required.");
        }

        var session = new MonitorSession(
            request.ConsumerGroup,
            namespaceName,
            eventHubName,
            request.StartTimeUtc,
            credential);
        try
        {
            await session.StartAsync(cancellationToken);
        }
        catch
        {
            await session.DisposeAsync();
            throw;
        }
        var sessionId = Guid.NewGuid();
        if (!sessions.TryAdd(sessionId, session))
        {
            await session.DisposeAsync();
            throw new InvalidOperationException("Unable to register Event Hub monitor session.");
        }
        return sessionId;
    }

    public async Task<IReadOnlyList<EventMonitorItem>> PollAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        if (!sessions.TryGetValue(sessionId, out var session))
        {
            throw new KeyNotFoundException("Event Hub monitor session was not found.");
        }
        session.Touch();
        await RemoveIdleSessionsAsync();
        return await session.PollAsync(cancellationToken);
    }

    private async Task RemoveIdleSessionsAsync()
    {
        var cutoff = DateTimeOffset.UtcNow - SessionIdleTimeout;
        foreach (var pair in sessions)
        {
            if (pair.Value.LastAccessUtc < cutoff &&
                sessions.TryRemove(pair.Key, out var idleSession))
            {
                await idleSession.DisposeAsync();
            }
        }
    }

    private async Task CleanupIdleSessionsLoopAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(shutdown.Token))
            {
                await RemoveIdleSessionsAsync();
            }
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
        }
    }

    public async Task<bool> StopAsync(Guid sessionId)
    {
        if (!sessions.TryRemove(sessionId, out var session))
        {
            return false;
        }
        await session.DisposeAsync();
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await shutdown.CancelAsync();
        await cleanupTask;
        foreach (var sessionId in sessions.Keys)
        {
            await StopAsync(sessionId);
        }
        shutdown.Dispose();
    }

    private sealed class MonitorSession : IAsyncDisposable
    {
        private readonly EventHubConsumerClient client;
        private readonly DateTimeOffset startTime;
        private readonly CancellationTokenSource shutdown = new();
        private readonly Channel<EventMonitorItem> events = Channel.CreateBounded<EventMonitorItem>(
            new BoundedChannelOptions(5000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        private Task readers = Task.CompletedTask;
        public DateTimeOffset LastAccessUtc { get; private set; } = DateTimeOffset.UtcNow;

        public void Touch() => LastAccessUtc = DateTimeOffset.UtcNow;

        public MonitorSession(
            string consumerGroup,
            string namespaceName,
            string eventHubName,
            DateTimeOffset startTime,
            TokenCredential credential)
        {
            this.startTime = startTime;
            client = new EventHubConsumerClient(
                consumerGroup,
                $"{namespaceName}.servicebus.windows.net",
                eventHubName,
                credential);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var partitionIds = await client.GetPartitionIdsAsync(cancellationToken);
            readers = Task.WhenAll(partitionIds.Select(ReadPartitionAsync));
        }

        public async Task<IReadOnlyList<EventMonitorItem>> PollAsync(CancellationToken cancellationToken)
        {
            Touch();
            if (readers.IsFaulted)
            {
                await readers;
            }

            using var waitTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            waitTimeout.CancelAfter(TimeSpan.FromSeconds(20));
            try
            {
                await events.Reader.WaitToReadAsync(waitTimeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return [];
            }

            var result = new List<EventMonitorItem>(200);
            while (result.Count < 200 && events.Reader.TryRead(out var item))
            {
                result.Add(item);
            }
            return result;
        }

        private async Task ReadPartitionAsync(string partitionId)
        {
            try
            {
                await foreach (var partitionEvent in client.ReadEventsFromPartitionAsync(
                    partitionId,
                    EventPosition.FromEnqueuedTime(startTime),
                    shutdown.Token))
                {
                    await events.Writer.WriteAsync(
                        new EventMonitorItem(Convert.ToBase64String(partitionEvent.Data.Body.ToArray())),
                        shutdown.Token);
                }
            }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                await shutdown.CancelAsync();
                events.Writer.TryComplete(ex);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await shutdown.CancelAsync();
            try
            {
                await readers;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception) when (readers.IsFaulted)
            {
            }
            await client.DisposeAsync();
            shutdown.Dispose();
        }
    }
}

internal sealed record EventMonitorStartRequest(
    [property: JsonPropertyName("namespaceName")] string NamespaceName,
    [property: JsonPropertyName("eventHubName")] string EventHubName,
    [property: JsonPropertyName("consumerGroup")] string ConsumerGroup,
    [property: JsonPropertyName("startTimeUtc")] DateTimeOffset StartTimeUtc);

internal sealed record EventMonitorItem(
    [property: JsonPropertyName("body")] string Body);
