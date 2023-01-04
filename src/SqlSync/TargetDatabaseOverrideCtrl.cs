using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
            List<DatabaseOverride> old = GetOverrideData();
            dataGridView1.Rows.Clear();

            for (int j = 0; j < buildFileDbs.Count; j++)
            {
                DataGridViewTextBoxCell defaultTargetCell = new DataGridViewTextBoxCell();
                defaultTargetCell.Value = buildFileDbs[j];

                DataGridViewComboBoxCell overrideTargetCell = new DataGridViewComboBoxCell();
                for (int i = 0; i < mainDatabaseList.Count; i++)
                    if (!mainDatabaseList[i].IsManuallyEntered)
                    {
                        overrideTargetCell.Items.Add(mainDatabaseList[i].DatabaseName);

                        //Pre-select override from refreshed list if it can be found.
                        for (int x = 0; x < old.Count; x++)
                        {
                            if (old[x].DefaultDbTarget == buildFileDbs[j] && old[x].OverrideDbTarget == mainDatabaseList[i].DatabaseName &&
                                old[x].OverrideDbTarget != buildFileDbs[j])
                            {
                                overrideTargetCell.Value = mainDatabaseList[i].DatabaseName;
                            }
                        }
                    }


                DataGridViewRow row = new DataGridViewRow();
                row.Cells.AddRange(defaultTargetCell, overrideTargetCell);
                dataGridView1.Rows.Add(row);
            }
            OverrideData.TargetDatabaseOverrides = GetOverrideData();
        }
        public List<DatabaseOverride> GetOverrideData()
        {
            List<DatabaseOverride> vals = new List<DatabaseOverride>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DatabaseOverride dBO = new DatabaseOverride();
                dBO.DefaultDbTarget = row.Cells[0].Value.ToString();
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
                string defaultDatabase = dataGridView1[0, e.RowIndex].Value.ToString();
                string overrideDatabase = dataGridView1[1, e.RowIndex].Value.ToString();
                if (TargetChanged != null)
                    TargetChanged(this, new TargetChangedEventArgs(defaultDatabase, overrideDatabase));
            }
            catch { }
        }
        public event TargetChangedEventHandler TargetChanged;
    }

    public delegate void TargetChangedEventHandler(object sender, TargetChangedEventArgs e);
    public class TargetChangedEventArgs : EventArgs
    {
        public readonly string DefaultDatabase;
        public readonly string OverrideDatabase;
        public TargetChangedEventArgs(string defaultDatabase, string overrideDatabase)
        {
            DefaultDatabase = defaultDatabase;
            OverrideDatabase = overrideDatabase;
        }
    }

}
