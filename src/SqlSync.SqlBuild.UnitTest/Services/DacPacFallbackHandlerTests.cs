using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DacPacFallbackHandlerTests
    {
        [TestMethod]
        public void IsCandidateForDacPacFallback_ReturnsFalse_ForCommittedStatus()
        {
            var handler = new DefaultDacPacFallbackHandler();

            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.Committed));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.CommittedWithTimeoutRetries));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.AlreadyInSync));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.TrialRolledBack));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.CommittedWithCustomDacpac));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.Pending));
        }

        [TestMethod]
        public void IsCandidateForDacPacFallback_ReturnsFalse_ForFailedTimeoutStatus()
        {
            var handler = new DefaultDacPacFallbackHandler();

            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.FailedDueToScriptTimeout));
            Assert.IsFalse(handler.IsCandidateForDacPacFallback(BuildItemStatus.FailedWithCustomDacpac));
        }

        [TestMethod]
        public void IsCandidateForDacPacFallback_ReturnsTrue_ForRolledBackStatuses()
        {
            var handler = new DefaultDacPacFallbackHandler();

            Assert.IsTrue(handler.IsCandidateForDacPacFallback(BuildItemStatus.RolledBack));
            Assert.IsTrue(handler.IsCandidateForDacPacFallback(BuildItemStatus.PendingRollBack));
            Assert.IsTrue(handler.IsCandidateForDacPacFallback(BuildItemStatus.FailedNoTransaction));
            Assert.IsTrue(handler.IsCandidateForDacPacFallback(BuildItemStatus.RolledBackAfterRetries));
        }

        [TestMethod]
        public async Task TryDacPacFallbackAsync_ReturnsNotAttempted_WhenNoDacPacFile()
        {
            var handler = new DefaultDacPacFallbackHandler();
            var context = new DacPacFallbackContext
            {
                RunData = new BuildModels.SqlBuildRunDataModel(
                    buildDataModel: null,
                    buildType: "test",
                    server: "srv",
                    buildDescription: "desc",
                    startIndex: 0,
                    projectFileName: null,
                    isTrial: false,
                    runItemIndexes: null,
                    runScriptOnly: false,
                    buildFileName: null,
                    logToDatabaseName: null,
                    isTransactional: true,
                    platinumDacPacFileName: null, // No DacPac file
                    targetDatabaseOverrides: null,
                    forceCustomDacpac: false,
                    buildRevision: null,
                    defaultScriptTimeout: 30,
                    allowObjectDelete: false)
            };
            var buildResult = new BuildModels.Build("test", "type", System.DateTime.Now, null, "srv", BuildItemStatus.RolledBack, "id", "user");

            var result = await handler.TryDacPacFallbackAsync(context, buildResult);

            Assert.IsFalse(result.WasAttempted);
            Assert.IsNull(result.NewStatus);
        }
    }
}
