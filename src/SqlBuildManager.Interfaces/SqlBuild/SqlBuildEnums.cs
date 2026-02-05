using System.Text.RegularExpressions;
namespace SqlSync.SqlBuild
{
    public enum DefaultScriptCopyStatus
    {
        Success,
        PreexistingDifferent,
        PreexistingReadOnly,
        PreexistingDifferentReadOnly,
        DefaultNotFound
    }

    public enum DefaultScriptCopyAction
    {
        OverwriteExisting,
        LeaveExisting,
        Undefined
    }

    public class ResortBuildType
    {
        public static readonly string[] SortOrder = new string[] { "TAB", "SQL", "VIW", "UDF", "PRC", "TRG" };

    }
    public enum ResequenceIgnore : int
    {
        StartNumber = 1000
    }
    public enum ImportFileStatus : int
    {
        Canceled = -1000,
        UnableToImport = -1,
        NoRowsImported = -500
    }
    public class XmlFileNames
    {
        public const string ExportFile = "SqlSyncBuildProject.xml";
        public const string MainProjectFile = "SqlSyncBuildProject.xml";
        public const string HistoryFile = "SqlSyncBuildHistory.xml";
    }
    public enum ScriptListIndex : int
    {
        PreRunIconCol = 0,
        PolicyIconColumn = 1,
        CodeReviewStatusIconColumn = 2,
        SequenceNumber = 3,
        FileName = 4,
        Database = 5,
        ScriptId = 6,
        FileSize = 7,
        ScriptTag = 8,
        DateAdded = 9,
        DateModified = 10

    }

    public enum BuildListIndex : int
    {
        SequenceNumber = 0,
        FileName = 1,
        Database = 2,
        OriginalSequenceNumber = 3,
        Status = 4,
        Duration = 5
    }

    public class BuildType
    {
        public const string Trial = "Trial";
        public const string TrialPartial = "Trial - Partial";
        public const string DevelopmentIntegration = "Development Integration";
        public const string QualityAssurance = "Quality Assurance";
        public const string UserAcceptance = "User Acceptance";
        public const string Staging = "Staging";
        public const string Production = "Production";
        public const string Partial = "Partial";
        public const string Other = "Other";
    }
    public class BuildTransaction
    {
        public const string TransactionName = "SqlBuildTrans";
    }

    public enum BuildResultStatus : int
    {
        BUILD_FAILED_AND_ROLLED_BACK,
        SCRIPT_GENERATION_COMPLETE,
        BUILD_COMMITTED,
        BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
        BUILD_CANCELLED_AND_ROLLED_BACK,
        BUILD_FAILED_NO_TRANSACTION,
        BUILD_CANCELLED_NO_TRANSACTION,
        UNKNOWN

    }
    public enum BuildItemStatus
    {
        Committed = 0,
        RolledBack = -1,
        TrialRolledBack = 1,
        /// <summary>
        /// This status reserved for MultiDb runs that have failed processing, but not yet rolled back
        /// </summary>
        PendingRollBack = -2,
        /// <summary>
        /// This status reserved for MultiDb runs that are currently in progress
        /// </summary>
        Pending = 10,
        /// <summary>
        /// For when a build fails but was run without a transaction
        /// </summary>
        FailedNoTransaction = -3,
        /// <summary>
        /// A build failure that was due to a script returning a SqlException with message "Timeout Expired."
        /// </summary>
        FailedDueToScriptTimeout = -4,
        /// <summary>
        /// A build that was committed, but required at least one retry due to a timeout
        /// </summary>
        CommittedWithTimeoutRetries = 5,
        /// <summary>
        /// A build that was committed, but required at least one retry due to a timeout
        /// </summary>
        RolledBackAfterRetries = -6,
        /// <summary>
        /// The databases were compared via dacpac and are already in sync
        /// </summary>
        AlreadyInSync = 6,
        /// <summary>
        /// Database was updated but required the creation and use of a custom dacpac
        /// </summary>
        CommittedWithCustomDacpac = 7,
        FailedWithCustomDacpac = -8,
        Unknown = -100
    }
    public class BatchParsing
    {
        public const string Delimiter = "GO";
    }

    public class ScriptTokens
    {
        public const string BuildDescription = "#BuildDescription#";
        public const string BuildFileName = "#BuildFileName#";
        public const string BuildPackageHash = "#BuildPackageHash#";

        public static Regex regBuildDescription = new Regex(ScriptTokens.BuildDescription, RegexOptions.IgnoreCase);
        public static Regex regBuildFileName = new Regex(ScriptTokens.BuildFileName, RegexOptions.IgnoreCase);
        public static Regex regBuildPackageHash = new Regex(ScriptTokens.BuildPackageHash, RegexOptions.IgnoreCase);
    }

}
