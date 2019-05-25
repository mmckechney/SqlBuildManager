using System;
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
    public enum BuildResultStatus : int
    {
        BUILD_FAILED_AND_ROLLED_BACK,
        SCRIPT_GENERATION_COMPLETE,
        BUILD_COMMITTED,
        BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
        BUILD_CANCELLED_AND_ROLLED_BACK,
        BUILD_FAILED_NO_TRANSACTION,
        BUILD_CANCELLED_NO_TRANSACTION

    }
	public class ResortBuildType
	{
		public static readonly string[] SortOrder = new string[]{"TAB","SQL","VIW","UDF","PRC", "TRG"};

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

	public class BuildItemStatus
	{
		public const string Committed = "Committed";
		public const string RolledBack = "RolledBack";
		public const string TrialRolledBack = "TrialRolledBack";
        /// <summary>
        /// This status reserved for MultiDb runs that have failed processing, but not yet rolled back
        /// </summary>
        public const string PendingRollBack = "PendingRollback";
        /// <summary>
        /// This status reserved for MultiDb runs that are currently in progress
        /// </summary>
        public const string Pending = "Pending";
        /// <summary>
        /// For when a build fails but was run without a transaction
        /// </summary>
        public const string FailedNoTransaction = "FailedNoTransaction";
        /// <summary>
        /// A build failure that was due to a script returning a SqlException with message "Timeout Expired."
        /// </summary>
        public const string FailedDueToScriptTimeout = "FailedDueToScriptTimeout";
        /// <summary>
        /// A build that was committed, but required at least one retry due to a timeout
        /// </summary>
        public const string CommittedWithTimeoutRetries = "CommittedWithTimeoutRetries";
        /// <summary>
        /// A build that was committed, but required at least one retry due to a timeout
        /// </summary>
        public const string RolledBackAfterRetries = "RolledBackAfterRetries";
        /// <summary>
        /// The databases were compared via dacpac and are already in sync
        /// </summary>
        public const string AlreadyInSync = "AlreadyInSync";
        /// <summary>
        /// Database was updated but required the creation and use of a custom dacpac
        /// </summary>
        public const string CommittedWithCustomDacpac = "CommittedWithCustomDacpac";
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
