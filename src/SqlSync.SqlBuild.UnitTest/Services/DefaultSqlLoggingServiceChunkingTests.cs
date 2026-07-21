using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    /// <summary>
    /// Regression tests for PERF-011: DefaultSqlLoggingService parameter chunking.
    /// Validates that the SqlServer 2100-parameter limit constants are correct and
    /// that the MaxRowsPerBatch calculation stays safe without requiring a live database.
    /// </summary>
    [TestClass]
    public class DefaultSqlLoggingServiceChunkingTests
    {
        private const int SqlServerMaxParams = 2100;
        private const int LogRowParamCount = 16;
        private static readonly int MaxRowsPerBatch = SqlServerMaxParams / LogRowParamCount; // 131

        [TestMethod]
        public void MaxRowsPerBatch_Is131()
        {
            // 2100 / 16 = 131 (integer division — exactly 131 rows × 16 params = 2096 < 2100)
            Assert.AreEqual(131, MaxRowsPerBatch);
        }

        [TestMethod]
        public void MaxRowsPerBatch_Times_LogRowParamCount_StaysUnder2100()
        {
            int totalParams = MaxRowsPerBatch * LogRowParamCount;
            Assert.IsTrue(totalParams < SqlServerMaxParams,
                $"Expected {totalParams} < {SqlServerMaxParams} but it was not.");
        }

        [TestMethod]
        public void MaxRowsPerBatch_PlusOne_TimesParamCount_ExceedsLimit()
        {
            // Verify that one additional row would breach the limit (sanity-check the boundary).
            int totalParams = (MaxRowsPerBatch + 1) * LogRowParamCount;
            Assert.IsTrue(totalParams > SqlServerMaxParams,
                $"Expected {totalParams} > {SqlServerMaxParams} but it was not — MaxRowsPerBatch may be too small.");
        }

        [TestMethod]
        public void DefaultSqlLoggingService_HasMaxRowsPerBatchConstant()
        {
            var type = typeof(SqlSync.SqlBuild.Services.DefaultSqlLoggingService);
            var field = type.GetField("MaxRowsPerBatch",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "DefaultSqlLoggingService should have a private static MaxRowsPerBatch field.");
            int value = (int)field.GetValue(null)!;
            Assert.AreEqual(131, value);
        }
    }
}
