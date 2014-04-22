using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.SqlBuild;
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
            this.lblCurrentName.Text = this.scriptRow.FileName;
            this.txtNewName.Text = this.scriptRow.FileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.oldName = this.lblCurrentName.Text;
            this.newName = this.txtNewName.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void RenameScriptForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}