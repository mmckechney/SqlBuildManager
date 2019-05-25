using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.Compare;
using System.IO;
namespace SqlSync.Analysis
{
    public partial class SimpleDiffForm : Form
    {
        string leftFileText = string.Empty;
        string rightFileText = string.Empty;
        string databaseName = string.Empty;
        string serverName = string.Empty;
        bool fileChanged = false;
        string leftFileDescriptor = string.Empty;
        string rightFileDescriptor = string.Empty;
        public bool ShowMenuStrip
        {
            get { return this.linkedBoxes.ShowMenuStrip; }
            set { this.linkedBoxes.ShowMenuStrip = value; }
        }
        public bool FileChanged
        {
            get { return fileChanged; }
            set { fileChanged = value; }
        }
        FileCompareResults results = null;
        private SimpleDiffForm()
        {
            InitializeComponent();
        }
        public SimpleDiffForm(string leftFileText, string rightFileText, string databaseName, string serverName, string leftFileDescriptor, string rightFileDescriptor)
            : this()
        {
            this.leftFileText = leftFileText;
            this.rightFileText = rightFileText;
            this.databaseName = databaseName;
            this.serverName = serverName;
            this.leftFileDescriptor = leftFileDescriptor;
            this.rightFileDescriptor = rightFileDescriptor;
        }
        public SimpleDiffForm(string leftFileText, string rightFileText, string databaseName, string serverName) :
            this(leftFileText, rightFileText, databaseName, serverName, "Current Script", "Script As Run on Server")
        {

        }
        public SimpleDiffForm(FileCompareResults results, string objectName, string databaseName, string serverName) : this()
        {
            this.results = results;
            this.Text = String.Format(this.Text, System.IO.Path.GetFileName(results.LeftScriptPath), serverName, databaseName, objectName);
        }

        private void SimpleDiffForm_Load(object sender, EventArgs e)
        {
            if (this.results != null)
            {
                this.linkedBoxes.UnifiedDiffText = this.results.UnifiedDiffText;
                this.linkedBoxes.LeftFileName = this.results.LeftScriptPath;
                this.linkedBoxes.RightFileName = this.results.RightScriptPath;
                this.linkedBoxes.SplitUnifiedDiffText();
            }
            else
            {
                string[] leftContents = this.leftFileText.Split('\n');
                for (int i = 0; i < leftContents.Length; i++)
                    leftContents[i] = leftContents[i].TrimEnd(); // trim the end because the diff blocks a a WriteLine, don't want extra \r\n's

                string[] rightContents = this.rightFileText.Split('\n');
                for (int i = 0; i < rightContents.Length; i++)
                    rightContents[i] = rightContents[i].TrimEnd(); // trim the end because the diff blocks a a WriteLine, don't want extra \r\n's


                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                Algorithm.Diff.UnifiedDiff.WriteUnifiedDiff(leftContents, "Current Script", rightContents, "Script Run on Server", sw, 500, false, false);
                if (sb.Length == 46)
                {
                    //This means that only the diff header came back ("--- Current Script\r\n+++ Script Run on Server\r\n") and there are no diffs, to just put in one of the files.
                    for (int i = 0; i < leftContents.Length; i++)
                        leftContents[i] = " " + leftContents[i]; //add a space b/c the unified diff adds it.

                    this.linkedBoxes.UnifiedDiffText = String.Join("\r\n", leftContents);
                }
                else
                { 
                    this.linkedBoxes.UnifiedDiffText = sb.ToString();
                }
                this.linkedBoxes.LeftFileName = this.leftFileDescriptor;
                this.linkedBoxes.RightFileName = this.rightFileDescriptor;
                this.linkedBoxes.SplitUnifiedDiffText();
                this.linkedBoxes.ShowMenuStrip = false;

                this.Text = String.Format("{0} <--> {1}", this.leftFileDescriptor, this.rightFileDescriptor);


            }
        }

        private void linkedBoxes_FileChanged(object sender, EventArgs e)
        {
            this.fileChanged = true;
        }

      
    }
}