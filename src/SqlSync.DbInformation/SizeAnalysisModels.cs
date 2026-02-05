using System;
using System.Collections.Generic;

#nullable enable

namespace SqlSync.DbInformation.Models
{
    public sealed record class SizeAnalysis(
        string TableName,
        int RowCount,
        int DataSize,
        int IndexSize,
        int UnusedSize,
        int TotalReservedSize,
        double AverageDataRowSize,
        double AverageIndexRowSize);

    public sealed record class ServerSizeInfo(
        string DatabaseName,
        string Location,
        long DataSize,
        DateTime DateCreated);

    public sealed record class SizeAnalysisModel(
        IReadOnlyList<SizeAnalysis> SizeAnalysis,
        IReadOnlyList<ServerSizeInfo> ServerSizeSummary);
}
