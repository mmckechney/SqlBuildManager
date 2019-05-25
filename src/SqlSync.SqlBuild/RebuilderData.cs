// ------------------------------------------------------------------------------
// <autogenerated>
//      This code was generated by a the SimpleDataClassGenerator tool.
//      (SimpleDataClassVSGenerator.dll  -- Michael McKechney, author)
// 		<Version> 3.4.0.2645 </Version>
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
// <autogenerated>
// ------------------------------------------------------------------------------
namespace SqlSync.SqlBuild {
    using System;
    using System.Data;
    using System.Xml;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    [System.Drawing.ToolboxBitmap(typeof(System.Data.DataSet))]
    public class RebuilderData {
        
        private string _ScriptFileName = String.Empty;
        
        private System.Guid _ScriptId;
        
        private int _Sequence = -1;
        
        private string _ScriptText = String.Empty;
        
        private string _Database = String.Empty;
        
        private string _Tag = String.Empty;
        
        private System.Collections.Hashtable _validationDict;
        
        public RebuilderData() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("ScriptFileName", false);
            this._validationDict.Add("ScriptId", false);
            this._validationDict.Add("Sequence", false);
            this._validationDict.Add("ScriptText", false);
            this._validationDict.Add("Database", false);
            this._validationDict.Add("Tag", false);
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
        
        public virtual string ScriptFileName {
            get {
                return this._ScriptFileName;
            }
            set {
                this._ScriptFileName = value;
                this._validationDict["ScriptFileName"] = true;
            }
        }
        
        public virtual System.Guid ScriptId {
            get {
                return this._ScriptId;
            }
            set {
                this._ScriptId = value;
                this._validationDict["ScriptId"] = true;
            }
        }
        
        public virtual int Sequence {
            get {
                return this._Sequence;
            }
            set {
                this._Sequence = value;
                this._validationDict["Sequence"] = true;
            }
        }
        
        public virtual string ScriptText {
            get {
                return this._ScriptText;
            }
            set {
                this._ScriptText = value;
                this._validationDict["ScriptText"] = true;
            }
        }
        
        public virtual string Database {
            get {
                return this._Database;
            }
            set {
                this._Database = value;
                this._validationDict["Database"] = true;
            }
        }
        
        public virtual string Tag {
            get {
                return this._Tag;
            }
            set {
                this._Tag = value;
                this._validationDict["Tag"] = true;
            }
        }
        
        public virtual string StrScriptFileName {
            get {
                return this._ScriptFileName.ToString();
            }
        }
        
        public virtual string StrScriptId {
            get {
                return this._ScriptId.ToString();
            }
        }
        
        public virtual string StrSequence {
            get {
                return this._Sequence.ToString();
            }
        }
        
        public virtual string StrScriptText {
            get {
                return this._ScriptText.ToString();
            }
        }
        
        public virtual string StrDatabase {
            get {
                return this._Database.ToString();
            }
        }
        
