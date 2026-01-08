using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.SqlBuild.Models;

#nullable enable

namespace SqlSync.SqlBuild
{
    public static class SqlSyncBuildDataMappers
    {
        public static SqlSyncBuildDataModel ToModel(this SqlSyncBuildData ds)
        {
            return new SqlSyncBuildDataModel(
                SqlSyncBuildProject: ds?.SqlSyncBuildProject?.Cast<SqlSyncBuildData.SqlSyncBuildProjectRow>().Select(r => r.ToModel()).ToList() ?? new List<SqlSyncBuildProject>(),
                Scripts: ds?.Scripts?.Cast<SqlSyncBuildData.ScriptsRow>().Select(r => r.ToModel()).ToList() ?? new List<Scripts>(),
                Script: ds?.Script?.Cast<SqlSyncBuildData.ScriptRow>().Select(r => r.ToModel()).ToList() ?? new List<Script>(),
                Builds: ds?.Builds?.Cast<SqlSyncBuildData.BuildsRow>().Select(r => r.ToModel()).ToList() ?? new List<Builds>(),
                Build: ds?.Build?.Cast<SqlSyncBuildData.BuildRow>().Select(r => r.ToModel()).ToList() ?? new List<Build>(),
                ScriptRun: ds?.ScriptRun?.Cast<SqlSyncBuildData.ScriptRunRow>().Select(r => r.ToModel()).ToList() ?? new List<ScriptRun>(),
                CommittedScript: ds?.CommittedScript?.Cast<SqlSyncBuildData.CommittedScriptRow>().Select(r => r.ToModel()).ToList() ?? new List<CommittedScript>(),
                CodeReview: ds?.CodeReview?.Cast<SqlSyncBuildData.CodeReviewRow>().Select(r => r.ToModel()).ToList() ?? new List<CodeReview>()
            );
        }

        public static SqlSyncBuildProject ToModel(this SqlSyncBuildData.SqlSyncBuildProjectRow row)
        {
            return new SqlSyncBuildProject(
                SqlSyncBuildProject_Id: row.SqlSyncBuildProject_Id,
                ProjectName: row.IsProjectNameNull() ? null : row.ProjectName,
                ScriptTagRequired: row.IsScriptTagRequiredNull() ? null : row.ScriptTagRequired);
        }

        public static Scripts ToModel(this SqlSyncBuildData.ScriptsRow row)
        {
            return new Scripts(
                Scripts_Id: row.Scripts_Id,
                SqlSyncBuildProject_Id: row.IsSqlSyncBuildProject_IdNull() ? null : row.SqlSyncBuildProject_Id);
        }

        public static Script ToModel(this SqlSyncBuildData.ScriptRow row)
        {
            return new Script(
                FileName: row.IsFileNameNull() ? null : row.FileName,
                BuildOrder: row.IsBuildOrderNull() ? null : row.BuildOrder,
                Description: row.IsDescriptionNull() ? null : row.Description,
                RollBackOnError: row.IsRollBackOnErrorNull() ? null : row.RollBackOnError,
                CausesBuildFailure: row.IsCausesBuildFailureNull() ? null : row.CausesBuildFailure,
                DateAdded: row.IsDateAddedNull() ? null : row.DateAdded,
                ScriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                Database: row.IsDatabaseNull() ? null : row.Database,
                StripTransactionText: row.IsStripTransactionTextNull() ? null : row.StripTransactionText,
                AllowMultipleRuns: row.IsAllowMultipleRunsNull() ? null : row.AllowMultipleRuns,
                AddedBy: row.IsAddedByNull() ? null : row.AddedBy,
                ScriptTimeOut: row.IsScriptTimeOutNull() ? null : row.ScriptTimeOut,
                DateModified: row.IsDateModifiedNull() ? null : row.DateModified,
                ModifiedBy: row.IsModifiedByNull() ? null : row.ModifiedBy,
                Scripts_Id: row.IsScripts_IdNull() ? null : row.Scripts_Id,
                Tag: row.IsTagNull() ? null : row.Tag);
        }

        public static Builds ToModel(this SqlSyncBuildData.BuildsRow row)
        {
            return new Builds(
                Builds_Id: row.Builds_Id,
                SqlSyncBuildProject_Id: row.IsSqlSyncBuildProject_IdNull() ? null : row.SqlSyncBuildProject_Id);
        }

        public static Build ToModel(this SqlSyncBuildData.BuildRow row)
        {
            return new Build(
                Name: row.IsNameNull() ? null : row.Name,
                BuildType: row.IsBuildTypeNull() ? null : row.BuildType,
                BuildStart: row.IsBuildStartNull() ? null : row.BuildStart,
                BuildEnd: row.IsBuildEndNull() ? null : row.BuildEnd,
                ServerName: row.IsServerNameNull() ? null : row.ServerName,
                FinalStatus: row.IsFinalStatusNull() ? null : row.FinalStatus,
                BuildId: row.IsBuildIdNull() ? null : row.BuildId,
                UserId: row.IsUserIdNull() ? null : row.UserId,
                Build_Id: row.Build_Id,
                Builds_Id: row.IsBuilds_IdNull() ? null : row.Builds_Id);
        }

