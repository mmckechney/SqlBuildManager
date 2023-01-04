using SqlSync.DbInformation;

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
            SetData(databaseList, "");
        }
        public void SetData(DatabaseList databaseList, string inputDatabase)
        {
            Items.Clear();
            this.databaseList = databaseList;
            databaseList.Sort(new DatabaseListComparer());
            this.inputDatabase = inputDatabase;

            for (int i = 0; i < databaseList.Count; i++)
            {

                if (databaseList[i].IsManuallyEntered)
                    Items.Add(databaseList[i].DatabaseName + "*");
                else
                    Items.Add(databaseList[i].DatabaseName);

                if (inputDatabase != null && databaseList[i].DatabaseName.ToLower().Trim() == inputDatabase.ToLower().Trim())
                    SelectedIndex = i;
            }

        }
        public string SelectedDatabase
        {
            get
            {
                if (SelectedItem == null)
                    return string.Empty;

                if (SelectedItem.ToString().EndsWith("*"))
                    return SelectedItem.ToString().Substring(0, SelectedItem.ToString().Length - 1);

                return SelectedItem.ToString();

            }
            set
            {
                string val;
                for (int i = 0; i < Items.Count; i++)
                {

                    if (Items[i].ToString().Trim().EndsWith("*"))
                        val = Items[i].ToString().Trim().Substring(0, Items[i].ToString().Length - 1).ToLower();
                    else
                        val = Items[i].ToString().ToLower();

                    if (value.Trim().ToLower() == val)
                    {
                        SelectedIndex = i;
                        break;
                    }

                }
            }


        }
    }
}
