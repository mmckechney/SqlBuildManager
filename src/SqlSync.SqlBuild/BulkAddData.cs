using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
namespace SqlSync.SqlBuild
{
    public class BulkAddData
    {
        public BulkAddData()
        {
            this.FileList = new List<string>();
            this.PreSetDatabase = string.Empty;
            this.DeleteOriginalFiles = false;
            this.CreateNewEntriesForPreExisting = false;
            this.LastBuildNumber = 0.0;           
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
