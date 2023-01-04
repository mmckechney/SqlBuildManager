using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
namespace SqlSync.BuildHistory
{
    /// <summary>
    /// Summary description for NewLookUpDatabaseForm.
    /// </summary>
    public class LogFileHistoryForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private string basePath;
        private System.Windows.Forms.ListView lstLogFiles;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenInNotePad;
        private string queryAnalyzerPath = @"C:\Program Files\Microsoft SQL Server\80\Tools\Binn\isqlw.exe";
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button btnArchive;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private string buildFileName;
        private System.Windows.Forms.LinkLabel lnkCheckAll;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private IContainer components;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath">The path to the temp folder created for the session</param>
        /// <param name="buildFileName">Name of the build file zip (sbm) file, used to pre-populate the destination file name</param>
        public LogFileHistoryForm(string basePath, string buildFileName)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.basePath = basePath;
            this.buildFileName = buildFileName;

            //

            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogFileHistoryForm));
            lstLogFiles = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuOpenInNotePad = new System.Windows.Forms.ToolStripMenuItem();
            btnArchive = new System.Windows.Forms.Button();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            lnkCheckAll = new System.Windows.Forms.LinkLabel();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            contextMenu1.SuspendLayout();
            statusBar1.SuspendLayout();
            SuspendLayout();
            // 
            // lstLogFiles
            // 
            lstLogFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstLogFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstLogFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstLogFiles.CheckBoxes = true;
            lstLogFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            lstLogFiles.ContextMenuStrip = contextMenu1;
            lstLogFiles.FullRowSelect = true;
            lstLogFiles.GridLines = true;
            lstLogFiles.HideSelection = false;
            lstLogFiles.Location = new System.Drawing.Point(19, 10);
            lstLogFiles.Name = "lstLogFiles";
            lstLogFiles.RightToLeft = System.Windows.Forms.RightToLeft.No;
            lstLogFiles.Size = new System.Drawing.Size(279, 316);
            lstLogFiles.TabIndex = 0;
            lstLogFiles.UseCompatibleStateImageBehavior = false;
            lstLogFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Log File Name";
            columnHeader1.Width = 204;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Size (Kb)";
            columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuOpenInNotePad});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(210, 26);
            // 
            // mnuOpenInNotePad
            // 
            mnuOpenInNotePad.Name = "mnuOpenInNotePad";
            mnuOpenInNotePad.Size = new System.Drawing.Size(209, 22);
            mnuOpenInNotePad.Text = "Open Log File";
            mnuOpenInNotePad.Click += new System.EventHandler(mnuOpenInNotePad_Click);
            // 
            // btnArchive
            // 
            btnArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnArchive.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnArchive.Location = new System.Drawing.Point(210, 331);
            btnArchive.Name = "btnArchive";
            btnArchive.Size = new System.Drawing.Size(90, 25);
            btnArchive.TabIndex = 1;
            btnArchive.Text = "Archive";
            btnArchive.Click += new System.EventHandler(btnArchive_Click);
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "zip";
            saveFileDialog1.Filter = "Zip Files *.zip|*.zip|All Files *.*|*.*";
            saveFileDialog1.Title = "Create / Append Log Archive";
            saveFileDialog1.ValidateNames = false;
            // 
            // lnkCheckAll
            // 
            lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            lnkCheckAll.Location = new System.Drawing.Point(19, 326);
            lnkCheckAll.Name = "lnkCheckAll";
            lnkCheckAll.Size = new System.Drawing.Size(120, 20);
            lnkCheckAll.TabIndex = 2;
            lnkCheckAll.TabStop = true;
            lnkCheckAll.Text = "Check All";
            lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkCheckAll_LinkClicked);
            // 
            // statusBar1
            // 
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statGeneral});
            statusBar1.Location = new System.Drawing.Point(0, 376);
            statusBar1.Name = "statusBar1";
            statusBar1.Size = new System.Drawing.Size(320, 22);
            statusBar1.TabIndex = 3;
            statusBar1.Text = "statusBar1";
            // 
            // statGeneral
            // 
            statGeneral.Name = "statGeneral";
            statGeneral.Size = new System.Drawing.Size(305, 17);
            statGeneral.Spring = true;
            // 
            // LogFileHistoryForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(320, 398);
            Controls.Add(statusBar1);
            Controls.Add(lnkCheckAll);
            Controls.Add(btnArchive);
            Controls.Add(lstLogFiles);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "LogFileHistoryForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Log File List";
            Load += new System.EventHandler(LogFileHistoryForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(LogFileHistoryForm_KeyDown);
            contextMenu1.ResumeLayout(false);
            statusBar1.ResumeLayout(false);
            statusBar1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion


        private void LogFileHistoryForm_Load(object sender, System.EventArgs e)
        {
            string[] logFiles = Directory.GetFiles(basePath, "*.log");
            long length;
            for (int i = 0; i < logFiles.Length; i++)
            {
                length = new FileInfo(logFiles[i]).Length;
                ListViewItem item = new ListViewItem(new string[] { Path.GetFileName(logFiles[i]), Convert.ToString(Convert.ToDouble(length) / 1000.0) });
                lstLogFiles.Items.Add(item);
            }

            if (File.Exists(queryAnalyzerPath))
            {
                ToolStripMenuItem item = new ToolStripMenuItem("Open in Query Analyzer");
                item.Click += new EventHandler(mnuOpenInQueryAnalyzer_Click);

                contextMenu1.Items.Add(item);
            }
        }

        private void mnuOpenInNotePad_Click(object sender, System.EventArgs e)
        {
            if (lstLogFiles.SelectedItems.Count > 0)
            {
                string fileName = lstLogFiles.SelectedItems[0].Text;
                Process prc = new Process();
                prc.StartInfo.FileName = "explorer";
                prc.StartInfo.Arguments = Path.Combine(basePath, fileName);
                prc.Start();
            }

        }
        private void mnuOpenInQueryAnalyzer_Click(object sender, System.EventArgs e)
        {
            if (lstLogFiles.SelectedItems.Count > 0)
            {
                string fileName = lstLogFiles.SelectedItems[0].Text;
                Process prc = new Process();
                prc.StartInfo.FileName = queryAnalyzerPath;
                prc.StartInfo.Arguments = "\"" + Path.Combine(basePath, fileName) + "\"";
                prc.Start();
            }

        }

        private void LogFileHistoryForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void btnArchive_Click(object sender, System.EventArgs e)
        {
            //Ensure there is something to do
            if (lstLogFiles.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select files to archive", "Selections Needed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            //Get the file list
            ArrayList logs = new ArrayList();
            for (int i = 0; i < lstLogFiles.CheckedItems.Count; i++)
                logs.Add(lstLogFiles.CheckedItems[i].Text);

            string[] logFiles = new string[logs.Count];
            logs.CopyTo(logFiles);
            int loggedCount = logs.Count;
            //Get the destination file name
            string startingName = Path.GetFileNameWithoutExtension(buildFileName) + " - Log Archive.zip";
            saveFileDialog1.FileName = startingName;
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                statGeneral.Text = "Archiving Log Files";
                string fileName = saveFileDialog1.FileName;
                if (SqlSync.SqlBuild.SqlBuildFileHelper.ArchiveLogFiles(logFiles, basePath, fileName))
                {
                    lstLogFiles.Items.Clear();
                    LogFileHistoryForm_Load(null, EventArgs.Empty);
                    if (LogFilesArchvied != null)
                        LogFilesArchvied(null, EventArgs.Empty);
                }
                statGeneral.Text = "Moved " + loggedCount.ToString() + " files to archive";
            }
        }

        private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if (lnkCheckAll.Text.ToLower() == "check all")
            {
                for (int i = 0; i < lstLogFiles.Items.Count; i++)
                    lstLogFiles.Items[i].Checked = true;

                lnkCheckAll.Text = "Uncheck All";
            }
            else
            {
                for (int i = 0; i < lstLogFiles.Items.Count; i++)
                    lstLogFiles.Items[i].Checked = false;

                lnkCheckAll.Text = "Check All";
            }
        }
        public event EventHandler LogFilesArchvied;
    }
}
