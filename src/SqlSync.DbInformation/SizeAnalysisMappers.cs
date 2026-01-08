using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.DbInformation.Models;
using System.Data;

#nullable enable

namespace SqlSync.DbInformation
{
    public static class SizeAnalysisMappers
    {
        public static SizeAnalysisModel ToModel(this SizeAnalysisTable table)
        {
            var list = table?.Rows.Cast<DataRow>()
                .Select(r => new SizeAnalysis(
                    TableName: r.Field<string>("Table Name") ?? string.Empty,
                    RowCount: r.Field<int>("Row Count"),
                    DataSize: r.Field<int>("Data Size"),
                    IndexSize: r.Field<int>("Index Size"),
                    UnusedSize: r.Field<int>("Unused Size"),
                    TotalReservedSize: r.Field<int>("Total Reserved Size"),
                    AverageDataRowSize: r.Field<double>("Average Data Row Size"),
                    AverageIndexRowSize: r.Field<double>("Average Index Row Size")))
                .ToList() ?? new List<SizeAnalysis>();
            return new SizeAnalysisModel(list, Array.Empty<ServerSizeInfo>());
        }

        public static SizeAnalysisTable ToDataTable(this IEnumerable<SizeAnalysis> items)
        {
            var t = new SizeAnalysisTable();
            foreach (var i in items)
            {
                var row = t.NewSizeAnalysisRow();
                row["Table Name"] = i.TableName;
                row["Row Count"] = i.RowCount;
                row["Data Size"] = i.DataSize;
                row["Index Size"] = i.IndexSize;
                row["Unused Size"] = i.UnusedSize;
                row["Total Reserved Size"] = i.TotalReservedSize;
                row["Average Data Row Size"] = i.AverageDataRowSize;
                row["Average Index Row Size"] = i.AverageIndexRowSize;
                t.AddSizeAnalysisRow(row);
            }
            return t;
        }

        public static List<SizeAnalysis> ToSizeAnalysisList(this SizeAnalysisTable table)
        {
            return table?.ToModel().SizeAnalysis.ToList() ?? new List<SizeAnalysis>();
        }

        public static ServerSizeInfo ToModel(this ServerSizeSummary summary, DataRow row)
        {
            return new ServerSizeInfo(
                DatabaseName: row.Field<string>("DatabaseName") ?? string.Empty,
                Location: row.Field<string>("Location") ?? string.Empty,
                DataSize: row.Field<long>("DataSize"),
                DateCreated: row.Field<DateTime>("DateCreated"));
        }

        public static List<ServerSizeInfo> ToServerSizeSummaryList(this ServerSizeSummary summary)
        {
            return summary?.Rows.Cast<DataRow>().Select(r => summary.ToModel(r)).ToList() ?? new List<ServerSizeInfo>();
        }
    }
}
