using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Events;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class EventManagerTest
    {
        [TestMethod]
        public void ShouldUseCheckpointFreeConsumer_ManagedIdentityWithProxy_ReturnsTrue()
        {
            Assert.IsTrue(EventManager.ShouldUseCheckpointFreeConsumer(
                string.Empty,
                "https://example.servicebus.windows.net/relayproxy"));
        }

        [TestMethod]
        public void ShouldUseCheckpointFreeConsumer_SharedKeyWithProxy_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseCheckpointFreeConsumer(
                "storage-key",
                "https://example.servicebus.windows.net/relayproxy"));
        }

        [TestMethod]
        public void ShouldUseCheckpointFreeConsumer_ManagedIdentityWithoutProxy_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseCheckpointFreeConsumer(string.Empty, string.Empty));
        }

        [TestMethod]
        public void ShouldUseRelayEventMonitor_PrivateEventHubFailureWithProxy_ReturnsTrue()
        {
            Assert.IsTrue(EventManager.ShouldUseRelayEventMonitor(
                new UnauthorizedAccessException("Ip has been prevented to connect to the endpoint."),
                "https://example.servicebus.windows.net/relayproxy"));
        }

        [TestMethod]
        public void ShouldUseRelayEventMonitor_PrivateEventHubFailureWithoutProxy_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseRelayEventMonitor(
                new UnauthorizedAccessException("Ip has been prevented to connect to the endpoint."),
                string.Empty));
        }

        [TestMethod]
        public void ShouldUseRelayEventMonitor_GenericAuthorizationFailure_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseRelayEventMonitor(
                new UnauthorizedAccessException("The identity is not authorized."),
                "https://example.servicebus.windows.net/relayproxy"));
        }

        [TestMethod]
        public void ShouldUseRelayEventMonitor_WrappedPrivateEventHubFailure_ReturnsTrue()
        {
            Assert.IsTrue(EventManager.ShouldUseRelayEventMonitor(
                new AggregateException(
                    new InvalidOperationException(
                        "Event Hub startup failed.",
                        new UnauthorizedAccessException(
                            "Ip has been prevented to connect to the endpoint."))),
                "https://example.servicebus.windows.net/relayproxy"));
        }

        [TestMethod]
        public void IsEventHubEmulatorConnectionString_DevelopmentEmulator_ReturnsTrue()
        {
            Assert.IsTrue(EventManager.IsEventHubEmulatorConnectionString(
                "Endpoint=sb://namespace/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY;EntityPath=events;UseDevelopmentEmulator=true"));
        }

        [TestMethod]
        public void IsEventHubEmulatorConnectionString_EmulatorHost_ReturnsTrue()
        {
            Assert.IsTrue(EventManager.IsEventHubEmulatorConnectionString(
                "Endpoint=sb://eventhubs-emulator:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY;EntityPath=events"));
        }

        [TestMethod]
        public void IsEventHubEmulatorConnectionString_AzureEndpoint_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.IsEventHubEmulatorConnectionString(
                "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY;EntityPath=events"));
        }

        [TestMethod]
        public void CreateCustomConsumerGroup_Emulator_UsesCg1WithoutArm()
        {
            using var manager = new EventManager(
                "Endpoint=sb://eventhubs-emulator:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY;EntityPath=events",
                "subscription",
                "resource-group",
                "devstoreaccount1",
                "storage-key",
                "job");

            Assert.AreEqual(
                EventManager.EmulatorConsumerGroupName,
                manager.CreateCustomConsumerGroup("subscription", "resource-group", "eventhubs-emulator:5672", "events", "job"));
        }
    }
}
