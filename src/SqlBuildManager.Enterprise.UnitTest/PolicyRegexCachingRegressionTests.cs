using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlBuildManager.Enterprise.UnitTest
{
    /// <summary>
    /// Regression tests verifying that PERF-006 static regex caching did not alter
    /// observable CheckPolicy behavior.  All assertions mirror the pre-existing policy
    /// tests — if they pass, the caching was transparent.
    /// </summary>
    [TestClass]
    public class PolicyRegexCachingRegressionTests
    {
        private static readonly List<Match> EmptyComments = new List<Match>();

        // ---------------------------------------------------------------
        // ReRunablePolicy
        // ---------------------------------------------------------------

        [TestMethod]
        public void ReRunablePolicy_WithIfExists_ReturnsTrue()
        {
            var policy = new ReRunablePolicy();
            bool result = policy.CheckPolicy(
                "IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'MyProc') DROP PROCEDURE dbo.MyProc",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReRunablePolicy_WithIfNotExists_ReturnsTrue()
        {
            var policy = new ReRunablePolicy();
            bool result = policy.CheckPolicy(
                "IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MyTable') CREATE TABLE ...",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReRunablePolicy_WithoutIfExists_ReturnsFalse()
        {
            var policy = new ReRunablePolicy();
            bool result = policy.CheckPolicy(
                "ALTER TABLE dbo.Orders ADD Column1 INT NULL",
                EmptyComments, out _);
            Assert.IsFalse(result);
        }

        // ---------------------------------------------------------------
        // GrantExecuteToPublicPolicy
        // ---------------------------------------------------------------

        [TestMethod]
        public void GrantExecuteToPublicPolicy_WithGrantToPublic_ReturnsFalse()
        {
            var policy = new GrantExecuteToPublicPolicy();
            bool result = policy.CheckPolicy(
                "GRANT EXECUTE ON dbo.MyProc TO [public]",
                EmptyComments, out _);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GrantExecuteToPublicPolicy_WithoutGrantToPublic_ReturnsTrue()
        {
            var policy = new GrantExecuteToPublicPolicy();
            bool result = policy.CheckPolicy(
                "GRANT EXECUTE ON dbo.MyProc TO [AppRole]",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        // ---------------------------------------------------------------
        // ViewAlterPolicy
        // ---------------------------------------------------------------

        [TestMethod]
        public void ViewAlterPolicy_WithAlterViewNoSuppression_ReturnsFalse()
        {
            var policy = new ViewAlterPolicy();
            bool result = policy.CheckPolicy(
                "ALTER VIEW dbo.MyView AS SELECT Id FROM dbo.Orders",
                EmptyComments, out _);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ViewAlterPolicy_WithAlterViewAndNoIndexesTag_ReturnsTrue()
        {
            var policy = new ViewAlterPolicy();
            bool result = policy.CheckPolicy(
                "-- [No Indexes]\r\nALTER VIEW dbo.MyView AS SELECT Id FROM dbo.Orders",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ViewAlterPolicy_WithoutAlterView_ReturnsTrue()
        {
            var policy = new ViewAlterPolicy();
            bool result = policy.CheckPolicy(
                "CREATE VIEW dbo.MyView AS SELECT Id FROM dbo.Orders",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        // ---------------------------------------------------------------
        // SelectStarPolicy
        // ---------------------------------------------------------------

        [TestMethod]
        public void SelectStarPolicy_WithSelectStar_ReturnsFalse()
        {
            var policy = new SelectStarPolicy();
            bool result = policy.CheckPolicy(
                "SELECT * FROM dbo.Orders WHERE Status = 1",
                EmptyComments, out _);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SelectStarPolicy_WithExplicitColumns_ReturnsTrue()
        {
            var policy = new SelectStarPolicy();
            bool result = policy.CheckPolicy(
                "SELECT Id, Name FROM dbo.Orders WHERE Status = 1",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        // ---------------------------------------------------------------
        // CommentHeaderPolicy (no proc/function → always passes)
        // ---------------------------------------------------------------

        [TestMethod]
        public void CommentHeaderPolicy_NoProcedure_ReturnsTrue()
        {
            var policy = new CommentHeaderPolicy();
            bool result = policy.CheckPolicy(
                "SELECT 1",
                EmptyComments, out string message);
            Assert.IsTrue(result);
            Assert.AreEqual("No routines found", message);
        }

        // ---------------------------------------------------------------
        // WithNoLockPolicy
        // ---------------------------------------------------------------

        [TestMethod]
        public void WithNoLockPolicy_SelectWithNoLock_ReturnsTrue()
        {
            var policy = new WithNoLockPolicy();
            bool result = policy.CheckPolicy(
                "SELECT Id FROM dbo.Orders WITH (NOLOCK) WHERE Status = 1",
                EmptyComments, out _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WithNoLockPolicy_SelectWithoutNoLock_ReturnsFalse()
        {
            var policy = new WithNoLockPolicy();
            bool result = policy.CheckPolicy(
                "SELECT Id FROM dbo.Orders WHERE Status = 1",
                EmptyComments, out _);
            Assert.IsFalse(result);
        }

        // ---------------------------------------------------------------
        // Multi-call idempotency: verify caching does not bleed state
        // ---------------------------------------------------------------

        [TestMethod]
        public void ReRunablePolicy_CalledRepeatedly_ReturnsSameResult()
        {
            var policy = new ReRunablePolicy();
            const string script = "IF EXISTS (SELECT 1 FROM sys.tables) ALTER TABLE dbo.X ADD Col1 INT";
            for (int i = 0; i < 10; i++)
            {
                bool result = policy.CheckPolicy(script, EmptyComments, out _);
                Assert.IsTrue(result, $"Iteration {i} returned false unexpectedly");
            }
        }

        [TestMethod]
        public void ViewAlterPolicy_CalledRepeatedly_ReturnsSameResult()
        {
            var policy = new ViewAlterPolicy();
            const string script = "ALTER VIEW dbo.V AS SELECT Id FROM dbo.T";
            for (int i = 0; i < 10; i++)
            {
                bool result = policy.CheckPolicy(script, EmptyComments, out _);
                Assert.IsFalse(result, $"Iteration {i} returned true unexpectedly");
            }
        }
    }
}
