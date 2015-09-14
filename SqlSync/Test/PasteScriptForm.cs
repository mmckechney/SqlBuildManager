using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.Test
{
    public partial class PasteScriptForm : Form
    {
        bool isBulkAdd = false;
        public PasteScriptForm(bool isBulkAdd)
        {
            InitializeComponent();
            this.isBulkAdd = isBulkAdd;
        }
        public PasteScriptForm() : this(false)
        {
        }

        private string scriptText = "";

        public string ScriptText
        {
            get { return scriptText; }
            set
            {
                scriptText = value;
                this.rtbScript.Text = value;
            }
        }
        private string[] bulkScripts = new string[0];

        public string[] BulkScripts
        {
            get { return bulkScripts; }
            set { bulkScripts = value; }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.isBulkAdd)
            {
                this.DialogResult = DialogResult.OK;
                this.bulkScripts = rtbScript.Lines;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.scriptText = rtbScript.Text;
                this.Close();
            }
        }

        private void PasteScriptForm_Load(object sender, EventArgs e)
        {
            if (this.isBulkAdd)
            {
                this.Text = "Bulk Add from Stored Procedure Execution Scripts";
                this.lblScript.Text = "Paste Execution scripts, 1 per line:";
                this.Height = 550;
                this.Width = 800;
            }
        }
    }
}