using System;

namespace SqlSync.DbInformation {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class CodeTableAuditColumnList {
        
        private System.Collections.Generic.List<string> _UpdateDateColumns = new System.Collections.Generic.List<string>();

        private System.Collections.Generic.List<string> _UpdateIdColumns = new System.Collections.Generic.List<string>();

        private System.Collections.Generic.List<string> _CreateDateColumns = new System.Collections.Generic.List<string>();

        private System.Collections.Generic.List<string> _CreateIdColumns = new System.Collections.Generic.List<string>();

        private System.Collections.Hashtable _validationDict;
        
        public CodeTableAuditColumnList() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("UpdateDateColumns", false);
            this._validationDict.Add("UpdateIdColumns", false);
            this._validationDict.Add("CreateDateColumns", false);
            this._validationDict.Add("CreateIdColumns", false);
        }
        
        public virtual bool IsValid {
            get {
                if ((this.Validate() == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        public virtual System.Collections.Generic.List<string> UpdateDateColumns {
            get {
                return this._UpdateDateColumns;
            }
            set {
                this._UpdateDateColumns = value;
                this._validationDict["UpdateDateColumns"] = true;
            }
        }
        
        public virtual System.Collections.Generic.List<string> UpdateIdColumns {
            get {
                return this._UpdateIdColumns;
            }
            set {
                this._UpdateIdColumns = value;
                this._validationDict["UpdateIdColumns"] = true;
            }
        }
        
        public virtual System.Collections.Generic.List<string> CreateDateColumns {
            get {
                return this._CreateDateColumns;
            }
            set {
                this._CreateDateColumns = value;
                this._validationDict["CreateDateColumns"] = true;
            }
        }
        
        public virtual System.Collections.Generic.List<string> CreateIdColumns {
            get {
                return this._CreateIdColumns;
            }
            set {
                this._CreateIdColumns = value;
                this._validationDict["CreateIdColumns"] = true;
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["UpdateDateColumns"], typeof(bool)))) == false)) {
                missingValues.Add("UpdateDateColumns");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["UpdateIdColumns"], typeof(bool)))) == false)) {
                missingValues.Add("UpdateIdColumns");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["CreateDateColumns"], typeof(bool)))) == false)) {
                missingValues.Add("CreateDateColumns");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["CreateIdColumns"], typeof(bool)))) == false)) {
                missingValues.Add("CreateIdColumns");
            }
            if ((missingValues.Count > 0)) {
                string[] missingVals = new string[missingValues.Count];
                missingValues.CopyTo(missingVals);
                return missingVals;
            }
            else {
                return null;
            }
        }
    }
}
