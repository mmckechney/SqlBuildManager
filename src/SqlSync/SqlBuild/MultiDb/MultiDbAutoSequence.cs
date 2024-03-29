﻿using System;
using System.Windows.Forms;

namespace SqlSync.SqlBuild.MultiDb
{
    public partial class MultiDbAutoSequence : Form
    {
        public MultiDbAutoSequence()
        {
            InitializeComponent();
        }
        public MultiDbAutoSequence(string databaseName)
            : this()
        {
            txtDatabaseName.Text = databaseName;
        }
        private string pattern = string.Empty;

        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }
        private int start;

        public int Start
        {
            get { return start; }
            set { start = value; }
        }
        private int increment;

        public int Increment
        {
            get { return increment; }
            set { increment = value; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int tmp;
            if (!int.TryParse(txtStart.Text, out tmp) || !int.TryParse(txtIncrement.Text, out tmp))
            {
                MessageBox.Show("The start and increment values must be numeric. Please re-enter", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (txtDatabaseName.SelectedText.Length > 0)
            {

                if (txtDatabaseName.Text.Length - txtDatabaseName.SelectionStart == txtDatabaseName.SelectionLength)
                    pattern = @"\w*" + txtDatabaseName.SelectedText + "$";
                else if (txtDatabaseName.SelectionStart == 0)
                    pattern = "^" + txtDatabaseName.SelectedText + @"\w*";
                else
                    pattern = @"\w*" + txtDatabaseName.SelectedText + @"\w*";

            }
            else
                pattern = txtDatabaseName.Text + @"\w?";

            increment = int.Parse(txtIncrement.Text);
            start = int.Parse(txtStart.Text);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtIncrement_TextChanged(object sender, EventArgs e)
        {
            double tmp;
            if (!double.TryParse(txtStart.Text, out tmp))
            {
                MessageBox.Show("The inctement value must be numeric. Please re-enter", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

        }

        private void txtStart_TextChanged(object sender, EventArgs e)
        {
            double tmp;
            if (!double.TryParse(txtStart.Text, out tmp))
            {
                MessageBox.Show("The start value must be numeric. Please re-enter", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
    }
}
