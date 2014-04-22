using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
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
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem mnuOpenInNotePad;
		private string queryAnalyzerPath = @"C:\Program Files\Microsoft SQL Server\80\Tools\Binn\isqlw.exe";
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button btnArchive;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private string buildFileName;
		private System.Windows.Forms.LinkLabel lnkCheckAll;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statGeneral;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


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
			this.basePath =  basePath;
			this.buildFileName = buildFileName;

			//
			
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogFileHistoryForm));
            this.lstLogFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.mnuOpenInNotePad = new System.Windows.Forms.MenuItem();
            this.btnArchive = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lnkCheckAll = new System.Windows.Forms.LinkLabel();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statGeneral = new System.Windows.Forms.StatusBarPanel();
            ((System.ComponentModel.ISupportInitialize)(this.statGeneral)).BeginInit();
            this.SuspendLayout();
            // 
            // lstLogFiles
            // 
            this.lstLogFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstLogFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLogFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLogFiles.CheckBoxes = true;
            this.lstLogFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstLogFiles.ContextMenu = this.contextMenu1;
            this.lstLogFiles.FullRowSelect = true;
            this.lstLogFiles.GridLines = true;
            this.lstLogFiles.Location = new System.Drawing.Point(16, 8);
            this.lstLogFiles.Name = "lstLogFiles";
            this.lstLogFiles.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lstLogFiles.Size = new System.Drawing.Size(286, 332);
            this.lstLogFiles.TabIndex = 0;
            this.lstLogFiles.UseCompatibleStateImageBehavior = false;
            this.lstLogFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Log File Name";
            this.columnHeader1.Width = 204;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size (Kb)";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuOpenInNotePad});
            // 
            // mnuOpenInNotePad
            // 
            this.mnuOpenInNotePad.Index = 0;
            this.mnuOpenInNotePad.Text = "Open Log File in NotePad";
            this.mnuOpenInNotePad.Click += new System.EventHandler(this.mnuOpenInNotePad_Click);
            // 
            // btnArchive
            // 
            this.btnArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnArchive.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnArchive.Location = new System.Drawing.Point(228, 344);
            this.btnArchive.Name = "btnArchive";
            this.btnArchive.Size = new System.Drawing.Size(75, 20);
            this.btnArchive.TabIndex = 1;
            this.btnArchive.Text = "Archive";
            this.btnArchive.Click += new System.EventHandler(this.btnArchive_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "zip";
            this.saveFileDialog1.Filter = "Zip Files *.zip|*.zip|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Create / Append Log Archive";
            this.saveFileDialog1.ValidateNames = false;
            // 
            // lnkCheckAll
            // 
            this.lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkCheckAll.Location = new System.Drawing.Point(16, 340);
            this.lnkCheckAll.Name = "lnkCheckAll";
            this.lnkCheckAll.Size = new System.Drawing.Size(100, 16);
            this.lnkCheckAll.TabIndex = 2;
            this.lnkCheckAll.TabStop = true;
            this.lnkCheckAll.Text = "Check All";
            this.lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckAll_LinkClicked);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 374);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statGeneral});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(320, 24);
            this.statusBar1.TabIndex = 3;
            this.statusBar1.Text = "statusBar1";
            // 
            // statGeneral
            // 
            this.statGeneral.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Width = 303;
            // 
            // LogFileHistoryForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(320, 398);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.lnkCheckAll);
            this.Controls.Add(this.btnArchive);
            this.Controls.Add(this.lstLogFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "LogFileHistoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Log File List";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LogFileHistoryForm_KeyDown);
            this.Load += new System.EventHandler(this.LogFileHistoryForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.statGeneral)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion


		private void LogFileHistoryForm_Load(object sender, System.EventArgs e)
		{
			string[] logFiles = Directory.GetFiles(this.basePath,"*.log");
			long length;
			for(int i=0;i<logFiles.Length;i++)
			{
				length = new FileInfo(logFiles[i]).Length;
				ListViewItem item = new ListViewItem(new string[]{Path.GetFileName(logFiles[i]),Convert.ToString(Convert.ToDouble(length)/1000.0)});
				this.lstLogFiles.Items.Add(item);
			}

			if(File.Exists(queryAnalyzerPath))
			{
				MenuItem item = new MenuItem("Open in Query Analyzer");
				item.Click +=new EventHandler(mnuOpenInQueryAnalyzer_Click);

				this.contextMenu1.MenuItems.Add(item);
			}
		}

		private void mnuOpenInNotePad_Click(object sender, System.EventArgs e)
		{
			if(this.lstLogFiles.SelectedItems.Count > 0)
			{
				string fileName = this.lstLogFiles.SelectedItems[0].Text;
				Process prc = new Process();
				prc.StartInfo.FileName = "notepad.exe";
				prc.StartInfo.Arguments = this.basePath+ fileName;
				prc.Start();
			}

		}
		private void mnuOpenInQueryAnalyzer_Click(object sender, System.EventArgs e)
		{
			if(this.lstLogFiles.SelectedItems.Count > 0)
			{
				string fileName = this.lstLogFiles.SelectedItems[0].Text;
				Process prc = new Process();
				prc.StartInfo.FileName = queryAnalyzerPath;
				prc.StartInfo.Arguments = "\""+this.basePath+ fileName+"\"";
				prc.Start();
			}

		}

		private void LogFileHistoryForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				this.Close();
			}
		}

		private void btnArchive_Click(object sender, System.EventArgs e)
		{
			//Ensure there is something to do
			if(this.lstLogFiles.CheckedItems.Count == 0)
			{
				MessageBox.Show("Please select files to archive","Selections Needed",MessageBoxButtons.OK,MessageBoxIcon.Hand);
				return;
			}

			//Get the file list
			ArrayList logs = new ArrayList();
			for(int i=0;i<this.lstLogFiles.CheckedItems.Count;i++)
				logs.Add(this.lstLogFiles.CheckedItems[i].Text);

			string[] logFiles = new string[logs.Count];
			logs.CopyTo(logFiles);
			int loggedCount = logs.Count;
			//Get the destination file name
			string startingName = Path.GetFileNameWithoutExtension(this.buildFileName) +" - Log Archive.zip";
			saveFileDialog1.FileName = startingName;
			DialogResult result = saveFileDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.statGeneral.Text = "Archiving Log Files";
				string fileName = saveFileDialog1.FileName;
				if( SqlSync.SqlBuild.SqlBuildFileHelper.ArchiveLogFiles(logFiles,this.basePath,fileName) )
				{
					this.lstLogFiles.Items.Clear();
					this.LogFileHistoryForm_Load(null, EventArgs.Empty);
					if(this.LogFilesArchvied != null)
						this.LogFilesArchvied(null, EventArgs.Empty);
				}
				this.statGeneral.Text = "Moved "+loggedCount.ToString()+" files to archive";
			}
		}

		private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(this.lnkCheckAll.Text.ToLower() == "check all")
			{
				for(int i=0;i<this.lstLogFiles.Items.Count;i++)
					this.lstLogFiles.Items[i].Checked = true;

				this.lnkCheckAll.Text = "Uncheck All";
			}
			else
			{
				for(int i=0;i<this.lstLogFiles.Items.Count;i++)
					this.lstLogFiles.Items[i].Checked = false;

				this.lnkCheckAll.Text = "Check All";
			}
		}
		public event EventHandler LogFilesArchvied;
	}
}
