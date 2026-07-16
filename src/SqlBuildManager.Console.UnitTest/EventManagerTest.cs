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
                "https://example.servicebus.windows.net/blobupload"));
        }

        [TestMethod]
        public void ShouldUseCheckpointFreeConsumer_SharedKeyWithProxy_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseCheckpointFreeConsumer(
                "storage-key",
                "https://example.servicebus.windows.net/blobupload"));
        }

        [TestMethod]
        public void ShouldUseCheckpointFreeConsumer_ManagedIdentityWithoutProxy_ReturnsFalse()
        {
            Assert.IsFalse(EventManager.ShouldUseCheckpointFreeConsumer(string.Empty, string.Empty));
        }
    }
}
