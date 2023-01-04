using SqlSync.DbInformation;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;
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
            dataGridView1.DataSource = bindingSource1;
            dataGridView1.AutoGenerateColumns = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            manualDBs.Clear();
            IList list = bindingSource1.List;
            for (int i = 0; i < list.Count; i++)
            {
                DatabaseItem item = (DatabaseItem)list[i];
                if (item.DatabaseName != null && item.DatabaseName.Trim().Length > 0)
                    manualDBs.Add(item.DatabaseName);

            }

            SqlSync.DbInformation.Properties.Settings.Default.ManuallyEnteredDatabases = manualDBs;
            SqlSync.DbInformation.Properties.Settings.Default.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


    }
}