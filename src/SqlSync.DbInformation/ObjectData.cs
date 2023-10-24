using System;

namespace SqlSync.DbInformation {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class ObjectData {

        public ObjectData()
        {
        }

        public string ObjectName { get; set; } = string.Empty;

        public string ObjectType { get; set; } = string.Empty;

        public System.DateTime CreateDate { get; set; } = DateTime.MinValue;

        public System.DateTime AlteredDate { get; set; } = DateTime.MinValue;

        public string SchemaOwner { get; set; } = "dbo";

        public string ParentObject { get; set; } = "";


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

    }
}
