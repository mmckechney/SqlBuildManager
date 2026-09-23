using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class QueueSubscriptionTest
    {
        [TestMethod]
        [DataRow(ConcurrencyType.Count)]
        [DataRow(ConcurrencyType.Server)]
        [DataRow(ConcurrencyType.MaxPerServer)]
        [DataRow(ConcurrencyType.Tag)]
        [DataRow(ConcurrencyType.MaxPerTag)]
        public async Task Creation_InstallsJobRuleAtomically(ConcurrencyType type)
        {
            var admin = new FakeAdmin();
            using var manager = new QueueManager("job-a", type, admin);
            await manager.CreateSubscriptions();
            Assert.AreEqual(1, admin.CreateCalls);
            Assert.IsNotNull(admin.Options);
            Assert.AreEqual(type != ConcurrencyType.Count, admin.Options.RequiresSession);
            Assert.AreEqual(type == ConcurrencyType.Count ? "job-a" : "job-asession", admin.Options.SubscriptionName);
            Assert.HasCount(1, admin.Rules);
            Assert.AreEqual("job-a", admin.Rules[0].Name);
            Assert.AreEqual("job-a", ((CorrelationRuleFilter)admin.Rules[0].Filter).Subject);
            Assert.AreEqual(0L, admin.ActiveMessages, "Unrelated traffic at creation must not be accepted.");
            await manager.ValidateSubscriptionForEnqueueAsync(20);
            Assert.AreEqual(admin.Options.SubscriptionName, admin.LastReadSubscription);
        }

        [TestMethod]
        public async Task ExistingCorrectSubscription_IsNotRecreated()
        {
            var admin = new FakeAdmin { Exists = true };
            admin.Rules.Add(JobRule());
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            await manager.CreateSubscriptions();
            await manager.ValidateSubscriptionForEnqueueAsync(20);
            Assert.AreEqual(0, admin.CreateCalls);
        }

        [TestMethod]
        [DataRow("default")]
        [DataRow("extra")]
        [DataRow("wrong-subject")]
        [DataRow("missing")]
        [DataRow("action")]
        [DataRow("extra-condition")]
        public async Task UnsafeExistingRules_AreRejectedWithoutMutation(string scenario)
        {
            var admin = new FakeAdmin { Exists = true };
            switch (scenario)
            {
                case "default":
                    admin.Rules.Add(ServiceBusModelFactory.RuleProperties("$Default", new TrueRuleFilter()));
                    break;
                case "extra":
                    admin.Rules.Add(JobRule());
                    admin.Rules.Add(ServiceBusModelFactory.RuleProperties("extra", new TrueRuleFilter()));
                    break;
                case "wrong-subject":
                    admin.Rules.Add(ServiceBusModelFactory.RuleProperties("job-a", new CorrelationRuleFilter { Subject = "job-b" }));
                    break;
                case "action":
                    admin.Rules.Add(ServiceBusModelFactory.RuleProperties("job-a",
                        new CorrelationRuleFilter { Subject = "job-a" }, new SqlRuleAction("SET priority = 1")));
                    break;
                case "extra-condition":
                    admin.Rules.Add(ServiceBusModelFactory.RuleProperties("job-a",
                        new CorrelationRuleFilter { Subject = "job-a", SessionId = "unexpected" }));
                    break;
            }
            var original = admin.Rules.ToArray();
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.ValidateSubscriptionForEnqueueAsync(20));
            StringAssert.Contains(exception.Message, "No targets were sent");
            CollectionAssert.AreEqual(original, admin.Rules.ToArray());
            Assert.AreEqual(0, admin.CountReads);
        }

        [TestMethod]
        [DataRow(20L, 20L)]
        [DataRow(0L, 1L)]
        public async Task ExistingMessages_AreRejectedWithoutPurging(long active, long total)
        {
            var admin = new FakeAdmin { Exists = true, ActiveMessages = active, TotalMessages = total };
            admin.Rules.Add(JobRule());
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.ValidateSubscriptionForEnqueueAsync(20));
            StringAssert.Contains(exception.Message, "No targets were sent or messages deleted");
            Assert.AreEqual(active, admin.ActiveMessages);
            Assert.AreEqual(total, admin.TotalMessages);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task AdministrationFailures_AbortEnqueue(bool failCount)
        {
            var admin = new FakeAdmin { Exists = true, FailRules = !failCount, FailCount = failCount };
            admin.Rules.Add(JobRule());
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            // No data-plane client is injected: attempting to send would fail for a different reason.
            // The preflight must report the original administration failure before client creation.
            await Assert.ThrowsAsync<RequestFailedException>(() => manager.ValidateSubscriptionForEnqueueAsync(20));
            Assert.AreEqual(-1, await manager.SendTargetsToQueue(new MultiDbData(), ConcurrencyType.Count));
            Assert.AreEqual(failCount ? 2 : 0, admin.CountReads);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task ConcurrentCreation_RevalidatesWinningSubscription(bool unsafeRule)
        {
            var admin = new FakeAdmin { Conflict = true, UnsafeConflict = unsafeRule };
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            await manager.CreateSubscriptions();
            if (unsafeRule)
                await Assert.ThrowsAsync<InvalidOperationException>(() => manager.ValidateSubscriptionForEnqueueAsync(20));
            else
                await manager.ValidateSubscriptionForEnqueueAsync(20);
        }

        [TestMethod]
        public async Task CreationFailure_IsNotSwallowed()
        {
            var admin = new FakeAdmin { FailCreation = true };
            using var manager = new QueueManager("job-a", ConcurrencyType.Count, admin);
            await Assert.ThrowsAsync<RequestFailedException>(() => manager.CreateSubscriptions());
        }

        [TestMethod]
        [DataRow(0L, 5)]
        [DataRow(20L, 1)]
        [DataRow(40L, 1)]
        public async Task VisibilityPolling_IsBoundedAndStopsOnExcess(long count, int expectedReads)
        {
            int reads = 0;
            var result = await QueueManager.WaitForQueueVisibilityAsync(
                () => { reads++; return Task.FromResult(count); }, 20, 4, TimeSpan.Zero);
            Assert.AreEqual(count, result);
            Assert.AreEqual(expectedReads, reads);
        }

        [TestMethod]
        public async Task VisibilityPolling_StopsWhenExpectedCountAppears()
        {
            var counts = new Queue<long>(new long[] { 0, 10, 20, 40 });
            Assert.AreEqual(20L, await QueueManager.WaitForQueueVisibilityAsync(
                () => Task.FromResult(counts.Dequeue()), 20, 4, TimeSpan.Zero));
            Assert.HasCount(1, counts);
        }

        private static RuleProperties JobRule() =>
            ServiceBusModelFactory.RuleProperties("job-a", new CorrelationRuleFilter { Subject = "job-a" });

        private sealed class FakeAdmin : ServiceBusAdministrationClient
        {
            public bool Exists, Conflict, UnsafeConflict, FailCreation, FailRules, FailCount;
            public long ActiveMessages, TotalMessages;
            public int CreateCalls, CountReads;
            public CreateSubscriptionOptions? Options;
            public string? LastReadSubscription;
            public List<RuleProperties> Rules { get; } = new();

            public override Task<Response<bool>> SubscriptionExistsAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default) =>
                Task.FromResult(Response.FromValue(Exists, null!));

            public override Task<Response<SubscriptionProperties>> CreateSubscriptionAsync(
                CreateSubscriptionOptions options, CreateRuleOptions rule, CancellationToken cancellationToken = default)
            {
                CreateCalls++;
                if (FailCreation)
                    throw new RequestFailedException(403, "Creation denied");
                Options = options;
                Rules.Add(UnsafeConflict
                    ? ServiceBusModelFactory.RuleProperties("$Default", new TrueRuleFilter())
                    : ServiceBusModelFactory.RuleProperties(rule.Name, rule.Filter, rule.Action));
                Exists = true;
                // Model unrelated publication at the instant the subscription comes into existence.
                if (Rules.Any(r => r.Filter is TrueRuleFilter ||
                    r.Filter is CorrelationRuleFilter filter && filter.Subject == "job-b"))
                    ActiveMessages = TotalMessages = 20;
                if (Conflict)
                    throw new ServiceBusException("Concurrent creation", ServiceBusFailureReason.MessagingEntityAlreadyExists);
                return Task.FromResult(Response.FromValue(
                    ServiceBusModelFactory.SubscriptionProperties(options.TopicName, options.SubscriptionName,
                        lockDuration: options.LockDuration, requiresSession: options.RequiresSession,
                        defaultMessageTimeToLive: options.DefaultMessageTimeToLive,
                        autoDeleteOnIdle: options.AutoDeleteOnIdle, maxDeliveryCount: options.MaxDeliveryCount,
                        userMetadata: string.Empty), null!));
            }

            public override AsyncPageable<RuleProperties> GetRulesAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default)
            {
                LastReadSubscription = subscriptionName;
                if (FailRules)
                    throw new RequestFailedException(403, "Rule inspection denied");
                return AsyncPageable<RuleProperties>.FromPages(new[] { Page<RuleProperties>.FromValues(Rules.ToArray(), null, null!) });
            }

            public override Task<Response<SubscriptionRuntimeProperties>> GetSubscriptionRuntimePropertiesAsync(
                string topicName, string subscriptionName, CancellationToken cancellationToken = default)
            {
                CountReads++;
                LastReadSubscription = subscriptionName;
                if (FailCount)
                    throw new RequestFailedException(503, "Count unavailable");
                return Task.FromResult(Response.FromValue(ServiceBusModelFactory.SubscriptionRuntimeProperties(
                    topicName, subscriptionName, activeMessageCount: ActiveMessages, totalMessageCount: TotalMessages), null!));
            }
        }
    }
}
