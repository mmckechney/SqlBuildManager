using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.TableScript;
using System.IO;
namespace SqlSync.DataDump
{
    public partial class DataExtractScriptCreateForm : Form
    {
        string[] sourceLines = null;
        public DataExtractScriptCreateForm()
        {
            InitializeComponent();
        }


        private void DataExtractScriptCreateForm_Load(object sender, EventArgs e)
        {
        }

        private void bgCreateScript_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] sourceLines = (string[])e.Argument;
            PopulateHelper popHelper = new PopulateHelper(null);
            string script = popHelper.GeneratePopulateScriptsFromDataExtractFile(sourceLines);
            e.Result = script;
        }

        private void bgCreateScript_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.rtbScript.Text = e.Result.ToString();
            if (this.rtbScript.Text.Length < 50000)
            {
                this.rtbScript.SuspendHighlighting = false;
                this.rtbScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            }

            this.statProgress.Style = ProgressBarStyle.Blocks;
            this.statGeneral.Text = "Ready";
            this.Cursor = Cursors.Default;
        }

        private void openDataExToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                this.sourceLines = File.ReadAllLines(openFileDialog1.FileName);

                this.rtbSource.Lines = this.sourceLines;
                this.rtbScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.None;

                this.statProgress.Style = ProgressBarStyle.Marquee;
                this.statGeneral.Text = "Processing Extract file. Creating Scripts.";
                this.Cursor = Cursors.AppStarting;

                bgCreateScript.RunWorkerAsync(this.sourceLines);
            }
            openFileDialog1.Dispose();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("DataInsertionScriptCreation");
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.rtbScript.Text);
        }

       
    }
}
