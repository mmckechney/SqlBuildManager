using SqlSync.Highlighting;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for ScriptDisplayForm.
    /// </summary>
    public class ScriptDisplayForm : System.Windows.Forms.Form
    {
        //private IContainer components;
        private CutCopyPasteContextMenuStrip contextMenuStrip1;
        private FinderCtrl finderCtrl1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem wordWrapToolStripMenuItem;
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox richTextBox1;
        private string remoteEndPoint = string.Empty;


        public ScriptDisplayForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            WordWrap = true;
        }
        /// <summary>
        /// Constructor that defaults to SQL syntax highlighting
        /// </summary>
        /// <param name="script"></param>
        /// <param name="server"></param>
        /// <param name="name"></param>
        public ScriptDisplayForm(string script, string server, string name)
            : this(script, server, name, SyntaxHightlightType.Sql)
        {
        }
        /// <summary>
        /// Constructor that allows for setting the desired highlighting type
        /// </summary>
        /// <param name="script"></param>
        /// <param name="server"></param>
        /// <param name="name"></param>
        /// <param name="highlightingType"></param>
		public ScriptDisplayForm(string script, string server, string name, SyntaxHightlightType highlightingType)
            : this(script, server, name, highlightingType, string.Empty)
        {

        }

        public ScriptDisplayForm(string script, string server, string name, SyntaxHightlightType highlightingType, string remoteEndpoint)
            : this()
        {
            remoteEndPoint = remoteEndpoint;
            Text = Text += " :: " + server + " .. " + name;

            richTextBox1.CaseSensitive = false;
            richTextBox1.FilterAutoComplete = true;
            richTextBox1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            richTextBox1.HighlightType = highlightingType;

            richTextBox1.MaxUndoRedoSteps = 50;


            if (highlightingType == SyntaxHightlightType.RemoteServiceLog)
            {
                richTextBox1.LinkClicked += new LinkClickedEventHandler(richTextBox1_LinkClicked);
                richTextBox1.Text = script;
            }
            else
            {
                richTextBox1.Text = script;
            }
            richTextBox1.RefreshHighlighting();
        }

        public bool WordWrap
        {
            get;
            set;
        }

        public bool ScrollToEndOnLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptDisplayForm));
            richTextBox1 = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            contextMenuStrip1 = new SqlSync.CutCopyPasteContextMenuStrip();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            finderCtrl1 = new SqlSync.FinderCtrl();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            richTextBox1.CaseSensitive = false;
            richTextBox1.ContextMenuStrip = contextMenuStrip1;
            richTextBox1.FilterAutoComplete = false;
            richTextBox1.HighlightDescriptors = highLightDescriptorCollection1;
            richTextBox1.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            richTextBox1.Location = new System.Drawing.Point(5, 2);
            richTextBox1.MaxUndoRedoSteps = 50;
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(963, 506);
            richTextBox1.SuspendHighlighting = false;
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripSeparator1,
            wordWrapToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(135, 98);
            contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStrip1_Opening);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(131, 6);
            // 
            // wordWrapToolStripMenuItem
            // 
            wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
            wordWrapToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            wordWrapToolStripMenuItem.Text = "Word Wrap";
            wordWrapToolStripMenuItem.Click += new System.EventHandler(wordWrapToolStripMenuItem_Click);
            // 
            // finderCtrl1
            // 
            finderCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            finderCtrl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            finderCtrl1.Location = new System.Drawing.Point(0, 513);
            finderCtrl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            finderCtrl1.Name = "finderCtrl1";
            finderCtrl1.Size = new System.Drawing.Size(972, 37);
            finderCtrl1.TabIndex = 1;
            // 
            // ScriptDisplayForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(972, 550);
            Controls.Add(finderCtrl1);
            Controls.Add(richTextBox1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "ScriptDisplayForm";
            Text = "Script Display ";
            Load += new System.EventHandler(ScriptDisplayForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(ScriptDisplayForm_KeyDown);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void ScriptDisplayForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void ScriptDisplayForm_Load(object sender, EventArgs e)
        {

            if (richTextBox1.HighlightType == SyntaxHightlightType.RemoteServiceLog)
            {
                AddLoggingLinks();
            }
            richTextBox1.WordWrap = WordWrap;
            finderCtrl1.AddControlToSearch(richTextBox1);
            if (ScrollToEndOnLoad)
            {
                if (richTextBox1.Text.Length > 2)
                {
                    richTextBox1.Select(richTextBox1.Text.Length - 2, 0);
                    richTextBox1.ScrollToCaret();
                }
            }

        }

        private void AddLoggingLinks()
        {
            //Expected line format:  [01/17/2011 09:40:31.199]		ServerName\InstanceName.DatabaseName : Changes Rolled back. Return code: -400
            Regex regDbLine = new Regex(@"\[\d\d/\d\d/\d\d\d\d \d\d:\d\d:\d\d\.\d\d\d\]\t\t\S+ :");
            Regex regServerDb = new Regex(@"\S+");
            int startAt = 0;
            //this.richTextBox1.Text = this.richTextBox1.Text.Replace("\\", ".");
            while (regDbLine.Match(richTextBox1.Text, startAt).Success)
            {
                Match m = regDbLine.Match(richTextBox1.Text, startAt);
                Match db = regServerDb.Match(richTextBox1.Text, m.Index + 26);
                string hyperLink = m.Value.Substring(0, m.Length - 1).Replace(db.Value, "").Trim();
                richTextBox1.AddLinkAt(db.Index, db.Length, hyperLink);

                startAt = m.Index + m.Length + 1;
            }
        }
        void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            ///MessageBox.Show(e.LinkText);
            if (LoggingLinkClicked != null)
                LoggingLinkClicked(e.LinkText, remoteEndPoint);
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.WordWrap = !richTextBox1.WordWrap;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (richTextBox1.WordWrap)
                wordWrapToolStripMenuItem.Checked = true;
            else
                wordWrapToolStripMenuItem.Checked = false;
        }

        public event LoggingLinkClickedEventHandler LoggingLinkClicked;
    }
    public delegate void LoggingLinkClickedEventHandler(string databaseInfo, string remoteEndPoint);
}
