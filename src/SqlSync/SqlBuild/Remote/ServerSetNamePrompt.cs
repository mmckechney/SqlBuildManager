using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.SqlBuild.Remote
{
    public partial class ServerSetNamePrompt : Form
    {
        public ServerSetNamePrompt()
        {
            InitializeComponent();
        }

        public string ServerSetName
        {
            get
            {
                return textBox1.Text;
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
