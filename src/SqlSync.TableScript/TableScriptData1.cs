// ------------------------------------------------------------------------------
// <autogenerated>
//      This code was generated by a the SimpleDataClassGenerator tool.
//      (SimpleDataClassVSGenerator.dll  -- Michael McKechney, author)
// 		<Version> 3.4.0.2645 </Version>
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
// <autogenerated>
// ------------------------------------------------------------------------------
namespace SqlSync {
    using System;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    [System.Drawing.ToolboxBitmap(typeof(System.Data.DataSet))]
    public class TableScriptData {
        
        private string _TableName = string.Empty;
        
        private System.Data.DataTable _ValuesTable = null;
        
        private string _InsertScript = string.Empty;
        
        private string _SelectStatement = string.Empty;
        
        private System.Collections.Hashtable _validationDict;
        
        public TableScriptData() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("TableName", false);
            this._validationDict.Add("ValuesTable", false);
            this._validationDict.Add("InsertScript", false);
            this._validationDict.Add("SelectStatement", false);
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
        
        public virtual System.Data.DataTable ValuesTable {
            get {
                return this._ValuesTable;
            }
            set {
                this._ValuesTable = value;
                this._validationDict["ValuesTable"] = true;
            }
        }
        
        public virtual string InsertScript {
            get {
                return this._InsertScript;
            }
            set {
                this._InsertScript = value;
                this._validationDict["InsertScript"] = true;
            }
        }
        
        public virtual string SelectStatement {
            get {
                return this._SelectStatement;
            }
            set {
                this._SelectStatement = value;
                this._validationDict["SelectStatement"] = true;
            }
        }
        
        public virtual string StrTableName {
            get {
                return this._TableName.ToString();
            }
        }
        
        public virtual string StrValuesTable {
            get {
                return this._ValuesTable.ToString();
            }
        }
        
        public virtual string StrInsertScript {
            get {
                return this._InsertScript.ToString();
            }
        }
        
        public virtual string StrSelectStatement {
            get {
                return this._SelectStatement.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrTableName);
                sb.Append(delimiter);
                sb.Append(this.StrValuesTable);
                sb.Append(delimiter);
                sb.Append(this.StrInsertScript);
                sb.Append(delimiter);
                sb.Append(this.StrSelectStatement);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(TableScriptData dataClass) {
            try {
                this.TableName = dataClass.TableName;
                this.ValuesTable = dataClass.ValuesTable;
                this.InsertScript = dataClass.InsertScript;
                this.SelectStatement = dataClass.SelectStatement;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(TableScriptData) Method", ex);
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
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(SqlDataReader) Method", ex);
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
                if ((nameValueColl.GetValues("ValuesTable") != null)) {
                    this.ValuesTable = ((System.Data.DataTable)(System.Convert.ChangeType(nameValueColl.GetValues("ValuesTable")[0], typeof(System.Data.DataTable))));
                }
                if ((nameValueColl.GetValues("InsertScript") != null)) {
                    this.InsertScript = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("InsertScript")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("SelectStatement") != null)) {
                    this.SelectStatement = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("SelectStatement")[0], typeof(string))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.TableName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.ValuesTable = ((System.Data.DataTable)(System.Convert.ChangeType(sourceArray.GetValue(1), typeof(System.Data.DataTable))));
                this.InsertScript = ((string)(System.Convert.ChangeType(sourceArray.GetValue(2), typeof(string))));
                this.SelectStatement = ((string)(System.Convert.ChangeType(sourceArray.GetValue(3), typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.TableName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.ValuesTable = ((System.Data.DataTable)(System.Convert.ChangeType(arrSplitString[1], typeof(System.Data.DataTable))));
                this.InsertScript = ((string)(System.Convert.ChangeType(arrSplitString[2], typeof(string))));
                this.SelectStatement = ((string)(System.Convert.ChangeType(arrSplitString[3], typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.TableName.ToString());
                sb.Append(delimiter);
                sb.Append(this.ValuesTable.ToString());
                sb.Append(delimiter);
                sb.Append(this.InsertScript.ToString());
                sb.Append(delimiter);
                sb.Append(this.SelectStatement.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[4];
            try {
                myArray[0] = this._TableName.ToString();
                myArray[1] = this._ValuesTable.ToString();
                myArray[2] = this._InsertScript.ToString();
                myArray[3] = this._SelectStatement.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.GetStringArray() Method", ex);
            }
        }
        
        public virtual string GetFixedLengthString() {
            throw new System.NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subS" +
"tringLength value set");
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("TableName", this.TableName.ToString());
                nameValueColl.Add("ValuesTable", this.ValuesTable.ToString());
                nameValueColl.Add("InsertScript", this.InsertScript.ToString());
                nameValueColl.Add("SelectStatement", this.SelectStatement.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: TableScriptData.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["TableName"], typeof(bool)))) == false)) {
                missingValues.Add("TableName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ValuesTable"], typeof(bool)))) == false)) {
                missingValues.Add("ValuesTable");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["InsertScript"], typeof(bool)))) == false)) {
                missingValues.Add("InsertScript");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["SelectStatement"], typeof(bool)))) == false)) {
                missingValues.Add("SelectStatement");
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