using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    public partial class GetTableNameForm : Form
    {

        public string TableName
        {
            get { return txtTableName.Text; }
        }

        public GetTableNameForm()
        {
            InitializeComponent();
        }

        private void GetTableNameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}