using SqlSync.DbInformation;
using System;
using System.Windows.Forms;
namespace SqlSync.Test
{
    public partial class SetDatabaseForm : Form
    {
        private DatabaseList databaseList;
        private string currentDatabase;

        public string CurrentDatabase
        {
            get
            {
                if (ddDatabaseList.SelectedItem == null || ddDatabaseList.SelectedItem.ToString().Length == 0)
                    return currentDatabase;
                else
                    return ddDatabaseList.SelectedItem.ToString();
            }
            set
            {
                currentDatabase = value;
            }
        }
        public SetDatabaseForm(DatabaseList databaseList, string currentDatabase)
        {
            InitializeComponent();
            this.databaseList = databaseList;
            this.currentDatabase = currentDatabase;
        }

        private void SetDatabase_Load(object sender, EventArgs e)
        {

            for (int i = 0; i < databaseList.Count; i++)
                ddDatabaseList.Items.Add(databaseList[i].DatabaseName);

            //Needs to be case insensititive
            for (int i = 0; i < ddDatabaseList.Items.Count; i++)
                if (ddDatabaseList.Items[i].ToString().ToLower() == currentDatabase.ToLower())
                    ddDatabaseList.SelectedItem = ddDatabaseList.Items[i];

        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}