using System;
using System.Collections.Generic;

#nullable enable

namespace SqlBuildManager.SqlBuild.Models
{
    public sealed record class ScriptRunLogEntry(
        string? BuildFileName,
        string? ScriptFileName,
        Guid? ScriptId,
        string? ScriptFileHash,
        DateTime? CommitDate,
        int? Sequence,
        string? UserId,
        bool? AllowScriptBlock,
        string? AllowBlockUpdateId,
        string? ScriptText,
        string? Tag);

    /// <summary>
    /// Pre-fetched blocking SQL log status for a single script.
    /// Returned by <c>IDatabaseUtility.GetBatchBlockingSqlLog</c>.
    /// </summary>
    public readonly record struct SqlLogStatus(
        bool HasBlock,
        string ScriptHash,
        string ScriptTextHash,
        DateTime CommitDate);
}