        public static ScriptRun ToModel(this SqlSyncBuildData.ScriptRunRow row)
        {
            return new ScriptRun(
                FileHash: row.IsFileHashNull() ? null : row.FileHash,
                Results: row.IsResultsNull() ? null : row.Results,
                FileName: row.IsFileNameNull() ? null : row.FileName,
                RunOrder: row.IsRunOrderNull() ? null : row.RunOrder,
                RunStart: row.IsRunStartNull() ? null : row.RunStart,
                RunEnd: row.IsRunEndNull() ? null : row.RunEnd,
                Success: row.IsSuccessNull() ? null : row.Success,
                Database: row.IsDatabaseNull() ? null : row.Database,
                ScriptRunId: row.IsScriptRunIdNull() ? null : row.ScriptRunId,
                Build_Id: row.IsBuild_IdNull() ? null : row.Build_Id);
        }

        public static CommittedScript ToModel(this SqlSyncBuildData.CommittedScriptRow row)
        {
            return new CommittedScript(
                ScriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                ServerName: row.IsServerNameNull() ? null : row.ServerName,
                CommittedDate: row.IsCommittedDateNull() ? null : row.CommittedDate,
                AllowScriptBlock: row.IsAllowScriptBlockNull() ? null : row.AllowScriptBlock,
                ScriptHash: row.IsScriptHashNull() ? null : row.ScriptHash,
                SqlSyncBuildProject_Id: row.IsSqlSyncBuildProject_IdNull() ? null : row.SqlSyncBuildProject_Id);
        }

        public static CodeReview ToModel(this SqlSyncBuildData.CodeReviewRow row)
        {
            return new CodeReview(
                CodeReviewId: row.IsCodeReviewIdNull() ? null : row.CodeReviewId,
                ScriptId: row.IsScriptIdNull() ? null : row.ScriptId,
                ReviewDate: row.IsReviewDateNull() ? null : row.ReviewDate,
                ReviewBy: row.IsReviewByNull() ? null : row.ReviewBy,
                ReviewStatus: row.IsReviewStatusNull() ? null : row.ReviewStatus,
                Comment: row.IsCommentNull() ? null : row.Comment,
                ReviewNumber: row.IsReviewNumberNull() ? null : row.ReviewNumber,
                CheckSum: row.IsCheckSumNull() ? null : row.CheckSum,
                ValidationKey: row.IsValidationKeyNull() ? null : row.ValidationKey);
        }

        public static SqlSyncBuildData ToDataSet(this SqlSyncBuildDataModel model)
        {
            var ds = new SqlSyncBuildData();

            foreach (var proj in model.SqlSyncBuildProject)
            {
                var row = ds.SqlSyncBuildProject.NewSqlSyncBuildProjectRow();
                row.SqlSyncBuildProject_Id = proj.SqlSyncBuildProject_Id;
                if (proj.ProjectName is not null) row.ProjectName = proj.ProjectName;
                if (proj.ScriptTagRequired.HasValue) row.ScriptTagRequired = proj.ScriptTagRequired.Value;
                ds.SqlSyncBuildProject.AddSqlSyncBuildProjectRow(row);
            }

            foreach (var scripts in model.Scripts)
            {
                var row = ds.Scripts.NewScriptsRow();
                row.Scripts_Id = scripts.Scripts_Id;
                if (scripts.SqlSyncBuildProject_Id.HasValue) row.SqlSyncBuildProject_Id = scripts.SqlSyncBuildProject_Id.Value;
                ds.Scripts.AddScriptsRow(row);
            }

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
                if (s.Scripts_Id.HasValue) row.Scripts_Id = s.Scripts_Id.Value;
                if (s.Tag is not null) row.Tag = s.Tag;
                ds.Script.AddScriptRow(row);
            }

            foreach (var b in model.Builds)
            {
                var row = ds.Builds.NewBuildsRow();
                row.Builds_Id = b.Builds_Id;
                if (b.SqlSyncBuildProject_Id.HasValue) row.SqlSyncBuildProject_Id = b.SqlSyncBuildProject_Id.Value;
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
                if (b.FinalStatus is not null) row.FinalStatus = b.FinalStatus;
                if (b.BuildId is not null) row.BuildId = b.BuildId;
                if (b.UserId is not null) row.UserId = b.UserId;
                row.Build_Id = b.Build_Id;
                if (b.Builds_Id.HasValue) row.Builds_Id = b.Builds_Id.Value;
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
                if (sr.Build_Id.HasValue) row.Build_Id = sr.Build_Id.Value;
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
                if (cs.SqlSyncBuildProject_Id.HasValue) row.SqlSyncBuildProject_Id = cs.SqlSyncBuildProject_Id.Value;
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
