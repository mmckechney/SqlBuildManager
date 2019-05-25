using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlSync
{
    public partial class GetTextValueForm : Form
    {
        private string textValue = string.Empty;
        public string TextValue
        {
            get
            {
                return this.textValue;
            }
        }
        public GetTextValueForm()
        {
            InitializeComponent();
        }
        public GetTextValueForm(string title) : this()
        {
            this.Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textValue = txtValue.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
