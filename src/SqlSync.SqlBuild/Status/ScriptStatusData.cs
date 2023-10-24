using System;

namespace SqlSync.SqlBuild.Status {


    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignTimeVisible(true)]
    public partial class ScriptStatusData {

        public string ScriptName { get; set; } = string.Empty;

        public string ScriptId { get; set; } = string.Empty;

        public SqlSync.SqlBuild.ScriptStatusType ScriptStatus { get; set; } = SqlSync.SqlBuild.ScriptStatusType.Unknown;

        public string ServerName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public System.DateTime LastCommitDate { get; set; }

        public System.DateTime ServerChangeDate { get; set; }


        public ScriptStatusData() {
         
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
    }
}
