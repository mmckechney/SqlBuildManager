using System;
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
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}