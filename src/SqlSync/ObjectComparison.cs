using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using SqlSync.ObjectScript;
using System.Diagnostics;
using SqlSync.DbInformation;
namespace SqlSync
{
	/// <summary>
	/// Summary description for ObjectComparison.
	/// </summary>
	public class ObjectComparison : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColumnHeader colObjectName;
		private System.Windows.Forms.ColumnHeader colInDatabase;
		private System.Windows.Forms.ColumnHeader colInFileSystem;
		private System.Windows.Forms.ColumnHeader colFileName;
		private System.Windows.Forms.ColumnHeader colFullPath;
		private System.Windows.Forms.ColumnHeader colObjectType;
		private ObjectSyncData[] arrData;
		private System.Windows.Forms.LinkLabel lnkScript;
		private System.Windows.Forms.ListView lstComparison;
		private ObjectScriptHelper helper;
		private string defaultSavePath = string.Empty;
		private System.Windows.Forms.FolderBrowserDialog fldrSaveDir;
		private System.Windows.Forms.ContextMenuStrip contextMenu1;
		private System.Windows.Forms.ToolStripMenuItem mnuNotePad;
		private System.Windows.Forms.StatusStrip statusBar1;
		private System.Windows.Forms.ToolStripStatusLabel statStatus;
		private System.Windows.Forms.ToolStripMenuItem mnuDefaultPath;
		private System.Windows.Forms.ToolStripStatusLabel statDefaultPath;
		private System.Windows.Forms.ToolStripMenuItem menuItem1;
		private System.Windows.Forms.ToolStripMenuItem mnuDeleteFile;
		private System.Windows.Forms.LinkLabel lnkDelete;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ObjectComparison(ObjectSyncData[] arrData, ref ObjectScriptHelper helper)
		{
			this.arrData = arrData;
			this.helper = helper;
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectComparison));
            this.lstComparison = new System.Windows.Forms.ListView();
            this.colObjectName = new System.Windows.Forms.ColumnHeader();
            this.colObjectType = new System.Windows.Forms.ColumnHeader();
            this.colInDatabase = new System.Windows.Forms.ColumnHeader();
            this.colInFileSystem = new System.Windows.Forms.ColumnHeader();
            this.colFileName = new System.Windows.Forms.ColumnHeader();
            this.colFullPath = new System.Windows.Forms.ColumnHeader();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
            this.mnuNotePad = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDefaultPath = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDeleteFile = new System.Windows.Forms.ToolStripMenuItem();
            this.lnkScript = new System.Windows.Forms.LinkLabel();
            this.fldrSaveDir = new System.Windows.Forms.FolderBrowserDialog();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.statDefaultPath = new System.Windows.Forms.ToolStripStatusLabel();
            this.lnkDelete = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statDefaultPath)).BeginInit();
            this.SuspendLayout();
            // 
            // lstComparison
            // 
            this.lstComparison.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstComparison.CheckBoxes = true;
            this.lstComparison.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colObjectName,
            this.colObjectType,
            this.colInDatabase,
            this.colInFileSystem,
            this.colFileName,
            this.colFullPath});
            this.lstComparison.ContextMenuStrip = this.contextMenu1;
            this.lstComparison.FullRowSelect = true;
            this.lstComparison.Location = new System.Drawing.Point(16, 40);
            this.lstComparison.Name = "lstComparison";
            this.lstComparison.Size = new System.Drawing.Size(832, 392);
            this.lstComparison.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstComparison.TabIndex = 0;
            this.lstComparison.UseCompatibleStateImageBehavior = false;
            this.lstComparison.View = System.Windows.Forms.View.Details;
            // 
            // colObjectName
            // 
            this.colObjectName.Text = "Object Name";
            this.colObjectName.Width = 163;
            // 
            // colObjectType
            // 
            this.colObjectType.Text = "Type";
            this.colObjectType.Width = 77;
            // 
            // colInDatabase
            // 
            this.colInDatabase.Text = "In Database";
            this.colInDatabase.Width = 74;
            // 
            // colInFileSystem
            // 
            this.colInFileSystem.Text = "In File System";
            this.colInFileSystem.Width = 81;
            // 
            // colFileName
            // 
            this.colFileName.Text = "FileName";
            this.colFileName.Width = 180;
            // 
            // colFullPath
            // 
            this.colFullPath.Text = "Full Path";
            this.colFullPath.Width = 291;
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.mnuNotePad,
            this.mnuDefaultPath,
            this.menuItem1,
            this.mnuDeleteFile});
            // 
            // mnuNotePad
            // 
            //this.mnuNotePad.Index = 0;
            this.mnuNotePad.Text = "Open File in NotePad";
            this.mnuNotePad.Click += new System.EventHandler(this.mnuNotePad_Click);
            // 
            // mnuDefaultPath
            // 
           // this.mnuDefaultPath.Index = 1;
            this.mnuDefaultPath.Text = "Set Path as Default Directory";
            this.mnuDefaultPath.Click += new System.EventHandler(this.mnuDefaultPath_Click);
            // 
            // menuItem1
            // 
            //this.menuItem1.Index = 2;
            this.menuItem1.Text = "-";
            // 
            // mnuDeleteFile
            // 
            //this.mnuDeleteFile.Index = 3;
            this.mnuDeleteFile.Text = "Delete File";
            this.mnuDeleteFile.Click += new System.EventHandler(this.mnuDeleteFile_Click);
            // 
            // lnkScript
            // 
            this.lnkScript.Location = new System.Drawing.Point(704, 8);
            this.lnkScript.Name = "lnkScript";
            this.lnkScript.Size = new System.Drawing.Size(144, 16);
            this.lnkScript.TabIndex = 1;
            this.lnkScript.TabStop = true;
            this.lnkScript.Text = "Script Checked Objects ";
            this.lnkScript.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkScript_LinkClicked);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 456);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.statStatus,
            this.statDefaultPath});
            //this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(864, 22);
            this.statusBar1.TabIndex = 2;
            this.statusBar1.Text = "statusBar1";
			// 
			// statStatus
			// 
			this.statStatus.AutoSize = true;
			this.statStatus.Spring = true;
            this.statStatus.Name = "statStatus";
            this.statStatus.Text = "Ready";
            this.statStatus.Width = 447;
            // 
            // statDefaultPath
            // 
            this.statDefaultPath.Name = "statDefaultPath";
            this.statDefaultPath.Width = 400;
            // 
            // lnkDelete
            // 
            this.lnkDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkDelete.Location = new System.Drawing.Point(712, 440);
            this.lnkDelete.Name = "lnkDelete";
            this.lnkDelete.Size = new System.Drawing.Size(136, 16);
            this.lnkDelete.TabIndex = 3;
            this.lnkDelete.TabStop = true;
            this.lnkDelete.Text = "Delete checked objects";
            this.lnkDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDelete_LinkClicked);
            // 
            // ObjectComparison
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(864, 478);
            this.Controls.Add(this.lnkDelete);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.lnkScript);
            this.Controls.Add(this.lstComparison);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ObjectComparison";
            this.Text = "ObjectComparison";
            this.Load += new System.EventHandler(this.ObjectComparison_Load);
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statDefaultPath)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void ObjectComparison_Load(object sender, System.EventArgs e)
		{
			for(int i=0;i<this.arrData.Length;i++)
			{
				ObjectSyncData data = this.arrData[i];
				ListViewItem item = new ListViewItem(new string[]{ ((data.SchemaOwner.Length > 0) ? data.SchemaOwner +"."+ data.ObjectName : data.ObjectName),
																	 data.ObjectType,
																	 data.IsInDatabase.ToString(),
																	 data.IsInFileSystem.ToString(),
																	 data.FileName,
																	 data.FullPath});
				if(data.IsInFileSystem == false || data.IsInDatabase == false)
				{
					if(data.IsInFileSystem == false)
					{
						item.BackColor = Color.Red;
						item.ForeColor = Color.White;
					}
					if(data.IsInDatabase == false)
					{
						item.BackColor = Color.Salmon;
						item.ForeColor = Color.Black;
					}
				}
				lstComparison.Items.Add(item);
			}
		}

	
		private void lnkScript_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			//check to see if we need to ask for a directory 
			bool needDir = false;
			foreach(ListViewItem item in  lstComparison.CheckedItems)
			{
				if(item.SubItems[5].Text.Length == 0)
				{
					needDir = true;
					break;
				}			
			}

			if(needDir &&  this.defaultSavePath.Length == 0)
			{
				DialogResult result = fldrSaveDir.ShowDialog();
				if(result == DialogResult.OK)
				{
					this.defaultSavePath = fldrSaveDir.SelectedPath;
				}
				else
				{
					return;
				}
			}

			
			//Loop through checked files
			bool scripted = false;
			foreach(ListViewItem item in  lstComparison.CheckedItems)
			{
                string name = item.SubItems[0].Text + item.SubItems[1].Text;
                string schema;
                InfoHelper.ExtractNameAndSchema(name, out name, out schema);

				string shortName = schema+"."+name.Replace("\\","-");
				string longName;
                string message;
				if(item.SubItems[5].Text.Length == 0)
				{
					longName = Path.Combine(this.defaultSavePath, shortName);
				}else
				 {
					longName= item.SubItems[5].Text;
				 }
				statStatus.Text = "Scripting " + shortName;


				scripted = ScriptObject(name,schema,item.SubItems[1].Text,longName, out message);
				if(scripted)
				{
					item.BackColor = Color.LawnGreen;
					item.SubItems[4].Text = shortName;
					item.SubItems[5].Text = longName;
					item.Checked = false;
				}
				else
				{
					item.BackColor = Color.Magenta;
                    MessageBox.Show(message, "Error Scripting", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

			}

			helper.DisconnectServer();
			statStatus.Text = "Scripting Complete";

		}
		private bool ScriptObject(string objectName,string schemaOwner, string type, string fullFileName, out string message)
		{
			bool success = false;
			string script = string.Empty;
			string objectTypeDesc = string.Empty;
            

			success = helper.ScriptDatabaseObject(type,objectName,schemaOwner,ref script,ref objectTypeDesc,out message);
			if(success)
			{
				helper.SaveScriptToFile(script,objectName,objectTypeDesc,Path.GetFileName(fullFileName),fullFileName,true,true);
				return true;
			}
			else
			{
				return false;
			}
			
		}
		

		private void mnuNotePad_Click(object sender, System.EventArgs e)
		{
			if(lstComparison.SelectedItems.Count > 0)
			{
				string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;

				if(fullPath.Length > 0)
				{
					Process prc = new Process();
					prc.StartInfo.FileName = "notepad.exe";
					prc.StartInfo.Arguments = fullPath;
					prc.Start();
				}
			}
			else
			{
				MessageBox.Show("Please select a file to display","Select a File",MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
		}

		private void mnuDefaultPath_Click(object sender, System.EventArgs e)
		{
			if(lstComparison.SelectedItems.Count > 0)
			{
				string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;
				string dir = Path.GetDirectoryName(fullPath);
				string displayPath = (dir.Length > 70) ? ".."+dir.Substring(dir.Length-68,68) : dir;
				this.defaultSavePath = dir;
				statDefaultPath.Text = displayPath;

			}
		}

		private void mnuDeleteFile_Click(object sender, System.EventArgs e)
		{
			if(lstComparison.SelectedItems.Count > 0)
			{
				string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;
				string fileName = Path.GetFileName(fullPath);
				DialogResult result = MessageBox.Show("Are You sure you want to delete "+fileName+"?","Delete File",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
				if(result == DialogResult.Yes)
				{
					try
					{
						File.Delete(fullPath);
						lstComparison.Items.Remove(lstComparison.SelectedItems[0]);
					}
					catch
					{
						MessageBox.Show("Unable to delete the file");
					}
				}

			}
		}

		private void lnkDelete_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			foreach(ListViewItem item in  lstComparison.CheckedItems)
			{
				string fullPath = item.SubItems[5].Text;
				try
				{
					File.Delete(fullPath);
					item.BackColor = Color.LightGray;
					item.Checked = false;
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}


	}
}
