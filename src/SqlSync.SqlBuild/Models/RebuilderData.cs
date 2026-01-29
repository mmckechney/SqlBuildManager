using System;

namespace SqlSync.SqlBuild {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class RebuilderData {

        public RebuilderData()
        {

        }

        public string ScriptFileName { get; set; } = String.Empty;

        public System.Guid ScriptId;

        public int Sequence { get; set; } = -1;

        public string ScriptText { get; set; } = String.Empty;

        public string Database { get; set; } = String.Empty;

        public string Tag { get; set; } = String.Empty;
        
        public virtual bool Fill(Microsoft.Data.SqlClient.SqlDataReader reader, bool closeReader) {
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
    }
}
