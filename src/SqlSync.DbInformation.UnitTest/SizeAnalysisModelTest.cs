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
        public void ToModel_FromDataTable_Works()
        {
            var table = new SizeAnalysisTable();
            var row = table.NewRow();
            row["Table Name"] = "dbo.Table";
            row["Row Count"] = 10;
            row["Data Size"] = 1;
            row["Index Size"] = 2;
            row["Unused Size"] = 1;
            row["Total Reserved Size"] = 4;
            row["Average Data Row Size"] = 0.1;
            row["Average Index Row Size"] = 0.2;
            table.Rows.Add(row);

            var model = table.ToModel();
            Assert.AreEqual(1, model.SizeAnalysis.Count);
            var item = model.SizeAnalysis.Single();
            Assert.AreEqual("dbo.Table", item.TableName);
            Assert.AreEqual(10, item.RowCount);
            Assert.AreEqual(4, item.TotalReservedSize);
        }
    }
}
