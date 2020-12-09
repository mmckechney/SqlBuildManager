using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.Highlighting;
using System.Text.RegularExpressions;
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
            this.WordWrap = true;
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
            : this(script,server,name,highlightingType,string.Empty)
		{
          
		}

        public ScriptDisplayForm(string script, string server, string name, SyntaxHightlightType highlightingType, string remoteEndpoint)
            : this()
        {
            this.remoteEndPoint = remoteEndpoint;
            this.Text = this.Text += " :: " + server + " .. " + name;

            this.richTextBox1.CaseSensitive = false;
            this.richTextBox1.FilterAutoComplete = true;
            this.richTextBox1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.richTextBox1.HighlightType = highlightingType;

            this.richTextBox1.MaxUndoRedoSteps = 50;


            if (highlightingType == SyntaxHightlightType.RemoteServiceLog)
            {
                this.richTextBox1.LinkClicked += new LinkClickedEventHandler(richTextBox1_LinkClicked);
                this.richTextBox1.Text = script; 
            }
            else
            {
                this.richTextBox1.Text = script;
            }
            this.richTextBox1.RefreshHighlighting();
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
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
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
            this.richTextBox1 = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.contextMenuStrip1 = new SqlSync.CutCopyPasteContextMenuStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finderCtrl1 = new SqlSync.FinderCtrl();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.CaseSensitive = false;
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBox1.FilterAutoComplete = false;
            this.richTextBox1.HighlightDescriptors = highLightDescriptorCollection1;
            this.richTextBox1.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.richTextBox1.Location = new System.Drawing.Point(5, 2);
            this.richTextBox1.MaxUndoRedoSteps = 50;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(963, 506);
            this.richTextBox1.SuspendHighlighting = false;
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.wordWrapToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(135, 98);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(131, 6);
            // 
            // wordWrapToolStripMenuItem
            // 
            this.wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
            this.wordWrapToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.wordWrapToolStripMenuItem.Text = "Word Wrap";
            this.wordWrapToolStripMenuItem.Click += new System.EventHandler(this.wordWrapToolStripMenuItem_Click);
            // 
            // finderCtrl1
            // 
            this.finderCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.finderCtrl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.finderCtrl1.Location = new System.Drawing.Point(0, 513);
            this.finderCtrl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.finderCtrl1.Name = "finderCtrl1";
            this.finderCtrl1.Size = new System.Drawing.Size(972, 37);
            this.finderCtrl1.TabIndex = 1;
            // 
            // ScriptDisplayForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.ClientSize = new System.Drawing.Size(972, 550);
            this.Controls.Add(this.finderCtrl1);
            this.Controls.Add(this.richTextBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ScriptDisplayForm";
            this.Text = "Script Display ";
            this.Load += new System.EventHandler(this.ScriptDisplayForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptDisplayForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

        private void ScriptDisplayForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void ScriptDisplayForm_Load(object sender, EventArgs e)
        {

            if (this.richTextBox1.HighlightType == SyntaxHightlightType.RemoteServiceLog)
            {
                AddLoggingLinks();
            }
            richTextBox1.WordWrap = this.WordWrap;
            this.finderCtrl1.AddControlToSearch(this.richTextBox1);
            if (this.ScrollToEndOnLoad)
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
            while (regDbLine.Match(this.richTextBox1.Text, startAt).Success)
            {
                Match m = regDbLine.Match(this.richTextBox1.Text, startAt);
                Match db = regServerDb.Match(this.richTextBox1.Text, m.Index + 26);
                string hyperLink = m.Value.Substring(0,m.Length - 1).Replace(db.Value, "").Trim();
                this.richTextBox1.AddLinkAt(db.Index, db.Length, hyperLink);

                startAt = m.Index + m.Length + 1;
            }
        }
        void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            ///MessageBox.Show(e.LinkText);
            if (this.LoggingLinkClicked != null)
                this.LoggingLinkClicked(e.LinkText, this.remoteEndPoint);
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
    public delegate void LoggingLinkClickedEventHandler(string databaseInfo,string remoteEndPoint);
}
