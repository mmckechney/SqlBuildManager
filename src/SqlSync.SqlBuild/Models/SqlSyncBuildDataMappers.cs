using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.SqlBuild.Legacy;

#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public static class SqlSyncBuildDataMappers
    {
        public static SqlSyncBuildDataModel ToModel(this SqlSyncBuildData ds)
        {
            if (ds == null)
            {
                return new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                    script: new List<Script>(),
                    build: new List<Build>(),
                    scriptRun: new List<ScriptRun>(),
                    committedScript: new List<CommittedScript>(),
                    codeReview: new List<CodeReview>()
                );
            }

            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: ds.SqlSyncBuildProject?.Cast<SqlSyncBuildData.SqlSyncBuildProjectRow>().Select(r => r.ToModel()).ToList() ?? new List<SqlSyncBuildProject>(),
                script: ds.Script?.Cast<SqlSyncBuildData.ScriptRow>().Select(r => r.ToModel()).ToList() ?? new List<Script>(),
                build: ds.Build?.Cast<SqlSyncBuildData.BuildRow>().Select(r => r.ToModel()).ToList() ?? new List<Build>(),
                scriptRun: ds.ScriptRun?.Cast<SqlSyncBuildData.ScriptRunRow>().Select(r => r.ToModel()).ToList() ?? new List<ScriptRun>(),
                committedScript: ds.CommittedScript?.Cast<SqlSyncBuildData.CommittedScriptRow>().Select(r => r.ToModel()).ToList() ?? new List<CommittedScript>(),
                codeReview: ds.CodeReview?.Cast<SqlSyncBuildData.CodeReviewRow>().Select(r => r.ToModel()).ToList() ?? new List<CodeReview>()
            );
        }

        public static SqlSyncBuildProject ToModel(this SqlSyncBuildData.SqlSyncBuildProjectRow row)
        {
            return new SqlSyncBuildProject(
                sqlSyncBuildProjectId: row.SqlSyncBuildProject_Id,
                projectName: row.IsProjectNameNull() ? null : row.ProjectName,
                scriptTagRequired: row.IsScriptTagRequiredNull() ? null : row.ScriptTagRequired);
        }

        public static Script ToModel(this SqlSyncBuildData.ScriptRow row)
        {
            return new Script(
                fileName: row.IsFileNameNull() ? null : row.FileName,
                buildOrder: row.IsBuildOrderNull() ? null : row.BuildOrder,
                description: row.IsDescriptionNull() ? null : row.Description,
                rollBackOnError: row.IsRollBackOnErrorNull() ? null : row.RollBackOnError,
                causesBuildFailure: row.IsCausesBuildFailureNull() ? null : row.CausesBuildFailure,
                dateAdded: row.IsDateAddedNull() ? null : row.DateAdded,
                scriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                database: row.IsDatabaseNull() ? null : row.Database,
                stripTransactionText: row.IsStripTransactionTextNull() ? null : row.StripTransactionText,
                allowMultipleRuns: row.IsAllowMultipleRunsNull() ? null : row.AllowMultipleRuns,
                addedBy: row.IsAddedByNull() ? null : row.AddedBy,
                scriptTimeOut: row.IsScriptTimeOutNull() ? null : row.ScriptTimeOut,
                dateModified: row.IsDateModifiedNull() ? null : row.DateModified,
                modifiedBy: row.IsModifiedByNull() ? null : row.ModifiedBy,
                tag: row.IsTagNull() ? null : row.Tag);
        }

        public static Build ToModel(this SqlSyncBuildData.BuildRow row)
        {
            return new Build(
                name: row.IsNameNull() ? null : row.Name,
                buildType: row.IsBuildTypeNull() ? null : row.BuildType,
                buildStart: row.IsBuildStartNull() ? null : row.BuildStart,
                buildEnd: row.IsBuildEndNull() ? null : row.BuildEnd,
                serverName: row.IsServerNameNull() ? null : row.ServerName,
                finalStatus: row.IsFinalStatusNull() ? BuildItemStatus.Unknown : Enum.TryParse<BuildItemStatus>(row.FinalStatus, out var finalStatus) ? finalStatus : BuildItemStatus.Unknown,
                buildId: row.IsBuildIdNull() ? null : row.BuildId,
                userId: row.IsUserIdNull() ? null : row.UserId);
        }

        public static ScriptRun ToModel(this SqlSyncBuildData.ScriptRunRow row)
        {
            return new ScriptRun(
                fileHash: row.IsFileHashNull() ? null : row.FileHash,
                results: row.IsResultsNull() ? null : row.Results,
                fileName: row.IsFileNameNull() ? null : row.FileName,
                runOrder: row.IsRunOrderNull() ? null : row.RunOrder,
                runStart: row.IsRunStartNull() ? null : row.RunStart,
                runEnd: row.IsRunEndNull() ? null : row.RunEnd,
                success: row.IsSuccessNull() ? null : row.Success,
                database: row.IsDatabaseNull() ? null : row.Database,
                scriptRunId: row.IsScriptRunIdNull() ? null : row.ScriptRunId,
                buildId: row.IsBuild_IdNull() ? null : row.Build_Id.ToString());
        }

        public static CommittedScript ToModel(this SqlSyncBuildData.CommittedScriptRow row)
        {
            return new CommittedScript(
                scriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                serverName: row.IsServerNameNull() ? null : row.ServerName,
                committedDate: row.IsCommittedDateNull() ? null : row.CommittedDate,
                allowScriptBlock: row.IsAllowScriptBlockNull() ? null : row.AllowScriptBlock,
                scriptHash: row.IsScriptHashNull() ? null : row.ScriptHash,
                sqlSyncBuildProjectId: row.IsSqlSyncBuildProject_IdNull() ? null : row.SqlSyncBuildProject_Id);
        }

        public static CodeReview ToModel(this SqlSyncBuildData.CodeReviewRow row)
        {
            return new CodeReview(
                codeReviewId: row.IsCodeReviewIdNull() ? null : row.CodeReviewId,
                scriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                reviewDate: row.IsReviewDateNull() ? null : row.ReviewDate,
                reviewBy: row.IsReviewByNull() ? null : row.ReviewBy,
                reviewStatus: row.IsReviewStatusNull() ? null : row.ReviewStatus,
                comment: row.IsCommentNull() ? null : row.Comment,
                reviewNumber: row.IsReviewNumberNull() ? null : row.ReviewNumber,
                checkSum: row.IsCheckSumNull() ? null : row.CheckSum,
                validationKey: row.IsValidationKeyNull() ? null : row.ValidationKey);
        }

        public static SqlSyncBuildData ToDataSet(this SqlSyncBuildDataModel model)
        {
            var ds = new SqlSyncBuildData();

            foreach (var proj in model.SqlSyncBuildProject)
            {
                var row = ds.SqlSyncBuildProject.NewSqlSyncBuildProjectRow();
                row.SqlSyncBuildProject_Id = proj.SqlSyncBuildProjectId;
                if (proj.ProjectName is not null) row.ProjectName = proj.ProjectName;
                if (proj.ScriptTagRequired.HasValue) row.ScriptTagRequired = proj.ScriptTagRequired.Value;
                ds.SqlSyncBuildProject.AddSqlSyncBuildProjectRow(row);
            }

            //foreach (var scripts in model.Scripts)
            //{
            //    var row = ds.Scripts.NewScriptsRow();
            //    row.Scripts_Id = scripts.Scripts_Id;
            //    if (scripts.SqlSyncBuildProject_Id.HasValue) row.SqlSyncBuildProject_Id = scripts.SqlSyncBuildProject_Id.Value;
            //    ds.Scripts.AddScriptsRow(row);
            //}

            foreach (var s in model.Script)
            {
                var row = ds.Script.NewScriptRow();
                if (s.FileName is not null) row.FileName = s.FileName;
                if (s.BuildOrder.HasValue) row.BuildOrder = s.BuildOrder.Value;
                if (s.Description is not null) row.Description = s.Description;
                if (s.RollBackOnError.HasValue) row.RollBackOnError = s.RollBackOnError.Value;
                if (s.CausesBuildFailure.HasValue) row.CausesBuildFailure = s.CausesBuildFailure.Value;
                if (s.DateAdded.HasValue) row.DateAdded = s.DateAdded.Value;
                if (s.ScriptId is not null) row.ScriptId = s.ScriptId;
                if (s.Database is not null) row.Database = s.Database;
                if (s.StripTransactionText.HasValue) row.StripTransactionText = s.StripTransactionText.Value;
                if (s.AllowMultipleRuns.HasValue) row.AllowMultipleRuns = s.AllowMultipleRuns.Value;
                if (s.AddedBy is not null) row.AddedBy = s.AddedBy;
                if (s.ScriptTimeOut.HasValue) row.ScriptTimeOut = s.ScriptTimeOut.Value;
                if (s.DateModified.HasValue) row.DateModified = s.DateModified.Value;
                if (s.ModifiedBy is not null) row.ModifiedBy = s.ModifiedBy;
                if (s.Tag is not null) row.Tag = s.Tag;
                ds.Script.AddScriptRow(row);
            }

            var buildGroups = model.Build.GroupBy(b => b.BuildId);
            foreach (var b in buildGroups)
            {
                var row = ds.Builds.NewBuildsRow();
                ds.Builds.AddBuildsRow(row);
            }

            foreach (var b in model.Build)
            {
                var row = ds.Build.NewBuildRow();
                if (b.Name is not null) row.Name = b.Name;
                if (b.BuildType is not null) row.BuildType = b.BuildType;
                if (b.BuildStart.HasValue) row.BuildStart = b.BuildStart.Value;
                if (b.BuildEnd.HasValue) row.BuildEnd = b.BuildEnd.Value;
                if (b.ServerName is not null) row.ServerName = b.ServerName;
                row.FinalStatus = b.FinalStatus.ToString();
                if (b.BuildId is not null) row.BuildId = b.BuildId;
                if (b.UserId is not null) row.UserId = b.UserId;
                ds.Build.AddBuildRow(row);
            }

            foreach (var sr in model.ScriptRun)
            {
                var row = ds.ScriptRun.NewScriptRunRow();
                if (sr.FileHash is not null) row.FileHash = sr.FileHash;
                if (sr.Results is not null) row.Results = sr.Results;
                if (sr.FileName is not null) row.FileName = sr.FileName;
                if (sr.RunOrder.HasValue) row.RunOrder = sr.RunOrder.Value;
                if (sr.RunStart.HasValue) row.RunStart = sr.RunStart.Value;
                if (sr.RunEnd.HasValue) row.RunEnd = sr.RunEnd.Value;
                if (sr.Success.HasValue) row.Success = sr.Success.Value;
                if (sr.Database is not null) row.Database = sr.Database;
                if (sr.ScriptRunId is not null) row.ScriptRunId = sr.ScriptRunId;
                ds.ScriptRun.AddScriptRunRow(row);
            }

            foreach (var cs in model.CommittedScript)
            {
                var row = ds.CommittedScript.NewCommittedScriptRow();
                if (cs.ScriptId is not null) row.ScriptId = cs.ScriptId;
                if (cs.ServerName is not null) row.ServerName = cs.ServerName;
                if (cs.CommittedDate.HasValue) row.CommittedDate = cs.CommittedDate.Value;
                if (cs.AllowScriptBlock.HasValue) row.AllowScriptBlock = cs.AllowScriptBlock.Value;
                if (cs.ScriptHash is not null) row.ScriptHash = cs.ScriptHash;
                ds.CommittedScript.AddCommittedScriptRow(row);
            }

            foreach (var cr in model.CodeReview)
            {
                var row = ds.CodeReview.NewCodeReviewRow();
                if (cr.CodeReviewId.HasValue) row.CodeReviewId = cr.CodeReviewId.Value;
                if (cr.ScriptId is not null) row.ScriptId = cr.ScriptId;
                if (cr.ReviewDate.HasValue) row.ReviewDate = cr.ReviewDate.Value;
                if (cr.ReviewBy is not null) row.ReviewBy = cr.ReviewBy;
                if (cr.ReviewStatus.HasValue) row.ReviewStatus = cr.ReviewStatus.Value;
                if (cr.Comment is not null) row.Comment = cr.Comment;
                if (cr.ReviewNumber is not null) row.ReviewNumber = cr.ReviewNumber;
                if (cr.CheckSum is not null) row.CheckSum = cr.CheckSum;
                if (cr.ValidationKey is not null) row.ValidationKey = cr.ValidationKey;
                ds.CodeReview.AddCodeReviewRow(row);
            }

            return ds;
        }
    }
}
