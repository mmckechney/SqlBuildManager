using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultBuildRetryPolicyTests
    {
        private DefaultBuildRetryPolicy policy = null!;

        [TestInitialize]
        public void Init()
        {
            policy = new DefaultBuildRetryPolicy();
        }

        [TestMethod]
        public void ShouldRetry_FailedDueToScriptTimeout_ReturnsTrue()
        {
            var result = new Build { FinalStatus = BuildItemStatus.FailedDueToScriptTimeout };
            Assert.IsTrue(policy.ShouldRetry(result, 0));
        }

        [TestMethod]
        public void ShouldRetry_FailedDueToScriptTimeout_AtHighAttemptIndex_ReturnsTrue()
        {
            var result = new Build { FinalStatus = BuildItemStatus.FailedDueToScriptTimeout };
            Assert.IsTrue(policy.ShouldRetry(result, 100));
        }

        [TestMethod]
        public void ShouldRetry_Committed_ReturnsFalse()
        {
            var result = new Build { FinalStatus = BuildItemStatus.Committed };
            Assert.IsFalse(policy.ShouldRetry(result, 0));
        }

        [TestMethod]
        public void ShouldRetry_RolledBack_ReturnsFalse()
        {
            var result = new Build { FinalStatus = BuildItemStatus.RolledBack };
            Assert.IsFalse(policy.ShouldRetry(result, 0));
        }

        [TestMethod]
        public void ShouldRetry_FailedNoTransaction_ReturnsFalse()
        {
            var result = new Build { FinalStatus = BuildItemStatus.FailedNoTransaction };
            Assert.IsFalse(policy.ShouldRetry(result, 0));
        }

        [TestMethod]
        public void ShouldRetry_TrialRolledBack_ReturnsFalse()
        {
            var result = new Build { FinalStatus = BuildItemStatus.TrialRolledBack };
            Assert.IsFalse(policy.ShouldRetry(result, 0));
        }

        [TestMethod]
        public void ShouldRetry_NullResult_ReturnsFalse()
        {
            Assert.IsFalse(policy.ShouldRetry(null!, 0));
        }
    }
}
