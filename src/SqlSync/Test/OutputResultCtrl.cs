using System;
using System.Windows.Forms;

namespace SqlSync.Test
{
    public partial class OutputResultCtrl : UserControl
    {
        public OutputResultCtrl()
        {
            InitializeComponent();
        }

        private SqlSync.SprocTest.Configuration.OutputResult outputResult;
        public SqlSync.SprocTest.Configuration.OutputResult OutputResult
        {
            get
            {
                if (outputResult == null)
                    outputResult = new SqlSync.SprocTest.Configuration.OutputResult();

                outputResult.Value = txtExpectedValue.Text;
                outputResult.ColumnName = txtColumnName.Text;
                int val;
                if (Int32.TryParse(txtRowNumber.Text, out val))
                {
                    outputResult.RowNumber = val;
                    outputResult.RowNumberSpecified = true;
                }

                return outputResult;
            }
            set
            {
                outputResult = value;
                txtExpectedValue.Text = outputResult.Value;
                txtColumnName.Text = outputResult.ColumnName;
                txtRowNumber.Text = outputResult.RowNumber.ToString();
            }
        }

        public OutputResultCtrl(ref SqlSync.SprocTest.Configuration.OutputResult outputResult)
        {
            InitializeComponent();
            this.outputResult = outputResult;
        }

        private void OutputResultCtrl_Load(object sender, EventArgs e)
        {
            if (outputResult != null)
            {
                txtExpectedValue.Text = outputResult.Value;
                txtColumnName.Text = outputResult.ColumnName;
                txtRowNumber.Text = outputResult.RowNumber.ToString();
            }
        }

        public event EventHandler DataChanged;

        private void txtColumnName_TextChanged(object sender, EventArgs e)
        {
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to remove the output result?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                if (RemoveOutputResult != null)
                    RemoveOutputResult(this, EventArgs.Empty);
            }
        }

        public event EventHandler RemoveOutputResult;
    }
}
