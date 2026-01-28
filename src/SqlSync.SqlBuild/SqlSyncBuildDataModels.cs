using System;
using System.Collections.Generic;
using SqlSync.Connection;

#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public sealed class SqlSyncBuildProject
    {
        public int SqlSyncBuildProjectId { get; set; }
        public string? ProjectName { get; set; }
        public bool? ScriptTagRequired { get; set; }

        public SqlSyncBuildProject(int sqlSyncBuildProjectId, string? projectName, bool? scriptTagRequired)
        {
            SqlSyncBuildProjectId = sqlSyncBuildProjectId;
            ProjectName = projectName;
            ScriptTagRequired = scriptTagRequired;
        }
    }

    public sealed class Script
    {
        public string? FileName { get; set; }
        public double? BuildOrder { get; set; }
        public string? Description { get; set; }
        public bool? RollBackOnError { get; set; }
        public bool? CausesBuildFailure { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? ScriptId { get; set; }
        public string? Database { get; set; }
        public bool? StripTransactionText { get; set; }
        public bool? AllowMultipleRuns { get; set; }
        public string? AddedBy { get; set; }
        public int? ScriptTimeOut { get; set; }
        public DateTime? DateModified { get; set; }
        public string? ModifiedBy { get; set; }
        public string? Tag { get; set; }

        public Script(
            string? fileName,
            double? buildOrder,
            string? description,
            bool? rollBackOnError,
            bool? causesBuildFailure,
            DateTime? dateAdded,
            string? scriptId,
            string? database,
            bool? stripTransactionText,
            bool? allowMultipleRuns,
            string? addedBy,
            int? scriptTimeOut,
            DateTime? dateModified,
            string? modifiedBy,
            string? tag)
        {
            FileName = fileName;
            BuildOrder = buildOrder;
            Description = description;
            RollBackOnError = rollBackOnError;
            CausesBuildFailure = causesBuildFailure;
            DateAdded = dateAdded;
            ScriptId = scriptId;
            Database = database;
            StripTransactionText = stripTransactionText;
            AllowMultipleRuns = allowMultipleRuns;
            AddedBy = addedBy;
            ScriptTimeOut = scriptTimeOut;
            DateModified = dateModified;
            ModifiedBy = modifiedBy;
            Tag = tag;
        }
    }

    public sealed class Build
    {
        public string? Name { get; set; }
        public string? BuildType { get; set; }
        public DateTime? BuildStart { get; set; }
        public DateTime? BuildEnd { get; set; }
        public string? ServerName { get; set; }
        public BuildItemStatus? FinalStatus { get; set; }
        public string? BuildId { get; set; }
        public string? UserId { get; set; }

        public Build(
            string? name,
            string? buildType,
            DateTime? buildStart,
            DateTime? buildEnd,
            string? serverName,
            BuildItemStatus? finalStatus,
            string? buildId,
            string? userId)
        {
            Name = name;
            BuildType = buildType;
            BuildStart = buildStart;
            BuildEnd = buildEnd;
            ServerName = serverName;
            FinalStatus = finalStatus;
            BuildId = buildId;
            UserId = userId;
        }
    }

    public sealed class ScriptRun
    {
        public string? FileHash { get; set; }
        public string? Results { get; set; }
        public string? FileName { get; set; }
        public double? RunOrder { get; set; }
        public DateTime? RunStart { get; set; }
        public DateTime? RunEnd { get; set; }
        public bool? Success { get; set; }
        public string? Database { get; set; }
        public string? ScriptRunId { get; set; }
        public string? BuildId { get; set; }

        public ScriptRun(
            string? fileHash,
            string? results,
            string? fileName,
            double? runOrder,
            DateTime? runStart,
            DateTime? runEnd,
            bool? success,
            string? database,
            string? scriptRunId,
            string? buildId)
        {
            FileHash = fileHash;
            Results = results;
            FileName = fileName;
            RunOrder = runOrder;
            RunStart = runStart;
            RunEnd = runEnd;
            Success = success;
            Database = database;
            ScriptRunId = scriptRunId;
            BuildId = buildId;
        }
    }

    public sealed class CommittedScript
    {
        public string? ScriptId { get; set; }
        public string? ServerName { get; set; }
        public DateTime? CommittedDate { get; set; }
        public bool? AllowScriptBlock { get; set; }
        public string? ScriptHash { get; set; }
        public int? SqlSyncBuildProjectId { get; set; }

        public CommittedScript(
            string? scriptId,
            string? serverName,
            DateTime? committedDate,
            bool? allowScriptBlock,
            string? scriptHash,
            int? sqlSyncBuildProjectId)
        {
            ScriptId = scriptId;
            ServerName = serverName;
            CommittedDate = committedDate;
            AllowScriptBlock = allowScriptBlock;
            ScriptHash = scriptHash;
            SqlSyncBuildProjectId = sqlSyncBuildProjectId;
        }
    }

    public sealed class CodeReview
    {
        public Guid? CodeReviewId { get; set; }
        public string? ScriptId { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewBy { get; set; }
        public short? ReviewStatus { get; set; }
        public string? Comment { get; set; }
        public string? ReviewNumber { get; set; }
        public string? CheckSum { get; set; }
        public string? ValidationKey { get; set; }

        public CodeReview(
            Guid? codeReviewId,
            string? scriptId,
            DateTime? reviewDate,
            string? reviewBy,
            short? reviewStatus,
            string? comment,
            string? reviewNumber,
            string? checkSum,
            string? validationKey)
        {
            CodeReviewId = codeReviewId;
            ScriptId = scriptId;
            ReviewDate = reviewDate;
            ReviewBy = reviewBy;
            ReviewStatus = reviewStatus;
            Comment = comment;
            ReviewNumber = reviewNumber;
            CheckSum = checkSum;
            ValidationKey = validationKey;
        }
    }

    public sealed class SqlSyncBuildDataModel
    {
        public IReadOnlyList<SqlSyncBuildProject> SqlSyncBuildProject { get; set; }
        public IReadOnlyList<Script> Script { get; set; }
        public IReadOnlyList<Build> Build { get; set; }
        public IReadOnlyList<ScriptRun> ScriptRun { get; set; }
        public IReadOnlyList<CommittedScript> CommittedScript { get; set; }
        public IReadOnlyList<CodeReview> CodeReview { get; set; }

        public SqlSyncBuildDataModel(
            IReadOnlyList<SqlSyncBuildProject> sqlSyncBuildProject,
            IReadOnlyList<Script> script,
            IReadOnlyList<Build> build,
            IReadOnlyList<ScriptRun> scriptRun,
            IReadOnlyList<CommittedScript> committedScript,
            IReadOnlyList<CodeReview> codeReview)
        {
            SqlSyncBuildProject = sqlSyncBuildProject;
            Script = script;
            Build = build;
            ScriptRun = scriptRun;
            CommittedScript = committedScript;
            CodeReview = codeReview;
        }
    }

    public sealed class SqlBuildRunDataModel
    {
        public SqlSyncBuildDataModel? BuildDataModel { get; set; }
        public string? BuildType { get; set; }
        public string? Server { get; set; }
        public string? BuildDescription { get; set; }
        public double? StartIndex { get; set; }
        public string? ProjectFileName { get; set; }
        public bool? IsTrial { get; set; }
        public IReadOnlyList<double>? RunItemIndexes { get; set; }
        public bool? RunScriptOnly { get; set; }
        public string? BuildFileName { get; set; }
        public string? LogToDatabaseName { get; set; }
        public bool? IsTransactional { get; set; }
        public string? PlatinumDacPacFileName { get; set; }
        public IReadOnlyList<DatabaseOverride>? TargetDatabaseOverrides { get; set; }
        public bool? ForceCustomDacpac { get; set; }
        public string? BuildRevision { get; set; }
        public int? DefaultScriptTimeout { get; set; }
        public bool? AllowObjectDelete { get; set; }

        public SqlBuildRunDataModel(
            SqlSyncBuildDataModel? buildDataModel,
            string? buildType,
            string? server,
            string? buildDescription,
            double? startIndex,
            string? projectFileName,
            bool? isTrial,
            IReadOnlyList<double>? runItemIndexes,
            bool? runScriptOnly,
            string? buildFileName,
            string? logToDatabaseName,
            bool? isTransactional,
            string? platinumDacPacFileName,
            IReadOnlyList<DatabaseOverride>? targetDatabaseOverrides,
            bool? forceCustomDacpac,
            string? buildRevision,
            int? defaultScriptTimeout,
            bool? allowObjectDelete)
        {
            BuildDataModel = buildDataModel;
            BuildType = buildType;
            Server = server;
            BuildDescription = buildDescription;
            StartIndex = startIndex;
            ProjectFileName = projectFileName;
            IsTrial = isTrial;
            RunItemIndexes = runItemIndexes;
            RunScriptOnly = runScriptOnly;
            BuildFileName = buildFileName;
            LogToDatabaseName = logToDatabaseName;
            IsTransactional = isTransactional;
            PlatinumDacPacFileName = platinumDacPacFileName;
            TargetDatabaseOverrides = targetDatabaseOverrides;
            ForceCustomDacpac = forceCustomDacpac;
            BuildRevision = buildRevision;
            DefaultScriptTimeout = defaultScriptTimeout;
            AllowObjectDelete = allowObjectDelete;
        }
    }
}
