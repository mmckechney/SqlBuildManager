using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.SqlBuild.DefaultScripts;
using System.IO;
using SqlSync.DbInformation;
namespace SqlSync.SqlBuild.Default_Scripts
{
    public partial class DefaultScriptMaintenanceForm : Form
    {
        DatabaseList dbList;
        DefaultScriptRegistry registry = null;
        string executablePath;
        string defaultScriptPath;
        string defaultScriptXmlFile;
        private DefaultScriptMaintenanceForm()
        {
            InitializeComponent();
        }

        public DefaultScriptMaintenanceForm(DatabaseList dbList) :this()
        {
            this.dbList = dbList;
        }
        private void DefaultScriptMaintenanceForm_Load(object sender, EventArgs e)
        {

            this.executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.defaultScriptPath = Path.Combine(executablePath ,"Default Scripts");
            this.defaultScriptXmlFile = Path.Combine(defaultScriptPath ,"DefaultScriptRegistry.xml");

            this.lnkScriptPath.Text = this.defaultScriptPath;
            
            this.scriptConfigCtrl1.DatabaseList = this.dbList;
            this.scriptConfigCtrl1.DataChanged += new EventHandler(scriptConfigCtrl1_DataChanged);

            if (File.Exists(defaultScriptXmlFile) == false)
            {
                DefaultScriptRegistry reg = new DefaultScriptRegistry();

            }

            using (StreamReader sr = new StreamReader(defaultScriptXmlFile))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(DefaultScripts.DefaultScriptRegistry));
                object obj = serializer.Deserialize(sr);
                this.registry = (DefaultScripts.DefaultScriptRegistry)obj;
                sr.Close();
            }

           if(registry == null)
               registry = new DefaultScriptRegistry();

           if (this.registry.Items == null)
               this.registry.Items = new DefaultScript[0];

           this.BindListView();
            
        }

        void scriptConfigCtrl1_DataChanged(object sender, EventArgs e)
        {
            this.btnSave.Text = "Save Changes";
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            scriptConfigCtrl1.UpdateDefaultScriptValues();

            if (lstScripts.SelectedItems.Count == 0)
                return;


            foreach (ListViewItem item in lstScripts.Items)
            {
                if (item.Selected == false)
                    item.BackColor = Color.White;
              
            }
            DefaultScript script = (DefaultScript)lstScripts.SelectedItems[0].Tag;
            lstScripts.SelectedItems[0].BackColor = Color.LightGray;
            scriptConfigCtrl1.SetDefaultScriptData(ref script);
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                progBar.Style = ProgressBarStyle.Marquee;
                statGeneral.Text = "Copying script to default script folder...";
                bgLoadScript.RunWorkerAsync(openFileDialog1.FileName);
            }
        }
        private bool CopyScript(string fileName)
        {
            try
            {
                string shortName = Path.GetFileName(fileName);
                File.Copy(fileName, Path.Combine(this.defaultScriptPath, shortName), true);
                return true;
            }
            catch
            {
                return false;
            }

        }
        private void BindListView()
        {
            lstScripts.Items.Clear();
            if (this.registry.Items == null)
                return;
            foreach (DefaultScript script in registry.Items)
            {
                ListViewItem item = new ListViewItem(new string[] { script.ScriptName, script.Description });
                item.Tag = script;
                lstScripts.Items.Add(item);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.btnSave.Text == "Save Changes")
            {
                if (this.scriptConfigCtrl1.ValidateValues("",""))
                {
                    this.scriptConfigCtrl1.UpdateDefaultScriptValues();
                    this.SaveDefaultRegistry();
                    this.BindListView();
                    this.btnSave.Text = "Close";
                    this.statGeneral.Text = "Configuration saved.";
                }
            }
            else
            {
                this.Close();
            }
        }
        private void SaveDefaultRegistry()
        {


            using (StreamWriter sw = new StreamWriter(defaultScriptXmlFile, false))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(DefaultScripts.DefaultScriptRegistry));
                serializer.Serialize(sw, this.registry);
                sw.Close();
            }

        }

        private void lnkScriptPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", lnkScriptPath.Text);
        }

        private void bgLoadScript_DoWork(object sender, DoWorkEventArgs e)
        {
            
            string fileName = e.Argument.ToString();
            e.Result = fileName;
            if (!CopyScript(fileName))
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void bgLoadScript_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                MessageBox.Show("Unable to copy selected script to Default Script directory", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                
                string fileName = e.Result.ToString();
                DefaultScript script = new DefaultScript();
                script.ScriptName = Path.GetFileName(fileName);
                List<DefaultScript> tmpLst = new List<DefaultScript>(this.registry.Items);
                tmpLst.Add(script);
                this.registry.Items = tmpLst.ToArray();
                BindListView();
                lstScripts.Items[lstScripts.Items.Count - 1].BackColor = Color.LightGray;

                scriptConfigCtrl1.SetDefaultScriptData(ref script);

                progBar.Style = ProgressBarStyle.Blocks;
                statGeneral.Text = "Script copied, ready to configure and save.";

                this.btnSave.Text = "Save Changes";
            }
        }

        private void deleteDefaultScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstScripts.SelectedItems.Count == 0 || this.registry.Items == null) 
                return;

            DefaultScript script = (DefaultScript)lstScripts.SelectedItems[0].Tag;
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to delete '" + script.ScriptName + "' from the default script registry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                List<DefaultScript> tmp = new List<DefaultScript>(this.registry.Items);
                bool success = tmp.Remove(script);
                this.registry.Items = tmp.ToArray();
                this.SaveDefaultRegistry();
                this.BindListView();
            }
        }
    }
}
