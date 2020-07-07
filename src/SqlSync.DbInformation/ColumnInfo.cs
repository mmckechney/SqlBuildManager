// ------------------------------------------------------------------------------
// <autogenerated>
//      This code was generated by a the SimpleDataClassGenerator tool.
//      (SimpleDataClassVSGenerator.dll  -- Michael McKechney, author)
// 		<Version> 3.4.1.16443 </Version>
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
// <autogenerated>
// ------------------------------------------------------------------------------
namespace SqlSync.DbInformation {
    using System;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class ColumnInfo {
        
        private string _ColumnName = string.Empty;
        
        private string _DataType = string.Empty;
        
        private int _CharMaximum = 0;
        
        private System.Collections.Hashtable _validationDict;
        
        public ColumnInfo() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("ColumnName", false);
            this._validationDict.Add("DataType", false);
            this._validationDict.Add("CharMaximum", false);
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
        
        public virtual string ColumnName {
            get {
                return this._ColumnName;
            }
            set {
                this._ColumnName = value;
                this._validationDict["ColumnName"] = true;
            }
        }
        
        public virtual string DataType {
            get {
                return this._DataType;
            }
            set {
                this._DataType = value;
                this._validationDict["DataType"] = true;
            }
        }
        
        public virtual int CharMaximum {
            get {
                return this._CharMaximum;
            }
            set {
                this._CharMaximum = value;
                this._validationDict["CharMaximum"] = true;
            }
        }
        
        public virtual string StrColumnName {
            get {
                return this._ColumnName.ToString();
            }
        }
        
        public virtual string StrDataType {
            get {
                return this._DataType.ToString();
            }
        }
        
        public virtual string StrCharMaximum {
            get {
                return this._CharMaximum.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrColumnName);
                sb.Append(delimiter);
                sb.Append(this.StrDataType);
                sb.Append(delimiter);
                sb.Append(this.StrCharMaximum);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(ColumnInfo dataClass) {
            try {
                this.ColumnName = dataClass.ColumnName;
                this.DataType = dataClass.DataType;
                this.CharMaximum = dataClass.CharMaximum;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(ColumnInfo) Method", ex);
            }
        }
        
        public virtual bool Fill(Microsoft.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    if ((reader["column_name"].Equals(System.DBNull.Value) == false)) {
                        this.ColumnName = ((string)(System.Convert.ChangeType(reader["column_name"], typeof(string))));
                    }
                    if ((reader["data_type"].Equals(System.DBNull.Value) == false)) {
                        this.DataType = ((string)(System.Convert.ChangeType(reader["data_type"], typeof(string))));
                    }
                    if ((reader["character_maximum_length"].Equals(System.DBNull.Value) == false)) {
                        this.CharMaximum = ((int)(System.Convert.ChangeType(reader["character_maximum_length"], typeof(int))));
                    }
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
        public virtual bool Fill(System.Data.DataRow sourceDataRow) {
            try {
                if ((sourceDataRow["column_name"].Equals(System.DBNull.Value) == false)) {
                    this.ColumnName = ((string)(System.Convert.ChangeType(sourceDataRow["column_name"], typeof(string))));
                }
                if ((sourceDataRow["data_type"].Equals(System.DBNull.Value) == false)) {
                    this.DataType = ((string)(System.Convert.ChangeType(sourceDataRow["data_type"], typeof(string))));
                }
                if ((sourceDataRow["character_maximum_length"].Equals(System.DBNull.Value) == false)) {
                    this.CharMaximum = ((int)(System.Convert.ChangeType(sourceDataRow["character_maximum_length"], typeof(int))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(System.Data.DataRow sourceDataRow) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Collections.Specialized.NameValueCollection nameValueColl) {
            try {
                if ((nameValueColl.GetValues("ColumnName") != null)) {
                    this.ColumnName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ColumnName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("DataType") != null)) {
                    this.DataType = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("DataType")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("CharMaximum") != null)) {
                    this.CharMaximum = ((int)(System.Convert.ChangeType(nameValueColl.GetValues("CharMaximum")[0], typeof(int))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.ColumnName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.DataType = ((string)(System.Convert.ChangeType(sourceArray.GetValue(1), typeof(string))));
                this.CharMaximum = ((int)(System.Convert.ChangeType(sourceArray.GetValue(2), typeof(int))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.ColumnName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.DataType = ((string)(System.Convert.ChangeType(arrSplitString[1], typeof(string))));
                this.CharMaximum = ((int)(System.Convert.ChangeType(arrSplitString[2], typeof(int))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.ColumnName.ToString());
                sb.Append(delimiter);
                sb.Append(this.DataType.ToString());
                sb.Append(delimiter);
                sb.Append(this.CharMaximum.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[3];
            try {
                myArray[0] = this._ColumnName.ToString();
                myArray[1] = this._DataType.ToString();
                myArray[2] = this._CharMaximum.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.GetStringArray() Method", ex);
            }
        }
        
        public virtual string GetFixedLengthString() {
            throw new System.NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subS" +
                    "tringLength value set");
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("ColumnName", this.ColumnName.ToString());
                nameValueColl.Add("DataType", this.DataType.ToString());
                nameValueColl.Add("CharMaximum", this.CharMaximum.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ColumnInfo.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ColumnName"], typeof(bool)))) == false)) {
                missingValues.Add("ColumnName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["DataType"], typeof(bool)))) == false)) {
                missingValues.Add("DataType");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["CharMaximum"], typeof(bool)))) == false)) {
                missingValues.Add("CharMaximum");
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
