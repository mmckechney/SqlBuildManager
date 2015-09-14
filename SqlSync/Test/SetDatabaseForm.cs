using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.DbInformation;
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
                    return this.currentDatabase;
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

            for (int i = 0; i < this.databaseList.Count;i++ )
                this.ddDatabaseList.Items.Add(this.databaseList[i].DatabaseName);

            //Needs to be case insensititive
            for(int i=0;i<ddDatabaseList.Items.Count;i++)
                if(this.ddDatabaseList.Items[i].ToString().ToLower() == this.currentDatabase.ToLower())
                    this.ddDatabaseList.SelectedItem = this.ddDatabaseList.Items[i];
 
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}