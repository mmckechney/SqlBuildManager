using System;
using System.Collections.Generic;
using System.Linq;
namespace SqlSync.SqlBuild.Syncronizer
{


    public partial class DatabaseRunHistory
    {

        private List<BuildFileHistory> buildFileHistoryField = new List<BuildFileHistory>();


        public List<BuildFileHistory> BuildFileHistory
        {
            get
            {
                return buildFileHistoryField;
            }
            set
            {
                buildFileHistoryField = value;
            }
        }
        public override string ToString()
        {
            if (BuildFileHistory.Any())
            {
                var result =
                    BuildFileHistory.Select(
                        h =>
                        h.BuildFileHash + "\t" + h.CommitDate.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "\t" +
                        h.BuildFileName)
                        .Aggregate((a, b) => a + "\r\n" + b);

                return result;
            }
            else
            {
                return "No History Retrieved";
            }
        }
    }

    public partial class BuildFileHistory
    {

        //private List<ScriptHistory> scriptHistoryField = new List<ScriptHistory>();

        private string buildFileNameField;

        private string buildFileHashField;

        private System.DateTime commitDateField;

        public BuildFileHistory()
        {
            buildFileNameField = "";
            buildFileHashField = "";
            commitDateField = DateTime.MinValue;
        }

        //public List<ScriptHistory>  ScriptHistory {
        //    get {
        //        return this.scriptHistoryField;
        //    }
        //    set {
        //        this.scriptHistoryField = value;
        //    }
        //}

        public string BuildFileName
        {
            get
            {
                return buildFileNameField;
            }
            set
            {
                buildFileNameField = value;
            }
        }

        public string BuildFileHash
        {
            get
            {
                return buildFileHashField;
            }
            set
            {
                buildFileHashField = value;
            }
        }

        public System.DateTime CommitDate
        {
            get
            {
                return commitDateField;
            }
            set
            {
                commitDateField = value;
            }
        }
    }


    //public partial class ScriptHistory {

    //    private string scriptNameField;

    //    private string scriptHashField;

    //    private int sequenceField;

    //    private string scriptIdField;

    //    public ScriptHistory() {
    //        this.scriptNameField = "";
    //        this.scriptHashField = "";
    //        this.sequenceField = -1;
    //        this.scriptIdField = "";
    //    }

    //    public string ScriptName {
    //        get {
    //            return this.scriptNameField;
    //        }
    //        set {
    //            this.scriptNameField = value;
    //        }
    //    }

    //    public string ScriptHash {
    //        get {
    //            return this.scriptHashField;
    //        }
    //        set {
    //            this.scriptHashField = value;
    //        }
    //    }

    //    public int Sequence {
    //        get {
    //            return this.sequenceField;
    //        }
    //        set {
    //            this.sequenceField = value;
    //        }
    //    }

    //    public string ScriptId {
    //        get {
    //            return this.scriptIdField;
    //        }
    //        set {
    //            this.scriptIdField = value;
    //        }
    //    }
    //}
}
