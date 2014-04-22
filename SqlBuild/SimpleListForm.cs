using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            this.lblHeader.Text = formHeaderText;
            this.lstList.Columns[0].Text = listHeaderText;
            this.Text = string.Empty;
        }

        private void SimpleListForm_Load(object sender, EventArgs e)
        {
            foreach (string item in this.list)
            {
                ListViewItem lvi = new ListViewItem(item);
                this.lstList.Items.Add(lvi);
            }

        }
    }
}
