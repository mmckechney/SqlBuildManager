using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlSync
{
    public partial class FromDacpacForm : Form
    {
        public FromDacpacForm()
        {
            InitializeComponent();
            DACPACPath = Properties.Settings.Default.DACPACPath;
        }


        private void updateDACPACPathToolTip(string text)
        {
            DACPACToolTip.SetToolTip(DACPACPathTextBox, text);
        }

        #region Properties

        private string DACPACPath
        {
            get { return DACPACPathTextBox.Text; }

            set
            {
                DACPACPathTextBox.Text = value;
                updateDACPACPathToolTip(value);
            }
        }

        ToolTip dacpacToolTip = null;
        ToolTip DACPACToolTip
        {
            get
            {
                if (dacpacToolTip == null)
                {
                    dacpacToolTip = new ToolTip();
                }

                return dacpacToolTip;
            }
        }

        #endregion


        #region Events
        private void BrowseForDACPACButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DACPAC|*.DACPAC";

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                DACPACPathTextBox.Text = dialog.FileName;
            }
        }

        private void DACPACPathTextBox_TextChanged(object sender, EventArgs e)
        {
            updateDACPACPathToolTip(DACPACPathTextBox.Text);
            Properties.Settings.Default.DACPACPath = DACPACPathTextBox.Text;
            Properties.Settings.Default.Save();
        }

        #endregion
    }
}
