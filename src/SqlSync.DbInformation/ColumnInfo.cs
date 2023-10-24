using System;

namespace SqlSync.DbInformation {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class ColumnInfo {

        public ColumnInfo()
        {

        }

        public string ColumnName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public int CharMaximum { get; set; } = 0;
        
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
    }
}
