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
    }
}
