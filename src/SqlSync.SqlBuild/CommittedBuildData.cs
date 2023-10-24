using System;

namespace SqlSync.SqlBuild {


    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class CommittedBuildData {
        public CommittedBuildData()
        {
        }
        public string BuildFileName { get; set; } = String.Empty;

        public int ScriptCount { get; set; }

        public System.DateTime CommitDate { get; set; }

        public string Database { get; set; }


      
       

        
       
        
       
        
        public virtual bool Fill(Microsoft.Data.SqlClient.SqlDataReader reader, bool closeReader) {
            try {
                if ((reader.Read() == false)) {
                    reader.Close();
                    return false;
                }
                else {
                    if ((reader["BuildFileName"].Equals(System.DBNull.Value) == false)) {
                        this.BuildFileName = ((string)(System.Convert.ChangeType(reader["BuildFileName"], typeof(string))));
                    }
                    if ((reader["ScriptCount"].Equals(System.DBNull.Value) == false)) {
                        this.ScriptCount = ((int)(System.Convert.ChangeType(reader["ScriptCount"], typeof(int))));
                    }
                    if ((reader["commitDate"].Equals(System.DBNull.Value) == false)) {
                        this.CommitDate = ((System.DateTime)(System.Convert.ChangeType(reader["commitDate"], typeof(System.DateTime))));
                    }
                    if ((reader["database"].Equals(System.DBNull.Value) == false)) {
                        this.Database = ((string)(System.Convert.ChangeType(reader["database"], typeof(string))));
                    }
                    return true;
                }
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: CommittedBuildData.Fill(SqlDataReader) Method", ex);
            }
            finally {
                if ((closeReader == true)) {
                    reader.Close();
                }
            }
        }
        
       
    }
}
