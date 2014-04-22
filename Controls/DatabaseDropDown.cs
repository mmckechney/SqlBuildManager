using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using SqlSync.DbInformation;
using System.Text;

namespace SqlSync
{
    public partial class DatabaseDropDown : System.Windows.Forms.ComboBox
    {
        private DatabaseList databaseList = null;
        private string inputDatabase = string.Empty;

        public DatabaseList DatabaseList
        {
            get { return databaseList; }
            set { databaseList = value; }
        }

        public DatabaseDropDown() 
        {
            //this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        }
        public void SetData(DatabaseList databaseList)
        {
            this.SetData(databaseList, "");
        }
        public void SetData(DatabaseList databaseList, string inputDatabase)
        {
            this.Items.Clear();
            this.databaseList = databaseList;
            databaseList.Sort(new DatabaseListComparer());
            this.inputDatabase = inputDatabase;

            for (int i = 0; i < databaseList.Count; i++)
            {

                if (databaseList[i].IsManuallyEntered)
                    this.Items.Add(databaseList[i].DatabaseName + "*");
                else
                    this.Items.Add(databaseList[i].DatabaseName);

                if (inputDatabase != null && databaseList[i].DatabaseName.ToLower().Trim() == inputDatabase.ToLower().Trim()) 
                    this.SelectedIndex = i;
            }

        }
        public string SelectedDatabase
        {
            get
            {
                if (this.SelectedItem == null)
                    return string.Empty;

                if (this.SelectedItem.ToString().EndsWith("*"))
                    return this.SelectedItem.ToString().Substring(0, this.SelectedItem.ToString().Length - 1);

                return this.SelectedItem.ToString();

            }
            set
            {
                string val;
                for (int i = 0; i < this.Items.Count; i++)
                {

                    if (this.Items[i].ToString().Trim().EndsWith("*"))
                        val = this.Items[i].ToString().Trim().Substring(0, this.Items[i].ToString().Length - 1).ToLower();
                    else
                        val = this.Items[i].ToString().ToLower();

                    if (value.Trim().ToLower() == val)
                    {
                        this.SelectedIndex = i;
                        break;
                    }

                }
            }


        }
    }
}
