using SqlSync.SprocTest;
using System;
using System.Text;
using System.Windows.Forms;
namespace SqlSync.Test
{
    public partial class ResultsViewForm : Form
    {
        private TestManager.TestResult args = null;
        internal ResultsViewForm()
        {
            InitializeComponent();
        }
        public ResultsViewForm(TestManager.TestResult args)
        {
            InitializeComponent();
            this.args = args;
        }

        private void ResultsViewForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void ResultsViewForm_Load(object sender, EventArgs e)
        {
            if (args == null)
                return;

            System.Text.StringBuilder sb = new StringBuilder();
            sb.Append("Stored Procedure: " + args.StoredProcedureName + "\r\n");
            sb.Append("Test Case Name: " + args.TestCase.Name + "\r\n");
            sb.Append("Passed? " + args.Passed.ToString() + "\r\n");
            sb.Append("Results:\r\n");
            sb.Append(args.StatusMessage);

            rtbResults.Text = sb.ToString();
            rtbSql.Text = args.ExecutedSql;
        }
    }
}