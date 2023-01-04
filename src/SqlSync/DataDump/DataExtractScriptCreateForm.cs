using SqlSync.TableScript;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
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
            rtbScript.Text = e.Result.ToString();
            if (rtbScript.Text.Length < 50000)
            {
                rtbScript.SuspendHighlighting = false;
                rtbScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            }

            statProgress.Style = ProgressBarStyle.Blocks;
            statGeneral.Text = "Ready";
            Cursor = Cursors.Default;
        }

        private void openDataExToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                sourceLines = File.ReadAllLines(openFileDialog1.FileName);

                rtbSource.Lines = sourceLines;
                rtbScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.None;

                statProgress.Style = ProgressBarStyle.Marquee;
                statGeneral.Text = "Processing Extract file. Creating Scripts.";
                Cursor = Cursors.AppStarting;

                bgCreateScript.RunWorkerAsync(sourceLines);
            }
            openFileDialog1.Dispose();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("DataInsertionScriptCreation");
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbScript.Text);
        }


    }
}
