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
    public class ObjectData {
        
        private string _ObjectName = string.Empty;
        
        private string _ObjectType = string.Empty;
        
        private System.DateTime _CreateDate = DateTime.MinValue;
        
        private System.DateTime _AlteredDate = DateTime.MinValue;
        
        private string _SchemaOwner = "dbo";

        private string _ParentObject = "";
        
        private System.Collections.Hashtable _validationDict;
        
        public ObjectData() {
            this._validationDict = new System.Collections.Hashtable();
            this._validationDict.Add("ObjectName", false);
            this._validationDict.Add("ObjectType", false);
            this._validationDict.Add("CreateDate", false);
            this._validationDict.Add("AlteredDate", false);
            this._validationDict.Add("SchemaOwner", false);
            this._validationDict.Add("ParentObject", false);
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
        
        public virtual string ObjectName {
            get {
                return this._ObjectName;
            }
            set {
                this._ObjectName = value;
                this._validationDict["ObjectName"] = true;
            }
        }
        
        public virtual string ObjectType {
            get {
                return this._ObjectType;
            }
            set {
                this._ObjectType = value;
                this._validationDict["ObjectType"] = true;
            }
        }
        
        public virtual System.DateTime CreateDate {
            get {
                return this._CreateDate;
            }
            set {
                this._CreateDate = value;
                this._validationDict["CreateDate"] = true;
            }
        }
        
        public virtual System.DateTime AlteredDate {
            get {
                return this._AlteredDate;
            }
            set {
                this._AlteredDate = value;
                this._validationDict["AlteredDate"] = true;
            }
        }
        
        public virtual string SchemaOwner {
            get {
                return this._SchemaOwner;
            }
            set {
                this._SchemaOwner = value;
                this._validationDict["SchemaOwner"] = true;
            }
        }

        public virtual string ParentObject
        {
            get
            {
                return this._ParentObject;
            }
            set
            {
                this._ParentObject = value;
                this._validationDict["ParentObject"] = true;
            }
        }
        
        public virtual string StrObjectName {
            get {
                return this._ObjectName.ToString();
            }
        }
        
        public virtual string StrObjectType {
            get {
                return this._ObjectType.ToString();
            }
        }
        
        public virtual string StrCreateDate {
            get {
                return this._CreateDate.ToString();
            }
        }
        
        public virtual string StrAlteredDate {
            get {
                return this._AlteredDate.ToString();
            }
        }
        
        public virtual string StrSchemaOwner {
            get {
                return this._SchemaOwner.ToString();
            }
        }
        
        public virtual string GetCustomDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.StrObjectName);
                sb.Append(delimiter);
                sb.Append(this.StrObjectType);
                sb.Append(delimiter);
                sb.Append(this.StrCreateDate);
                sb.Append(delimiter);
                sb.Append(this.StrAlteredDate);
                sb.Append(delimiter);
                sb.Append(this.StrSchemaOwner);
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.GetCustomDelimitedString(string) Method", ex);
            }
        }
        
        public virtual bool Fill(ObjectData dataClass) {
            try {
                this.ObjectName = dataClass.ObjectName;
                this.ObjectType = dataClass.ObjectType;
                this.CreateDate = dataClass.CreateDate;
                this.AlteredDate = dataClass.AlteredDate;
                this.SchemaOwner = dataClass.SchemaOwner;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(ObjectData) Method", ex);
            }
        }
        
        public virtual bool Fill(Microsoft.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    if ((reader["ObjectName"].Equals(System.DBNull.Value) == false)) {
                        this.ObjectName = ((string)(System.Convert.ChangeType(reader["ObjectName"], typeof(string))));
                    }
                    if ((reader["ObjectType"].Equals(System.DBNull.Value) == false)) {
                        this.ObjectType = ((string)(System.Convert.ChangeType(reader["ObjectType"], typeof(string))));
                    }
                    if ((reader["CreateDate"].Equals(System.DBNull.Value) == false)) {
                        this.CreateDate = ((System.DateTime)(System.Convert.ChangeType(reader["CreateDate"], typeof(System.DateTime))));
                    }
                    if ((reader["AlteredDate"].Equals(System.DBNull.Value) == false)) {
                        this.AlteredDate = ((System.DateTime)(System.Convert.ChangeType(reader["AlteredDate"], typeof(System.DateTime))));
                    }
                    if ((reader["SchemaOwner"].Equals(System.DBNull.Value) == false)) {
                        this.SchemaOwner = ((string)(System.Convert.ChangeType(reader["SchemaOwner"], typeof(string))));
                    }
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
        public virtual bool Fill(System.Data.DataRow sourceDataRow) {
            try {
                if ((sourceDataRow["ObjectName"].Equals(System.DBNull.Value) == false)) {
                    this.ObjectName = ((string)(System.Convert.ChangeType(sourceDataRow["ObjectName"], typeof(string))));
                }
                if ((sourceDataRow["ObjectType"].Equals(System.DBNull.Value) == false)) {
                    this.ObjectType = ((string)(System.Convert.ChangeType(sourceDataRow["ObjectType"], typeof(string))));
                }
                if ((sourceDataRow["CreateDate"].Equals(System.DBNull.Value) == false)) {
                    this.CreateDate = ((System.DateTime)(System.Convert.ChangeType(sourceDataRow["CreateDate"], typeof(System.DateTime))));
                }
                if ((sourceDataRow["AlteredDate"].Equals(System.DBNull.Value) == false)) {
                    this.AlteredDate = ((System.DateTime)(System.Convert.ChangeType(sourceDataRow["AlteredDate"], typeof(System.DateTime))));
                }
                if ((sourceDataRow["SchemaOwner"].Equals(System.DBNull.Value) == false)) {
                    this.SchemaOwner = ((string)(System.Convert.ChangeType(sourceDataRow["SchemaOwner"], typeof(string))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(System.Data.DataRow sourceDataRow) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Collections.Specialized.NameValueCollection nameValueColl) {
            try {
                if ((nameValueColl.GetValues("ObjectName") != null)) {
                    this.ObjectName = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ObjectName")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("ObjectType") != null)) {
                    this.ObjectType = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("ObjectType")[0], typeof(string))));
                }
                if ((nameValueColl.GetValues("CreateDate") != null)) {
                    this.CreateDate = ((System.DateTime)(System.Convert.ChangeType(nameValueColl.GetValues("CreateDate")[0], typeof(System.DateTime))));
                }
                if ((nameValueColl.GetValues("AlteredDate") != null)) {
                    this.AlteredDate = ((System.DateTime)(System.Convert.ChangeType(nameValueColl.GetValues("AlteredDate")[0], typeof(System.DateTime))));
                }
                if ((nameValueColl.GetValues("SchemaOwner") != null)) {
                    this.SchemaOwner = ((string)(System.Convert.ChangeType(nameValueColl.GetValues("SchemaOwner")[0], typeof(string))));
                }
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(NameValueCollection) Method", ex);
            }
        }
        
        public virtual bool Fill(System.Array sourceArray) {
            try {
                this.ObjectName = ((string)(System.Convert.ChangeType(sourceArray.GetValue(0), typeof(string))));
                this.ObjectType = ((string)(System.Convert.ChangeType(sourceArray.GetValue(1), typeof(string))));
                this.CreateDate = ((System.DateTime)(System.Convert.ChangeType(sourceArray.GetValue(2), typeof(System.DateTime))));
                this.AlteredDate = ((System.DateTime)(System.Convert.ChangeType(sourceArray.GetValue(3), typeof(System.DateTime))));
                this.SchemaOwner = ((string)(System.Convert.ChangeType(sourceArray.GetValue(4), typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(System.Array) Method", ex);
            }
        }
        
        public virtual bool Fill(string delimString, char delimiter) {
            string[] arrSplitString;
            arrSplitString = delimString.Split(delimiter);
            try {
                this.ObjectName = ((string)(System.Convert.ChangeType(arrSplitString[0], typeof(string))));
                this.ObjectType = ((string)(System.Convert.ChangeType(arrSplitString[1], typeof(string))));
                this.CreateDate = ((System.DateTime)(System.Convert.ChangeType(arrSplitString[2], typeof(System.DateTime))));
                this.AlteredDate = ((System.DateTime)(System.Convert.ChangeType(arrSplitString[3], typeof(System.DateTime))));
                this.SchemaOwner = ((string)(System.Convert.ChangeType(arrSplitString[4], typeof(string))));
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(string,char) Method", ex);
            }
        }
        
        public virtual bool Fill(string fixedString) {
            try {
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.Fill(string) Method", ex);
            }
        }
        
        public virtual string GetDelimitedString(string delimiter) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                sb.Append(this.ObjectName.ToString());
                sb.Append(delimiter);
                sb.Append(this.ObjectType.ToString());
                sb.Append(delimiter);
                sb.Append(this.CreateDate.ToString());
                sb.Append(delimiter);
                sb.Append(this.AlteredDate.ToString());
                sb.Append(delimiter);
                sb.Append(this.SchemaOwner.ToString());
                return sb.ToString();
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.GetDelimitedString(string) Method", ex);
            }
        }
        
        public virtual string[] GetStringArray() {
            string[] myArray = new string[5];
            try {
                myArray[0] = this._ObjectName.ToString();
                myArray[1] = this._ObjectType.ToString();
                myArray[2] = this._CreateDate.ToString();
                myArray[3] = this._AlteredDate.ToString();
                myArray[4] = this._SchemaOwner.ToString();
                return myArray;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.GetStringArray() Method", ex);
            }
        }
        
        public virtual string GetFixedLengthString() {
            throw new System.NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subS" +
                    "tringLength value set");
        }
        
        public virtual System.Collections.Specialized.NameValueCollection GetNameValueCollection() {
            System.Collections.Specialized.NameValueCollection nameValueColl = new System.Collections.Specialized.NameValueCollection();
            try {
                nameValueColl.Add("ObjectName", this.ObjectName.ToString());
                nameValueColl.Add("ObjectType", this.ObjectType.ToString());
                nameValueColl.Add("CreateDate", this.CreateDate.ToString());
                nameValueColl.Add("AlteredDate", this.AlteredDate.ToString());
                nameValueColl.Add("SchemaOwner", this.SchemaOwner.ToString());
                return nameValueColl;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ObjectData.GetNameValueCollection() Method", ex);
            }
        }
        
        public virtual string[] Validate() {
            System.Collections.ArrayList missingValues = new System.Collections.ArrayList();
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ObjectName"], typeof(bool)))) == false)) {
                missingValues.Add("ObjectName");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["ObjectType"], typeof(bool)))) == false)) {
                missingValues.Add("ObjectType");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["CreateDate"], typeof(bool)))) == false)) {
                missingValues.Add("CreateDate");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["AlteredDate"], typeof(bool)))) == false)) {
                missingValues.Add("AlteredDate");
            }
            if ((((bool)(System.Convert.ChangeType(this._validationDict["SchemaOwner"], typeof(bool)))) == false)) {
                missingValues.Add("SchemaOwner");
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