        public virtual string StrTag {
            get {
                return this._Tag.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrScriptFileName);
                sb.Append(delimiter);
                sb.Append(this.StrScriptId);
                sb.Append(delimiter);
                sb.Append(this.StrSequence);
                sb.Append(delimiter);
                sb.Append(this.StrScriptText);
                sb.Append(delimiter);
                sb.Append(this.StrDatabase);
                sb.Append(delimiter);
                sb.Append(this.StrTag);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(RebuilderData dataClass) {
            try {
                this.ScriptFileName = dataClass.ScriptFileName;
                this.ScriptId = dataClass.ScriptId;
                this.Sequence = dataClass.Sequence;
                this.ScriptText = dataClass.ScriptText;
                this.Database = dataClass.Database;
                this.Tag = dataClass.Tag;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(RebuilderData) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    if ((reader["ScriptFileName"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptFileName = ((string)(System.Convert.ChangeType(reader["ScriptFileName"], typeof(string))));
                    }
                    if ((reader["ScriptId"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptId = new System.Guid(reader["ScriptId"].ToString());
                    }
                    if ((reader["Sequence"].Equals(System.DBNull.Value) == false)) {
                        this.Sequence = ((int)(System.Convert.ChangeType(reader["Sequence"], typeof(int))));
                    }
                    if ((reader["ScriptText"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptText = ((string)(System.Convert.ChangeType(reader["ScriptText"], typeof(string))));
                    }
                    if ((reader["Database"].Equals(System.DBNull.Value) == false)) {
                        this.Database = ((string)(System.Convert.ChangeType(reader["Database"], typeof(string))));
                    }
                    if ((reader["Tag"].Equals(System.DBNull.Value) == false)) {
                        this.Tag = ((string)(System.Convert.ChangeType(reader["Tag"], typeof(string))));
                    }
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
        public virtual bool Fill(System.Collections.Specialized.NameValueCollection nameValueColl) {
            try {
                if ((nameValueColl.GetValues("ScriptFileName") != null)) {
                    this.ScriptFileName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ScriptFileName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("ScriptId") != null)) {
                    this.ScriptId = new System.Guid(nameValueColl.GetValues("ScriptId")[0].ToString());
                }
                if ((nameValueColl.GetValues("Sequence") != null)) {
                    this.Sequence = ((int)(System.Convert.ChangeType(nameValueColl.GetValues("Sequence")[0], typeof(int))));
                }
                if ((nameValueColl.GetValues("ScriptText") != null)) {
                    this.ScriptText = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ScriptText")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("Database") != null)) {
                    this.Database = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("Database")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("Tag") != null)) {
                    this.Tag = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("Tag")[0], typeof(string))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.ScriptFileName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.ScriptId = new System.Guid(sourceArray.GetValue(1).ToString());
                this.Sequence = ((int)(System.Convert.ChangeType(sourceArray.GetValue(2), typeof(int))));
                this.ScriptText = ((string)(System.Convert.ChangeType(sourceArray.GetValue(3), typeof(string))));
                this.Database = ((string)(System.Convert.ChangeType(sourceArray.GetValue(4), typeof(string))));
                this.Tag = ((string)(System.Convert.ChangeType(sourceArray.GetValue(5), typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.ScriptFileName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.ScriptId = new System.Guid(arrSplitString[1].ToString());
                this.Sequence = ((int)(System.Convert.ChangeType(arrSplitString[2], typeof(int))));
                this.ScriptText = ((string)(System.Convert.ChangeType(arrSplitString[3], typeof(string))));
                this.Database = ((string)(System.Convert.ChangeType(arrSplitString[4], typeof(string))));
                this.Tag = ((string)(System.Convert.ChangeType(arrSplitString[5], typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.ScriptFileName.ToString());
                sb.Append(delimiter);
                sb.Append(this.ScriptId.ToString());
                sb.Append(delimiter);
                sb.Append(this.Sequence.ToString());
                sb.Append(delimiter);
                sb.Append(this.ScriptText.ToString());
                sb.Append(delimiter);
                sb.Append(this.Database.ToString());
                sb.Append(delimiter);
                sb.Append(this.Tag.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[6];
            try {
                myArray[0] = this._ScriptFileName.ToString();
                myArray[1] = this._ScriptId.ToString();
                myArray[2] = this._Sequence.ToString();
                myArray[3] = this._ScriptText.ToString();
                myArray[4] = this._Database.ToString();
                myArray[5] = this._Tag.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.GetStringArray() Method", ex);
            }
        }
        
        public virtual string GetFixedLengthString() {
            throw new System.NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subS" +
                    "tringLength value set");
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("ScriptFileName", this.ScriptFileName.ToString());
                nameValueColl.Add("ScriptId", this.ScriptId.ToString());
                nameValueColl.Add("Sequence", this.Sequence.ToString());
                nameValueColl.Add("ScriptText", this.ScriptText.ToString());
                nameValueColl.Add("Database", this.Database.ToString());
                nameValueColl.Add("Tag", this.Tag.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: RebuilderData.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptFileName"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptFileName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptId"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptId");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["Sequence"], typeof(bool)))) == false)) {
                missingValues.Add("Sequence");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptText"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptText");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["Database"], typeof(bool)))) == false)) {
                missingValues.Add("Database");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["Tag"], typeof(bool)))) == false)) {
                missingValues.Add("Tag");
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