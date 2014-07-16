using System.Linq;
using System;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.Syncronizer {


    public partial class DatabaseRunHistory {
        
        private List<BuildFileHistory> buildFileHistoryField = new List<BuildFileHistory>();
        

        public List<BuildFileHistory> BuildFileHistory {
            get {
                return this.buildFileHistoryField;
            }
            set {
                this.buildFileHistoryField = value;
            }
        }
    }
    
       public partial class BuildFileHistory {
        
        private List<ScriptHistory> scriptHistoryField = new List<ScriptHistory>();
        
        private string buildFileNameField;
        
        private string buildFileHashField;
        
        private System.DateTime commitDateField;
        
        public BuildFileHistory() {
            this.buildFileNameField = "";
            this.buildFileHashField = "";
            this.commitDateField = DateTime.MinValue;
        }
        
        public List<ScriptHistory>  ScriptHistory {
            get {
                return this.scriptHistoryField;
            }
            set {
                this.scriptHistoryField = value;
            }
        }
        
        public string BuildFileName {
            get {
                return this.buildFileNameField;
            }
            set {
                this.buildFileNameField = value;
            }
        }
        
        public string BuildFileHash {
            get {
                return this.buildFileHashField;
            }
            set {
                this.buildFileHashField = value;
            }
        }
        
        public System.DateTime CommitDate {
            get {
                return this.commitDateField;
            }
            set {
                this.commitDateField = value;
            }
        }
    }
    

    public partial class ScriptHistory {
        
        private string scriptNameField;
        
        private string scriptHashField;
        
        private int sequenceField;
        
        private string scriptIdField;
        
        public ScriptHistory() {
            this.scriptNameField = "";
            this.scriptHashField = "";
            this.sequenceField = -1;
            this.scriptIdField = "";
        }
        
        public string ScriptName {
            get {
                return this.scriptNameField;
            }
            set {
                this.scriptNameField = value;
            }
        }
        
        public string ScriptHash {
            get {
                return this.scriptHashField;
            }
            set {
                this.scriptHashField = value;
            }
        }
        
        public int Sequence {
            get {
                return this.sequenceField;
            }
            set {
                this.sequenceField = value;
            }
        }
        
        public string ScriptId {
            get {
                return this.scriptIdField;
            }
            set {
                this.scriptIdField = value;
            }
        }
    }
}
