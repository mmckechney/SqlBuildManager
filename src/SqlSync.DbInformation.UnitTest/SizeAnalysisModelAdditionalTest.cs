using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation.Models;
using System;
using System.Linq;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Additional unit tests for SizeAnalysis models
    /// </summary>
    [TestClass]
    public class SizeAnalysisModelAdditionalTest
    {
        #region SizeAnalysis Record Tests

        [TestMethod]
        public void SizeAnalysis_WithAllProperties_ShouldReturnCorrectValues()
        {
            var target = new SizeAnalysis(
                TableName: "dbo.Customers",
                RowCount: 10000,
                DataSize: 512,
                IndexSize: 256,
                UnusedSize: 64,
                TotalReservedSize: 832,
                AverageDataRowSize: 0.0512,
                AverageIndexRowSize: 0.0256);

            Assert.AreEqual("dbo.Customers", target.TableName);
            Assert.AreEqual(10000, target.RowCount);
            Assert.AreEqual(512, target.DataSize);
            Assert.AreEqual(256, target.IndexSize);
            Assert.AreEqual(64, target.UnusedSize);
            Assert.AreEqual(832, target.TotalReservedSize);
            Assert.AreEqual(0.0512, target.AverageDataRowSize, 0.0001);
            Assert.AreEqual(0.0256, target.AverageIndexRowSize, 0.0001);
        }

        [TestMethod]
        public void SizeAnalysis_Equality_ShouldWorkCorrectly()
        {
            var analysis1 = new SizeAnalysis("dbo.Table", 100, 10, 5, 2, 17, 0.1, 0.05);
            var analysis2 = new SizeAnalysis("dbo.Table", 100, 10, 5, 2, 17, 0.1, 0.05);

            Assert.AreEqual(analysis1, analysis2);
        }

        [TestMethod]
        public void SizeAnalysis_DifferentValues_ShouldNotBeEqual()
        {
            var analysis1 = new SizeAnalysis("dbo.Table1", 100, 10, 5, 2, 17, 0.1, 0.05);
            var analysis2 = new SizeAnalysis("dbo.Table2", 100, 10, 5, 2, 17, 0.1, 0.05);

            Assert.AreNotEqual(analysis1, analysis2);
        }

        [TestMethod]
        public void SizeAnalysis_WithZeroRowCount_ShouldWork()
        {
            var target = new SizeAnalysis(
                TableName: "dbo.EmptyTable",
                RowCount: 0,
                DataSize: 0,
                IndexSize: 0,
                UnusedSize: 0,
                TotalReservedSize: 0,
                AverageDataRowSize: 0,
                AverageIndexRowSize: 0);

            Assert.AreEqual(0, target.RowCount);
            Assert.AreEqual("dbo.EmptyTable", target.TableName);
        }

        #endregion

        #region ServerSizeInfo Record Tests

        [TestMethod]
        public void ServerSizeInfo_WithAllProperties_ShouldReturnCorrectValues()
        {
            var dateCreated = new DateTime(2024, 1, 15);
            var target = new ServerSizeInfo(
                DatabaseName: "ProductionDb",
                Location: @"C:\Data\ProductionDb.mdf",
                DataSize: 1073741824,
                DateCreated: dateCreated);

            Assert.AreEqual("ProductionDb", target.DatabaseName);
            Assert.AreEqual(@"C:\Data\ProductionDb.mdf", target.Location);
            Assert.AreEqual(1073741824, target.DataSize);
            Assert.AreEqual(dateCreated, target.DateCreated);
        }

        [TestMethod]
        public void ServerSizeInfo_Equality_ShouldWorkCorrectly()
        {
            var date = new DateTime(2024, 1, 1);
            var info1 = new ServerSizeInfo("Db1", "Location1", 100, date);
            var info2 = new ServerSizeInfo("Db1", "Location1", 100, date);

            Assert.AreEqual(info1, info2);
        }

        #endregion

        #region SizeAnalysisModel Record Tests

        [TestMethod]
        public void SizeAnalysisModel_WithEmptyCollections_ShouldWork()
        {
            var target = new SizeAnalysisModel(
                SizeAnalysis: Array.Empty<SizeAnalysis>(),
                ServerSizeSummary: Array.Empty<ServerSizeInfo>());

            Assert.AreEqual(0, target.SizeAnalysis.Count);
            Assert.AreEqual(0, target.ServerSizeSummary.Count);
        }

        [TestMethod]
        public void SizeAnalysisModel_WithMultipleItems_ShouldContainAll()
        {
            var sizeAnalyses = new[]
            {
                new SizeAnalysis("dbo.Table1", 100, 10, 5, 2, 17, 0.1, 0.05),
                new SizeAnalysis("dbo.Table2", 200, 20, 10, 4, 34, 0.1, 0.05),
                new SizeAnalysis("dbo.Table3", 300, 30, 15, 6, 51, 0.1, 0.05)
            };

            var serverInfos = new[]
            {
                new ServerSizeInfo("Db1", "Location1", 1000, DateTime.Now),
                new ServerSizeInfo("Db2", "Location2", 2000, DateTime.Now)
            };

            var target = new SizeAnalysisModel(sizeAnalyses, serverInfos);

            Assert.AreEqual(3, target.SizeAnalysis.Count);
            Assert.AreEqual(2, target.ServerSizeSummary.Count);
            Assert.AreEqual("dbo.Table1", target.SizeAnalysis[0].TableName);
        }

        [TestMethod]
        public void SizeAnalysisModel_CanAccessItemsByIndex()
        {
            var sizeAnalyses = new[]
            {
                new SizeAnalysis("dbo.First", 100, 10, 5, 2, 17, 0.1, 0.05),
                new SizeAnalysis("dbo.Second", 200, 20, 10, 4, 34, 0.2, 0.1)
            };

            var target = new SizeAnalysisModel(sizeAnalyses, Array.Empty<ServerSizeInfo>());

            Assert.AreEqual("dbo.First", target.SizeAnalysis[0].TableName);
            Assert.AreEqual("dbo.Second", target.SizeAnalysis[1].TableName);
        }

        [TestMethod]
        public void SizeAnalysisModel_CanUseLinqQueries()
        {
            var sizeAnalyses = new[]
            {
                new SizeAnalysis("dbo.Small", 100, 10, 5, 2, 17, 0.1, 0.05),
                new SizeAnalysis("dbo.Large", 10000, 1000, 500, 200, 1700, 0.1, 0.05),
                new SizeAnalysis("dbo.Medium", 1000, 100, 50, 20, 170, 0.1, 0.05)
            };

            var target = new SizeAnalysisModel(sizeAnalyses, Array.Empty<ServerSizeInfo>());

            var largeTable = target.SizeAnalysis.FirstOrDefault(s => s.RowCount > 5000);
            Assert.IsNotNull(largeTable);
            Assert.AreEqual("dbo.Large", largeTable.TableName);
        }

        #endregion
    }
}
