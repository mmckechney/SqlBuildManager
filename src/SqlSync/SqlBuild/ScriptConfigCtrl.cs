using SqlBuildManager.Enterprise;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlBuildManager.ScriptHandling;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.SqlBuild.Validator;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    public partial class ScriptConfigCtrl : UserControl
    {
        List<string> tagList = new List<string>();
        private Color highlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(180)))));
        int collapsedSize = 27;
        int fullSize = 111;
        bool showFull = false;
        bool hasChanged = false;
        bool tagRequired = false;
        private SqlSyncBuildData.ScriptRow scriptConfig = null;

        private string parentFileName = string.Empty;
        public string ParentFileName
        {
            set
            {
                parentFileName = value;
            }
        }
        private DefaultScripts.DefaultScript defaultScript = null;
        private DatabaseList databaseList;

        public DatabaseList DatabaseList
        {
            get { return databaseList; }
            set { databaseList = value; }
        }

        public void ShowHighlightColor()
        {
            ddDatabaseList.BackColor = highlightColor;
        }
        public void HideToggleImage()
        {
            picToggle.Visible = false;
        }
        public bool HasChanged
        {
            get { return hasChanged; }
            set { hasChanged = value; }
        }
        private bool buildSequenceChanged = false;

        public bool BuildSequenceChanged
        {
            get { return buildSequenceChanged; }
            set { buildSequenceChanged = value; }
        }
        public bool ShowFull
        {
            get { return showFull; }
            set
            {
                showFull = value;
                picToggle_Click(null, EventArgs.Empty);
            }
        }

        public string SelectedDatabase
        {
            get
            {
                return ddDatabaseList.SelectedDatabase;
            }
        }
        public ScriptConfigCtrl()
        {
            InitializeComponent();

            if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout > 0)
                txtScriptTimeout.Text = SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout.ToString();

            ddInfer.SelectedIndex = 0;


        }
        //public ScriptConfigCtrl(ref SqlSyncBuildData.ScriptRow scriptConfig, string[] databaseList)
        //{
        //    InitializeComponent();
        //    this.scriptConfig = scriptConfig;
        //    this.databaseList = databaseList;
        //    SetUp();
        //}
        public void SetConfigData(ref SqlSyncBuildData.ScriptRow scriptConfig, DatabaseList databaseList, List<string> tagList, bool tagRequired)
        {
            this.scriptConfig = scriptConfig;
            this.databaseList = databaseList;
            this.tagList = tagList;
            this.tagRequired = tagRequired;
            SetUp();
        }
        public void SetDefaultScriptData(ref DefaultScripts.DefaultScript defaultScript)
        {
            tagRequired = false;
            this.defaultScript = defaultScript;
            SetUp();

        }
        private void SetUp()
        {

            string dbName = (scriptConfig != null) ? scriptConfig.Database : string.Empty;
            ddDatabaseList.SetData(databaseList, dbName);

            for (int i = 0; i < tagList.Count; i++)
            {
                cbTag.Items.Add(tagList[i]);
                if (scriptConfig != null && tagList[i].Trim().ToLower() == scriptConfig.Tag.Trim().ToLower()) cbTag.SelectedIndex = i;
            }
            if (scriptConfig != null)
            {
                txtBuildOrder.Text = scriptConfig.BuildOrder.ToString();
                txtScriptTimeout.Text = scriptConfig.ScriptTimeOut.ToString();
                if (!scriptConfig.IsDescriptionNull())
                    rtbDescription.Text = scriptConfig.Description;

                chkAllowMultipleRuns.Checked = scriptConfig.AllowMultipleRuns;
                chkRollBackBuild.Checked = scriptConfig.CausesBuildFailure;
                chkRollBackScript.Checked = scriptConfig.RollBackOnError;
                if (scriptConfig.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                    scriptConfig.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                    scriptConfig.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                {
                    chkStripTransactions.Checked = false;
                    chkStripTransactions.Enabled = false;
                    toolTip1.SetToolTip(chkStripTransactions, "Stored Procedures,  Functions, and Triggers do not allow stripping of transactions");
                    if (scriptConfig.StripTransactionText == true)
                    {
                        scriptConfig.StripTransactionText = false;
                        hasChanged = true;
                    }
                }
                else
                    chkStripTransactions.Checked = scriptConfig.StripTransactionText;
            }

            if (defaultScript != null)
            {
                ddDatabaseList.SetData(databaseList, defaultScript.DatabaseName);

                txtBuildOrder.Text = defaultScript.BuildOrder.ToString();
                rtbDescription.Text = defaultScript.Description;

                chkAllowMultipleRuns.Checked = defaultScript.AllowMultipleRuns;
                chkRollBackBuild.Checked = defaultScript.RollBackBuild;
                chkRollBackScript.Checked = defaultScript.RollBackScript;
                chkStripTransactions.Checked = defaultScript.StripTransactions;
                cbTag.Text = defaultScript.ScriptTag;

                //txtScriptTimeout.Enabled = false;

            }


            cbTag.SelectedValueChanged += new EventHandler(SetDirtyFlag);
            cbTag.TextChanged += new EventHandler(SetDirtyFlag);
            ddDatabaseList.SelectionChangeCommitted += new EventHandler(SetDirtyFlag);
            rtbDescription.TextChanged += new System.EventHandler(SetDirtyFlag);
            chkRollBackBuild.CheckedChanged += new System.EventHandler(SetDirtyFlag);
            chkRollBackScript.CheckedChanged += new System.EventHandler(SetDirtyFlag);
            chkStripTransactions.CheckedChanged += new System.EventHandler(SetDirtyFlag);
            chkAllowMultipleRuns.CheckedChanged += new System.EventHandler(SetDirtyFlag);
            txtBuildOrder.TextChanged += new System.EventHandler(SetDirtyFlag);
            txtScriptTimeout.TextChanged += new System.EventHandler(SetDirtyFlag);

            int minValue = EnterpriseConfigHelper.GetMinumumScriptTimeout(parentFileName, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
            int val;
            if (int.TryParse(txtScriptTimeout.Text, out val))
            {
                if (val < minValue)
                    txtScriptTimeout_Leave(null, EventArgs.Empty);
            }




        }

        public void UpdateDefaultScriptValues()
        {
            if (defaultScript == null)
                return;

            defaultScript.DatabaseName = ddDatabaseList.SelectedDatabase;
            defaultScript.BuildOrder = Int32.Parse(txtBuildOrder.Text);
            defaultScript.Description = rtbDescription.Text;

            defaultScript.AllowMultipleRuns = chkAllowMultipleRuns.Checked;
            defaultScript.RollBackBuild = chkRollBackBuild.Checked;
            defaultScript.RollBackScript = chkRollBackScript.Checked;
            defaultScript.StripTransactions = chkStripTransactions.Checked;
            defaultScript.ScriptTag = (cbTag.Text != null) ? cbTag.Text : "";
        }
        public void UpdateScriptConfigValues()
        {
            //TODO: use override method
            scriptConfig.Database = ddDatabaseList.SelectedDatabase;
            scriptConfig.BuildOrder = double.Parse(txtBuildOrder.Text);
            scriptConfig.ScriptTimeOut = int.Parse(txtScriptTimeout.Text);
            scriptConfig.Description = rtbDescription.Text;
            scriptConfig.Tag = cbTag.SelectedValue == null ? cbTag.Text : cbTag.SelectedValue.ToString();

            scriptConfig.AllowMultipleRuns = chkAllowMultipleRuns.Checked;
            scriptConfig.CausesBuildFailure = chkRollBackBuild.Checked;
            scriptConfig.RollBackOnError = chkRollBackScript.Checked;
            scriptConfig.StripTransactionText = chkStripTransactions.Checked;
        }
        private void picToggle_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.PictureBox)
                showFull = !showFull;

            if (showFull)
            {
                Height = fullSize;
                picToggle.Image = global::SqlSync.Properties.Resources.downarrow_white;
            }
            else
            {
                Height = collapsedSize;
                picToggle.Image = global::SqlSync.Properties.Resources.uparrow_white;
            }


        }


        #region Change Event Handlers
        private void SetDirtyFlag(object sender, EventArgs e)
        {
            hasChanged = true;
            if (sender is TextBox && ((TextBox)sender).Equals(txtBuildOrder))
                buildSequenceChanged = true;

            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);

        }
        #endregion


        private void txtScriptTimeout_Leave(object sender, System.EventArgs e)
        {
            int minValue = EnterpriseConfigHelper.GetMinumumScriptTimeout(parentFileName, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
            ScriptTimeoutValidationResult result = ScriptSettingValidation.CheckScriptTimeoutValue(txtScriptTimeout.Text, minValue);
            switch (result)
            {
                case ScriptTimeoutValidationResult.TimeOutTooSmall:
                    string message = string.Format("The script timeout setting was smaller than the minimum setting of {0} seconds. The value has been increased accordingly.", minValue.ToString());
                    txtScriptTimeout.Text = minValue.ToString();
                    MessageBox.Show(message, "Timeout value too small", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptTimeout.Focus();

                    break;
                case ScriptTimeoutValidationResult.NonIntegerValue:
                    MessageBox.Show("The Script Timeout value must be a valid 32 bit integer", "Bad Timeout value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptTimeout.Focus();
                    break;
            }

        }

        private void chkStripTransactions_CheckedChanged(object sender, EventArgs e)
        {
            if (scriptConfig == null)
                return;

            if (scriptConfig.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                scriptConfig.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                scriptConfig.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                return;
            if (chkStripTransactions.Checked == false)
            {
                DialogResult result = MessageBox.Show("Are you certain you want to leave transaction references?\r\nGenerally, you should let Sql Build Manager handle them", "Leave References", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    chkStripTransactions.Checked = true;
                }
            }
        }
        private void chkRollBackBuild_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRollBackBuild.Checked)
            {
                chkRollBackScript.Checked = true;
                chkRollBackScript.Enabled = false;
            }
            else
            {
                chkRollBackScript.Enabled = true;
            }
        }

        public bool ValidateValues(string scriptContents, string scriptName)
        {
            int minValue = EnterpriseConfigHelper.GetMinumumScriptTimeout(parentFileName, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
            if (ScriptSettingValidation.CheckScriptTimeoutValue(txtScriptTimeout.Text, minValue)
                != ScriptTimeoutValidationResult.Ok)
            {
                txtScriptTimeout_Leave(null, EventArgs.Empty);
                return false;
            }

            bool showMessage = false;
            if (txtBuildOrder.Enabled == true)
            {
                if (txtBuildOrder.Text.Length != 0)
                {
                    try
                    {
                        Double.Parse(txtBuildOrder.Text);
                    }
                    catch
                    {
                        showMessage = true;
                    }
                }
                else
                {
                    showMessage = true;
                }
            }

            if (ddDatabaseList.Enabled == true)
                if (ddDatabaseList.SelectedDatabase.Length == 0)
                    showMessage = true;

            if (showMessage)
            {
                MessageBox.Show("A Target Database and Build Sequence number are required", "Missing Values", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (tagRequired && cbTag.Text.Length == 0 && cbTag.SelectedValue == null)
            {
                bool alertOnTag = true;
                if (ddInfer.Text != null && ddInfer.Text.ToString() != "None")
                {
                    TagInferenceSource source;
                    switch (ddInfer.Text)
                    {

                        case "Name/Script":
                            source = TagInferenceSource.NameOverText;
                            break;
                        case "Script Only":
                            source = TagInferenceSource.ScriptText;
                            break;
                        case "Name Only":
                            source = TagInferenceSource.ScriptName;
                            break;
                        case "Script/Name":
                        default:
                            source = TagInferenceSource.TextOverName;
                            break;
                    }
                    List<string> regex = new List<string>(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());
                    string tag = ScriptTagProcessing.InferScriptTag(scriptName, scriptContents, regex, source);
                    if (tag.Length > 0)
                    {
                        cbTag.Text = tag;
                        alertOnTag = false;
                    }
                }

                if (alertOnTag)
                {
                    string message = "A Target Database, Build Sequence number and Tag are required";

                    if (SqlSync.Properties.Settings.Default.RequireScriptTagsMessage != null)
                        message = SqlSync.Properties.Settings.Default.RequireScriptTagsMessage;

                    MessageBox.Show(message, "Missing Tag Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void txtBuildOrder_Leave(object sender, EventArgs e)
        {
            double rslt;
            if (!Double.TryParse(txtBuildOrder.Text, out rslt))
            {
                MessageBox.Show("The build order value must be a valid decimal value", "Incorrect Input", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void databaseDropDown1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public event EventHandler DataChanged;
    }
}
