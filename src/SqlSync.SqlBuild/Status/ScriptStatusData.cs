// ------------------------------------------------------------------------------
// <autogenerated>
//      This code was generated by a the SimpleDataClassGenerator tool.
//      (SimpleDataClassVSGenerator.dll  -- Michael McKechney, author)
// 		<Version> 4.0.1.17856 </Version>
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
// <autogenerated>
// ------------------------------------------------------------------------------
namespace SqlSync.SqlBuild.Status {
    using System;
    using System.Data;
    using System.Xml;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignTimeVisible(true)]
    public partial class ScriptStatusData {
        
        private string _ScriptName = string.Empty;
        
        private string _ScriptId = string.Empty;
        
        private SqlSync.SqlBuild.ScriptStatusType _ScriptStatus = SqlSync.SqlBuild.ScriptStatusType.Unknown;
        
        private string _ServerName = String.Empty;
        
        private string _DatabaseName = String.Empty;
        
        private System.DateTime _LastCommitDate;
        
        private System.DateTime _ServerChangeDate;
        
        private System.Collections.Hashtable _validationDict;
        
        public ScriptStatusData() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("ScriptName", false);
            this._validationDict.Add("ScriptId", false);
            this._validationDict.Add("ScriptStatus", false);
            this._validationDict.Add("ServerName", false);
            this._validationDict.Add("DatabaseName", false);
            this._validationDict.Add("LastCommitDate", false);
            this._validationDict.Add("ServerChangeDate", false);
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
        
        public virtual string ScriptName {
            get {
                return this._ScriptName;
            }
            set {
                this._ScriptName = value;
                this._validationDict["ScriptName"] = true;
            }
        }
        
        public virtual string ScriptId {
            get {
                return this._ScriptId;
            }
            set {
                this._ScriptId = value;
                this._validationDict["ScriptId"] = true;
            }
        }
        
        public virtual SqlSync.SqlBuild.ScriptStatusType ScriptStatus {
            get {
                return this._ScriptStatus;
            }
            set {
                this._ScriptStatus = value;
                this._validationDict["ScriptStatus"] = true;
            }
        }
        
        public virtual string ServerName {
            get {
                return this._ServerName;
            }
            set {
                this._ServerName = value;
                this._validationDict["ServerName"] = true;
            }
        }
        
        public virtual string DatabaseName {
            get {
                return this._DatabaseName;
            }
            set {
                this._DatabaseName = value;
                this._validationDict["DatabaseName"] = true;
            }
        }
        
        public virtual System.DateTime LastCommitDate {
            get {
                return this._LastCommitDate;
            }
            set {
                this._LastCommitDate = value;
                this._validationDict["LastCommitDate"] = true;
            }
        }
        
        public virtual System.DateTime ServerChangeDate {
            get {
                return this._ServerChangeDate;
            }
            set {
                this._ServerChangeDate = value;
                this._validationDict["ServerChangeDate"] = true;
            }
        }
        
        public virtual string StrScriptName {
            get {
                return this._ScriptName.ToString();
            }
        }
        
        public virtual string StrScriptId {
            get {
                return this._ScriptId.ToString();
            }
        }
        
        public virtual string StrScriptStatus {
            get {
                return this._ScriptStatus.ToString();
            }
        }
        
        public virtual string StrServerName {
            get {
                return this._ServerName.ToString();
            }
        }
        
        public virtual string StrDatabaseName {
            get {
                return this._DatabaseName.ToString();
            }
        }
        
        public virtual string StrLastCommitDate {
            get {
                return this._LastCommitDate.ToString();
            }
        }
        
        public virtual string StrServerChangeDate {
            get {
                return this._ServerChangeDate.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrScriptName);
                sb.Append(delimiter);
                sb.Append(this.StrScriptId);
                sb.Append(delimiter);
                sb.Append(this.StrScriptStatus);
                sb.Append(delimiter);
                sb.Append(this.StrServerName);
                sb.Append(delimiter);
                sb.Append(this.StrDatabaseName);
                sb.Append(delimiter);
                sb.Append(this.StrLastCommitDate);
                sb.Append(delimiter);
                sb.Append(this.StrServerChangeDate);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(ScriptStatusData dataClass) {
            try {
                this.ScriptName = dataClass.ScriptName;
                this.ScriptId = dataClass.ScriptId;
                this.ScriptStatus = dataClass.ScriptStatus;
                this.ServerName = dataClass.ServerName;
                this.DatabaseName = dataClass.DatabaseName;
                this.LastCommitDate = dataClass.LastCommitDate;
                this.ServerChangeDate = dataClass.ServerChangeDate;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(ScriptStatusData) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    if ((reader["FileName"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptName = ((string)(System.Convert.ChangeType(reader["FileName"], typeof(string))));
                    }
                    if ((reader["ScriptId"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptId = ((string)(System.Convert.ChangeType(reader["ScriptId"], typeof(string))));
                    }
                    if ((reader["Database"].Equals(System.DBNull.Value) == false)) {
                        this.DatabaseName = ((string)(System.Convert.ChangeType(reader["Database"], typeof(string))));
                    }
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
        public virtual bool Fill(System.Data.DataRow sourceDataRow) {
            try {
                if ((sourceDataRow["FileName"].Equals(System.DBNull.Value) == false)) {
                    this.ScriptName = ((string)(System.Convert.ChangeType(sourceDataRow["FileName"], typeof(string))));
                }
                if ((sourceDataRow["ScriptId"].Equals(System.DBNull.Value) == false)) {
                    this.ScriptId = ((string)(System.Convert.ChangeType(sourceDataRow["ScriptId"], typeof(string))));
                }
                if ((sourceDataRow["Database"].Equals(System.DBNull.Value) == false)) {
                    this.DatabaseName = ((string)(System.Convert.ChangeType(sourceDataRow["Database"], typeof(string))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(System.Data.DataRow sourceDataRow) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Collections.Specialized.NameValueCollection nameValueColl) {
            try {
                if ((nameValueColl.GetValues("ScriptName") != null)) {
                    this.ScriptName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ScriptName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("ScriptId") != null)) {
                    this.ScriptId = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ScriptId")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("ScriptStatus") != null)) {
                    this.ScriptStatus = ((SqlSync.SqlBuild.ScriptStatusType)(System.Convert.ChangeType(nameValueColl.GetValues("ScriptStatus")[0], typeof(SqlSync.SqlBuild.ScriptStatusType))));
                }
                if ((nameValueColl.GetValues("ServerName") != null)) {
                    this.ServerName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ServerName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("DatabaseName") != null)) {
                    this.DatabaseName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("DatabaseName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("LastCommitDate") != null)) {
                    this.LastCommitDate = ((System.DateTime)(System.Convert.ChangeType(nameValueColl.GetValues("LastCommitDate")[0], typeof(System.DateTime))));
                }
                if ((nameValueColl.GetValues("ServerChangeDate") != null)) {
                    this.ServerChangeDate = ((System.DateTime)(System.Convert.ChangeType(nameValueColl.GetValues("ServerChangeDate")[0], typeof(System.DateTime))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.ScriptName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.ScriptId = ((string)(System.Convert.ChangeType(sourceArray.GetValue(1), typeof(string))));
                this.ScriptStatus = ((SqlSync.SqlBuild.ScriptStatusType)(System.Convert.ChangeType(sourceArray.GetValue(2), typeof(SqlSync.SqlBuild.ScriptStatusType))));
                this.ServerName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(3), typeof(string))));
                this.DatabaseName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(4), typeof(string))));
                this.LastCommitDate = ((System.DateTime)(System.Convert.ChangeType(sourceArray.GetValue(5), typeof(System.DateTime))));
                this.ServerChangeDate = ((System.DateTime)(System.Convert.ChangeType(sourceArray.GetValue(6), typeof(System.DateTime))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.ScriptName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.ScriptId = ((string)(System.Convert.ChangeType(arrSplitString[1], typeof(string))));
                this.ScriptStatus = ((SqlSync.SqlBuild.ScriptStatusType)(System.Convert.ChangeType(arrSplitString[2], typeof(SqlSync.SqlBuild.ScriptStatusType))));
                this.ServerName = ((string)(System.Convert.ChangeType(arrSplitString[3], typeof(string))));
                this.DatabaseName = ((string)(System.Convert.ChangeType(arrSplitString[4], typeof(string))));
                this.LastCommitDate = ((System.DateTime)(System.Convert.ChangeType(arrSplitString[5], typeof(System.DateTime))));
                this.ServerChangeDate = ((System.DateTime)(System.Convert.ChangeType(arrSplitString[6], typeof(System.DateTime))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.ScriptName.ToString());
                sb.Append(delimiter);
                sb.Append(this.ScriptId.ToString());
                sb.Append(delimiter);
                sb.Append(this.ScriptStatus.ToString());
                sb.Append(delimiter);
                sb.Append(this.ServerName.ToString());
                sb.Append(delimiter);
                sb.Append(this.DatabaseName.ToString());
                sb.Append(delimiter);
                sb.Append(this.LastCommitDate.ToString());
                sb.Append(delimiter);
                sb.Append(this.ServerChangeDate.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[7];
            try {
                myArray[0] = this._ScriptName.ToString();
                myArray[1] = this._ScriptId.ToString();
                myArray[2] = this._ScriptStatus.ToString();
                myArray[3] = this._ServerName.ToString();
                myArray[4] = this._DatabaseName.ToString();
                myArray[5] = this._LastCommitDate.ToString();
                myArray[6] = this._ServerChangeDate.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.GetStringArray() Method", ex);
            }
        }
        
        public virtual string GetFixedLengthString() {
            throw new System.NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subS" +
                    "tringLength value set");
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("ScriptName", this.ScriptName.ToString());
                nameValueColl.Add("ScriptId", this.ScriptId.ToString());
                nameValueColl.Add("ScriptStatus", this.ScriptStatus.ToString());
                nameValueColl.Add("ServerName", this.ServerName.ToString());
                nameValueColl.Add("DatabaseName", this.DatabaseName.ToString());
                nameValueColl.Add("LastCommitDate", this.LastCommitDate.ToString());
                nameValueColl.Add("ServerChangeDate", this.ServerChangeDate.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ScriptStatusData.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptName"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptId"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptId");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ScriptStatus"], typeof(bool)))) == false)) {
                missingValues.Add("ScriptStatus");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ServerName"], typeof(bool)))) == false)) {
                missingValues.Add("ServerName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["DatabaseName"], typeof(bool)))) == false)) {
                missingValues.Add("DatabaseName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["LastCommitDate"], typeof(bool)))) == false)) {
                missingValues.Add("LastCommitDate");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ServerChangeDate"], typeof(bool)))) == false)) {
                missingValues.Add("ServerChangeDate");
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
