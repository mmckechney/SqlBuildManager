using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation;
using SqlSync.DbInformation.Models;
using System;
using System.Linq;

namespace SqlSync.DbInformation.UnitTest
{
    [TestClass]
    public class SizeAnalysisModelTest
    {
        [TestMethod]
        public void ToModel_FromModelLists_Works()
        {
            var model = new SizeAnalysisModel(
                SizeAnalysis: new[] {
                    new SizeAnalysis(
                        TableName: "dbo.Table",
                        RowCount: 10,
                        DataSize: 1,
                        IndexSize: 2,
                        UnusedSize: 1,
                        TotalReservedSize: 4,
                        AverageDataRowSize: 0.1,
                        AverageIndexRowSize: 0.2)
                },
                ServerSizeSummary: Array.Empty<ServerSizeInfo>());
            Assert.AreEqual(1, model.SizeAnalysis.Count);
            var item = model.SizeAnalysis.Single();
            Assert.AreEqual("dbo.Table", item.TableName);
            Assert.AreEqual(10, item.RowCount);
            Assert.AreEqual(4, item.TotalReservedSize);
        }
    }
}
