using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using System.Collections.Generic;
namespace SqlSync.SqlBuild
{
    public class BulkAddData
    {
        public BulkAddData()
        {
            FileList = new List<string>();
            PreSetDatabase = string.Empty;
            DeleteOriginalFiles = false;
            CreateNewEntriesForPreExisting = false;
            LastBuildNumber = 0.0;
        }
        public List<string> FileList
        {
            get;
            set;

        }
        public string PreSetDatabase
        {
            get;
            set;
        }
        public bool DeleteOriginalFiles
        {
            get;
            set;
        }
        public bool CreateNewEntriesForPreExisting
        {
            get;
            set;
        }

        public double LastBuildNumber
        {
            get;
            set;
        }
        public TagInferenceSource TagInferSource
        {
            get;
            set;
        }
        public List<string> TagInferSourceRegexFormats
        {
            get;
            set;
        }
        public string ScriptTag
        {
            get;
            set;
        }
        public bool StripTransactions
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
        public bool RollBackScript
        {
            get;
            set;
        }
        public bool RollBackBuild
        {
            get;
            set;
        }
        public string DatabaseName
        {
            get;
            set;
        }
        public bool AllowMultipleRuns
        {
            get;
            set;
        }

    }
}
