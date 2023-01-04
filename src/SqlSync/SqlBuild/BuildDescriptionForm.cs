using System;
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
            BuildDescription = textBox1.Text;
            DialogResult = DialogResult.OK;
            Close();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
