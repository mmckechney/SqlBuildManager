using System;
using System.Collections.Generic;

#nullable enable

namespace SqlSync.SqlBuild.Models
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
}
