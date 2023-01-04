using System;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    public partial class RenameScriptForm : Form
    {
        SqlSyncBuildData.ScriptRow scriptRow = null;
        string oldName = string.Empty;

        public string OldName
        {
            get { return oldName; }
            set { oldName = value; }
        }
        string newName = string.Empty;

        public string NewName
        {
            get { return newName; }
            set { newName = value; }
        }
        public RenameScriptForm(ref SqlSyncBuildData.ScriptRow scriptRow)
        {
            InitializeComponent();
            this.scriptRow = scriptRow;
        }

        private void RenameScriptForm_Load(object sender, EventArgs e)
        {
            lblCurrentName.Text = scriptRow.FileName;
            txtNewName.Text = scriptRow.FileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            oldName = lblCurrentName.Text;
            newName = txtNewName.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void RenameScriptForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}