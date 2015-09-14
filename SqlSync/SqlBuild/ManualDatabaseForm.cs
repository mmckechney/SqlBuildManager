using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using SqlSync.DbInformation;
using System.Collections;
namespace SqlSync.SqlBuild
{
    public partial class ManualDatabaseForm : Form
    {
        StringCollection manualDBs = new StringCollection();
        public ManualDatabaseForm()
        {
            InitializeComponent();
        }

        private void ManualDatabaseForm_Load(object sender, EventArgs e)
        {
            StringCollection manualDBs = SqlSync.DbInformation.Properties.Settings.Default.ManuallyEnteredDatabases;
            for (int i = 0; i < manualDBs.Count; i++)
            {
                DatabaseItem db = new DatabaseItem();
                db.DatabaseName = manualDBs[i];
                bindingSource1.Add(db);
            }
            this.dataGridView1.DataSource = bindingSource1;
            this.dataGridView1.AutoGenerateColumns = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            manualDBs.Clear();
            IList list = this.bindingSource1.List;
            for (int i = 0; i < list.Count; i++)
            {
                DatabaseItem item = (DatabaseItem)list[i];
                if(item.DatabaseName != null && item.DatabaseName.Trim().Length > 0) 
                    manualDBs.Add(item.DatabaseName);

            }

            SqlSync.DbInformation.Properties.Settings.Default.ManuallyEnteredDatabases = manualDBs;
            SqlSync.DbInformation.Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}