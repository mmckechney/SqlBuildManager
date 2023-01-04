using System;
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
                rtbScript.Text = value;
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (isBulkAdd)
            {
                DialogResult = DialogResult.OK;
                bulkScripts = rtbScript.Lines;
                Close();
            }
            else
            {
                DialogResult = DialogResult.OK;
                scriptText = rtbScript.Text;
                Close();
            }
        }

        private void PasteScriptForm_Load(object sender, EventArgs e)
        {
            if (isBulkAdd)
            {
                Text = "Bulk Add from Stored Procedure Execution Scripts";
                lblScript.Text = "Paste Execution scripts, 1 per line:";
                Height = 550;
                Width = 800;
            }
        }
    }
}