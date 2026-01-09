using System;
using System.Collections.Generic;
using SqlSync.Connection;

#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public sealed record class SqlSyncBuildProject(int SqlSyncBuildProject_Id, string? ProjectName, bool? ScriptTagRequired);

    public sealed record class Scripts(int Scripts_Id, int? SqlSyncBuildProject_Id);

    public sealed record class Script(
        string? FileName,
        double? BuildOrder,
        string? Description,
        bool? RollBackOnError,
        bool? CausesBuildFailure,
        DateTime? DateAdded,
        string? ScriptId,
        string? Database,
        bool? StripTransactionText,
        bool? AllowMultipleRuns,
        string? AddedBy,
        int? ScriptTimeOut,
        DateTime? DateModified,
        string? ModifiedBy,
        int? Scripts_Id,
        string? Tag);

    public sealed record class Builds(int Builds_Id, int? SqlSyncBuildProject_Id);

    public sealed record class Build(
        string? Name,
        string? BuildType,
        DateTime? BuildStart,
        DateTime? BuildEnd,
        string? ServerName,
        string? FinalStatus,
        string? BuildId,
        string? UserId,
        int Build_Id,
        int? Builds_Id);

    public sealed record class ScriptRun(
        string? FileHash,
        string? Results,
        string? FileName,
        double? RunOrder,
        DateTime? RunStart,
        DateTime? RunEnd,
        bool? Success,
        string? Database,
        string? ScriptRunId,
        int? Build_Id);

    public sealed record class CommittedScript(
        string? ScriptId,
        string? ServerName,
        DateTime? CommittedDate,
        bool? AllowScriptBlock,
        string? ScriptHash,
        int? SqlSyncBuildProject_Id);

    public sealed record class CodeReview(
        Guid? CodeReviewId,
        string? ScriptId,
        DateTime? ReviewDate,
        string? ReviewBy,
        short? ReviewStatus,
        string? Comment,
        string? ReviewNumber,
        string? CheckSum,
        string? ValidationKey);

    public sealed record class SqlSyncBuildDataModel(
        IReadOnlyList<SqlSyncBuildProject> SqlSyncBuildProject,
        IReadOnlyList<Scripts> Scripts,
        IReadOnlyList<Script> Script,
        IReadOnlyList<Builds> Builds,
        IReadOnlyList<Build> Build,
        IReadOnlyList<ScriptRun> ScriptRun,
        IReadOnlyList<CommittedScript> CommittedScript,
        IReadOnlyList<CodeReview> CodeReview);

    public sealed record class SqlBuildRunDataModel(
        SqlSyncBuildDataModel? BuildDataModel,
        string? BuildType,
        string? Server,
        string? BuildDescription,
        double? StartIndex,
        string? ProjectFileName,
        bool? IsTrial,
        IReadOnlyList<double>? RunItemIndexes,
        bool? RunScriptOnly,
        string? BuildFileName,
        string? LogToDatabaseName,
        bool? IsTransactional,
        string? PlatinumDacPacFileName,
        IReadOnlyList<DatabaseOverride>? TargetDatabaseOverrides,
        bool? ForceCustomDacpac,
        string? BuildRevision,
        int? DefaultScriptTimeout,
        bool? AllowObjectDelete);
}
