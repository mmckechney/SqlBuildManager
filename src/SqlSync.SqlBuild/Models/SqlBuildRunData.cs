using System;
using SqlSync.Connection;
using System.Collections.Generic;
using System.Linq;
namespace SqlSync.SqlBuild.Models
{
    #nullable enable
    public class SqlBuildRunData
    {

        public SqlSyncBuildDataModel BuildDataModel { get; set; } 
        public string BuildType { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string BuildDescription { get; set; } = string.Empty;
        public double StartIndex { get; set; } = 0;
        public string ProjectFileName { get; set; } = string.Empty;
        public bool IsTrial { get; set; } = false;
        public double[] RunItemIndexes { get; set; } = Array.Empty<double>();
        public bool RunScriptOnly { get; set; } = false;
        public string BuildFileName { get; set; } = string.Empty;
        public string LogToDatabaseName { get; set; } = string.Empty;
        public bool IsTransactional { get; set; } = true;
        public string PlatinumDacPacFileName { get; set; } = string.Empty;
        public List<DatabaseOverride> TargetDatabaseOverrides { get; set; } = new();
        public bool ForceCustomDacpac { get; set; }
        public string BuildRevision { get; set; } = string.Empty;
        public int DefaultScriptTimeout { get; set; } = 500;
        public bool AllowObjectDelete { get; set; } = false;

        //public Models.SqlBuildRunDataModel ToModel() => new Models.SqlBuildRunDataModel(
        //    buildDataModel: BuildDataModel,
        //    buildType: BuildType,
        //    server: Server,
        //    buildDescription: BuildDescription,
        //    startIndex: StartIndex,
        //    projectFileName: ProjectFileName,
        //    isTrial: IsTrial,
        //    runItemIndexes: RunItemIndexes,
        //    runScriptOnly: RunScriptOnly,
        //    buildFileName: BuildFileName,
        //    logToDatabaseName: LogToDatabaseName,
        //    isTransactional: IsTransactional,
        //    platinumDacPacFileName: PlatinumDacPacFileName,
        //    targetDatabaseOverrides: TargetDatabaseOverrides,
        //    forceCustomDacpac: ForceCustomDacpac,
        //    buildRevision: BuildRevision,
        //    defaultScriptTimeout: DefaultScriptTimeout,
        //    allowObjectDelete: AllowObjectDelete);

        //public static SqlBuildRunData FromModel(Models.SqlBuildRunDataModel model) => new SqlBuildRunData
        //{
        //    BuildDataModel = model.BuildDataModel,
        //    BuildType = model.BuildType ?? string.Empty,
        //    Server = model.Server ?? string.Empty,
        //    BuildDescription = model.BuildDescription ?? string.Empty,
        //    StartIndex = model.StartIndex ?? 0,
        //    ProjectFileName = model.ProjectFileName ?? string.Empty,
        //    IsTrial = model.IsTrial ?? false,
        //    RunItemIndexes = model.RunItemIndexes?.ToArray() ?? Array.Empty<double>(),
        //    RunScriptOnly = model.RunScriptOnly ?? false,
        //    BuildFileName = model.BuildFileName ?? string.Empty,
        //    LogToDatabaseName = model.LogToDatabaseName ?? string.Empty,
        //    IsTransactional = model.IsTransactional ?? true,
        //    PlatinumDacPacFileName = model.PlatinumDacPacFileName ?? string.Empty,
        //    TargetDatabaseOverrides = model.TargetDatabaseOverrides?.ToList() ?? new List<DatabaseOverride>(),
        //    ForceCustomDacpac = model.ForceCustomDacpac ?? false,
        //    BuildRevision = model.BuildRevision ?? string.Empty,
        //    DefaultScriptTimeout = model.DefaultScriptTimeout,
        //    AllowObjectDelete = model.AllowObjectDelete ?? false
        //};
    }

    internal static class SqlBuildDataDefaults
    {
        public static Models.SqlSyncBuildDataModel Create() => SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
    }
}
