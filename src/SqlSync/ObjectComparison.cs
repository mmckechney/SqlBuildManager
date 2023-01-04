using SqlSync.DbInformation;
using SqlSync.ObjectScript;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectComparison));
            lstComparison = new System.Windows.Forms.ListView();
            colObjectName = new System.Windows.Forms.ColumnHeader();
            colObjectType = new System.Windows.Forms.ColumnHeader();
            colInDatabase = new System.Windows.Forms.ColumnHeader();
            colInFileSystem = new System.Windows.Forms.ColumnHeader();
            colFileName = new System.Windows.Forms.ColumnHeader();
            colFullPath = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
            mnuNotePad = new System.Windows.Forms.ToolStripMenuItem();
            mnuDefaultPath = new System.Windows.Forms.ToolStripMenuItem();
            menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            mnuDeleteFile = new System.Windows.Forms.ToolStripMenuItem();
            lnkScript = new System.Windows.Forms.LinkLabel();
            fldrSaveDir = new System.Windows.Forms.FolderBrowserDialog();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            statDefaultPath = new System.Windows.Forms.ToolStripStatusLabel();
            lnkDelete = new System.Windows.Forms.LinkLabel();
            SuspendLayout();
            // 
            // lstComparison
            // 
            lstComparison.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstComparison.CheckBoxes = true;
            lstComparison.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colObjectName,
            colObjectType,
            colInDatabase,
            colInFileSystem,
            colFileName,
            colFullPath});
            lstComparison.ContextMenuStrip = contextMenu1;
            lstComparison.FullRowSelect = true;
            lstComparison.Location = new System.Drawing.Point(16, 40);
            lstComparison.Name = "lstComparison";
            lstComparison.Size = new System.Drawing.Size(832, 392);
            lstComparison.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lstComparison.TabIndex = 0;
            lstComparison.UseCompatibleStateImageBehavior = false;
            lstComparison.View = System.Windows.Forms.View.Details;
            // 
            // colObjectName
            // 
            colObjectName.Text = "Object Name";
            colObjectName.Width = 163;
            // 
            // colObjectType
            // 
            colObjectType.Text = "Type";
            colObjectType.Width = 77;
            // 
            // colInDatabase
            // 
            colInDatabase.Text = "In Database";
            colInDatabase.Width = 74;
            // 
            // colInFileSystem
            // 
            colInFileSystem.Text = "In File System";
            colInFileSystem.Width = 81;
            // 
            // colFileName
            // 
            colFileName.Text = "FileName";
            colFileName.Width = 180;
            // 
            // colFullPath
            // 
            colFullPath.Text = "Full Path";
            colFullPath.Width = 291;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            mnuNotePad,
            mnuDefaultPath,
            menuItem1,
            mnuDeleteFile});
            // 
            // mnuNotePad
            // 
            //this.mnuNotePad.Index = 0;
            mnuNotePad.Text = "Open File in NotePad";
            mnuNotePad.Click += new System.EventHandler(mnuNotePad_Click);
            // 
            // mnuDefaultPath
            // 
            // this.mnuDefaultPath.Index = 1;
            mnuDefaultPath.Text = "Set Path as Default Directory";
            mnuDefaultPath.Click += new System.EventHandler(mnuDefaultPath_Click);
            // 
            // menuItem1
            // 
            //this.menuItem1.Index = 2;
            menuItem1.Text = "-";
            // 
            // mnuDeleteFile
            // 
            //this.mnuDeleteFile.Index = 3;
            mnuDeleteFile.Text = "Delete File";
            mnuDeleteFile.Click += new System.EventHandler(mnuDeleteFile_Click);
            // 
            // lnkScript
            // 
            lnkScript.Location = new System.Drawing.Point(704, 8);
            lnkScript.Name = "lnkScript";
            lnkScript.Size = new System.Drawing.Size(144, 16);
            lnkScript.TabIndex = 1;
            lnkScript.TabStop = true;
            lnkScript.Text = "Script Checked Objects ";
            lnkScript.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkScript_LinkClicked);
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 456);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            statStatus,
            statDefaultPath});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(864, 22);
            statusBar1.TabIndex = 2;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.AutoSize = true;
            statStatus.Spring = true;
            statStatus.Name = "statStatus";
            statStatus.Text = "Ready";
            statStatus.Width = 447;
            // 
            // statDefaultPath
            // 
            statDefaultPath.Name = "statDefaultPath";
            statDefaultPath.Width = 400;
            // 
            // lnkDelete
            // 
            lnkDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            lnkDelete.Location = new System.Drawing.Point(712, 440);
            lnkDelete.Name = "lnkDelete";
            lnkDelete.Size = new System.Drawing.Size(136, 16);
            lnkDelete.TabIndex = 3;
            lnkDelete.TabStop = true;
            lnkDelete.Text = "Delete checked objects";
            lnkDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkDelete_LinkClicked);
            // 
            // ObjectComparison
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(864, 478);
            Controls.Add(lnkDelete);
            Controls.Add(statusBar1);
            Controls.Add(lnkScript);
            Controls.Add(lstComparison);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "ObjectComparison";
            Text = "ObjectComparison";
            Load += new System.EventHandler(ObjectComparison_Load);
            ResumeLayout(false);

        }
        #endregion

        private void ObjectComparison_Load(object sender, System.EventArgs e)
        {
            for (int i = 0; i < arrData.Length; i++)
            {
                ObjectSyncData data = arrData[i];
                ListViewItem item = new ListViewItem(new string[]{ ((data.SchemaOwner.Length > 0) ? data.SchemaOwner +"."+ data.ObjectName : data.ObjectName),
                                                                     data.ObjectType,
                                                                     data.IsInDatabase.ToString(),
                                                                     data.IsInFileSystem.ToString(),
                                                                     data.FileName,
                                                                     data.FullPath});
                if (data.IsInFileSystem == false || data.IsInDatabase == false)
                {
                    if (data.IsInFileSystem == false)
                    {
                        item.BackColor = Color.Red;
                        item.ForeColor = Color.White;
                    }
                    if (data.IsInDatabase == false)
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
            foreach (ListViewItem item in lstComparison.CheckedItems)
            {
                if (item.SubItems[5].Text.Length == 0)
                {
                    needDir = true;
                    break;
                }
            }

            if (needDir && defaultSavePath.Length == 0)
            {
                DialogResult result = fldrSaveDir.ShowDialog();
                if (result == DialogResult.OK)
                {
                    defaultSavePath = fldrSaveDir.SelectedPath;
                }
                else
                {
                    return;
                }
            }


            //Loop through checked files
            bool scripted = false;
            foreach (ListViewItem item in lstComparison.CheckedItems)
            {
                string name = item.SubItems[0].Text + item.SubItems[1].Text;
                string schema;
                InfoHelper.ExtractNameAndSchema(name, out name, out schema);

                string shortName = schema + "." + name.Replace("\\", "-");
                string longName;
                string message;
                if (item.SubItems[5].Text.Length == 0)
                {
                    longName = Path.Combine(defaultSavePath, shortName);
                }
                else
                {
                    longName = item.SubItems[5].Text;
                }
                statStatus.Text = "Scripting " + shortName;


                scripted = ScriptObject(name, schema, item.SubItems[1].Text, longName, out message);
                if (scripted)
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
        private bool ScriptObject(string objectName, string schemaOwner, string type, string fullFileName, out string message)
        {
            bool success = false;
            string script = string.Empty;
            string objectTypeDesc = string.Empty;


            success = helper.ScriptDatabaseObject(type, objectName, schemaOwner, ref script, ref objectTypeDesc, out message);
            if (success)
            {
                helper.SaveScriptToFile(script, objectName, objectTypeDesc, Path.GetFileName(fullFileName), fullFileName, true, true);
                return true;
            }
            else
            {
                return false;
            }

        }


        private void mnuNotePad_Click(object sender, System.EventArgs e)
        {
            if (lstComparison.SelectedItems.Count > 0)
            {
                string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;

                if (fullPath.Length > 0)
                {
                    Process prc = new Process();
                    prc.StartInfo.FileName = "notepad.exe";
                    prc.StartInfo.Arguments = fullPath;
                    prc.Start();
                }
            }
            else
            {
                MessageBox.Show("Please select a file to display", "Select a File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuDefaultPath_Click(object sender, System.EventArgs e)
        {
            if (lstComparison.SelectedItems.Count > 0)
            {
                string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;
                string dir = Path.GetDirectoryName(fullPath);
                string displayPath = (dir.Length > 70) ? ".." + dir.Substring(dir.Length - 68, 68) : dir;
                defaultSavePath = dir;
                statDefaultPath.Text = displayPath;

            }
        }

        private void mnuDeleteFile_Click(object sender, System.EventArgs e)
        {
            if (lstComparison.SelectedItems.Count > 0)
            {
                string fullPath = lstComparison.SelectedItems[0].SubItems[5].Text;
                string fileName = Path.GetFileName(fullPath);
                DialogResult result = MessageBox.Show("Are You sure you want to delete " + fileName + "?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
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
            foreach (ListViewItem item in lstComparison.CheckedItems)
            {
                string fullPath = item.SubItems[5].Text;
                try
                {
                    File.Delete(fullPath);
                    item.BackColor = Color.LightGray;
                    item.Checked = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


    }
}
