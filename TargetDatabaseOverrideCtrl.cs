using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SqlSync.DbInformation;
using SqlSync.SqlBuild;
using SqlSync.Connection;
namespace SqlSync
{
    public partial class TargetDatabaseOverrideCtrl : UserControl
    {
        public TargetDatabaseOverrideCtrl()
        {
            InitializeComponent();
        }
        public void SetDatabaseData(DatabaseList mainDatabaseList, List<string> buildFileDbs)
        {
            List<DatabaseOverride> old = this.GetOverrideData();
            this.dataGridView1.Rows.Clear();

            for(int j=0;j<buildFileDbs.Count;j++)
            {
                DataGridViewTextBoxCell defaultTargetCell = new DataGridViewTextBoxCell();
                defaultTargetCell.Value = buildFileDbs[j];
                
                DataGridViewComboBoxCell overrideTargetCell = new DataGridViewComboBoxCell();
                for(int i=0;i<mainDatabaseList.Count;i++)
                    if (!mainDatabaseList[i].IsManuallyEntered)
                    {
                        overrideTargetCell.Items.Add(mainDatabaseList[i].DatabaseName);

                        //Pre-select override from refreshed list if it can be found.
                        for (int x = 0; x < old.Count; x++)
                        {
                            if(old[x].DefaultDbTarget == buildFileDbs[j] && old[x].OverrideDbTarget == mainDatabaseList[i].DatabaseName &&
                                old[x].OverrideDbTarget != buildFileDbs[j])
                            {
                                overrideTargetCell.Value = mainDatabaseList[i].DatabaseName;
                            }
                        }
                    }

           
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.AddRange(defaultTargetCell,overrideTargetCell);
                this.dataGridView1.Rows.Add(row);
            }
            OverrideData.TargetDatabaseOverrides = GetOverrideData();
        }
        public List<DatabaseOverride> GetOverrideData()
        {
            List<DatabaseOverride> vals = new List<DatabaseOverride>();
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                DatabaseOverride dBO = new DatabaseOverride();
                dBO.DefaultDbTarget =  row.Cells[0].Value.ToString();
                if (((DataGridViewComboBoxCell)row.Cells[1]).Value == null)
                    dBO.OverrideDbTarget = dBO.DefaultDbTarget;
                else
                    dBO.OverrideDbTarget = ((DataGridViewComboBoxCell)row.Cells[1]).Value.ToString();
                vals.Add(dBO);
            }
            return vals;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            OverrideData.TargetDatabaseOverrides = GetOverrideData();
            try
            {
                string defaultDatabase = this.dataGridView1[0, e.RowIndex].Value.ToString();
                string overrideDatabase = this.dataGridView1[1, e.RowIndex].Value.ToString();
                if (this.TargetChanged != null)
                    this.TargetChanged(this, new TargetChangedEventArgs(defaultDatabase, overrideDatabase));
            }
            catch { }
        }
        public event TargetChangedEventHandler TargetChanged;
    }

    public delegate void TargetChangedEventHandler(object sender,TargetChangedEventArgs e);
    public class TargetChangedEventArgs : EventArgs
    {
        public readonly string DefaultDatabase;
        public readonly string OverrideDatabase;
        public TargetChangedEventArgs(string defaultDatabase, string overrideDatabase)
        {
            this.DefaultDatabase = defaultDatabase;
            this.OverrideDatabase = overrideDatabase;
        }
    }
    
}
