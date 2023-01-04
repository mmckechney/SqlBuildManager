using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    public partial class SimpleListForm : Form
    {
        IList<string> list = new List<string>();
        public SimpleListForm()
        {
            InitializeComponent();
        }
        public SimpleListForm(IList<string> list, string formHeaderText, string listHeaderText) : this()
        {
            this.list = list;
            lblHeader.Text = formHeaderText;
            lstList.Columns[0].Text = listHeaderText;
            Text = string.Empty;
        }

        private void SimpleListForm_Load(object sender, EventArgs e)
        {
            foreach (string item in list)
            {
                ListViewItem lvi = new ListViewItem(item);
                lstList.Items.Add(lvi);
            }

        }
    }
}
