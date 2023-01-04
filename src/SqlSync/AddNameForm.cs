using System;
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
                return textValue;
            }
        }
        public GetTextValueForm()
        {
            InitializeComponent();
        }
        public GetTextValueForm(string title) : this()
        {
            Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textValue = txtValue.Text;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
