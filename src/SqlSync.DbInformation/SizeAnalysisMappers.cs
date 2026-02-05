using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.DbInformation.Models;

#nullable enable

namespace SqlSync.DbInformation
{
    public static class SizeAnalysisMappers
    {
        public static SizeAnalysisModel ToModel(this IEnumerable<SizeAnalysis>? sizeAnalysis, IEnumerable<ServerSizeInfo>? serverSizeSummary = null)
        {
            var list = sizeAnalysis?.ToList() ?? new List<SizeAnalysis>();
            var server = serverSizeSummary?.ToList() ?? new List<ServerSizeInfo>();
            return new SizeAnalysisModel(list, server);
        }

        public static List<SizeAnalysis> ToSizeAnalysisList(this SizeAnalysisModel model)
        {
            return model.SizeAnalysis.ToList();
        }

        public static List<ServerSizeInfo> ToServerSizeSummaryList(this SizeAnalysisModel model)
        {
            return model.ServerSizeSummary.ToList();
        }
    }
}
