// ------------------------------------------------------------------------------
// <autogenerated>
//      This code was generated by a the SimpleDataClassGenerator tool.
//      (SimpleDataClassVSGenerator.dll  -- Michael McKechney, author)
// 		<Version> 3.3.1.27814 </Version>
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
// <autogenerated>
// ------------------------------------------------------------------------------
namespace SqlSync.TableScript {
    using System;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class TableScriptingRule {
        
        private string _TableName = string.Empty;
        
        private string[] _CheckKeyColumns = new string[0];
        
        private System.Collections.Hashtable _validationDict;
        
        public TableScriptingRule() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("TableName", false);
            this._validationDict.Add("CheckKeyColumns", false);
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
        
        public virtual string TableName {
            get {
                return this._TableName;
            }
            set {
                this._TableName = value;
                this._validationDict["TableName"] = true;
            }
        }
        
        public virtual string[] CheckKeyColumns {
            get {
                return this._CheckKeyColumns;
            }
            set {
                this._CheckKeyColumns = value;
                this._validationDict["CheckKeyColumns"] = true;
            }
        }
        
        public virtual string StrTableName {
            get {
                return this._TableName.ToString();
            }
        }
        
        public virtual string StrCheckKeyColumns {
            get {
                return this._CheckKeyColumns.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrTableName);
                sb.Append(delimiter);
                sb.Append(this.StrCheckKeyColumns);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(TableScriptingRule dataClass) {
            try {
                this.TableName = dataClass.TableName;
                this.CheckKeyColumns = dataClass.CheckKeyColumns;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(TableScriptingRule) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
        public virtual bool Fill(System.Collections.Specialized.NameValueCollection nameValueColl) {
            try {
                if ((nameValueColl.GetValues("TableName") != null)) {
                    this.TableName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("TableName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("CheckKeyColumns") != null)) {
                    this.CheckKeyColumns = ((string[])(System.Convert.ChangeType(nameValueColl.GetValues("CheckKeyColumns")[0], typeof(string[]))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.TableName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.CheckKeyColumns = ((string[])(System.Convert.ChangeType(sourceArray.GetValue(1), typeof(string[]))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.TableName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.CheckKeyColumns = ((string[])(System.Convert.ChangeType(arrSplitString[1], typeof(string[]))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.TableName.ToString());
                sb.Append(delimiter);
                sb.Append(this.CheckKeyColumns.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[2];
            try {
                myArray[0] = this._TableName.ToString();
                myArray[1] = this._CheckKeyColumns.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.GetStringArray() Method", ex);
            }
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("TableName", this.TableName.ToString());
                nameValueColl.Add("CheckKeyColumns", this.CheckKeyColumns.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptingRule.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["TableName"], typeof(bool)))) == false)) {
                missingValues.Add("TableName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["CheckKeyColumns"], typeof(bool)))) == false)) {
                missingValues.Add("CheckKeyColumns");
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
