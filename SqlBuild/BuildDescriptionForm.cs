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
    public partial class BuildDescriptionForm : Form
    {
        public BuildDescriptionForm()
        {
            InitializeComponent();
        }

        public string BuildDescription
        {
            get;
            set;
        }
        private void BuildDescriptionForm_Load(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.BuildDescription = textBox1.Text;
            this.DialogResult =DialogResult.OK;
            this.Close();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
