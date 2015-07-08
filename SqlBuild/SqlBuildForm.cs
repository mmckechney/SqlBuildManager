using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using SqlBuildManager.Enterprise;
using SqlBuildManager.Enterprise.ActiveDirectory;
using SqlBuildManager.Enterprise.Feature;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlBuildManager.Interfaces.SourceControl;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.ScriptHandling;
using SqlSync.BuildHistory;
using SqlSync.Compare;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.Controls.OAKControls;
using SqlSync.DbInformation;
using SqlSync.MRU;
using SqlSync.ObjectScript;
using SqlSync.SqlBuild.CodeTable;
using SqlSync.SqlBuild.DefaultScripts;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Remote;
using SqlSync.SqlBuild.Status;
using SqlSync.TableScript;
using SqlSync.Validator;
using SqlBuildManager.Enterprise.CodeReview;
namespace SqlSync.SqlBuild

{
    /// <summary>
    /// Summary description for SqlBuildForm.
    /// </summary>
    public class SqlBuildForm : System.Windows.Forms.Form, IMRUClient
    {
        bool projectIsUnderSourceControl = false;
        private List<string> codeReviewDbaMembers = new List<string>();
        internal bool ProjectIsUnderSourceControl
        {
            get { return projectIsUnderSourceControl; }
            set
            {
                projectIsUnderSourceControl = value;
                if (projectIsUnderSourceControl)
                {
                    this.statControlStatusLabel.Text = "Yes";
                    this.statControlStatusLabel.ForeColor = Color.Green;

                }
                else
                {
                    this.statControlStatusLabel.Text = "No";
                    this.statControlStatusLabel.ForeColor = Color.Red;
                }
            }
        }
        Package currentViolations = new Package();
        private bool scriptPkWithTables = true;
        List<string> adGroupMemberships = new List<string>();
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        List<string> scriptsRequiringBuildDescription = new List<string>();
        private string sbxBuildControlFileName = string.Empty;
        private string externalScriptLogFileName = string.Empty;
        #region Variables and Fields
        private Color colorReadOnlyFile;
        List<string> databasesUsed = new List<string>();
        private bool createSqlRunLogFile = false;
        private bool runPolicyCheckingOnLoad = false;
        private int buildDuration = 0;
        private int scriptDuration = 0;
        private List<string> tagList = new List<string>();
        private const string selectDatabaseString = "<< Select Active Database >>";
        private SqlSync.ColumnSorter listSorter = new ColumnSorter();
        private MRUManager mruManager;
        private System.Security.Principal.WindowsImpersonationContext impersonatedUser = null;


        Color colorMultipleRun;
        Color colorLeaveTrans;
        Color colorBoth;
        Color colorSkipped;

        /// <summary>
        /// Flag for unattended running of app.
        /// </summary>
        private bool runningUnattended = false;
        /// <summary>
        /// Used for unattended mode
        /// </summary>
        private int returnCode = -1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Utility.SqlSyncUtilityRegistry utilityRegistry = null;
        private string autoXmlFile = string.Empty;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.OpenFileDialog openSbmFileDialog;
        private string projectFileName = string.Empty;
        private string projectFilePath = string.Empty;
        private string workingDirectory = string.Empty;
        private string buildZipFileName = string.Empty;
        ConnectionData connData = null;
        private System.Windows.Forms.ColumnHeader colScriptFile;
        private System.Windows.Forms.ColumnHeader colSequence;
        private System.Windows.Forms.ToolStripMenuItem mnuAddScript;
        private System.Windows.Forms.ToolStripMenuItem mnuRemoveScriptFile;
        private System.Windows.Forms.OpenFileDialog dlgAddScriptFile;
        private System.Windows.Forms.ContextMenuStrip ctxScriptFile;
        //private System.Windows.Forms.ListView lstScriptFiles;
        private SqlSync.Controls.OAKControls.OAKListView lstScriptFiles;
        private System.Windows.Forms.ToolStripMenuItem mnuEditScriptFile;
        SqlSyncBuildData buildData = null;
        private System.Windows.Forms.ColumnHeader colDatabaseName;
        private System.Windows.Forms.GroupBox grbBuildScripts;
        private System.Windows.Forms.GroupBox grpManager;
        private System.Windows.Forms.LinkLabel lnkStartBuild;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ListView lstBuild;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ContextMenuStrip ctxResults;
        private System.Windows.Forms.ToolStripMenuItem mnuDisplayRowResults;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenRunScriptFile;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateExportFile;
        private System.Windows.Forms.SaveFileDialog saveExportFile;
        private System.Windows.Forms.OpenFileDialog openScriptExportFile;
        private System.Windows.Forms.MenuStrip mainMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuActionMain;
        private System.Windows.Forms.ToolStripMenuItem mnuLoadProject;
        private System.Windows.Forms.ToolStripMenuItem mnuChangeSqlServer;
        private System.Windows.Forms.ToolStripMenuItem mnuImportScriptFromFile;
        private System.Windows.Forms.ToolStripMenuItem mnuExportBuildList;
        private System.Windows.Forms.ToolStripMenuItem mnuAddSqlScriptText;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFile;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFromResults;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ToolStripMenuItem mnuBulkAdd;
        private System.Windows.Forms.OpenFileDialog openFileBulkLoad;
        private System.Windows.Forms.ToolStripMenuItem mnuFindScript;

        private string searchText = string.Empty;
        private int searchStartIndex = 0;
        private System.Windows.Forms.ToolStripMenuItem menuItem9;
        private System.Windows.Forms.ToolStripMenuItem mnuRenumberSequence;
        private System.Windows.Forms.ToolStripMenuItem mnuListTop;
        private System.Windows.Forms.ToolStripMenuItem mnuBulkFromFile;
        private System.Windows.Forms.ToolStripMenuItem mnuTryScript;
        private System.Windows.Forms.ToolStripMenuItem mnuRunScript;
        private System.Windows.Forms.ToolStripMenuItem mnuExportBuildListToFile;
        private System.Windows.Forms.ToolStripMenuItem mnuExportBuildListToClipBoard;
        private System.Windows.Forms.ToolStripMenuItem mnuUpdatePopulate;
        private System.Windows.Forms.ToolStripMenuItem mnuBulkFromList;
        private System.Windows.Forms.ToolStripMenuItem mnuClearPreviouslyRunBlocks;
        private System.Windows.Forms.ToolStripMenuItem mnuShowBuildLogs;
        private System.Windows.Forms.ToolStripMenuItem mnuScriptToLogFile;
        private System.Windows.Forms.ToolStripMenuItem mnuShowBuildHistory;
        private System.Windows.Forms.ToolStripMenuItem mnuLogging;
        DbInformation.DatabaseList databaseList;


        private System.Windows.Forms.ToolStripMenuItem mnuResortByFileType;
        private System.Windows.Forms.TextBox txtStartIndex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem mnuObjectUpdates;
        private SqlSync.SettingsControl settingsControl1;
        private System.Windows.Forms.Panel pnlManager;
        private System.Windows.Forms.Panel pnlBuildScripts;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddBuildType;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ToolStripMenuItem mnuFileMRU;
        private System.Windows.Forms.ColumnHeader colScriptId;
        private System.Windows.Forms.ColumnHeader colScriptSize;
        private System.Windows.Forms.ToolStripMenuItem mnuAddObjectCreate;
        private System.Windows.Forms.ToolStripMenuItem mnuAddStoredProcs;
        private System.Windows.Forms.ToolStripMenuItem mnuAddFunctions;
        private System.Windows.Forms.ToolStripMenuItem mnuAddViews;
        private System.Windows.Forms.ToolStripMenuItem mnuAddTables;
        private string activeDatabase = string.Empty;
        private System.Windows.Forms.ToolStripMenuItem mnuScripting;
        private System.Windows.Forms.ToolStripMenuItem mnuAddCodeTablePop;
        private System.Windows.Forms.ToolStripMenuItem menuItem16;
        private System.Windows.Forms.ToolStripMenuItem mnuSchemaScripting;
        private System.Windows.Forms.ToolStripMenuItem mnuCodeTableScripting;
        private System.Windows.Forms.ToolStripMenuItem mnuDataExtraction;
        private AutoScriptList autoScriptListRegistration = null;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoScripting;
        private System.Windows.Forms.OpenFileDialog openFileAutoScript;
        public System.Windows.Forms.ToolStripMenuItem mnuDatabaseSize;
        private System.Windows.Forms.ToolStripMenuItem mnuDataAuditScripting;
        private System.Windows.Forms.ImageList imageListBuildScripts;
        private System.Windows.Forms.ColumnHeader colImage;
        private System.Windows.Forms.CheckBox chkScriptChanges;
        private System.Windows.Forms.ToolStripMenuItem mnuUpdatePopulates;
        private System.Windows.Forms.ToolStripMenuItem mnuUpdateObjectScripts;
        private System.Windows.Forms.ToolStripMenuItem mnuObjectValidation;
        private System.Windows.Forms.ToolStripMenuItem mnuExportScriptText;
        private System.Windows.Forms.ToolStripMenuItem mnuIndividualFiles;
        private System.Windows.Forms.ToolStripMenuItem mnuCombinedFile;
        private System.Windows.Forms.FolderBrowserDialog fdrSaveScripts;
        private System.Windows.Forms.SaveFileDialog saveCombinedScript;
        private System.Windows.Forms.ToolStripMenuItem mnuIncludeUSE;
        private System.Windows.Forms.ToolStripMenuItem mnuIncludeSequence;
        private System.Windows.Forms.ToolStripMenuItem mnuViewRunHistory;
        private BackgroundWorker bgBuildProcess;
        private ToolStripContainer toolStripContainer1;
        private ToolStripContainer toolStripContainer2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator menuItem12;
        private ToolStripSeparator menuItem15;
        private ToolStripSeparator menuItem4;
        private ToolStripSeparator menuItem1;
        private ToolStripSeparator menuItem2;
        private ToolStripSeparator menuItem6;
        private ToolStripSeparator mnuViewRunHistorySep;
        private ToolStripSeparator menuItem8;
        private ToolStripSeparator menuItem10;
        private ToolStripSeparator menuItem13;
        private ToolStripSeparator menuItem3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator menuItem21;
        private ToolStripSeparator menuItem18;
        private ToolStripSeparator menuItem19;
        private ToolStripSeparator menuItem11;
        private ToolStripSeparator menuItem22;
        private ToolStripComboBox mnuDDActiveDatabase;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem rebuildPreviouslyCommitedBuildFileToolStripMenuItem;
        private ToolStripMenuItem mnuCompare;
        private ToolStripMenuItem mnuMainAddSqlScript;
        private ToolStripMenuItem mnuMainAddNewFile;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem mnuCompareObject;
        private ToolStripMenuItem archiveBuildHistoryToolStripMenuItem;
        private ToolStripMenuItem renameScriptFIleToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem scriptTagsRequiredToolStripMenuItem;
        private ColumnHeader colTag;
        private BackgroundWorker bgCheckForUpdates;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statGeneral;
        private ToolStripProgressBar progressBuild;
        private Button btnCancel;
        private ToolStripStatusLabel statBuildTime;
        private ToolStripStatusLabel statScriptTime;
        private System.Windows.Forms.Timer tmrBuild;
        private System.Windows.Forms.Timer tmrScript;
        private BackgroundWorker bgLoadZipFle;
        private BackgroundWorker bgRefreshScriptList;
        private ToolStripMenuItem storedProcedureTestingToolStripMenuItem;
        private ColumnHeader columnHeader13;
        private TextBox txtBuildDesc;
        private ToolStripMenuItem createSQLLogOfBuildRunsToolStripMenuItem;
        private Label label2;
        private TargetDatabaseOverrideCtrl targetDatabaseOverrideCtrl1;
        private ToolTip toolTip1;
        private GroupBox grpBuildResults;
        private ToolStripMenuItem maintainManualDatabaseEntriesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private CheckBox chkUpdateOnOverride;
        private ColumnHeader colDateAdded;
        private ColumnHeader colDateModified;
        private Button btnSlideBuildScripts;
        private ImageList imageListSlide;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem iconLegendToolStripMenuItem;
        private ToolStripMenuItem scriptNotRunOnCurrentServerToolStripMenuItem;
        private ToolStripMenuItem alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem;
        private ToolStripMenuItem runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem;
        private ToolStripMenuItem runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem;
        private ToolStripMenuItem runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem;
        private ToolStripMenuItem backgrounLegendToolStripMenuItem;
        private ToolStripMenuItem allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem;
        private ToolStripMenuItem leaveTransactionTextInScriptsdontStripOutToolStripMenuItem;
        private ToolStripMenuItem allowMultipleRunsOfScriptOnSameServerToolStripMenuItem;
        private ToolStripMenuItem scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem;
        private BackgroundWorker bgDatabaseRoutine;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem runOnCurrentServerdatabaseCombinationAndToolStripMenuItem;
        private ToolStripMenuItem mnuObjectScripts_FileDefault;
        private ToolStripMenuItem mnuObjectScripts_CurrentSettings;
        private ToolStripMenuItem notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem;
        private ToolStripMenuItem serverVersionIsNewerThanSBMVersionToolStripMenuItem;
        private BackgroundWorker bgObjectScripting;
        private BackgroundWorker bgGetObjectList;
        private ToolStripMenuItem maintainDefaultScriptRegistryToolStripMenuItem;
        private SplitContainer splitContainer1;
        private ToolStripMenuItem startConfigureMultiServerDatabaseRunToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem projectSiteToolStripMenuItem;

        DataView committedScriptView = null;
        private ToolStripMenuItem createToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private OpenFileDialog openFileDataExtract;
        private ToolStripMenuItem loadNewDirectoryControlFilesbxToolStripMenuItem;
        private OpenFileDialog openSbxFileDialog;
        private ToolStripMenuItem fileMissingToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem packageScriptsIntoProjectFilesbmToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private SaveFileDialog saveScriptsToPackage;
        private ToolStripMenuItem scriptingOptionsToolStripMenuItem;
        private ToolStripMenuItem scriptALTERAndCREATEToolStripMenuItem;
        private ToolStripMenuItem includeObjectPermissionsToolStripMenuItem1;
        private ToolStripMenuItem scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem makeFileWriteableremoveToolStripMenuItem;
        private ToolStripMenuItem constructCommandLineStringToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem scriptPolicyCheckingToolStripMenuItem;
        private CheckBox chkNotTransactional;
        private Label label5;
        private ComboBox ddOverrideLogDatabase;
        private Label lblAdvanced;
        private GroupBox grpAdvanced;
        private Panel pnlAdvanced;
        private ToolStripMenuItem howToToolStripMenuItem;
        private PictureBox pictureBox1;
        private ToolStripMenuItem remoteExecutionServiceToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripMenuItem calculateScriptPackageHashSignatureToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem defaultScriptTimeoutsecondsToolStripMenuItem;
        private ToolStripTextBox mnuDefaultScriptTimeout;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripSeparator toolStripSeparator17;
        private BackgroundWorker bgEnterpriseSettings;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem createBackoutPackageToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem inferScriptTagFromFileNameToolStripMenuItem;
        //private Controls.ViewLogFileMenuItem viewLogFileMenuItem1;
        //private Controls.SetLoggingLevelMenuItem setLoggingLevelMenuItem1;
        private ToolStripMenuItem scriptContentsFirstThenFileNameToolStripMenuItem;
        private ToolStripMenuItem fileNameFirstThenSciptContentsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripMenuItem scriptContentsOnlyToolStripMenuItem;
        private ToolStripMenuItem fileNamesOnlyToolStripMenuItem;
        private BackgroundWorker bgBulkAdd;
        private ToolStripMenuItem mnuAddTriggers;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripMenuItem mnuAddRoles;
        private Controls.ViewLogFileMenuItem viewLogFileMenuItem2;
        private Controls.SetLoggingLevelMenuItem setLoggingLevelMenuItem2;
        private ToolStripStatusLabel statScriptCount;
        private ToolStripMenuItem showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem;
        private ToolStripMenuItem runPolicyCheckingonloadtoolStripMenuItem;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripMenuItem policyCheckIconHelpToolStripMenuItem;
        private ToolStripMenuItem policyCheckFailedActionRequiredToolStripMenuItem;
        private ToolStripMenuItem policyChecksNotRunToolStripMenuItem;
        private ToolStripMenuItem passesPolicyChecksToolStripMenuItem;
        private ColumnHeader colPolicyIcon;
        private BackgroundWorker bgPolicyCheck;
        private ToolStripMenuItem scriptPrimaryKeyWithTableToolStripMenuItem;
        private ToolStripMenuItem runPolicyChecksToolStripMenuItem;
        private ToolStripMenuItem savePolicyResultsInCSVToolStripMenuItem;
        private SaveFileDialog savePolicyViolationCsv;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem savePolicyResultsAsXMLToolStripMenuItem;
        private ToolStripMenuItem sourceControlServerURLToolStripMenuItem;
        private ToolStripTextBox sourceControlServerURLTextboxMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel statControlStatusLabel;
        private BackgroundWorker bgBulkAddStep2;
        private ToolStripMenuItem policyWarningActionMayBeRequiredToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator23;
        private ColumnHeader colCodeReviewIcon;
        private ToolStripMenuItem codeReviewIconToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem statusHelpToolStripMenuItem1;
        private ToolStripMenuItem reviewNotStartedToolStripMenuItem;
        private ToolStripMenuItem statusHelpToolStripMenuItem;
        private ToolStripMenuItem reviewInProgressToolStripMenuItem;
        private ToolStripMenuItem reviewAcceptedToolStripMenuItem;
        private ToolStripMenuItem reviewAcceptedByDBAToolStripMenuItem;
        private BackgroundWorker bgCodeReview;
        private ToolTip toolTip2;
        private ToolStripMenuItem waitingOnStatusCheckToolStripMenuItem;
        private ToolStripMenuItem reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem;
        private MultiDbData multiDbRunData = null;
        #endregion
        public SqlBuildForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            
        }
        public SqlBuildForm(ConnectionData data)
            : this()
        {
            this.connData = data;
        }
        public SqlBuildForm(string buildZipFileName)
            : this()
        {
            if (Path.GetExtension(buildZipFileName).ToLower() == ".sbx")
            {
                this.sbxBuildControlFileName = buildZipFileName;
            }
            else
            {
                this.buildZipFileName = buildZipFileName;
            }
           
        }
        public SqlBuildForm(string buildZipFileName, string serverName,string scriptLogFileName)
            : this()
        {
            this.buildZipFileName = buildZipFileName;
            this.connData = new ConnectionData();
            this.connData.SQLServerName = serverName;
            this.connData.UseWindowAuthentication = true;

            if (scriptLogFileName.Length > 0)
            {
                this.externalScriptLogFileName = scriptLogFileName;
                this.createSqlRunLogFile = true;
            }

            this.runningUnattended = true;
        }
        public SqlBuildForm(string buildZipFileName, ConnectionData data)
            : this()
        {
            this.buildZipFileName = buildZipFileName;
            this.connData = data;
        }
        public SqlBuildForm(string buildZipFileName, MultiDb.MultiDbData multiDbData, string scriptLogFileName) : this()
        {
            //Initalize the multiDbData object here. Final initializtion will happen just before the build is run.
            //The build will get kicked off after the build file is loaded in the "bgLoadZipFle_RunWorkerCompleted" method
            this.multiDbRunData = multiDbData;
            this.multiDbRunData.BuildFileName = buildZipFileName;

            if (scriptLogFileName.Length > 0)
            {
                this.externalScriptLogFileName = scriptLogFileName;
                this.createSqlRunLogFile = true;
            }

            this.buildZipFileName = buildZipFileName;
            
            this.runningUnattended = true;
            this.connData = new ConnectionData(); //initialize, but don't need to populate it...
            this.connData.UseWindowAuthentication = true;
        }
        private void SqlBuildForm_Load(object sender, System.EventArgs e)
        {
            bgEnterpriseSettings.RunWorkerAsync();


            try
            {
                //If no connection, will need to pop up the connection dialog. 
                if (this.connData == null)
                {
                    ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
                    DialogResult result = frmConnect.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        this.connData = frmConnect.SqlConnection;
                        this.databaseList = frmConnect.DatabaseList;
                    }
                    else
                    {
                        MessageBox.Show("Sql Build manager can not continue without a valid SQL Server Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        this.Close();
                        Application.Exit();
                        return;
                    }
                }

                this.InitMRU();

                //Set the color coding for the script list...

                this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptDontStripTransactions;
                this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptAllowMultipleRuns;
                this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptWillBeSkippedMarkedAsRunOnce;
                this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptAllowMultipleRunsAndLeaveTransactions;
                this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptReadOnly;

                this.colorLeaveTrans = this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor;
                this.colorMultipleRun = this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor;
                this.colorSkipped = this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor;
                this.colorBoth = this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor;
                this.colorReadOnlyFile = this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor;

                if (SqlSync.Properties.Settings.Default.Description == null) SqlSync.Properties.Settings.Default.Description = new AutoCompleteStringCollection();
                txtBuildDesc.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.Description;

                try
                {
                    scriptALTERAndCREATEToolStripMenuItem.Checked = SqlSync.Properties.Settings.Default.ScriptAsAlter;
                    includeObjectPermissionsToolStripMenuItem1.Checked = SqlSync.Properties.Settings.Default.ScriptPermissions;
                }
                catch
                {
                }


                //If the database list isn't initialized yet , go and grab it
                if (this.databaseList == null)
                    this.databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);


                //If the connection object is initalized, which it should be, set the current server and bind the build type dropdown box
                if (connData != null)
                {
                    this.settingsControl1.Server = this.connData.SQLServerName;
                    DatabindBuildType();
                }

                if (PassesReadOnlyProjectFileCheck())
                {

                    //If we have a build file name already on Load, go ahead an load it up via asyc method. This will get the form to the user quicker for better response.
                    //If this is an unattended/non-interactive build, processing will get kicked off in the "RunWorkerCompleted" method of the async call
                    if (!String.IsNullOrEmpty(this.buildZipFileName))
                    {
                        this.bgLoadZipFle.RunWorkerAsync(this.buildZipFileName);
                    }
                    else if (!String.IsNullOrEmpty(this.sbxBuildControlFileName))
                    {
                        LoadXmlControlFile(this.sbxBuildControlFileName);
                    }
                }


                //Show the form, but minimized if non-interactive.
                if (this.runningUnattended)
                    this.WindowState = FormWindowState.Minimized;
                this.Show();

                //Retrieve the helper queries.
                GenerateUtiltityItems();

                //Get auto scripting configurations
                GenerateAutoScriptList();


               
                //If not running in an unattended mode, check for updates via an asyc call.
                if (!this.runningUnattended)
                    bgCheckForUpdates.RunWorkerAsync(false);

            }
            catch(Exception exe)
            {
                log.ErrorFormat("Sorry...There was an error loading. Returing with code 998.", exe);

                //Any errors?... Close out with a non-Zero exit code
                if (this.UnattendedProcessingCompleteEvent != null)
                    this.UnattendedProcessingCompleteEvent(998);
            }

            

            //Set the database list as found in the main menu "Scripting" selection
            this.SetDatabaseMenuList();

            

            
        }
        private void bgEnterpriseSettings_DoWork(object sender, DoWorkEventArgs e)
        {
            bool tagsRequiredEnabled, defaultScriptEnabled, remoteExecutionEnabled, policyCheckOnLoadEnabled, scriptPkWithTable;
            SetPropertiesFromEnterpriseConfiguration(out tagsRequiredEnabled, out defaultScriptEnabled, out remoteExecutionEnabled, out policyCheckOnLoadEnabled, out scriptPkWithTable);
            e.Result = new List<bool>(new bool[] { tagsRequiredEnabled, defaultScriptEnabled, remoteExecutionEnabled, policyCheckOnLoadEnabled, scriptPkWithTable });
        }

        private void bgEnterpriseSettings_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is List<bool>)
            {
                List<bool> x = (List<bool>)e.Result;
                scriptTagsRequiredToolStripMenuItem.Enabled = x[0];
                maintainDefaultScriptRegistryToolStripMenuItem.Enabled = x[1];
                remoteExecutionServiceToolStripMenuItem.Enabled = x[2];
                runPolicyCheckingonloadtoolStripMenuItem.Enabled = x[3];
                if (!x[4])
                {
                    scriptPrimaryKeyWithTableToolStripMenuItem.Enabled = x[4];
                    scriptPrimaryKeyWithTableToolStripMenuItem.Checked = false;
                }
                else
                {
                    scriptPrimaryKeyWithTableToolStripMenuItem.Checked = true;
                }

            }

            if (!this.runPolicyCheckingOnLoad)
            {
                this.lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 0;
                this.policyCheckIconHelpToolStripMenuItem.Visible = false;
                this.runPolicyCheckingonloadtoolStripMenuItem.Checked = true;
            }
            else
            {
                this.runPolicyCheckingonloadtoolStripMenuItem.Checked = false;
            }

            //Code Review status
            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null)
                EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig = new CodeReviewConfig();

            if (!EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
            {
                this.lstScriptFiles.Columns[(int)ScriptListIndex.CodeReviewStatusIconColumn].Width = 0;
            }
        }
        private void SetPropertiesFromEnterpriseConfiguration(out bool tagsRequiredEnabled, out bool defaultScriptEnabled, out bool remoteExecutionEnabled, out bool policyCheckOnLoadEnabled, out bool scriptPkWithTable)
        {
            scriptPkWithTable = true;

            this.adGroupMemberships = AdHelper.GetGroupMemberships(System.Environment.UserName).ToList();
            if (this.adGroupMemberships.Count == 0)
                log.WarnFormat("No Group Memberships found for {0}", System.Environment.UserName);
            else if (log.IsDebugEnabled)
                log.DebugFormat("Group memberships for {0}: {1}", System.Environment.UserName, String.Join("; ", adGroupMemberships.ToArray()));


            tagsRequiredEnabled = true;
            defaultScriptEnabled = true;
            bool saveSettings = false;
            EnterpriseConfiguration entConfig = EnterpriseConfigHelper.EnterpriseConfig;

            //Get the Feature control sets..
            if (entConfig.FeatureAccess != null)
                remoteExecutionEnabled = FeatureAccessHelper.IsFeatureEnabled(FeatureKey.RemoteExecution, System.Environment.UserName, this.adGroupMemberships, entConfig.FeatureAccess);
            else
                remoteExecutionEnabled = false;


            //Get script minimum timeout
            if (entConfig.DefaultMinumumScriptTimeOut != null && entConfig.DefaultMinumumScriptTimeOut.Seconds > 0)
            {
                mnuDefaultScriptTimeout.Enabled = false;

                if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout != entConfig.DefaultMinumumScriptTimeOut.Seconds)
                {
                    SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout = entConfig.DefaultMinumumScriptTimeOut.Seconds;
                    saveSettings = true;
                }
            }

            //Get required script tags setting
            if (entConfig.RequireScriptTags != null)
            {
                if (entConfig.RequireScriptTags.Value == true)
                    tagsRequiredEnabled = false;

                if (SqlSync.Properties.Settings.Default.RequireScriptTags != entConfig.RequireScriptTags.Value)
                {
                    SqlSync.Properties.Settings.Default.RequireScriptTags = entConfig.RequireScriptTags.Value;
                    saveSettings = true;
                }

                if (entConfig.RequireScriptTags.Message.Length > 0)
                {
                    if (SqlSync.Properties.Settings.Default.RequireScriptTagsMessage != entConfig.RequireScriptTags.Message)
                    {
                        SqlSync.Properties.Settings.Default.RequireScriptTagsMessage = entConfig.RequireScriptTags.Message;
                        saveSettings = true;
                    }
                }
            }

            //Get default scripts setting
            if (entConfig.DefaultScriptConfiguration != null && entConfig.DefaultScriptConfiguration.Count() > 0)
            {
                defaultScriptEnabled = false;
                SqlBuildManager.Enterprise.DefaultScripts.DefaultScriptHelper.SetEnterpriseDefaultScripts(entConfig.DefaultScriptConfiguration.ToList(), this.adGroupMemberships);
            }

            //Get enterprise level Tag regex
            if (entConfig.ScriptTagInference != null && entConfig.ScriptTagInference.Count() > 0)
            {
                List<string> regexList = SqlBuildManager.Enterprise.Tag.EnterpriseTagHelper.GetEnterpriseTagRegexValues(entConfig.ScriptTagInference.ToList(), this.adGroupMemberships);
                if (regexList != null)
                {
                    SqlSync.Properties.Settings.Default.TagInferenceRegexList = new System.Collections.Specialized.StringCollection();
                    SqlSync.Properties.Settings.Default.TagInferenceRegexList.AddRange(regexList.ToArray());
                    saveSettings = true;
                }
            }

            //Set the Script policy run on load
            if (entConfig.SciptPolicyRunOnLoad != null && entConfig.SciptPolicyRunOnLoad.Enabled)
            {
                if (entConfig.SciptPolicyRunOnLoad.ApplyToGroup != null)
                {
                    var d = (from g in this.adGroupMemberships
                             join a in entConfig.SciptPolicyRunOnLoad.ApplyToGroup on g.ToLower() equals a.GroupName.ToLower()
                             select g);

                    if (d.Count() > 0)
                        this.runPolicyCheckingOnLoad = true;
                }
                else
                {
                    this.runPolicyCheckingOnLoad = true;
                }

                policyCheckOnLoadEnabled = false;
                //this.runPolicyCheckingOnLoad = entConfig.SciptPolicyRunOnLoad.Enabled;
            }
            else
            {
                policyCheckOnLoadEnabled = true;
            }

            if (entConfig.CustomObjectScriptingSettings != null)
            {
                foreach (CustomObjectScriptingSettings set in entConfig.CustomObjectScriptingSettings)
                {
                    var ss = (from g in this.adGroupMemberships
                              join a in set.ApplyToGroup on g.ToLower() equals a.GroupName.ToLower()
                              select set.ScriptingSetting);

                    if (ss.Count() > 0)
                    {
                        var pk = (from sa in ss
                                  from sc in sa
                                  where sc.Name == "PrimaryKeyWithTable"
                                  select sc.Value);

                        if (pk.Count() > 0)
                        {
                            this.scriptPkWithTables = pk.ToList()[0];
                            scriptPkWithTable = this.scriptPkWithTables;
                            SqlSync.Properties.Settings.Default.ScriptPkWithTables = this.scriptPkWithTables;
                        }
                    }
                }
            }

            //Get users that are considered DBA's for code reviews
            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig != null && EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.DbaReviewGroup != null)
            {
                
                foreach (AccessSetting g in EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.DbaReviewGroup)
                    codeReviewDbaMembers.AddRange(AdHelper.GetMembersForGroup(g.GroupName));

                var users = from d in EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.DbaReviewGroup
                            where d.LoginId.Length > 0
                            select d.LoginId;


                codeReviewDbaMembers.AddRange(users);
            }


            //Get the list of SelfReviewers for CodeReview
            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig != null && EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.SelfReviewer != null)
            {
                List<string> lstSelfReviewers = new List<string>();

                AccessSetting[] selfReview = EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.SelfReviewer;
                var groups = from s in selfReview
                             where s.GroupName.Length > 0
                             select s.GroupName;

                foreach (string group in groups)
                {
                    lstSelfReviewers.AddRange(AdHelper.GetMembersForGroup(group));
                }

               var users = from s in selfReview
                            where s.LoginId.Length > 0
                           select s.LoginId;

               lstSelfReviewers.AddRange(users.ToList());

               var updatedList = from l in lstSelfReviewers
                                 select new AccessSetting { LoginId = l };

               EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.SelfReviewer = updatedList.ToArray();
            }


            //Update local settings as appropriate
            if (saveSettings)
                SqlSync.Properties.Settings.Default.Save();
        }
        private bool PassesReadOnlyProjectFileCheck(string fileName)
        {

            if (!String.IsNullOrEmpty(fileName))
            {
                if(!File.Exists(fileName))
                    return true;

                SourceControlStatus srcStatus = SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl ,fileName);
                if (SourceControlStatus.NotUnderSourceControl == srcStatus || SourceControlStatus.Error == srcStatus)
                {
                    this.ProjectIsUnderSourceControl = false;
                }
                else
                {
                    this.ProjectIsUnderSourceControl = true;
                }

                FileAttributes attrib = File.GetAttributes(fileName);

                if ((attrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    string msg = "Your selected file " + Path.GetFileName(fileName) + " is marked as Read-Only. To continue, this file needs to be writable.\r\nDo you want to make the file writable and continue loading?";
                    if (DialogResult.Yes == MessageBox.Show(msg, "File is Read Only!", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal);
                        if (fileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string xml = Path.GetDirectoryName(fileName) + @"\SqlSyncBuildHistory.xml";
                            if(File.Exists(xml))
                                File.SetAttributes(xml,FileAttributes.Normal);
                        }
                        return true;
                    }
                    else
                    {
                        this.buildZipFileName = null;
                        this.sbxBuildControlFileName = null;
                    }
                }
                else if (fileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
                {
                    string xml = Path.GetDirectoryName(fileName) + @"\SqlSyncBuildHistory.xml";
                    if (File.Exists(xml))
                    {
                        SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, xml);

                        attrib = File.GetAttributes(xml);
                        if ((attrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            string msg = "The XML logging file \"SqlSyncBuildHistory.xml\" for your project file " + Path.GetFileName(fileName) + " is marked as Read-Only. To continue, this file needs to be writable.\r\nDo you want to make the file writable and continue loading?";
                            if (DialogResult.Yes == MessageBox.Show(msg, "File is Read Only!", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                            {
                                File.SetAttributes(xml, FileAttributes.Normal);
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }

                }
                else
                {
                    return true;
                }
            }
            return false;
        }
       /// <summary>
       /// Checks either the build zip project file (sbm) or the sbx control file to see if it's read-only.
       /// If it is, it prompts to change the attribute.
       /// </summary>
       /// <returns>True if file is writeable</returns>
        private bool PassesReadOnlyProjectFileCheck()
        {
            //Check for a read-only project of control file...
            string tmp = string.Empty;
            if (!String.IsNullOrEmpty(this.buildZipFileName))
                tmp = this.buildZipFileName;
            else if (!String.IsNullOrEmpty(this.sbxBuildControlFileName))
                tmp = this.sbxBuildControlFileName;

            return PassesReadOnlyProjectFileCheck(tmp);
        }

        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            this.mruManager = new MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Sql Build Manager"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 40;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlBuildForm));
            this.grbBuildScripts = new System.Windows.Forms.GroupBox();
            this.imageListSlide = new System.Windows.Forms.ImageList(this.components);
            this.chkUpdateOnOverride = new System.Windows.Forms.CheckBox();
            this.chkScriptChanges = new System.Windows.Forms.CheckBox();
            this.lstScriptFiles = new SqlSync.Controls.OAKControls.OAKListView();
            this.colImage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPolicyIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCodeReviewIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSequence = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScriptFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDatabaseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScriptId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScriptSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateAdded = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctxScriptFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuEditScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCompareObject = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewRunHistorySep = new System.Windows.Forms.ToolStripSeparator();
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.imageListBuildScripts = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statusHelpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.iconLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.runOnCurrentServerdatabaseCombinationAndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.policyCheckIconHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.codeReviewIconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgrounLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSbmFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dlgAddScriptFile = new System.Windows.Forms.OpenFileDialog();
            this.grpManager = new System.Windows.Forms.GroupBox();
            this.targetDatabaseOverrideCtrl1 = new SqlSync.TargetDatabaseOverrideCtrl();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBuildDesc = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtStartIndex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lnkStartBuild = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.ddBuildType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ddOverrideLogDatabase = new System.Windows.Forms.ComboBox();
            this.chkNotTransactional = new System.Windows.Forms.CheckBox();
            this.lstBuild = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctxResults = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.saveExportFile = new System.Windows.Forms.SaveFileDialog();
            this.openScriptExportFile = new System.Windows.Forms.OpenFileDialog();
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.openFileBulkLoad = new System.Windows.Forms.OpenFileDialog();
            this.pnlManager = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlAdvanced = new System.Windows.Forms.Panel();
            this.lblAdvanced = new System.Windows.Forms.Label();
            this.grpAdvanced = new System.Windows.Forms.GroupBox();
            this.grpBuildResults = new System.Windows.Forms.GroupBox();
            this.pnlBuildScripts = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.openFileAutoScript = new System.Windows.Forms.OpenFileDialog();
            this.fdrSaveScripts = new System.Windows.Forms.FolderBrowserDialog();
            this.saveCombinedScript = new System.Windows.Forms.SaveFileDialog();
            this.bgBuildProcess = new System.ComponentModel.BackgroundWorker();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.bgCheckForUpdates = new System.ComponentModel.BackgroundWorker();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statScriptCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statBuildTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.statScriptTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statControlStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBuild = new System.Windows.Forms.ToolStripProgressBar();
            this.tmrBuild = new System.Windows.Forms.Timer(this.components);
            this.tmrScript = new System.Windows.Forms.Timer(this.components);
            this.bgLoadZipFle = new System.ComponentModel.BackgroundWorker();
            this.bgRefreshScriptList = new System.ComponentModel.BackgroundWorker();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.bgDatabaseRoutine = new System.ComponentModel.BackgroundWorker();
            this.bgObjectScripting = new System.ComponentModel.BackgroundWorker();
            this.bgGetObjectList = new System.ComponentModel.BackgroundWorker();
            this.openFileDataExtract = new System.Windows.Forms.OpenFileDialog();
            this.openSbxFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveScriptsToPackage = new System.Windows.Forms.SaveFileDialog();
            this.bgEnterpriseSettings = new System.ComponentModel.BackgroundWorker();
            this.bgBulkAdd = new System.ComponentModel.BackgroundWorker();
            this.bgPolicyCheck = new System.ComponentModel.BackgroundWorker();
            this.savePolicyViolationCsv = new System.Windows.Forms.SaveFileDialog();
            this.bgBulkAddStep2 = new System.ComponentModel.BackgroundWorker();
            this.bgCodeReview = new System.ComponentModel.BackgroundWorker();
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.mnuDisplayRowResults = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditFromResults = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenRunScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSlideBuildScripts = new System.Windows.Forms.Button();
            this.mnuEditFile = new System.Windows.Forms.ToolStripMenuItem();
            this.makeFileWriteableremoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddSqlScriptText = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddScript = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUpdatePopulates = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUpdateObjectScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuObjectScripts_FileDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuObjectScripts_CurrentSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateExportFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRemoveScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTryScript = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRunScript = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewRunHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.renameScriptFIleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptNotRunOnCurrentServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileMissingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.policyChecksNotRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.policyCheckFailedActionRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.policyWarningActionMayBeRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.passesPolicyChecksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waitingOnStatusCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewNotStartedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewInProgressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewAcceptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewAcceptedByDBAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuActionMain = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLoadProject = new System.Windows.Forms.ToolStripMenuItem();
            this.loadNewDirectoryControlFilesbxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuChangeSqlServer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maintainManualDatabaseEntriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maintainDefaultScriptRegistryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptTagsRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSQLLogOfBuildRunsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runPolicyCheckingonloadtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptPrimaryKeyWithTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultScriptTimeoutsecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDefaultScriptTimeout = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.sourceControlServerURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceControlServerURLTextboxMenuItem = new System.Windows.Forms.ToolStripTextBox();
            this.mnuMainAddSqlScript = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainAddNewFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuImportScriptFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompare = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem12 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExportScriptText = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuIndividualFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCombinedFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem22 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuIncludeUSE = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuIncludeSequence = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem15 = new System.Windows.Forms.ToolStripSeparator();
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteExecutionServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuListTop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFindScript = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuRenumberSequence = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResortByFileType = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExportBuildList = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExportBuildListToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExportBuildListToClipBoard = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuBulkAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBulkFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBulkFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuClearPreviouslyRunBlocks = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.inferScriptTagFromFileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptContentsFirstThenFileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNameFirstThenSciptContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptContentsOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNamesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScripting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDDActiveDatabase = new System.Windows.Forms.ToolStripComboBox();
            this.mnuAddObjectCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddStoredProcs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddFunctions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddViews = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddTables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddTriggers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddRoles = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptingOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptALTERAndCREATEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.includeObjectPermissionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddCodeTablePop = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuUpdatePopulate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuObjectUpdates = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.createBackoutPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLogging = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowBuildLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptToLogFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShowBuildHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveBuildHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem16 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuObjectValidation = new System.Windows.Forms.ToolStripMenuItem();
            this.storedProcedureTestingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem21 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSchemaScripting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCodeTableScripting = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem18 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuDataAuditScripting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDataExtraction = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuDatabaseSize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem19 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAutoScripting = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem11 = new System.Windows.Forms.ToolStripSeparator();
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.constructCommandLineStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptPolicyCheckingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runPolicyChecksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.savePolicyResultsInCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePolicyResultsAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateScriptPackageHashSignatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.howToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.projectSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogFileMenuItem2 = new SqlSync.Controls.ViewLogFileMenuItem();
            this.setLoggingLevelMenuItem2 = new SqlSync.Controls.SetLoggingLevelMenuItem();
            this.grbBuildScripts.SuspendLayout();
            this.ctxScriptFile.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.grpManager.SuspendLayout();
            this.ctxResults.SuspendLayout();
            this.mainMenu1.SuspendLayout();
            this.pnlManager.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlAdvanced.SuspendLayout();
            this.grpAdvanced.SuspendLayout();
            this.grpBuildResults.SuspendLayout();
            this.pnlBuildScripts.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStripContainer2.ContentPanel.SuspendLayout();
            this.toolStripContainer2.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // grbBuildScripts
            // 
            this.grbBuildScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grbBuildScripts.Controls.Add(this.btnSlideBuildScripts);
            this.grbBuildScripts.Controls.Add(this.chkUpdateOnOverride);
            this.grbBuildScripts.Controls.Add(this.chkScriptChanges);
            this.grbBuildScripts.Controls.Add(this.lstScriptFiles);
            this.grbBuildScripts.Controls.Add(this.menuStrip1);
            this.grbBuildScripts.Enabled = false;
            this.grbBuildScripts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grbBuildScripts.Location = new System.Drawing.Point(2, 2);
            this.grbBuildScripts.Name = "grbBuildScripts";
            this.grbBuildScripts.Size = new System.Drawing.Size(518, 540);
            this.grbBuildScripts.TabIndex = 14;
            this.grbBuildScripts.TabStop = false;
            this.grbBuildScripts.Text = "Build Scripts";
            // 
            // imageListSlide
            // 
            this.imageListSlide.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSlide.ImageStream")));
            this.imageListSlide.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSlide.Images.SetKeyName(0, "rightarrow_white.GIF");
            this.imageListSlide.Images.SetKeyName(1, "leftarrow_white.GIF");
            // 
            // chkUpdateOnOverride
            // 
            this.chkUpdateOnOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkUpdateOnOverride.Checked = true;
            this.chkUpdateOnOverride.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUpdateOnOverride.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.chkUpdateOnOverride.Location = new System.Drawing.Point(6, 494);
            this.chkUpdateOnOverride.Name = "chkUpdateOnOverride";
            this.chkUpdateOnOverride.Size = new System.Drawing.Size(420, 16);
            this.chkUpdateOnOverride.TabIndex = 16;
            this.chkUpdateOnOverride.Text = "Update Icons on Override Target Change (will slow list refresh)";
            this.chkUpdateOnOverride.CheckedChanged += new System.EventHandler(this.chkUpdateOnOverride_CheckedChanged);
            // 
            // chkScriptChanges
            // 
            this.chkScriptChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkScriptChanges.Checked = true;
            this.chkScriptChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptChanges.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.chkScriptChanges.Location = new System.Drawing.Point(6, 478);
            this.chkScriptChanges.Name = "chkScriptChanges";
            this.chkScriptChanges.Size = new System.Drawing.Size(420, 16);
            this.chkScriptChanges.TabIndex = 15;
            this.chkScriptChanges.Text = "Check for script changes (Pre-run scripts only, may slow list refresh)";
            this.chkScriptChanges.Click += new System.EventHandler(this.chkScriptChanges_Click);
            // 
            // lstScriptFiles
            // 
            this.lstScriptFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstScriptFiles.AllowDrop = true;
            this.lstScriptFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstScriptFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstScriptFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colImage,
            this.colPolicyIcon,
            this.colCodeReviewIcon,
            this.colSequence,
            this.colScriptFile,
            this.colDatabaseName,
            this.colScriptId,
            this.colScriptSize,
            this.colTag,
            this.colDateAdded,
            this.colDateModified});
            this.lstScriptFiles.ContextMenuStrip = this.ctxScriptFile;
            this.lstScriptFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstScriptFiles.FullRowSelect = true;
            this.lstScriptFiles.GridLines = true;
            this.lstScriptFiles.Location = new System.Drawing.Point(5, 17);
            this.lstScriptFiles.Name = "lstScriptFiles";
            this.lstScriptFiles.ShowItemToolTips = true;
            this.lstScriptFiles.Size = new System.Drawing.Size(508, 455);
            this.lstScriptFiles.SmallImageList = this.imageListBuildScripts;
            this.lstScriptFiles.TabIndex = 0;
            this.lstScriptFiles.UseCompatibleStateImageBehavior = false;
            this.lstScriptFiles.View = System.Windows.Forms.View.Details;
            this.lstScriptFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstScriptFiles_ColumnClick);
            this.lstScriptFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lstScriptFiles_ItemDrag);
            this.lstScriptFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstScriptFiles_DragDrop);
            this.lstScriptFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.lstScriptFiles_DragEnter);
            this.lstScriptFiles.DoubleClick += new System.EventHandler(this.lstScriptFiles_DoubleClick);
            this.lstScriptFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstScriptFiles_KeyDown);
            // 
            // colImage
            // 
            this.colImage.Text = "";
            this.colImage.Width = 21;
            // 
            // colPolicyIcon
            // 
            this.colPolicyIcon.Text = "";
            this.colPolicyIcon.Width = 16;
            // 
            // colCodeReviewIcon
            // 
            this.colCodeReviewIcon.Text = "";
            this.colCodeReviewIcon.Width = 16;
            // 
            // colSequence
            // 
            this.colSequence.Text = "Seq #";
            this.colSequence.Width = 47;
            // 
            // colScriptFile
            // 
            this.colScriptFile.Text = "Script File";
            this.colScriptFile.Width = 217;
            // 
            // colDatabaseName
            // 
            this.colDatabaseName.Text = "Database ";
            this.colDatabaseName.Width = 80;
            // 
            // colScriptId
            // 
            this.colScriptId.Text = "Script Id";
            this.colScriptId.Width = 0;
            // 
            // colScriptSize
            // 
            this.colScriptSize.Text = "Size";
            this.colScriptSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colScriptSize.Width = 0;
            // 
            // colTag
            // 
            this.colTag.Text = "Tag";
            this.colTag.Width = 85;
            // 
            // colDateAdded
            // 
            this.colDateAdded.Text = "Date Added";
            this.colDateAdded.Width = 0;
            // 
            // colDateModified
            // 
            this.colDateModified.Text = "Date Modified";
            this.colDateModified.Width = 0;
            // 
            // ctxScriptFile
            // 
            this.ctxScriptFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditFile,
            this.mnuEditScriptFile,
            this.menuItem4,
            this.makeFileWriteableremoveToolStripMenuItem,
            this.toolStripSeparator23,
            this.mnuAddSqlScriptText,
            this.mnuAddScript,
            this.menuItem1,
            this.mnuUpdatePopulates,
            this.mnuUpdateObjectScripts,
            this.mnuCompareObject,
            this.menuItem2,
            this.mnuCreateExportFile,
            this.mnuRemoveScriptFile,
            this.menuItem6,
            this.mnuTryScript,
            this.mnuRunScript,
            this.mnuViewRunHistorySep,
            this.mnuViewRunHistory,
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem,
            this.renameScriptFIleToolStripMenuItem,
            this.toolStripSeparator12});
            this.ctxScriptFile.Name = "ctxScriptFile";
            this.ctxScriptFile.Size = new System.Drawing.Size(389, 376);
            this.ctxScriptFile.Opening += new System.ComponentModel.CancelEventHandler(this.ctxScriptFile_Opening);
            // 
            // mnuEditScriptFile
            // 
            this.mnuEditScriptFile.MergeIndex = 0;
            this.mnuEditScriptFile.Name = "mnuEditScriptFile";
            this.mnuEditScriptFile.Size = new System.Drawing.Size(388, 22);
            this.mnuEditScriptFile.Text = "Edit/View Script Build Detail";
            this.mnuEditScriptFile.Click += new System.EventHandler(this.mnuEditScriptFile_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.MergeIndex = 2;
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(385, 6);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(385, 6);
            // 
            // menuItem1
            // 
            this.menuItem1.MergeIndex = 5;
            this.menuItem1.Name = "menuItem1";
            this.menuItem1.Size = new System.Drawing.Size(385, 6);
            // 
            // mnuCompareObject
            // 
            this.mnuCompareObject.Name = "mnuCompareObject";
            this.mnuCompareObject.Size = new System.Drawing.Size(388, 22);
            this.mnuCompareObject.Text = "Compare packaged object script to Server/Database version";
            this.mnuCompareObject.Click += new System.EventHandler(this.mnuCompareObject_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MergeIndex = 8;
            this.menuItem2.Name = "menuItem2";
            this.menuItem2.Size = new System.Drawing.Size(385, 6);
            // 
            // menuItem6
            // 
            this.menuItem6.MergeIndex = 11;
            this.menuItem6.Name = "menuItem6";
            this.menuItem6.Size = new System.Drawing.Size(385, 6);
            // 
            // mnuViewRunHistorySep
            // 
            this.mnuViewRunHistorySep.MergeIndex = 14;
            this.mnuViewRunHistorySep.Name = "mnuViewRunHistorySep";
            this.mnuViewRunHistorySep.Size = new System.Drawing.Size(385, 6);
            // 
            // showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem
            // 
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Enabled = false;
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Name = "showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem";
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Size = new System.Drawing.Size(388, 22);
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Text = "View object change history as run by Sql Build Manager";
            this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Click += new System.EventHandler(this.showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(385, 6);
            // 
            // imageListBuildScripts
            // 
            this.imageListBuildScripts.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBuildScripts.ImageStream")));
            this.imageListBuildScripts.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListBuildScripts.Images.SetKeyName(0, "");
            this.imageListBuildScripts.Images.SetKeyName(1, "lock.png");
            this.imageListBuildScripts.Images.SetKeyName(2, "");
            this.imageListBuildScripts.Images.SetKeyName(3, "");
            this.imageListBuildScripts.Images.SetKeyName(4, "");
            this.imageListBuildScripts.Images.SetKeyName(5, "question.ico");
            this.imageListBuildScripts.Images.SetKeyName(6, "Delete.png");
            this.imageListBuildScripts.Images.SetKeyName(7, "Document-Protect.png");
            this.imageListBuildScripts.Images.SetKeyName(8, "Help-2.png");
            this.imageListBuildScripts.Images.SetKeyName(9, "Tick.png");
            this.imageListBuildScripts.Images.SetKeyName(10, "Exclamation-square.png");
            this.imageListBuildScripts.Images.SetKeyName(11, "exclamation-shield-frame.png");
            this.imageListBuildScripts.Images.SetKeyName(12, "Hand.ico");
            this.imageListBuildScripts.Images.SetKeyName(13, "Clock.ico");
            this.imageListBuildScripts.Images.SetKeyName(14, "Ok-blueSquare.ico");
            this.imageListBuildScripts.Images.SetKeyName(15, "Ok-greenSquare.ico");
            this.imageListBuildScripts.Images.SetKeyName(16, "Wait.png");
            this.imageListBuildScripts.Images.SetKeyName(17, "Discuss.ico");
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Gainsboro;
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusHelpToolStripMenuItem1,
            this.iconLegendToolStripMenuItem,
            this.policyCheckIconHelpToolStripMenuItem,
            this.codeReviewIconToolStripMenuItem,
            this.backgrounLegendToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(3, 513);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(512, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // statusHelpToolStripMenuItem1
            // 
            this.statusHelpToolStripMenuItem1.Name = "statusHelpToolStripMenuItem1";
            this.statusHelpToolStripMenuItem1.Size = new System.Drawing.Size(47, 20);
            this.statusHelpToolStripMenuItem1.Text = "Help:";
            // 
            // iconLegendToolStripMenuItem
            // 
            this.iconLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem,
            this.scriptNotRunOnCurrentServerToolStripMenuItem,
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem,
            this.toolStripSeparator6,
            this.runOnCurrentServerdatabaseCombinationAndToolStripMenuItem,
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem,
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem,
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem,
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem,
            this.toolStripSeparator22,
            this.fileMissingToolStripMenuItem});
            this.iconLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.iconLegendToolStripMenuItem.Name = "iconLegendToolStripMenuItem";
            this.iconLegendToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.iconLegendToolStripMenuItem.Text = "Status Icon";
            // 
            // notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem
            // 
            this.notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Name = "notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem";
            this.notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Text = "Not yet run on current server/database combination and...";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(596, 6);
            // 
            // runOnCurrentServerdatabaseCombinationAndToolStripMenuItem
            // 
            this.runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Name = "runOnCurrentServerdatabaseCombinationAndToolStripMenuItem";
            this.runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Text = "Run on current server/database combination and... ";
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(596, 6);
            // 
            // policyCheckIconHelpToolStripMenuItem
            // 
            this.policyCheckIconHelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.policyChecksNotRunToolStripMenuItem,
            this.policyCheckFailedActionRequiredToolStripMenuItem,
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem,
            this.policyWarningActionMayBeRequiredToolStripMenuItem,
            this.passesPolicyChecksToolStripMenuItem});
            this.policyCheckIconHelpToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.policyCheckIconHelpToolStripMenuItem.Name = "policyCheckIconHelpToolStripMenuItem";
            this.policyCheckIconHelpToolStripMenuItem.Size = new System.Drawing.Size(113, 20);
            this.policyCheckIconHelpToolStripMenuItem.Text = "Policy Check Icon";
            // 
            // codeReviewIconToolStripMenuItem
            // 
            this.codeReviewIconToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.waitingOnStatusCheckToolStripMenuItem,
            this.reviewNotStartedToolStripMenuItem,
            this.reviewInProgressToolStripMenuItem,
            this.reviewAcceptedToolStripMenuItem,
            this.reviewAcceptedByDBAToolStripMenuItem});
            this.codeReviewIconToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.codeReviewIconToolStripMenuItem.Name = "codeReviewIconToolStripMenuItem";
            this.codeReviewIconToolStripMenuItem.Size = new System.Drawing.Size(113, 20);
            this.codeReviewIconToolStripMenuItem.Text = "Code Review Icon";
            // 
            // backgrounLegendToolStripMenuItem
            // 
            this.backgrounLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem,
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem,
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem,
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem,
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem});
            this.backgrounLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.backgrounLegendToolStripMenuItem.Name = "backgrounLegendToolStripMenuItem";
            this.backgrounLegendToolStripMenuItem.Size = new System.Drawing.Size(115, 20);
            this.backgrounLegendToolStripMenuItem.Text = "Background Color";
            // 
            // allowMultipleRunsOfScriptOnSameServerToolStripMenuItem
            // 
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = System.Drawing.Color.DarkSalmon;
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Name = "allowMultipleRunsOfScriptOnSameServerToolStripMenuItem";
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Size = new System.Drawing.Size(460, 22);
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Text = "Allow multiple runs of script on same server";
            // 
            // leaveTransactionTextInScriptsdontStripOutToolStripMenuItem
            // 
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = System.Drawing.Color.LightBlue;
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Name = "leaveTransactionTextInScriptsdontStripOutToolStripMenuItem";
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Size = new System.Drawing.Size(460, 22);
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Text = "Leave transaction text in scripts (don\'t strip out transaction references)";
            // 
            // allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem
            // 
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor = System.Drawing.Color.Thistle;
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Name = "allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem";
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Size = new System.Drawing.Size(460, 22);
            this.allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Text = "Allow multiple runs AND Leave transaction text";
            // 
            // scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem
            // 
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Name = "scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem";
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Size = new System.Drawing.Size(460, 22);
            this.scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Text = "Script will be skipped on server (already run and marked as \"run once\")";
            // 
            // scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem
            // 
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor = System.Drawing.Color.Gray;
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Name = "scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem";
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Size = new System.Drawing.Size(460, 22);
            this.scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Text = "Script file is read-only! You will need to make writeable before modifying.";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusHelpToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // statusHelpToolStripMenuItem
            // 
            this.statusHelpToolStripMenuItem.Name = "statusHelpToolStripMenuItem";
            this.statusHelpToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.statusHelpToolStripMenuItem.Text = "Status Help:";
            // 
            // openSbmFileDialog
            // 
            this.openSbmFileDialog.CheckFileExists = false;
            this.openSbmFileDialog.DefaultExt = "xml";
            this.openSbmFileDialog.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
    "iles (*.zip)|*.zip|All Files|*.*";
            this.openSbmFileDialog.Title = "Open or Create New Sql Build Manager Project File";
            // 
            // dlgAddScriptFile
            // 
            this.dlgAddScriptFile.Filter = resources.GetString("dlgAddScriptFile.Filter");
            this.dlgAddScriptFile.Title = "Add Script File to Build";
            // 
            // grpManager
            // 
            this.grpManager.Controls.Add(this.pictureBox1);
            this.grpManager.Controls.Add(this.targetDatabaseOverrideCtrl1);
            this.grpManager.Controls.Add(this.label2);
            this.grpManager.Controls.Add(this.txtBuildDesc);
            this.grpManager.Controls.Add(this.btnCancel);
            this.grpManager.Controls.Add(this.txtStartIndex);
            this.grpManager.Controls.Add(this.label4);
            this.grpManager.Controls.Add(this.label3);
            this.grpManager.Controls.Add(this.lnkStartBuild);
            this.grpManager.Controls.Add(this.label1);
            this.grpManager.Controls.Add(this.ddBuildType);
            this.grpManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpManager.Enabled = false;
            this.grpManager.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpManager.Location = new System.Drawing.Point(3, 3);
            this.grpManager.MinimumSize = new System.Drawing.Size(533, 154);
            this.grpManager.Name = "grpManager";
            this.grpManager.Size = new System.Drawing.Size(541, 168);
            this.grpManager.TabIndex = 15;
            this.grpManager.TabStop = false;
            this.grpManager.Text = "Build Manager / Run Settings";
            // 
            // targetDatabaseOverrideCtrl1
            // 
            this.targetDatabaseOverrideCtrl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDatabaseOverrideCtrl1.Location = new System.Drawing.Point(99, 41);
            this.targetDatabaseOverrideCtrl1.Name = "targetDatabaseOverrideCtrl1";
            this.targetDatabaseOverrideCtrl1.Size = new System.Drawing.Size(437, 77);
            this.targetDatabaseOverrideCtrl1.TabIndex = 1;
            this.targetDatabaseOverrideCtrl1.TargetChanged += new SqlSync.TargetChangedEventHandler(this.targetDatabaseOverrideCtrl1_TargetChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 65);
            this.label2.TabIndex = 27;
            this.label2.Text = "Target Database\r\nOverride:\r\n";
            // 
            // txtBuildDesc
            // 
            this.txtBuildDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtBuildDesc.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtBuildDesc.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtBuildDesc.Location = new System.Drawing.Point(99, 124);
            this.txtBuildDesc.Name = "txtBuildDesc";
            this.txtBuildDesc.Size = new System.Drawing.Size(370, 21);
            this.txtBuildDesc.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtBuildDesc, "Enter a Description for the build. This is required before you can execute.\r\n\r\nNO" +
        "TE: This value will be inserted whereever the token #BuildDescription# is found " +
        "in scripts in the package.");
            this.txtBuildDesc.TextChanged += new System.EventHandler(this.txtBuildDesc_TextChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancel.ForeColor = System.Drawing.Color.Red;
            this.btnCancel.Location = new System.Drawing.Point(475, 124);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(55, 23);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtStartIndex
            // 
            this.txtStartIndex.Enabled = false;
            this.txtStartIndex.Location = new System.Drawing.Point(460, 16);
            this.txtStartIndex.Name = "txtStartIndex";
            this.txtStartIndex.Size = new System.Drawing.Size(40, 21);
            this.txtStartIndex.TabIndex = 2;
            this.txtStartIndex.Text = "0";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(295, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "Partial Run Start Index:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Location = new System.Drawing.Point(8, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Description *:";
            // 
            // lnkStartBuild
            // 
            this.lnkStartBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkStartBuild.Enabled = false;
            this.lnkStartBuild.Location = new System.Drawing.Point(96, 146);
            this.lnkStartBuild.Name = "lnkStartBuild";
            this.lnkStartBuild.Size = new System.Drawing.Size(261, 16);
            this.lnkStartBuild.TabIndex = 3;
            this.lnkStartBuild.TabStop = true;
            this.lnkStartBuild.Text = "Please Enter a Description";
            this.lnkStartBuild.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkStartBuild.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkStartBuild_LinkClicked);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Build Type:";
            this.toolTip1.SetToolTip(this.label1, "NOTE: A \"Trial\" will always Roll Back the build");
            // 
            // ddBuildType
            // 
            this.ddBuildType.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddBuildType.ItemHeight = 13;
            this.ddBuildType.Location = new System.Drawing.Point(115, 16);
            this.ddBuildType.Name = "ddBuildType";
            this.ddBuildType.Size = new System.Drawing.Size(173, 21);
            this.ddBuildType.TabIndex = 0;
            this.toolTip1.SetToolTip(this.ddBuildType, "NOTE: A \"Trial\" will always Roll Back the build");
            this.ddBuildType.SelectionChangeCommitted += new System.EventHandler(this.ddBuildType_SelectionChangeCommitted);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Log Target Database:";
            // 
            // ddOverrideLogDatabase
            // 
            this.ddOverrideLogDatabase.FormattingEnabled = true;
            this.ddOverrideLogDatabase.Location = new System.Drawing.Point(146, 24);
            this.ddOverrideLogDatabase.Name = "ddOverrideLogDatabase";
            this.ddOverrideLogDatabase.Size = new System.Drawing.Size(142, 21);
            this.ddOverrideLogDatabase.TabIndex = 29;
            this.toolTip1.SetToolTip(this.ddOverrideLogDatabase, "Sets an alternate database to log the changes to.\r\nMost of the time, this will be" +
        " left blank.");
            this.ddOverrideLogDatabase.SelectionChangeCommitted += new System.EventHandler(this.ddOverrideLogDatabase_SelectionChangeCommitted);
            // 
            // chkNotTransactional
            // 
            this.chkNotTransactional.AutoSize = true;
            this.chkNotTransactional.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkNotTransactional.Location = new System.Drawing.Point(301, 26);
            this.chkNotTransactional.Name = "chkNotTransactional";
            this.chkNotTransactional.Size = new System.Drawing.Size(206, 17);
            this.chkNotTransactional.TabIndex = 28;
            this.chkNotTransactional.Text = "Run Build without a Transaction";
            this.toolTip1.SetToolTip(this.chkNotTransactional, "CAUTION!!\r\nSets whether or not to run this build in a transaction. In most cases " +
        "this should be left unchecked.\r\n** When checked, your scripts will not be rolled" +
        " back in the event of a failure! **");
            this.chkNotTransactional.UseVisualStyleBackColor = true;
            this.chkNotTransactional.CheckedChanged += new System.EventHandler(this.chkNotTransactional_CheckedChanged);
            // 
            // lstBuild
            // 
            this.lstBuild.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBuild.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstBuild.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader8,
            this.columnHeader7,
            this.columnHeader13});
            this.lstBuild.ContextMenuStrip = this.ctxResults;
            this.lstBuild.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstBuild.FullRowSelect = true;
            this.lstBuild.GridLines = true;
            this.lstBuild.Location = new System.Drawing.Point(5, 15);
            this.lstBuild.Name = "lstBuild";
            this.lstBuild.Size = new System.Drawing.Size(531, 303);
            this.lstBuild.TabIndex = 10;
            this.lstBuild.UseCompatibleStateImageBehavior = false;
            this.lstBuild.View = System.Windows.Forms.View.Details;
            this.lstBuild.DoubleClick += new System.EventHandler(this.mnuEditFromResults_Click);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Seq #";
            this.columnHeader4.Width = 44;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Script File";
            this.columnHeader5.Width = 170;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Database Name";
            this.columnHeader6.Width = 91;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Orig #";
            this.columnHeader8.Width = 45;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Status";
            this.columnHeader7.Width = 100;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Time";
            this.columnHeader13.Width = 56;
            // 
            // ctxResults
            // 
            this.ctxResults.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDisplayRowResults,
            this.mnuEditFromResults,
            this.mnuOpenRunScriptFile});
            this.ctxResults.Name = "ctxResults";
            this.ctxResults.Size = new System.Drawing.Size(153, 70);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "txt";
            this.saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            this.saveFileDialog1.Title = "Save Build Export";
            // 
            // saveExportFile
            // 
            this.saveExportFile.DefaultExt = "sbe";
            this.saveExportFile.Filter = "Sql Build Export Files (*.sbe)|*.sbe|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.saveExportFile.Title = "Save File";
            // 
            // openScriptExportFile
            // 
            this.openScriptExportFile.Filter = "Sql Build Files (*.sbm)|*.sbm|Sql Build Export Files (*.sbe)|*.sbe|XML Files (*.x" +
    "ml)|*.xml|All Files (*.*)|*.*";
            this.openScriptExportFile.Title = "Import Sql Build Script File";
            // 
            // mainMenu1
            // 
            this.mainMenu1.Dock = System.Windows.Forms.DockStyle.None;
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionMain,
            this.mnuListTop,
            this.mnuScripting,
            this.mnuLogging,
            this.menuItem16,
            this.mnuHelp});
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(1073, 24);
            this.mainMenu1.TabIndex = 0;
            // 
            // openFileBulkLoad
            // 
            this.openFileBulkLoad.AddExtension = false;
            this.openFileBulkLoad.Filter = "All Files (*.*)|*.*";
            this.openFileBulkLoad.Multiselect = true;
            this.openFileBulkLoad.Title = "Select Files to Add";
            // 
            // pnlManager
            // 
            this.pnlManager.Controls.Add(this.splitContainer1);
            this.pnlManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlManager.Location = new System.Drawing.Point(526, 56);
            this.pnlManager.Name = "pnlManager";
            this.pnlManager.Size = new System.Drawing.Size(547, 544);
            this.pnlManager.TabIndex = 19;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpManager);
            this.splitContainer1.Panel1.Controls.Add(this.pnlAdvanced);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.grpBuildResults);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3);
            this.splitContainer1.Size = new System.Drawing.Size(547, 544);
            this.splitContainer1.SplitterDistance = 189;
            this.splitContainer1.TabIndex = 17;
            // 
            // pnlAdvanced
            // 
            this.pnlAdvanced.Controls.Add(this.lblAdvanced);
            this.pnlAdvanced.Controls.Add(this.grpAdvanced);
            this.pnlAdvanced.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAdvanced.Location = new System.Drawing.Point(3, 171);
            this.pnlAdvanced.Name = "pnlAdvanced";
            this.pnlAdvanced.Size = new System.Drawing.Size(541, 15);
            this.pnlAdvanced.TabIndex = 17;
            // 
            // lblAdvanced
            // 
            this.lblAdvanced.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.lblAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAdvanced.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblAdvanced.Location = new System.Drawing.Point(0, 0);
            this.lblAdvanced.Name = "lblAdvanced";
            this.lblAdvanced.Size = new System.Drawing.Size(541, 13);
            this.lblAdvanced.TabIndex = 0;
            this.lblAdvanced.Text = "Advanced Runtime Settings (use with caution) >>";
            this.lblAdvanced.Click += new System.EventHandler(this.lblAdvanced_Click);
            // 
            // grpAdvanced
            // 
            this.grpAdvanced.Controls.Add(this.label5);
            this.grpAdvanced.Controls.Add(this.ddOverrideLogDatabase);
            this.grpAdvanced.Controls.Add(this.chkNotTransactional);
            this.grpAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAdvanced.Location = new System.Drawing.Point(0, 0);
            this.grpAdvanced.Name = "grpAdvanced";
            this.grpAdvanced.Size = new System.Drawing.Size(541, 15);
            this.grpAdvanced.TabIndex = 16;
            this.grpAdvanced.TabStop = false;
            // 
            // grpBuildResults
            // 
            this.grpBuildResults.Controls.Add(this.lstBuild);
            this.grpBuildResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBuildResults.Enabled = false;
            this.grpBuildResults.Location = new System.Drawing.Point(3, 3);
            this.grpBuildResults.Margin = new System.Windows.Forms.Padding(6);
            this.grpBuildResults.Name = "grpBuildResults";
            this.grpBuildResults.Size = new System.Drawing.Size(541, 345);
            this.grpBuildResults.TabIndex = 16;
            this.grpBuildResults.TabStop = false;
            this.grpBuildResults.Text = "Build Results";
            // 
            // pnlBuildScripts
            // 
            this.pnlBuildScripts.Controls.Add(this.grbBuildScripts);
            this.pnlBuildScripts.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlBuildScripts.Location = new System.Drawing.Point(0, 56);
            this.pnlBuildScripts.Name = "pnlBuildScripts";
            this.pnlBuildScripts.Size = new System.Drawing.Size(522, 544);
            this.pnlBuildScripts.TabIndex = 20;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(522, 56);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 544);
            this.splitter1.TabIndex = 21;
            this.splitter1.TabStop = false;
            // 
            // openFileAutoScript
            // 
            this.openFileAutoScript.Filter = "AutoScript *.sqlauto|*.sqlauto|All Files *.*|*.*";
            this.openFileAutoScript.Title = "Add Auto Script Registration";
            // 
            // fdrSaveScripts
            // 
            this.fdrSaveScripts.Description = "Save Script Files";
            // 
            // saveCombinedScript
            // 
            this.saveCombinedScript.DefaultExt = "sql";
            this.saveCombinedScript.Filter = "SQL Files *.sql|*.sql|All Files *.*|*.*";
            this.saveCombinedScript.Title = "Save Combined Scripts";
            // 
            // bgBuildProcess
            // 
            this.bgBuildProcess.WorkerReportsProgress = true;
            this.bgBuildProcess.WorkerSupportsCancellation = true;
            this.bgBuildProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgBuildProcess_DoWork);
            this.bgBuildProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgBuildProcess_ProgressChanged);
            this.bgBuildProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgBuildProcess_RunWorkerCompleted);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(150, 150);
            this.toolStripContainer1.Location = new System.Drawing.Point(8, 8);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(150, 175);
            this.toolStripContainer1.TabIndex = 22;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer2
            // 
            // 
            // toolStripContainer2.ContentPanel
            // 
            this.toolStripContainer2.ContentPanel.AutoScroll = true;
            this.toolStripContainer2.ContentPanel.Controls.Add(this.pnlManager);
            this.toolStripContainer2.ContentPanel.Controls.Add(this.splitter1);
            this.toolStripContainer2.ContentPanel.Controls.Add(this.pnlBuildScripts);
            this.toolStripContainer2.ContentPanel.Controls.Add(this.settingsControl1);
            this.toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(1073, 600);
            this.toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer2.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer2.Name = "toolStripContainer2";
            this.toolStripContainer2.Size = new System.Drawing.Size(1073, 624);
            this.toolStripContainer2.TabIndex = 23;
            this.toolStripContainer2.Text = "toolStripContainer2";
            // 
            // toolStripContainer2.TopToolStripPanel
            // 
            this.toolStripContainer2.TopToolStripPanel.Controls.Add(this.mainMenu1);
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 0);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "(select / create project)";
            this.settingsControl1.ProjectLabelText = "Project File:";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(1073, 56);
            this.settingsControl1.TabIndex = 17;
            this.settingsControl1.Click += new System.EventHandler(this.settingsControl1_Click);
            this.settingsControl1.DoubleClick += new System.EventHandler(this.settingsControl1_DoubleClick);
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // bgCheckForUpdates
            // 
            this.bgCheckForUpdates.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgCheckForUpdates_DoWork);
            this.bgCheckForUpdates.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgCheckForUpdates_RunWorkerCompleted);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statScriptCount,
            this.statBuildTime,
            this.statScriptTime,
            this.toolStripStatusLabel1,
            this.statControlStatusLabel,
            this.progressBuild});
            this.statusStrip1.Location = new System.Drawing.Point(0, 624);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1073, 24);
            this.statusStrip1.TabIndex = 24;
            this.statusStrip1.Text = "Build Duration: ";
            // 
            // statGeneral
            // 
            this.statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.statGeneral.Size = new System.Drawing.Size(323, 19);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statScriptCount
            // 
            this.statScriptCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statScriptCount.Name = "statScriptCount";
            this.statScriptCount.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.statScriptCount.Size = new System.Drawing.Size(99, 19);
            this.statScriptCount.Text = "Script Count: 0";
            this.statScriptCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statBuildTime
            // 
            this.statBuildTime.AutoSize = false;
            this.statBuildTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statBuildTime.Name = "statBuildTime";
            this.statBuildTime.Size = new System.Drawing.Size(175, 19);
            this.statBuildTime.Text = "Build Duration: ";
            this.statBuildTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statScriptTime
            // 
            this.statScriptTime.AutoSize = false;
            this.statScriptTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statScriptTime.Name = "statScriptTime";
            this.statScriptTime.Padding = new System.Windows.Forms.Padding(0, 0, 100, 0);
            this.statScriptTime.Size = new System.Drawing.Size(175, 19);
            this.statScriptTime.Text = "Script Duration (sec):";
            this.statScriptTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(107, 19);
            this.toolStripStatusLabel1.Text = "Source Controlled?";
            // 
            // statControlStatusLabel
            // 
            this.statControlStatusLabel.AutoToolTip = true;
            this.statControlStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statControlStatusLabel.ForeColor = System.Drawing.Color.Red;
            this.statControlStatusLabel.Name = "statControlStatusLabel";
            this.statControlStatusLabel.Size = new System.Drawing.Size(27, 19);
            this.statControlStatusLabel.Text = "No";
            this.statControlStatusLabel.ToolTipText = "If under source control, files will be checked out and added automatically.\r\nChan" +
    "ges will need to be committed/checked-in manually.\r\n";
            // 
            // progressBuild
            // 
            this.progressBuild.Name = "progressBuild";
            this.progressBuild.Size = new System.Drawing.Size(150, 18);
            // 
            // tmrBuild
            // 
            this.tmrBuild.Interval = 1000;
            this.tmrBuild.Tick += new System.EventHandler(this.tmrBuild_Tick);
            // 
            // tmrScript
            // 
            this.tmrScript.Interval = 1000;
            this.tmrScript.Tick += new System.EventHandler(this.tmrScript_Tick);
            // 
            // bgLoadZipFle
            // 
            this.bgLoadZipFle.WorkerReportsProgress = true;
            this.bgLoadZipFle.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgLoadZipFle_DoWork);
            this.bgLoadZipFle.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgLoadZipFle_RunWorkerCompleted);
            // 
            // bgRefreshScriptList
            // 
            this.bgRefreshScriptList.WorkerReportsProgress = true;
            this.bgRefreshScriptList.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgRefreshScriptList_DoWork);
            this.bgRefreshScriptList.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgRefreshScriptList_ProgressChanged);
            this.bgRefreshScriptList.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgRefreshScriptList_RunWorkerCompleted);
            // 
            // bgObjectScripting
            // 
            this.bgObjectScripting.WorkerReportsProgress = true;
            this.bgObjectScripting.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgObjectScripting_DoWork);
            this.bgObjectScripting.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgObjectScripting_ProgressChanged);
            this.bgObjectScripting.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgObjectScripting_RunWorkerCompleted);
            // 
            // bgGetObjectList
            // 
            this.bgGetObjectList.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgGetObjectList_DoWork);
            this.bgGetObjectList.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgGetObjectList_RunWorkerCompleted);
            // 
            // openFileDataExtract
            // 
            this.openFileDataExtract.Filter = "Data Extract *.data|*.data|All Files *.*|*.*";
            this.openFileDataExtract.RestoreDirectory = true;
            this.openFileDataExtract.Title = "Open Data Extract File";
            // 
            // openSbxFileDialog
            // 
            this.openSbxFileDialog.CheckFileExists = false;
            this.openSbxFileDialog.DefaultExt = "xml";
            this.openSbxFileDialog.Filter = "Sql Build Manager Control File (*.sbx)|*.sbx|Xml Files (*.xml)|*.xml|All Files|*." +
    "*";
            this.openSbxFileDialog.Title = "Open or Create New Sql Build Manager Control File";
            // 
            // saveScriptsToPackage
            // 
            this.saveScriptsToPackage.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
    "iles (*.zip)|*.zip|All Files|*.*";
            this.saveScriptsToPackage.Title = "Save scripts to build package";
            // 
            // bgEnterpriseSettings
            // 
            this.bgEnterpriseSettings.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgEnterpriseSettings_DoWork);
            this.bgEnterpriseSettings.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgEnterpriseSettings_RunWorkerCompleted);
            // 
            // bgBulkAdd
            // 
            this.bgBulkAdd.WorkerReportsProgress = true;
            this.bgBulkAdd.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgBulkAdd_DoWork);
            this.bgBulkAdd.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgBulkAdd_ProgressChanged);
            this.bgBulkAdd.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgBulkAdd_RunWorkerCompleted);
            // 
            // bgPolicyCheck
            // 
            this.bgPolicyCheck.WorkerReportsProgress = true;
            this.bgPolicyCheck.WorkerSupportsCancellation = true;
            this.bgPolicyCheck.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgPolicyCheck_DoWork);
            this.bgPolicyCheck.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgPolicyCheck_ProgressChanged);
            this.bgPolicyCheck.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgPolicyCheck_RunWorkerCompleted);
            // 
            // savePolicyViolationCsv
            // 
            this.savePolicyViolationCsv.DefaultExt = "csv";
            this.savePolicyViolationCsv.Filter = "CSV *.csv|*.csv|All Files *.*|*.*";
            this.savePolicyViolationCsv.Title = "Save Policy Violations as CSV";
            // 
            // bgBulkAddStep2
            // 
            this.bgBulkAddStep2.WorkerReportsProgress = true;
            this.bgBulkAddStep2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgBulkAddStep2_DoWork);
            this.bgBulkAddStep2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgBulkAddStep2_ProgressChanged);
            this.bgBulkAddStep2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgBulkAddStep2_RunWorkerCompleted);
            // 
            // bgCodeReview
            // 
            this.bgCodeReview.WorkerReportsProgress = true;
            this.bgCodeReview.WorkerSupportsCancellation = true;
            this.bgCodeReview.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgCodeReview_DoWork);
            this.bgCodeReview.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgCodeReview_ProgressChanged);
            this.bgCodeReview.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgCodeReview_RunWorkerCompleted);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.pictureBox1.Location = new System.Drawing.Point(519, 16);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(17, 16);
            this.pictureBox1.TabIndex = 28;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Click for Help on Build Settings");
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_Click);
            // 
            // mnuDisplayRowResults
            // 
            this.mnuDisplayRowResults.Image = global::SqlSync.Properties.Resources.Book_Open;
            this.mnuDisplayRowResults.MergeIndex = 0;
            this.mnuDisplayRowResults.Name = "mnuDisplayRowResults";
            this.mnuDisplayRowResults.Size = new System.Drawing.Size(152, 22);
            this.mnuDisplayRowResults.Text = "Display Results";
            this.mnuDisplayRowResults.Click += new System.EventHandler(this.mnuDisplayRowResults_Click);
            // 
            // mnuEditFromResults
            // 
            this.mnuEditFromResults.Image = global::SqlSync.Properties.Resources.Script_Edit;
            this.mnuEditFromResults.MergeIndex = 2;
            this.mnuEditFromResults.Name = "mnuEditFromResults";
            this.mnuEditFromResults.Size = new System.Drawing.Size(152, 22);
            this.mnuEditFromResults.Text = "Edit File";
            this.mnuEditFromResults.Click += new System.EventHandler(this.mnuEditFromResults_Click);
            // 
            // mnuOpenRunScriptFile
            // 
            this.mnuOpenRunScriptFile.Image = global::SqlSync.Properties.Resources.Script_Load;
            this.mnuOpenRunScriptFile.MergeIndex = 1;
            this.mnuOpenRunScriptFile.Name = "mnuOpenRunScriptFile";
            this.mnuOpenRunScriptFile.Size = new System.Drawing.Size(152, 22);
            this.mnuOpenRunScriptFile.Text = "Open File";
            this.mnuOpenRunScriptFile.Click += new System.EventHandler(this.mnuOpenRunScriptFile_Click);
            // 
            // btnSlideBuildScripts
            // 
            this.btnSlideBuildScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSlideBuildScripts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSlideBuildScripts.FlatAppearance.BorderSize = 0;
            this.btnSlideBuildScripts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSlideBuildScripts.ImageIndex = 0;
            this.btnSlideBuildScripts.ImageList = this.imageListSlide;
            this.btnSlideBuildScripts.Location = new System.Drawing.Point(497, 485);
            this.btnSlideBuildScripts.Margin = new System.Windows.Forms.Padding(0);
            this.btnSlideBuildScripts.Name = "btnSlideBuildScripts";
            this.btnSlideBuildScripts.Size = new System.Drawing.Size(15, 15);
            this.btnSlideBuildScripts.TabIndex = 17;
            this.toolTip1.SetToolTip(this.btnSlideBuildScripts, "Toggle Date displays");
            this.btnSlideBuildScripts.UseVisualStyleBackColor = true;
            this.btnSlideBuildScripts.Click += new System.EventHandler(this.btnSlideBuildScripts_Click);
            // 
            // mnuEditFile
            // 
            this.mnuEditFile.Image = global::SqlSync.Properties.Resources.Script_Edit;
            this.mnuEditFile.MergeIndex = 1;
            this.mnuEditFile.Name = "mnuEditFile";
            this.mnuEditFile.Size = new System.Drawing.Size(388, 22);
            this.mnuEditFile.Text = "Edit File";
            this.mnuEditFile.Click += new System.EventHandler(this.mnuEditFile_Click);
            // 
            // makeFileWriteableremoveToolStripMenuItem
            // 
            this.makeFileWriteableremoveToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Key;
            this.makeFileWriteableremoveToolStripMenuItem.Name = "makeFileWriteableremoveToolStripMenuItem";
            this.makeFileWriteableremoveToolStripMenuItem.Size = new System.Drawing.Size(388, 22);
            this.makeFileWriteableremoveToolStripMenuItem.Text = "Make file writeable (remove Read Only attribute)";
            this.makeFileWriteableremoveToolStripMenuItem.Click += new System.EventHandler(this.makeFileWriteableremoveToolStripMenuItem_Click);
            // 
            // mnuAddSqlScriptText
            // 
            this.mnuAddSqlScriptText.Image = global::SqlSync.Properties.Resources.Script_New;
            this.mnuAddSqlScriptText.MergeIndex = 4;
            this.mnuAddSqlScriptText.Name = "mnuAddSqlScriptText";
            this.mnuAddSqlScriptText.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuAddSqlScriptText.Size = new System.Drawing.Size(388, 22);
            this.mnuAddSqlScriptText.Text = "Add New Sql Script (Text)";
            this.mnuAddSqlScriptText.Click += new System.EventHandler(this.mnuAddSqlScriptText_Click);
            // 
            // mnuAddScript
            // 
            this.mnuAddScript.Image = global::SqlSync.Properties.Resources.Script_Load;
            this.mnuAddScript.MergeIndex = 3;
            this.mnuAddScript.Name = "mnuAddScript";
            this.mnuAddScript.Size = new System.Drawing.Size(388, 22);
            this.mnuAddScript.Text = "Add New File";
            this.mnuAddScript.Click += new System.EventHandler(this.mnuAddScript_Click);
            // 
            // mnuUpdatePopulates
            // 
            this.mnuUpdatePopulates.Enabled = false;
            this.mnuUpdatePopulates.Image = global::SqlSync.Properties.Resources.DB_Refresh;
            this.mnuUpdatePopulates.MergeIndex = 6;
            this.mnuUpdatePopulates.Name = "mnuUpdatePopulates";
            this.mnuUpdatePopulates.Size = new System.Drawing.Size(388, 22);
            this.mnuUpdatePopulates.Text = "Update Code Table Populate Scripts";
            this.mnuUpdatePopulates.Click += new System.EventHandler(this.mnuUpdatePopulates_Click);
            // 
            // mnuUpdateObjectScripts
            // 
            this.mnuUpdateObjectScripts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuObjectScripts_FileDefault,
            this.mnuObjectScripts_CurrentSettings});
            this.mnuUpdateObjectScripts.Enabled = false;
            this.mnuUpdateObjectScripts.Image = global::SqlSync.Properties.Resources.Update;
            this.mnuUpdateObjectScripts.MergeIndex = 7;
            this.mnuUpdateObjectScripts.Name = "mnuUpdateObjectScripts";
            this.mnuUpdateObjectScripts.Size = new System.Drawing.Size(388, 22);
            this.mnuUpdateObjectScripts.Text = "Update Object Create Scripts";
            // 
            // mnuObjectScripts_FileDefault
            // 
            this.mnuObjectScripts_FileDefault.Name = "mnuObjectScripts_FileDefault";
            this.mnuObjectScripts_FileDefault.Size = new System.Drawing.Size(346, 22);
            this.mnuObjectScripts_FileDefault.Text = "Using File Default Setting";
            this.mnuObjectScripts_FileDefault.Click += new System.EventHandler(this.mnuObjectScripts_FileDefault_Click);
            // 
            // mnuObjectScripts_CurrentSettings
            // 
            this.mnuObjectScripts_CurrentSettings.Name = "mnuObjectScripts_CurrentSettings";
            this.mnuObjectScripts_CurrentSettings.Size = new System.Drawing.Size(346, 22);
            this.mnuObjectScripts_CurrentSettings.Text = "Using Current Server and Database/Override Setting";
            this.mnuObjectScripts_CurrentSettings.Click += new System.EventHandler(this.mnuObjectScripts_CurrentSettings_Click);
            // 
            // mnuCreateExportFile
            // 
            this.mnuCreateExportFile.Image = global::SqlSync.Properties.Resources.Export;
            this.mnuCreateExportFile.MergeIndex = 9;
            this.mnuCreateExportFile.Name = "mnuCreateExportFile";
            this.mnuCreateExportFile.Size = new System.Drawing.Size(388, 22);
            this.mnuCreateExportFile.Text = "Export Selected Script Entries";
            this.mnuCreateExportFile.Click += new System.EventHandler(this.mnuCreateExportFile_Click);
            // 
            // mnuRemoveScriptFile
            // 
            this.mnuRemoveScriptFile.Image = global::SqlSync.Properties.Resources.Script_Delete;
            this.mnuRemoveScriptFile.MergeIndex = 10;
            this.mnuRemoveScriptFile.Name = "mnuRemoveScriptFile";
            this.mnuRemoveScriptFile.Size = new System.Drawing.Size(388, 22);
            this.mnuRemoveScriptFile.Text = "Remove File(s)";
            this.mnuRemoveScriptFile.Click += new System.EventHandler(this.mnuRemoveScriptFile_Click);
            // 
            // mnuTryScript
            // 
            this.mnuTryScript.Image = global::SqlSync.Properties.Resources.db_next;
            this.mnuTryScript.MergeIndex = 12;
            this.mnuTryScript.Name = "mnuTryScript";
            this.mnuTryScript.Size = new System.Drawing.Size(388, 22);
            this.mnuTryScript.Text = "Try Script against Database (Rollback)";
            this.mnuTryScript.Click += new System.EventHandler(this.mnuTryScript_Click);
            // 
            // mnuRunScript
            // 
            this.mnuRunScript.Image = global::SqlSync.Properties.Resources.db_edit_green;
            this.mnuRunScript.MergeIndex = 13;
            this.mnuRunScript.Name = "mnuRunScript";
            this.mnuRunScript.Size = new System.Drawing.Size(388, 22);
            this.mnuRunScript.Text = "Run Script against Database (Commit)";
            this.mnuRunScript.Click += new System.EventHandler(this.mnuRunScript_Click);
            // 
            // mnuViewRunHistory
            // 
            this.mnuViewRunHistory.Image = global::SqlSync.Properties.Resources.History;
            this.mnuViewRunHistory.MergeIndex = 15;
            this.mnuViewRunHistory.Name = "mnuViewRunHistory";
            this.mnuViewRunHistory.Size = new System.Drawing.Size(388, 22);
            this.mnuViewRunHistory.Text = "View packaged script run history against current server";
            this.mnuViewRunHistory.Click += new System.EventHandler(this.mnuViewRunHistory_Click);
            // 
            // renameScriptFIleToolStripMenuItem
            // 
            this.renameScriptFIleToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Rename;
            this.renameScriptFIleToolStripMenuItem.Name = "renameScriptFIleToolStripMenuItem";
            this.renameScriptFIleToolStripMenuItem.Size = new System.Drawing.Size(388, 22);
            this.renameScriptFIleToolStripMenuItem.Text = "Rename Script File";
            this.renameScriptFIleToolStripMenuItem.Click += new System.EventHandler(this.renameScriptFIleToolStripMenuItem_Click);
            // 
            // scriptNotRunOnCurrentServerToolStripMenuItem
            // 
            this.scriptNotRunOnCurrentServerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("scriptNotRunOnCurrentServerToolStripMenuItem.Image")));
            this.scriptNotRunOnCurrentServerToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.scriptNotRunOnCurrentServerToolStripMenuItem.Name = "scriptNotRunOnCurrentServerToolStripMenuItem";
            this.scriptNotRunOnCurrentServerToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.scriptNotRunOnCurrentServerToolStripMenuItem.Text = "...SBM version is newer than server version (OK)";
            // 
            // serverVersionIsNewerThanSBMVersionToolStripMenuItem
            // 
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("serverVersionIsNewerThanSBMVersionToolStripMenuItem.Image")));
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem.Name = "serverVersionIsNewerThanSBMVersionToolStripMenuItem";
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.serverVersionIsNewerThanSBMVersionToolStripMenuItem.Text = "...server version is newer than SBM version (possible conflict, compare before ru" +
    "nning)";
            // 
            // runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem
            // 
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Image")));
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Name = "runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem";
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Text = "...unchanged since last run";
            // 
            // alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem
            // 
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Image")));
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Name = "alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem";
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Text = "...marked as \"Run Once\" (will be skipped if run again)";
            // 
            // runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem
            // 
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Image")));
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Name = "runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem";
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Text = "...changed in build file since last run";
            // 
            // runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem
            // 
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.I" +
        "mage")));
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Name = "runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem";
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Text = "...manually changed in database (out of sync with SBM and should be compared befo" +
    "re re-running)";
            this.runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.ToolTipText = "This status is valid for Stored Procedures and Functions and is\r\nbased on the las" +
    "t commit date from the SBM file vs. the change date\r\nof the Procedure or Functio" +
    "n.";
            // 
            // fileMissingToolStripMenuItem
            // 
            this.fileMissingToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            this.fileMissingToolStripMenuItem.Name = "fileMissingToolStripMenuItem";
            this.fileMissingToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            this.fileMissingToolStripMenuItem.Text = "File is missing from file system or build file. This must be corrected prior to e" +
    "xecution. ";
            // 
            // policyChecksNotRunToolStripMenuItem
            // 
            this.policyChecksNotRunToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.policyChecksNotRunToolStripMenuItem.Name = "policyChecksNotRunToolStripMenuItem";
            this.policyChecksNotRunToolStripMenuItem.Size = new System.Drawing.Size(432, 22);
            this.policyChecksNotRunToolStripMenuItem.Text = "Policy checks not run";
            // 
            // policyCheckFailedActionRequiredToolStripMenuItem
            // 
            this.policyCheckFailedActionRequiredToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Exclamation_square;
            this.policyCheckFailedActionRequiredToolStripMenuItem.Name = "policyCheckFailedActionRequiredToolStripMenuItem";
            this.policyCheckFailedActionRequiredToolStripMenuItem.Size = new System.Drawing.Size(432, 22);
            this.policyCheckFailedActionRequiredToolStripMenuItem.Text = "Policy checks failed - ACTION REQUIRED!";
            // 
            // policyWarningActionMayBeRequiredToolStripMenuItem
            // 
            this.policyWarningActionMayBeRequiredToolStripMenuItem.Image = global::SqlSync.Properties.Resources.exclamation_shield_frame;
            this.policyWarningActionMayBeRequiredToolStripMenuItem.Name = "policyWarningActionMayBeRequiredToolStripMenuItem";
            this.policyWarningActionMayBeRequiredToolStripMenuItem.Size = new System.Drawing.Size(432, 22);
            this.policyWarningActionMayBeRequiredToolStripMenuItem.Text = "Policy warning - action may be required.";
            // 
            // passesPolicyChecksToolStripMenuItem
            // 
            this.passesPolicyChecksToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Tick;
            this.passesPolicyChecksToolStripMenuItem.Name = "passesPolicyChecksToolStripMenuItem";
            this.passesPolicyChecksToolStripMenuItem.Size = new System.Drawing.Size(432, 22);
            this.passesPolicyChecksToolStripMenuItem.Text = "Policy checks passed";
            // 
            // reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem
            // 
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Discuss;
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Name = "reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem";
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Size = new System.Drawing.Size(432, 22);
            this.reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Text = "Review warning - this script should be examined before deployment";
            // 
            // waitingOnStatusCheckToolStripMenuItem
            // 
            this.waitingOnStatusCheckToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Wait;
            this.waitingOnStatusCheckToolStripMenuItem.Name = "waitingOnStatusCheckToolStripMenuItem";
            this.waitingOnStatusCheckToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.waitingOnStatusCheckToolStripMenuItem.Text = "Waiting on status check";
            // 
            // reviewNotStartedToolStripMenuItem
            // 
            this.reviewNotStartedToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Hand;
            this.reviewNotStartedToolStripMenuItem.Name = "reviewNotStartedToolStripMenuItem";
            this.reviewNotStartedToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.reviewNotStartedToolStripMenuItem.Text = "Review Not Started";
            // 
            // reviewInProgressToolStripMenuItem
            // 
            this.reviewInProgressToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Clock;
            this.reviewInProgressToolStripMenuItem.Name = "reviewInProgressToolStripMenuItem";
            this.reviewInProgressToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.reviewInProgressToolStripMenuItem.Text = "Review in Progress";
            // 
            // reviewAcceptedToolStripMenuItem
            // 
            this.reviewAcceptedToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Ok_blueSquare;
            this.reviewAcceptedToolStripMenuItem.Name = "reviewAcceptedToolStripMenuItem";
            this.reviewAcceptedToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.reviewAcceptedToolStripMenuItem.Text = "Review Accepted";
            // 
            // reviewAcceptedByDBAToolStripMenuItem
            // 
            this.reviewAcceptedByDBAToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Ok_greenSquare;
            this.reviewAcceptedByDBAToolStripMenuItem.Name = "reviewAcceptedByDBAToolStripMenuItem";
            this.reviewAcceptedByDBAToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.reviewAcceptedByDBAToolStripMenuItem.Text = "Review Accepted by DBA";
            // 
            // mnuActionMain
            // 
            this.mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoadProject,
            this.loadNewDirectoryControlFilesbxToolStripMenuItem,
            this.toolStripSeparator11,
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem,
            this.toolStripSeparator10,
            this.mnuChangeSqlServer,
            this.toolStripSeparator1,
            this.settingsToolStripMenuItem,
            this.mnuMainAddSqlScript,
            this.mnuMainAddNewFile,
            this.toolStripSeparator4,
            this.mnuImportScriptFromFile,
            this.mnuCompare,
            this.menuItem12,
            this.mnuExportScriptText,
            this.menuItem15,
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem,
            this.remoteExecutionServiceToolStripMenuItem,
            this.toolStripSeparator7,
            this.mnuFileMRU,
            this.toolStripSeparator13,
            this.exitToolStripMenuItem});
            this.mnuActionMain.Image = global::SqlSync.Properties.Resources.Execute;
            this.mnuActionMain.MergeIndex = 0;
            this.mnuActionMain.Name = "mnuActionMain";
            this.mnuActionMain.Size = new System.Drawing.Size(70, 20);
            this.mnuActionMain.Text = "&Action";
            this.mnuActionMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mnuActionMain_MouseUp);
            // 
            // mnuLoadProject
            // 
            this.mnuLoadProject.Image = global::SqlSync.Properties.Resources.Open;
            this.mnuLoadProject.MergeIndex = 0;
            this.mnuLoadProject.Name = "mnuLoadProject";
            this.mnuLoadProject.Size = new System.Drawing.Size(344, 22);
            this.mnuLoadProject.Text = "&Load/New Project File (*.sbm)";
            this.mnuLoadProject.ToolTipText = "Open existing or create new self contained build project file (.sbm)";
            this.mnuLoadProject.Click += new System.EventHandler(this.mnuLoadProject_Click);
            // 
            // loadNewDirectoryControlFilesbxToolStripMenuItem
            // 
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.Image = global::SqlSync.Properties.Resources.open_xml;
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.Name = "loadNewDirectoryControlFilesbxToolStripMenuItem";
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.Text = "Load/New Directory Based Build Control File (*.sbx)";
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.ToolTipText = "Open existing or create new directory control file (.sbx) to manage \"loose\" scrip" +
    "ts ";
            this.loadNewDirectoryControlFilesbxToolStripMenuItem.Click += new System.EventHandler(this.loadNewDirectoryControlFilesbxToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(341, 6);
            // 
            // packageScriptsIntoProjectFilesbmToolStripMenuItem
            // 
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = false;
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Box_Open_2;
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Name = "packageScriptsIntoProjectFilesbmToolStripMenuItem";
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Text = "Package scripts into project file (.sbm)";
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Click += new System.EventHandler(this.packageScriptsIntoProjectFilesbmToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(341, 6);
            // 
            // mnuChangeSqlServer
            // 
            this.mnuChangeSqlServer.Image = global::SqlSync.Properties.Resources.Server1;
            this.mnuChangeSqlServer.MergeIndex = 1;
            this.mnuChangeSqlServer.Name = "mnuChangeSqlServer";
            this.mnuChangeSqlServer.Size = new System.Drawing.Size(344, 22);
            this.mnuChangeSqlServer.Text = "&Change Sql Server Connection";
            this.mnuChangeSqlServer.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(341, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.maintainManualDatabaseEntriesToolStripMenuItem,
            this.maintainDefaultScriptRegistryToolStripMenuItem,
            this.toolStripSeparator5,
            this.scriptTagsRequiredToolStripMenuItem,
            this.createSQLLogOfBuildRunsToolStripMenuItem,
            this.runPolicyCheckingonloadtoolStripMenuItem,
            this.scriptPrimaryKeyWithTableToolStripMenuItem,
            this.toolStripSeparator15,
            this.defaultScriptTimeoutsecondsToolStripMenuItem,
            this.mnuDefaultScriptTimeout,
            this.toolStripSeparator16,
            this.sourceControlServerURLToolStripMenuItem,
            this.sourceControlServerURLTextboxMenuItem});
            this.settingsToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Wizard;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // maintainManualDatabaseEntriesToolStripMenuItem
            // 
            this.maintainManualDatabaseEntriesToolStripMenuItem.Name = "maintainManualDatabaseEntriesToolStripMenuItem";
            this.maintainManualDatabaseEntriesToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.maintainManualDatabaseEntriesToolStripMenuItem.Text = "Maintain Manual Database Entries";
            this.maintainManualDatabaseEntriesToolStripMenuItem.Click += new System.EventHandler(this.maintainManualDatabaseEntriesToolStripMenuItem_Click);
            // 
            // maintainDefaultScriptRegistryToolStripMenuItem
            // 
            this.maintainDefaultScriptRegistryToolStripMenuItem.Name = "maintainDefaultScriptRegistryToolStripMenuItem";
            this.maintainDefaultScriptRegistryToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.maintainDefaultScriptRegistryToolStripMenuItem.Text = "Maintain Default Script Registry";
            this.maintainDefaultScriptRegistryToolStripMenuItem.Click += new System.EventHandler(this.maintainDefaultScriptRegistryToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(307, 6);
            // 
            // scriptTagsRequiredToolStripMenuItem
            // 
            this.scriptTagsRequiredToolStripMenuItem.CheckOnClick = true;
            this.scriptTagsRequiredToolStripMenuItem.Name = "scriptTagsRequiredToolStripMenuItem";
            this.scriptTagsRequiredToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.scriptTagsRequiredToolStripMenuItem.Text = "Script Tags Required";
            this.scriptTagsRequiredToolStripMenuItem.ToolTipText = "Check as to whether or not this build file requires script tags for all of its sc" +
    "ripts.";
            this.scriptTagsRequiredToolStripMenuItem.Click += new System.EventHandler(this.scriptTagsRequiredToolStripMenuItem_Click);
            // 
            // createSQLLogOfBuildRunsToolStripMenuItem
            // 
            this.createSQLLogOfBuildRunsToolStripMenuItem.CheckOnClick = true;
            this.createSQLLogOfBuildRunsToolStripMenuItem.Name = "createSQLLogOfBuildRunsToolStripMenuItem";
            this.createSQLLogOfBuildRunsToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.createSQLLogOfBuildRunsToolStripMenuItem.Text = "Create SQL Log of Build Runs";
            this.createSQLLogOfBuildRunsToolStripMenuItem.ToolTipText = "Check as to whether or not to create a SQL script log \r\nof builds as they are run" +
    ".";
            this.createSQLLogOfBuildRunsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.createSQLLogOfBuildRunsToolStripMenuItem_Click);
            this.createSQLLogOfBuildRunsToolStripMenuItem.Click += new System.EventHandler(this.createSQLLogOfBuildRunsToolStripMenuItem_Click);
            // 
            // runPolicyCheckingonloadtoolStripMenuItem
            // 
            this.runPolicyCheckingonloadtoolStripMenuItem.CheckOnClick = true;
            this.runPolicyCheckingonloadtoolStripMenuItem.Name = "runPolicyCheckingonloadtoolStripMenuItem";
            this.runPolicyCheckingonloadtoolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.runPolicyCheckingonloadtoolStripMenuItem.Text = "Run Policy Checking on load";
            this.runPolicyCheckingonloadtoolStripMenuItem.ToolTipText = "Check as to whether or not to create a SQL script log \r\nof builds as they are run" +
    ".";
            this.runPolicyCheckingonloadtoolStripMenuItem.Click += new System.EventHandler(this.runPolicyCheckingonloadtoolStripMenuItem_Click);
            // 
            // scriptPrimaryKeyWithTableToolStripMenuItem
            // 
            this.scriptPrimaryKeyWithTableToolStripMenuItem.Name = "scriptPrimaryKeyWithTableToolStripMenuItem";
            this.scriptPrimaryKeyWithTableToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.scriptPrimaryKeyWithTableToolStripMenuItem.Text = "Script Primary Key with Table";
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(307, 6);
            // 
            // defaultScriptTimeoutsecondsToolStripMenuItem
            // 
            this.defaultScriptTimeoutsecondsToolStripMenuItem.Name = "defaultScriptTimeoutsecondsToolStripMenuItem";
            this.defaultScriptTimeoutsecondsToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.defaultScriptTimeoutsecondsToolStripMenuItem.Text = "Default/Minimum Script Timeout (seconds):";
            // 
            // mnuDefaultScriptTimeout
            // 
            this.mnuDefaultScriptTimeout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.mnuDefaultScriptTimeout.BackColor = System.Drawing.SystemColors.Window;
            this.mnuDefaultScriptTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mnuDefaultScriptTimeout.Name = "mnuDefaultScriptTimeout";
            this.mnuDefaultScriptTimeout.Size = new System.Drawing.Size(50, 23);
            this.mnuDefaultScriptTimeout.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.mnuDefaultScriptTimeout.TextChanged += new System.EventHandler(this.mnuDefaultScriptTimeout_TextChanged);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(307, 6);
            // 
            // sourceControlServerURLToolStripMenuItem
            // 
            this.sourceControlServerURLToolStripMenuItem.Name = "sourceControlServerURLToolStripMenuItem";
            this.sourceControlServerURLToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.sourceControlServerURLToolStripMenuItem.Text = "Source Control Server URL:";
            // 
            // sourceControlServerURLTextboxMenuItem
            // 
            this.sourceControlServerURLTextboxMenuItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sourceControlServerURLTextboxMenuItem.Name = "sourceControlServerURLTextboxMenuItem";
            this.sourceControlServerURLTextboxMenuItem.Size = new System.Drawing.Size(250, 23);
            this.sourceControlServerURLTextboxMenuItem.TextChanged += new System.EventHandler(this.sourceControlServerURLTextboxMenuItem_TextChanged);
            // 
            // mnuMainAddSqlScript
            // 
            this.mnuMainAddSqlScript.Enabled = false;
            this.mnuMainAddSqlScript.Image = global::SqlSync.Properties.Resources.Script_New;
            this.mnuMainAddSqlScript.MergeIndex = 4;
            this.mnuMainAddSqlScript.Name = "mnuMainAddSqlScript";
            this.mnuMainAddSqlScript.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuMainAddSqlScript.Size = new System.Drawing.Size(344, 22);
            this.mnuMainAddSqlScript.Text = "Add New Sql Script (Text)";
            this.mnuMainAddSqlScript.Click += new System.EventHandler(this.mnuAddSqlScriptText_Click);
            // 
            // mnuMainAddNewFile
            // 
            this.mnuMainAddNewFile.Enabled = false;
            this.mnuMainAddNewFile.Image = global::SqlSync.Properties.Resources.Script_Load;
            this.mnuMainAddNewFile.MergeIndex = 3;
            this.mnuMainAddNewFile.Name = "mnuMainAddNewFile";
            this.mnuMainAddNewFile.Size = new System.Drawing.Size(344, 22);
            this.mnuMainAddNewFile.Text = "Add New File";
            this.mnuMainAddNewFile.Click += new System.EventHandler(this.mnuAddScript_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(341, 6);
            // 
            // mnuImportScriptFromFile
            // 
            this.mnuImportScriptFromFile.Enabled = false;
            this.mnuImportScriptFromFile.Image = global::SqlSync.Properties.Resources.Import;
            this.mnuImportScriptFromFile.MergeIndex = 3;
            this.mnuImportScriptFromFile.Name = "mnuImportScriptFromFile";
            this.mnuImportScriptFromFile.Size = new System.Drawing.Size(344, 22);
            this.mnuImportScriptFromFile.Text = "&Import Scripts from Sql Build File";
            this.mnuImportScriptFromFile.Click += new System.EventHandler(this.mnuImportScriptFromFile_Click);
            // 
            // mnuCompare
            // 
            this.mnuCompare.Enabled = false;
            this.mnuCompare.Name = "mnuCompare";
            this.mnuCompare.Size = new System.Drawing.Size(344, 22);
            this.mnuCompare.Text = "Compare Build File To...";
            this.mnuCompare.Click += new System.EventHandler(this.mnuCompare_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.MergeIndex = 4;
            this.menuItem12.Name = "menuItem12";
            this.menuItem12.Size = new System.Drawing.Size(341, 6);
            // 
            // mnuExportScriptText
            // 
            this.mnuExportScriptText.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuIndividualFiles,
            this.mnuCombinedFile,
            this.menuItem22,
            this.mnuIncludeUSE,
            this.mnuIncludeSequence});
            this.mnuExportScriptText.Enabled = false;
            this.mnuExportScriptText.Image = global::SqlSync.Properties.Resources.Export;
            this.mnuExportScriptText.MergeIndex = 6;
            this.mnuExportScriptText.Name = "mnuExportScriptText";
            this.mnuExportScriptText.Size = new System.Drawing.Size(344, 22);
            this.mnuExportScriptText.Text = "Export Scripts To";
            // 
            // mnuIndividualFiles
            // 
            this.mnuIndividualFiles.MergeIndex = 0;
            this.mnuIndividualFiles.Name = "mnuIndividualFiles";
            this.mnuIndividualFiles.Size = new System.Drawing.Size(246, 22);
            this.mnuIndividualFiles.Text = "Individual Script Files";
            this.mnuIndividualFiles.Click += new System.EventHandler(this.mnuIndividualFiles_Click);
            // 
            // mnuCombinedFile
            // 
            this.mnuCombinedFile.MergeIndex = 1;
            this.mnuCombinedFile.Name = "mnuCombinedFile";
            this.mnuCombinedFile.Size = new System.Drawing.Size(246, 22);
            this.mnuCombinedFile.Text = "Consolidated Script File";
            this.mnuCombinedFile.Click += new System.EventHandler(this.mnuCombinedFile_Click);
            // 
            // menuItem22
            // 
            this.menuItem22.MergeIndex = 2;
            this.menuItem22.Name = "menuItem22";
            this.menuItem22.Size = new System.Drawing.Size(243, 6);
            // 
            // mnuIncludeUSE
            // 
            this.mnuIncludeUSE.Checked = true;
            this.mnuIncludeUSE.CheckOnClick = true;
            this.mnuIncludeUSE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuIncludeUSE.MergeIndex = 3;
            this.mnuIncludeUSE.Name = "mnuIncludeUSE";
            this.mnuIncludeUSE.Size = new System.Drawing.Size(246, 22);
            this.mnuIncludeUSE.Text = "Include \"USE\" Statements";
            // 
            // mnuIncludeSequence
            // 
            this.mnuIncludeSequence.Checked = true;
            this.mnuIncludeSequence.CheckOnClick = true;
            this.mnuIncludeSequence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuIncludeSequence.MergeIndex = 4;
            this.mnuIncludeSequence.Name = "mnuIncludeSequence";
            this.mnuIncludeSequence.Size = new System.Drawing.Size(246, 22);
            this.mnuIncludeSequence.Text = "Include Sequence Number Prefix";
            // 
            // menuItem15
            // 
            this.menuItem15.MergeIndex = 7;
            this.menuItem15.Name = "menuItem15";
            this.menuItem15.Size = new System.Drawing.Size(341, 6);
            // 
            // startConfigureMultiServerDatabaseRunToolStripMenuItem
            // 
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Image = global::SqlSync.Properties.Resources.multi_db;
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Name = "startConfigureMultiServerDatabaseRunToolStripMenuItem";
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Text = "Configure Multi Server/Database Run";
            this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Click += new System.EventHandler(this.startConfigureMultiServerDatabaseRunToolStripMenuItem_Click);
            // 
            // remoteExecutionServiceToolStripMenuItem
            // 
            this.remoteExecutionServiceToolStripMenuItem.Enabled = false;
            this.remoteExecutionServiceToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server_Internet;
            this.remoteExecutionServiceToolStripMenuItem.Name = "remoteExecutionServiceToolStripMenuItem";
            this.remoteExecutionServiceToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.remoteExecutionServiceToolStripMenuItem.Text = "Remote Execution Service";
            this.remoteExecutionServiceToolStripMenuItem.Click += new System.EventHandler(this.remoteExecutionServiceToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(341, 6);
            // 
            // mnuFileMRU
            // 
            this.mnuFileMRU.MergeIndex = 8;
            this.mnuFileMRU.Name = "mnuFileMRU";
            this.mnuFileMRU.Size = new System.Drawing.Size(344, 22);
            this.mnuFileMRU.Text = "Recent Files";
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(341, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(344, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // mnuListTop
            // 
            this.mnuListTop.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFindScript,
            this.menuItem9,
            this.menuItem8,
            this.mnuRenumberSequence,
            this.mnuResortByFileType,
            this.mnuExportBuildList,
            this.menuItem10,
            this.mnuBulkAdd,
            this.mnuBulkFromList,
            this.mnuBulkFromFile,
            this.menuItem13,
            this.mnuClearPreviouslyRunBlocks,
            this.toolStripSeparator18,
            this.inferScriptTagFromFileNameToolStripMenuItem});
            this.mnuListTop.Enabled = false;
            this.mnuListTop.Image = global::SqlSync.Properties.Resources.Columns_2;
            this.mnuListTop.MergeIndex = 1;
            this.mnuListTop.Name = "mnuListTop";
            this.mnuListTop.Size = new System.Drawing.Size(53, 20);
            this.mnuListTop.Text = "&List";
            this.mnuListTop.DropDownOpening += new System.EventHandler(this.mnuListTop_DropDownOpening);
            // 
            // mnuFindScript
            // 
            this.mnuFindScript.Image = global::SqlSync.Properties.Resources.Search;
            this.mnuFindScript.MergeIndex = 0;
            this.mnuFindScript.Name = "mnuFindScript";
            this.mnuFindScript.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.mnuFindScript.Size = new System.Drawing.Size(327, 22);
            this.mnuFindScript.Text = "&Find Script by Name";
            this.mnuFindScript.Click += new System.EventHandler(this.mnuFindScript_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Image = global::SqlSync.Properties.Resources.Search_Next;
            this.menuItem9.MergeIndex = 1;
            this.menuItem9.Name = "menuItem9";
            this.menuItem9.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.menuItem9.Size = new System.Drawing.Size(327, 22);
            this.menuItem9.Text = "Find &Again";
            this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.MergeIndex = 2;
            this.menuItem8.Name = "menuItem8";
            this.menuItem8.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuRenumberSequence
            // 
            this.mnuRenumberSequence.Image = global::SqlSync.Properties.Resources.Numbering_2;
            this.mnuRenumberSequence.MergeIndex = 3;
            this.mnuRenumberSequence.Name = "mnuRenumberSequence";
            this.mnuRenumberSequence.Size = new System.Drawing.Size(327, 22);
            this.mnuRenumberSequence.Text = "&Re-number Build Sequence";
            this.mnuRenumberSequence.Click += new System.EventHandler(this.mnuRenumberSequence_Click);
            // 
            // mnuResortByFileType
            // 
            this.mnuResortByFileType.Image = global::SqlSync.Properties.Resources.Sort_Descending;
            this.mnuResortByFileType.MergeIndex = 4;
            this.mnuResortByFileType.Name = "mnuResortByFileType";
            this.mnuResortByFileType.Size = new System.Drawing.Size(327, 22);
            this.mnuResortByFileType.Text = "Re&sort Build By File Type";
            this.mnuResortByFileType.Click += new System.EventHandler(this.mnuResortByFileType_Click);
            // 
            // mnuExportBuildList
            // 
            this.mnuExportBuildList.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExportBuildListToFile,
            this.mnuExportBuildListToClipBoard});
            this.mnuExportBuildList.Enabled = false;
            this.mnuExportBuildList.MergeIndex = 5;
            this.mnuExportBuildList.Name = "mnuExportBuildList";
            this.mnuExportBuildList.Size = new System.Drawing.Size(327, 22);
            this.mnuExportBuildList.Text = "&Export Build List for Documentation";
            // 
            // mnuExportBuildListToFile
            // 
            this.mnuExportBuildListToFile.MergeIndex = 0;
            this.mnuExportBuildListToFile.Name = "mnuExportBuildListToFile";
            this.mnuExportBuildListToFile.Size = new System.Drawing.Size(143, 22);
            this.mnuExportBuildListToFile.Text = "To &File";
            this.mnuExportBuildListToFile.Click += new System.EventHandler(this.mnuExportBuildList_Click);
            // 
            // mnuExportBuildListToClipBoard
            // 
            this.mnuExportBuildListToClipBoard.MergeIndex = 1;
            this.mnuExportBuildListToClipBoard.Name = "mnuExportBuildListToClipBoard";
            this.mnuExportBuildListToClipBoard.Size = new System.Drawing.Size(143, 22);
            this.mnuExportBuildListToClipBoard.Text = "To &Clipboard";
            this.mnuExportBuildListToClipBoard.Click += new System.EventHandler(this.mnuExportBuildListToClipBoard_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.MergeIndex = 5;
            this.menuItem10.Name = "menuItem10";
            this.menuItem10.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuBulkAdd
            // 
            this.mnuBulkAdd.MergeIndex = 6;
            this.mnuBulkAdd.Name = "mnuBulkAdd";
            this.mnuBulkAdd.Size = new System.Drawing.Size(327, 22);
            this.mnuBulkAdd.Text = "&Bulk Add";
            this.mnuBulkAdd.Click += new System.EventHandler(this.mnuBulkAdd_Click);
            // 
            // mnuBulkFromList
            // 
            this.mnuBulkFromList.MergeIndex = 7;
            this.mnuBulkFromList.Name = "mnuBulkFromList";
            this.mnuBulkFromList.Size = new System.Drawing.Size(327, 22);
            this.mnuBulkFromList.Text = "Bulk Add From &List";
            this.mnuBulkFromList.Click += new System.EventHandler(this.mnuBulkFromList_Click);
            // 
            // mnuBulkFromFile
            // 
            this.mnuBulkFromFile.MergeIndex = 8;
            this.mnuBulkFromFile.Name = "mnuBulkFromFile";
            this.mnuBulkFromFile.Size = new System.Drawing.Size(327, 22);
            this.mnuBulkFromFile.Text = "Bulk Add From &Text File";
            this.mnuBulkFromFile.Click += new System.EventHandler(this.mnuBulkFromFile_Click);
            // 
            // menuItem13
            // 
            this.menuItem13.MergeIndex = 9;
            this.menuItem13.Name = "menuItem13";
            this.menuItem13.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuClearPreviouslyRunBlocks
            // 
            this.mnuClearPreviouslyRunBlocks.MergeIndex = 10;
            this.mnuClearPreviouslyRunBlocks.Name = "mnuClearPreviouslyRunBlocks";
            this.mnuClearPreviouslyRunBlocks.Size = new System.Drawing.Size(327, 22);
            this.mnuClearPreviouslyRunBlocks.Text = "&Clear \"Previously Run\" Blocks for Selected Script";
            this.mnuClearPreviouslyRunBlocks.Click += new System.EventHandler(this.mnuClearPreviouslyRunBlocks_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(324, 6);
            // 
            // inferScriptTagFromFileNameToolStripMenuItem
            // 
            this.inferScriptTagFromFileNameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scriptContentsFirstThenFileNameToolStripMenuItem,
            this.fileNameFirstThenSciptContentsToolStripMenuItem,
            this.toolStripSeparator20,
            this.scriptContentsOnlyToolStripMenuItem,
            this.fileNamesOnlyToolStripMenuItem});
            this.inferScriptTagFromFileNameToolStripMenuItem.Name = "inferScriptTagFromFileNameToolStripMenuItem";
            this.inferScriptTagFromFileNameToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.inferScriptTagFromFileNameToolStripMenuItem.Text = "Infer Script Tag from...";
            // 
            // scriptContentsFirstThenFileNameToolStripMenuItem
            // 
            this.scriptContentsFirstThenFileNameToolStripMenuItem.Name = "scriptContentsFirstThenFileNameToolStripMenuItem";
            this.scriptContentsFirstThenFileNameToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.scriptContentsFirstThenFileNameToolStripMenuItem.Tag = "TextOverName";
            this.scriptContentsFirstThenFileNameToolStripMenuItem.Text = "Script Contents first, then File Name";
            this.scriptContentsFirstThenFileNameToolStripMenuItem.ToolTipText = "Scans the script for a tag value in a header comment block. If not found it will " +
    "check the file name.";
            this.scriptContentsFirstThenFileNameToolStripMenuItem.Click += new System.EventHandler(this.InferScriptTag_Click);
            // 
            // fileNameFirstThenSciptContentsToolStripMenuItem
            // 
            this.fileNameFirstThenSciptContentsToolStripMenuItem.Name = "fileNameFirstThenSciptContentsToolStripMenuItem";
            this.fileNameFirstThenSciptContentsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.fileNameFirstThenSciptContentsToolStripMenuItem.Tag = "NameOverText";
            this.fileNameFirstThenSciptContentsToolStripMenuItem.Text = "File Name First, then Scipt Contents";
            this.fileNameFirstThenSciptContentsToolStripMenuItem.ToolTipText = "Checks the file name for a script tag value. If not found, it scans the script fo" +
    "r a tag value in a header comment block.";
            this.fileNameFirstThenSciptContentsToolStripMenuItem.Click += new System.EventHandler(this.InferScriptTag_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(261, 6);
            // 
            // scriptContentsOnlyToolStripMenuItem
            // 
            this.scriptContentsOnlyToolStripMenuItem.Name = "scriptContentsOnlyToolStripMenuItem";
            this.scriptContentsOnlyToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.scriptContentsOnlyToolStripMenuItem.Tag = "ScriptText";
            this.scriptContentsOnlyToolStripMenuItem.Text = "Script Contents Only";
            this.scriptContentsOnlyToolStripMenuItem.ToolTipText = "Scans the script for a tag value in a header comment block.";
            this.scriptContentsOnlyToolStripMenuItem.Click += new System.EventHandler(this.InferScriptTag_Click);
            // 
            // fileNamesOnlyToolStripMenuItem
            // 
            this.fileNamesOnlyToolStripMenuItem.Name = "fileNamesOnlyToolStripMenuItem";
            this.fileNamesOnlyToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.fileNamesOnlyToolStripMenuItem.Tag = "ScriptName";
            this.fileNamesOnlyToolStripMenuItem.Text = "File Names Only";
            this.fileNamesOnlyToolStripMenuItem.ToolTipText = "Checks the file name for a script tag value. ";
            this.fileNamesOnlyToolStripMenuItem.Click += new System.EventHandler(this.InferScriptTag_Click);
            // 
            // mnuScripting
            // 
            this.mnuScripting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDDActiveDatabase,
            this.mnuAddObjectCreate,
            this.scriptingOptionsToolStripMenuItem,
            this.scriptALTERAndCREATEToolStripMenuItem,
            this.includeObjectPermissionsToolStripMenuItem1,
            this.toolStripSeparator3,
            this.mnuAddCodeTablePop,
            this.menuItem3,
            this.mnuUpdatePopulate,
            this.mnuObjectUpdates,
            this.toolStripSeparator19,
            this.createBackoutPackageToolStripMenuItem});
            this.mnuScripting.Enabled = false;
            this.mnuScripting.Image = global::SqlSync.Properties.Resources.Script_Edit;
            this.mnuScripting.MergeIndex = 2;
            this.mnuScripting.Name = "mnuScripting";
            this.mnuScripting.Size = new System.Drawing.Size(82, 20);
            this.mnuScripting.Text = "Scripting";
            // 
            // mnuDDActiveDatabase
            // 
            this.mnuDDActiveDatabase.Font = new System.Drawing.Font("Tahoma", 9F);
            this.mnuDDActiveDatabase.Name = "mnuDDActiveDatabase";
            this.mnuDDActiveDatabase.Size = new System.Drawing.Size(200, 22);
            this.mnuDDActiveDatabase.Text = "<< Select Active Database >>";
            this.mnuDDActiveDatabase.ToolTipText = "Select the database to target for scripting";
            this.mnuDDActiveDatabase.SelectedIndexChanged += new System.EventHandler(this.mnuDDActiveDatabase_SelectedIndexChanged);
            // 
            // mnuAddObjectCreate
            // 
            this.mnuAddObjectCreate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAddStoredProcs,
            this.mnuAddFunctions,
            this.mnuAddViews,
            this.mnuAddTables,
            this.mnuAddTriggers,
            this.toolStripSeparator21,
            this.mnuAddRoles});
            this.mnuAddObjectCreate.Enabled = false;
            this.mnuAddObjectCreate.MergeIndex = 1;
            this.mnuAddObjectCreate.Name = "mnuAddObjectCreate";
            this.mnuAddObjectCreate.Size = new System.Drawing.Size(263, 22);
            this.mnuAddObjectCreate.Text = "Add Object Create Scripts";
            // 
            // mnuAddStoredProcs
            // 
            this.mnuAddStoredProcs.Image = global::SqlSync.Properties.Resources.storedproc1;
            this.mnuAddStoredProcs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuAddStoredProcs.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            this.mnuAddStoredProcs.MergeIndex = 0;
            this.mnuAddStoredProcs.Name = "mnuAddStoredProcs";
            this.mnuAddStoredProcs.Size = new System.Drawing.Size(175, 26);
            this.mnuAddStoredProcs.Text = "Stored Procedures";
            this.mnuAddStoredProcs.Click += new System.EventHandler(this.mnuAddStoredProcs_Click);
            // 
            // mnuAddFunctions
            // 
            this.mnuAddFunctions.Image = global::SqlSync.Properties.Resources.Function_2;
            this.mnuAddFunctions.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuAddFunctions.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            this.mnuAddFunctions.MergeIndex = 1;
            this.mnuAddFunctions.Name = "mnuAddFunctions";
            this.mnuAddFunctions.Size = new System.Drawing.Size(175, 26);
            this.mnuAddFunctions.Text = "Functions";
            this.mnuAddFunctions.Click += new System.EventHandler(this.mnuAddFunctions_Click);
            // 
            // mnuAddViews
            // 
            this.mnuAddViews.Image = global::SqlSync.Properties.Resources.Debug_Watch;
            this.mnuAddViews.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuAddViews.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            this.mnuAddViews.MergeIndex = 2;
            this.mnuAddViews.Name = "mnuAddViews";
            this.mnuAddViews.Size = new System.Drawing.Size(175, 26);
            this.mnuAddViews.Text = "Views";
            this.mnuAddViews.Click += new System.EventHandler(this.mnuAddViews_Click);
            // 
            // mnuAddTables
            // 
            this.mnuAddTables.Image = global::SqlSync.Properties.Resources.Table;
            this.mnuAddTables.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuAddTables.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            this.mnuAddTables.MergeIndex = 3;
            this.mnuAddTables.Name = "mnuAddTables";
            this.mnuAddTables.Size = new System.Drawing.Size(175, 26);
            this.mnuAddTables.Text = "Tables";
            this.mnuAddTables.Click += new System.EventHandler(this.mnuAddTables_Click);
            // 
            // mnuAddTriggers
            // 
            this.mnuAddTriggers.Image = global::SqlSync.Properties.Resources.Execute;
            this.mnuAddTriggers.Name = "mnuAddTriggers";
            this.mnuAddTriggers.Size = new System.Drawing.Size(175, 26);
            this.mnuAddTriggers.Text = "Triggers";
            this.mnuAddTriggers.Click += new System.EventHandler(this.mnuAddTriggers_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(172, 6);
            this.toolStripSeparator21.Visible = false;
            // 
            // mnuAddRoles
            // 
            this.mnuAddRoles.Name = "mnuAddRoles";
            this.mnuAddRoles.Size = new System.Drawing.Size(175, 26);
            this.mnuAddRoles.Text = "Roles";
            this.mnuAddRoles.Visible = false;
            this.mnuAddRoles.Click += new System.EventHandler(this.mnuAddRoles_Click);
            // 
            // scriptingOptionsToolStripMenuItem
            // 
            this.scriptingOptionsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.scriptingOptionsToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.scriptingOptionsToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Wizard;
            this.scriptingOptionsToolStripMenuItem.Name = "scriptingOptionsToolStripMenuItem";
            this.scriptingOptionsToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.scriptingOptionsToolStripMenuItem.Text = "Scripting Options";
            // 
            // scriptALTERAndCREATEToolStripMenuItem
            // 
            this.scriptALTERAndCREATEToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.scriptALTERAndCREATEToolStripMenuItem.CheckOnClick = true;
            this.scriptALTERAndCREATEToolStripMenuItem.Name = "scriptALTERAndCREATEToolStripMenuItem";
            this.scriptALTERAndCREATEToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.scriptALTERAndCREATEToolStripMenuItem.Text = "Script Object ALTER and CREATE";
            this.scriptALTERAndCREATEToolStripMenuItem.Click += new System.EventHandler(this.scriptALTERVsCREATEToolStripMenuItem_Click);
            // 
            // includeObjectPermissionsToolStripMenuItem1
            // 
            this.includeObjectPermissionsToolStripMenuItem1.BackColor = System.Drawing.SystemColors.Control;
            this.includeObjectPermissionsToolStripMenuItem1.CheckOnClick = true;
            this.includeObjectPermissionsToolStripMenuItem1.Name = "includeObjectPermissionsToolStripMenuItem1";
            this.includeObjectPermissionsToolStripMenuItem1.Size = new System.Drawing.Size(263, 22);
            this.includeObjectPermissionsToolStripMenuItem1.Text = "Include Object Permissions";
            this.includeObjectPermissionsToolStripMenuItem1.Click += new System.EventHandler(this.includeObjectPermissionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(260, 6);
            // 
            // mnuAddCodeTablePop
            // 
            this.mnuAddCodeTablePop.Image = global::SqlSync.Properties.Resources.Table_Import;
            this.mnuAddCodeTablePop.MergeIndex = 2;
            this.mnuAddCodeTablePop.Name = "mnuAddCodeTablePop";
            this.mnuAddCodeTablePop.Size = new System.Drawing.Size(263, 22);
            this.mnuAddCodeTablePop.Text = "Add Code Table Populate Scripts";
            this.mnuAddCodeTablePop.Click += new System.EventHandler(this.mnuAddCodeTablePop_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.MergeIndex = 3;
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(260, 6);
            // 
            // mnuUpdatePopulate
            // 
            this.mnuUpdatePopulate.Image = global::SqlSync.Properties.Resources.DB_Refresh;
            this.mnuUpdatePopulate.MergeIndex = 4;
            this.mnuUpdatePopulate.Name = "mnuUpdatePopulate";
            this.mnuUpdatePopulate.Size = new System.Drawing.Size(263, 22);
            this.mnuUpdatePopulate.Text = "Update Code Table Populate Scripts";
            this.mnuUpdatePopulate.Click += new System.EventHandler(this.mnuUpdatePopulate_Click);
            // 
            // mnuObjectUpdates
            // 
            this.mnuObjectUpdates.Image = global::SqlSync.Properties.Resources.Update;
            this.mnuObjectUpdates.MergeIndex = 5;
            this.mnuObjectUpdates.Name = "mnuObjectUpdates";
            this.mnuObjectUpdates.Size = new System.Drawing.Size(263, 22);
            this.mnuObjectUpdates.Text = "Update Object Create Scripts";
            this.mnuObjectUpdates.Click += new System.EventHandler(this.mnuObjectUpdates_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(260, 6);
            // 
            // createBackoutPackageToolStripMenuItem
            // 
            this.createBackoutPackageToolStripMenuItem.Image = global::SqlSync.Properties.Resources.History;
            this.createBackoutPackageToolStripMenuItem.Name = "createBackoutPackageToolStripMenuItem";
            this.createBackoutPackageToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.createBackoutPackageToolStripMenuItem.Text = "Create back out package";
            this.createBackoutPackageToolStripMenuItem.Click += new System.EventHandler(this.createBackoutPackageToolStripMenuItem_Click);
            // 
            // mnuLogging
            // 
            this.mnuLogging.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShowBuildLogs,
            this.mnuScriptToLogFile,
            this.toolStripSeparator2,
            this.mnuShowBuildHistory,
            this.archiveBuildHistoryToolStripMenuItem});
            this.mnuLogging.Enabled = false;
            this.mnuLogging.Image = global::SqlSync.Properties.Resources.Note_Book;
            this.mnuLogging.MergeIndex = 3;
            this.mnuLogging.Name = "mnuLogging";
            this.mnuLogging.Size = new System.Drawing.Size(79, 20);
            this.mnuLogging.Text = "Lo&gging";
            // 
            // mnuShowBuildLogs
            // 
            this.mnuShowBuildLogs.MergeIndex = 0;
            this.mnuShowBuildLogs.Name = "mnuShowBuildLogs";
            this.mnuShowBuildLogs.Size = new System.Drawing.Size(192, 22);
            this.mnuShowBuildLogs.Text = "&Show Build Logs";
            this.mnuShowBuildLogs.Click += new System.EventHandler(this.mnuShowBuildLogs_Click);
            // 
            // mnuScriptToLogFile
            // 
            this.mnuScriptToLogFile.MergeIndex = 1;
            this.mnuScriptToLogFile.Name = "mnuScriptToLogFile";
            this.mnuScriptToLogFile.Size = new System.Drawing.Size(192, 22);
            this.mnuScriptToLogFile.Text = "Script Build to Log &File";
            this.mnuScriptToLogFile.Click += new System.EventHandler(this.mnuScriptToLogFile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(189, 6);
            // 
            // mnuShowBuildHistory
            // 
            this.mnuShowBuildHistory.MergeIndex = 3;
            this.mnuShowBuildHistory.Name = "mnuShowBuildHistory";
            this.mnuShowBuildHistory.Size = new System.Drawing.Size(192, 22);
            this.mnuShowBuildHistory.Text = "Show Build &History";
            this.mnuShowBuildHistory.Click += new System.EventHandler(this.mnuShowBuildHistory_Click);
            // 
            // archiveBuildHistoryToolStripMenuItem
            // 
            this.archiveBuildHistoryToolStripMenuItem.Name = "archiveBuildHistoryToolStripMenuItem";
            this.archiveBuildHistoryToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.archiveBuildHistoryToolStripMenuItem.Text = "Archive Build History";
            this.archiveBuildHistoryToolStripMenuItem.Click += new System.EventHandler(this.archiveBuildHistoryToolStripMenuItem_Click);
            // 
            // menuItem16
            // 
            this.menuItem16.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuObjectValidation,
            this.storedProcedureTestingToolStripMenuItem,
            this.menuItem21,
            this.mnuSchemaScripting,
            this.mnuCodeTableScripting,
            this.menuItem18,
            this.mnuDataAuditScripting,
            this.mnuDataExtraction,
            this.createToolStripMenuItem,
            this.toolStripSeparator8,
            this.mnuDatabaseSize,
            this.menuItem19,
            this.mnuAutoScripting,
            this.menuItem11,
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem,
            this.constructCommandLineStringToolStripMenuItem,
            this.toolStripSeparator14,
            this.scriptPolicyCheckingToolStripMenuItem,
            this.calculateScriptPackageHashSignatureToolStripMenuItem});
            this.menuItem16.Image = global::SqlSync.Properties.Resources.Tools;
            this.menuItem16.MergeIndex = 4;
            this.menuItem16.Name = "menuItem16";
            this.menuItem16.Size = new System.Drawing.Size(64, 20);
            this.menuItem16.Text = "Tools";
            // 
            // mnuObjectValidation
            // 
            this.mnuObjectValidation.Image = global::SqlSync.Properties.Resources.Certificate;
            this.mnuObjectValidation.MergeIndex = 0;
            this.mnuObjectValidation.Name = "mnuObjectValidation";
            this.mnuObjectValidation.Size = new System.Drawing.Size(286, 22);
            this.mnuObjectValidation.Text = "Database Object Validation";
            this.mnuObjectValidation.Click += new System.EventHandler(this.mnuObjectValidation_Click);
            // 
            // storedProcedureTestingToolStripMenuItem
            // 
            this.storedProcedureTestingToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Procedure;
            this.storedProcedureTestingToolStripMenuItem.Name = "storedProcedureTestingToolStripMenuItem";
            this.storedProcedureTestingToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.storedProcedureTestingToolStripMenuItem.Text = "Stored Procedure Testing";
            this.storedProcedureTestingToolStripMenuItem.Click += new System.EventHandler(this.storedProcedureTestingToolStripMenuItem_Click);
            // 
            // menuItem21
            // 
            this.menuItem21.MergeIndex = 1;
            this.menuItem21.Name = "menuItem21";
            this.menuItem21.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuSchemaScripting
            // 
            this.mnuSchemaScripting.Image = global::SqlSync.Properties.Resources.Database_Schema;
            this.mnuSchemaScripting.MergeIndex = 2;
            this.mnuSchemaScripting.Name = "mnuSchemaScripting";
            this.mnuSchemaScripting.Size = new System.Drawing.Size(286, 22);
            this.mnuSchemaScripting.Text = "Database Schema Scripting";
            this.mnuSchemaScripting.Click += new System.EventHandler(this.mnuSchemaScripting_Click);
            // 
            // mnuCodeTableScripting
            // 
            this.mnuCodeTableScripting.Image = global::SqlSync.Properties.Resources.Table_Relationships;
            this.mnuCodeTableScripting.MergeIndex = 3;
            this.mnuCodeTableScripting.Name = "mnuCodeTableScripting";
            this.mnuCodeTableScripting.Size = new System.Drawing.Size(286, 22);
            this.mnuCodeTableScripting.Text = "Code Table Scripting and Auditing";
            this.mnuCodeTableScripting.Click += new System.EventHandler(this.mnuCodeTableScripting_Click);
            // 
            // menuItem18
            // 
            this.menuItem18.MergeIndex = 4;
            this.menuItem18.Name = "menuItem18";
            this.menuItem18.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuDataAuditScripting
            // 
            this.mnuDataAuditScripting.Image = global::SqlSync.Properties.Resources.Datasheet_View;
            this.mnuDataAuditScripting.MergeIndex = 5;
            this.mnuDataAuditScripting.Name = "mnuDataAuditScripting";
            this.mnuDataAuditScripting.Size = new System.Drawing.Size(286, 22);
            this.mnuDataAuditScripting.Text = "User Data History and Audit Scripting";
            this.mnuDataAuditScripting.Click += new System.EventHandler(this.mnuDataAuditScripting_Click);
            // 
            // mnuDataExtraction
            // 
            this.mnuDataExtraction.Image = global::SqlSync.Properties.Resources.Database_Open;
            this.mnuDataExtraction.MergeIndex = 6;
            this.mnuDataExtraction.Name = "mnuDataExtraction";
            this.mnuDataExtraction.Size = new System.Drawing.Size(286, 22);
            this.mnuDataExtraction.Text = "Data Extraction";
            this.mnuDataExtraction.Click += new System.EventHandler(this.mnuDataExtraction_Click);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Script_Load;
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.createToolStripMenuItem.Text = "Create Scripts from Extracted Data";
            this.createToolStripMenuItem.Click += new System.EventHandler(this.createToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuDatabaseSize
            // 
            this.mnuDatabaseSize.Image = global::SqlSync.Properties.Resources.Database_Search;
            this.mnuDatabaseSize.MergeIndex = 7;
            this.mnuDatabaseSize.Name = "mnuDatabaseSize";
            this.mnuDatabaseSize.Size = new System.Drawing.Size(286, 22);
            this.mnuDatabaseSize.Text = "Database Analysis";
            this.mnuDatabaseSize.Click += new System.EventHandler(this.mnuDatabaseSize_Click);
            // 
            // menuItem19
            // 
            this.menuItem19.MergeIndex = 8;
            this.menuItem19.Name = "menuItem19";
            this.menuItem19.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuAutoScripting
            // 
            this.mnuAutoScripting.MergeIndex = 9;
            this.mnuAutoScripting.Name = "mnuAutoScripting";
            this.mnuAutoScripting.Size = new System.Drawing.Size(286, 22);
            this.mnuAutoScripting.Text = "Auto Scripting ";
            // 
            // menuItem11
            // 
            this.menuItem11.MergeIndex = 10;
            this.menuItem11.Name = "menuItem11";
            this.menuItem11.Size = new System.Drawing.Size(283, 6);
            // 
            // rebuildPreviouslyCommitedBuildFileToolStripMenuItem
            // 
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Insert;
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Name = "rebuildPreviouslyCommitedBuildFileToolStripMenuItem";
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Text = "Rebuild Previously Commited Build File";
            this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Click += new System.EventHandler(this.rebuildPreviouslyCommitedBuildFileToolStripMenuItem_Click);
            // 
            // constructCommandLineStringToolStripMenuItem
            // 
            this.constructCommandLineStringToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Database_commandline;
            this.constructCommandLineStringToolStripMenuItem.Name = "constructCommandLineStringToolStripMenuItem";
            this.constructCommandLineStringToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.constructCommandLineStringToolStripMenuItem.Text = "Construct Command Line String";
            this.constructCommandLineStringToolStripMenuItem.Click += new System.EventHandler(this.constructCommandLineStringToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(283, 6);
            // 
            // scriptPolicyCheckingToolStripMenuItem
            // 
            this.scriptPolicyCheckingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runPolicyChecksToolStripMenuItem,
            this.toolStripSeparator9,
            this.savePolicyResultsInCSVToolStripMenuItem,
            this.savePolicyResultsAsXMLToolStripMenuItem});
            this.scriptPolicyCheckingToolStripMenuItem.Image = global::SqlSync.Properties.Resources.ScriptCheck;
            this.scriptPolicyCheckingToolStripMenuItem.Name = "scriptPolicyCheckingToolStripMenuItem";
            this.scriptPolicyCheckingToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.scriptPolicyCheckingToolStripMenuItem.Text = "Script Policy Checking";
            // 
            // runPolicyChecksToolStripMenuItem
            // 
            this.runPolicyChecksToolStripMenuItem.Image = global::SqlSync.Properties.Resources.ScriptCheck;
            this.runPolicyChecksToolStripMenuItem.Name = "runPolicyChecksToolStripMenuItem";
            this.runPolicyChecksToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.runPolicyChecksToolStripMenuItem.Text = "Run Policy Checks";
            this.runPolicyChecksToolStripMenuItem.Click += new System.EventHandler(this.scriptPolicyCheckingToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(211, 6);
            // 
            // savePolicyResultsInCSVToolStripMenuItem
            // 
            this.savePolicyResultsInCSVToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.savePolicyResultsInCSVToolStripMenuItem.Name = "savePolicyResultsInCSVToolStripMenuItem";
            this.savePolicyResultsInCSVToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.savePolicyResultsInCSVToolStripMenuItem.Text = "Save Policy Results as CSV";
            this.savePolicyResultsInCSVToolStripMenuItem.Click += new System.EventHandler(this.savePolicyResultsInCSVToolStripMenuItem_Click);
            // 
            // savePolicyResultsAsXMLToolStripMenuItem
            // 
            this.savePolicyResultsAsXMLToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.savePolicyResultsAsXMLToolStripMenuItem.Name = "savePolicyResultsAsXMLToolStripMenuItem";
            this.savePolicyResultsAsXMLToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.savePolicyResultsAsXMLToolStripMenuItem.Text = "Save Policy Results as XML";
            this.savePolicyResultsAsXMLToolStripMenuItem.Click += new System.EventHandler(this.savePolicyResultsAsXMLToolStripMenuItem_Click);
            // 
            // calculateScriptPackageHashSignatureToolStripMenuItem
            // 
            this.calculateScriptPackageHashSignatureToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Signature;
            this.calculateScriptPackageHashSignatureToolStripMenuItem.Name = "calculateScriptPackageHashSignatureToolStripMenuItem";
            this.calculateScriptPackageHashSignatureToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.calculateScriptPackageHashSignatureToolStripMenuItem.Text = "Calculate Script Package Hash Signature";
            this.calculateScriptPackageHashSignatureToolStripMenuItem.Click += new System.EventHandler(this.calculateScriptPackageHashSignatureToolStripMenuItem_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAbout,
            this.howToToolStripMenuItem,
            this.toolStripSeparator17,
            this.projectSiteToolStripMenuItem,
            this.viewLogFileMenuItem2,
            this.setLoggingLevelMenuItem2});
            this.mnuHelp.Image = global::SqlSync.Properties.Resources.Help;
            this.mnuHelp.MergeIndex = 5;
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(60, 20);
            this.mnuHelp.Text = "Help";
            // 
            // mnuAbout
            // 
            this.mnuAbout.Image = global::SqlSync.Properties.Resources.About;
            this.mnuAbout.MergeIndex = 0;
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(221, 22);
            this.mnuAbout.Text = "About Sql Build Manager";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // howToToolStripMenuItem
            // 
            this.howToToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.howToToolStripMenuItem.Name = "howToToolStripMenuItem";
            this.howToToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.howToToolStripMenuItem.Text = "How Do I?";
            this.howToToolStripMenuItem.Click += new System.EventHandler(this.howToToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(218, 6);
            // 
            // projectSiteToolStripMenuItem
            // 
            this.projectSiteToolStripMenuItem.Name = "projectSiteToolStripMenuItem";
            this.projectSiteToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.projectSiteToolStripMenuItem.Text = "www.SqlBuildManager.com";
            this.projectSiteToolStripMenuItem.Visible = false;
            this.projectSiteToolStripMenuItem.Click += new System.EventHandler(this.projectSiteToolStripMenuItem_Click);
            // 
            // viewLogFileMenuItem2
            // 
            this.viewLogFileMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("viewLogFileMenuItem2.Image")));
            this.viewLogFileMenuItem2.Name = "viewLogFileMenuItem2";
            this.viewLogFileMenuItem2.Size = new System.Drawing.Size(221, 22);
            this.viewLogFileMenuItem2.Text = "View Application Log File";
            // 
            // setLoggingLevelMenuItem2
            // 
            this.setLoggingLevelMenuItem2.Name = "setLoggingLevelMenuItem2";
            this.setLoggingLevelMenuItem2.Size = new System.Drawing.Size(221, 22);
            this.setLoggingLevelMenuItem2.Text = "Set Logging Level";
            // 
            // SqlBuildForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1073, 648);
            this.Controls.Add(this.toolStripContainer2);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(1, 1);
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "SqlBuildForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SqlBuildForm_FormClosing);
            this.Load += new System.EventHandler(this.SqlBuildForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SqlBuildForm_KeyDown);
            this.grbBuildScripts.ResumeLayout(false);
            this.grbBuildScripts.PerformLayout();
            this.ctxScriptFile.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.grpManager.ResumeLayout(false);
            this.grpManager.PerformLayout();
            this.ctxResults.ResumeLayout(false);
            this.mainMenu1.ResumeLayout(false);
            this.mainMenu1.PerformLayout();
            this.pnlManager.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlAdvanced.ResumeLayout(false);
            this.grpAdvanced.ResumeLayout(false);
            this.grpAdvanced.PerformLayout();
            this.grpBuildResults.ResumeLayout(false);
            this.pnlBuildScripts.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStripContainer2.ContentPanel.ResumeLayout(false);
            this.toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer2.TopToolStripPanel.PerformLayout();
            this.toolStripContainer2.ResumeLayout(false);
            this.toolStripContainer2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region .: Build Project File Methods :.
        internal void LoadSqlBuildProjectFileData(ref SqlSyncBuildData buildData, string projFileName, bool validateSchema)
        {
            bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projFileName, validateSchema);
            if (successfulLoad)
            {
                this.projectFileName = projFileName;
                if (!this.settingsControl1.InvokeRequired)
                {
                    if (String.IsNullOrEmpty(this.buildZipFileName) && !String.IsNullOrEmpty(this.sbxBuildControlFileName))
                        this.settingsControl1.Project = this.projectFileName;
                    else
                        this.settingsControl1.Project = this.buildZipFileName;
                }
                if (!grbBuildScripts.InvokeRequired)
                    grbBuildScripts.Enabled = true;
                if (!grpManager.InvokeRequired)
                    grpManager.Enabled = true;
                if (!grpBuildResults.InvokeRequired)
                    grpBuildResults.Enabled = true;
                if (this.buildData.SqlSyncBuildProject.Count != 0 && this.buildData.SqlSyncBuildProject[0] != null && !this.buildData.SqlSyncBuildProject[0].IsScriptTagRequiredNull())
                    this.buildData.SqlSyncBuildProject[0].ScriptTagRequired = scriptTagsRequiredToolStripMenuItem.Checked;
                else
                {
                    if (this.buildData.SqlSyncBuildProject.Count == 0)
                        this.buildData.SqlSyncBuildProject.Rows.Add(this.buildData.SqlSyncBuildProject.NewSqlSyncBuildProjectRow());

                    this.buildData.SqlSyncBuildProject[0].ScriptTagRequired = scriptTagsRequiredToolStripMenuItem.Checked;
                }
            }
            else
            {
                MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid build file", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        #region .: Zipping/Unzipping File Handling Methods :.
        #region .: LoadZipFile Background event methods :.
        private void bgLoadZipFle_DoWork(object sender, DoWorkEventArgs e)
        {
            string zipFileName = e.Argument.ToString();
            string result;
            bool success = SqlBuildFileHelper.ExtractSqlBuildZipFile(zipFileName, ref this.workingDirectory, ref this.projectFilePath, ref this.projectFileName, out result);
            if (success)
            {
                LoadSqlBuildProjectFileData(ref this.buildData, this.projectFileName, false);
                PopulateTagList();
            
                e.Result = null;
            }
            else
                e.Result = result;
        }

        private void bgLoadZipFle_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //If there is a result object, it will be an error.. so either alert the user of make an EventLog entry as appropriate
            if (e.Result != null)
            {
                if (!this.runningUnattended)
                {
                    MessageBox.Show(e.Result.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.statGeneral.Text = "Build File Load Error.";
                }
                else
                {
                    //EventLog.WriteEntry("SqlSync", "Build File Load Error.", EventLogEntryType.Error, 867);
                    Program.WriteLog("Build File Load Error");
                    if (this.UnattendedProcessingCompleteEvent != null)
                        this.UnattendedProcessingCompleteEvent(867);
                }
            }
            else
            {
                //All must have loaded ok!
                this.statGeneral.Text = "Ready.";
                if(String.IsNullOrEmpty(this.buildZipFileName) && !string.IsNullOrEmpty(this.sbxBuildControlFileName))
                    this.settingsControl1.Project = this.projectFileName;
                else
                    this.settingsControl1.Project = this.buildZipFileName;
            }

            //If this was loaded with the "runningUnattended" flag as false (i.e. an interactive session) enable the form's controls
            if (!this.runningUnattended)
            {
                this.Cursor = Cursors.Default;
                this.progressBuild.Style = ProgressBarStyle.Blocks;

                this.SetUsedDatabases();
                this.RefreshScriptFileList(true);
                this.mnuListTop.Enabled = true;
                this.mnuLogging.Enabled = true;
                this.mnuScripting.Enabled = true;
                this.grbBuildScripts.Enabled = true;
                this.grpManager.Enabled = true;
                this.grpBuildResults.Enabled = true;


                //If the log file is bloaded, altert the user and suggest that it be archived
                Int64 logSize = SqlBuild.SqlBuildFileHelper.GetTotalLogFilesSize(this.projectFilePath);
                if (logSize > 4000000)
                {
                    string message = "This build file contains log files totalling about " + logSize / 1000000 + "Mb. This can cause slow reloads and refreshes.\r\n" +
                    "Archive the logs via the Logging --> Show Build Logs --> Archive Menu";
                    MessageBox.Show(message, "Large Log File Size", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {

                //If we are running unattended, write to the console and go at it!
                Program.WriteLog("Build File Loaded Successfully.");

                //What kind of unattended run-- single or multiDb?
                if (this.multiDbRunData != null)
                {
                    this.multiDbRunData.BuildData = this.buildData;
                    this.multiDbRunData.ProjectFileName = this.projectFileName;
                    this.ProcessMultiDbBuildUnattended();
                }
                else
                {
                    this.ProcessBuildUnattended();
                }
            }
        }
        #endregion

        private bool PackageExportIntoZip(SqlSyncBuildData exportData, string zipFileName)
        {

            exportData.WriteXml(this.projectFilePath + XmlFileNames.ExportFile);

            string[] fileList = new string[exportData.Script.Rows.Count + 1];
            for (int i = 0; i < exportData.Script.Rows.Count; i++)
            {
                fileList[i] = ((SqlSyncBuildData.ScriptRow)exportData.Script.Rows[i]).FileName;
            }
            fileList[exportData.Script.Rows.Count] = XmlFileNames.ExportFile;
            bool val = ZipHelper.CreateZipPackage(fileList, this.projectFilePath, zipFileName);
            File.Delete(this.projectFilePath + XmlFileNames.ExportFile);
            return val;

        }

        private double ImportSqlScriptFile(string fileName, double lastBuildNumber, out string[] addedFileNames)
        {
            double startBuildNumber = lastBuildNumber + 1;
            //bool haveImportedRows = false;
            string tmpDir = System.IO.Path.GetTempPath();
            string workingDir = tmpDir + @"SqlsyncImport-" + System.Guid.NewGuid().ToString() + @"\";
            if (Directory.Exists(workingDir) == false)
            {
                Directory.CreateDirectory(workingDir);
            }
            if (File.Exists(fileName))
            {
                if (ZipHelper.UnpackZipPackage(workingDir, fileName) == false)
                {
                    addedFileNames = new string[0];
                    return (double)ImportFileStatus.UnableToImport;
                }
                try
                {
                    //Save any outstanding changes
                    this.buildData.AcceptChanges();
                    bool isValid = true;

                    //Validate that the selected file is a Sql build file
                    string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    SchemaValidator validator = new SchemaValidator();
                    string importFileXml = string.Empty;
                    if (File.Exists(workingDir + XmlFileNames.ExportFile))
                    {
                        importFileXml = workingDir + XmlFileNames.ExportFile;
                    }
                    else if (File.Exists(workingDir + XmlFileNames.MainProjectFile))
                    {
                        importFileXml = workingDir + XmlFileNames.MainProjectFile;
                    }
                    string valErrorMessage;
                    isValid = SqlBuildFileHelper.ValidateAgainstSchema(importFileXml, out valErrorMessage);
                    if (isValid == false)
                    {
                        addedFileNames = new string[0];
                        return (double)ImportFileStatus.UnableToImport;
                    }


                    //Get the list of scripts in the file to import
                    ArrayList list = new ArrayList();
                    SqlSyncBuildData importData = new SqlSyncBuildData();
                    importData.ReadXml(importFileXml);
                    importData.AcceptChanges();

                    ImportListForm frmImport = new ImportListForm(importData, workingDir, this.buildData, new string[0]);
                    if (DialogResult.OK != frmImport.ShowDialog())
                    {
                        addedFileNames = new string[0];
                        return 0;
                    }

                    SqlBuild.SqlBuildFileHelper.ImportSqlScriptFile(ref this.buildData,
                         importData, workingDir, lastBuildNumber, this.projectFilePath, this.projectFileName, this.buildZipFileName, true, out addedFileNames);

                    if (addedFileNames.Length > 0)
                        return startBuildNumber + 1;

                    //    importData = frmImport.ImportData;
                    //    importData.AcceptChanges();
                    //    int increment = 0;
                    //    DataView importView = importData.Script.DefaultView;
                    //    importView.Sort = "BuildOrder  ASC";
                    //    importView.RowStateFilter = DataViewRowState.OriginalRows;

                    //    if(importView.Count > 0)
                    //        haveImportedRows = true;

                    //    for(int i=0;i<importView.Count;i++)
                    //    {
                    //        SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)importView[i].Row;

                    //        row.BuildOrder = startBuildNumber + increment;
                    //        row.DateAdded = DateTime.Now;
                    //        list.Add(row.FileName);
                    //        this.buildData.Script.ImportRow(row);
                    //        if(File.Exists(this.projectFilePath+row.FileName))
                    //            File.Delete(this.projectFilePath+row.FileName);

                    //        File.Move(workingDir+row.FileName,this.projectFilePath+row.FileName);
                    //        increment++;
                    //    }

                    //    this.buildData.AcceptChanges();
                    //    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData,this.projectFileName, this.buildZipFileName);
                    //    addedFileNames = new string[list.Count];
                    //    list.CopyTo(addedFileNames);
                    //    try
                    //    {
                    //        string[] files = Directory.GetFiles(workingDir);
                    //        for(int i=0;i<files.Length;i++)
                    //            File.Delete(files[i]);

                    //        Directory.Delete(workingDir);
                    //    }
                    //    catch
                    //    {
                    //    }

                    //    if(haveImportedRows)
                    //        return startBuildNumber;
                    //    else
                    //        return (double)ImportFileStatus.NoRowsImported;

                    //}
                    //catch(Exception e)
                    //{
                    //    string error = e.ToString();
                    //    this.buildData.RejectChanges();
                    //}

                }
                catch { }



            }
            addedFileNames = new string[0];
            return -1;
        }
        #endregion

        private void UpdateScriptFileDetails(System.Guid scriptID, double buildOrder, string description, bool rollBackScript, bool rollBackBuild, string databaseName, bool stripTransactions, bool allowMultipleRuns, int scriptTimeOut)
        {
            this.Cursor = Cursors.AppStarting;
            SqlSyncBuildData.ScriptRow row = this.GetScriptRow(scriptID);
            if (row != null)
            {
                row.BuildOrder = buildOrder;
                row.Description = description;
                row.RollBackOnError = rollBackScript;
                row.CausesBuildFailure = rollBackBuild;
                row.Database = databaseName;
                row.StripTransactionText = stripTransactions;
                row.AllowMultipleRuns = allowMultipleRuns;
                row.ScriptTimeOut = scriptTimeOut;
                row.AcceptChanges();

                this.UpdateScriptFileDetails(row);
            }
            this.Cursor = Cursors.Default;
        }
        private void UpdateScriptFileDetails(SqlSyncBuildData.ScriptRow row)
        {
            row.AcceptChanges();
            row.Table.AcceptChanges();
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

        }
        public void RenumberBuildSequence()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                SqlBuildFileHelper.RenumberBuildSequence(ref this.buildData, this.projectFileName, this.buildZipFileName);
                this.LoadSqlBuildProjectFileData(ref buildData, projectFileName, false);
                this.RefreshScriptFileList();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private SqlSyncBuildData.ScriptRow GetScriptRow(System.Guid scriptId)
        {
            DataRow[] rows = this.buildData.Script.Select("ScriptId ='" + scriptId.ToString() + "'");
            if (rows.Length > 0)
            {
                return (SqlSyncBuildData.ScriptRow)rows[0];
            }
            return null;
        }
        private string BuildExportString()
        {
            if (this.buildData.Script.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder("Seq #\tScript File Name\tDefault Database\tComments\tTag\r\n");
                DataView view = this.buildData.Script.DefaultView;
                view.Sort = "BuildOrder  ASC";
                for (int i = 0; i < view.Count; i++)
                {
                    SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)view[i].Row;
                    sb.Append(row.BuildOrder.ToString() + "\t");
                    sb.Append(row.FileName.ToString() + "\t");
                    sb.Append(row.Database.ToString() + "\t");
                    sb.Append(row.Description.ToString() + "\t");
                    sb.Append(row.Tag.ToString() + "\r\n");
                }
                return sb.ToString();
            }
            return string.Empty;
        }
        private bool SaveBuildExportFile(SqlSyncBuildData.ScriptRow[] rows, string fileName)
        {
            if (rows.Length > 0)
            {
                SqlSyncBuildData newdata = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                SqlSyncBuildData.ScriptsRow parentRow = (SqlSyncBuildData.ScriptsRow)newdata.Scripts.Rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    SqlSyncBuildData.ScriptRow typedRow = (SqlSyncBuildData.ScriptRow)rows[i];
                    newdata.Script.AddScriptRow(
                        typedRow.FileName,
                        typedRow.BuildOrder,
                        typedRow.Description,
                        typedRow.RollBackOnError,
                        typedRow.CausesBuildFailure,
                        typedRow.DateAdded,
                        typedRow.ScriptId,
                        typedRow.Database,
                        typedRow.StripTransactionText,
                        typedRow.AllowMultipleRuns,
                        typedRow.AddedBy,
                        typedRow.ScriptTimeOut,
                        (typedRow.IsDateModifiedNull()) ? DateTime.MinValue : typedRow.DateModified,
                        typedRow.ModifiedBy,
                        parentRow,
                        typedRow.Tag);
                }
                return PackageExportIntoZip(newdata, fileName);
            }
            return false;

        }
        private void GetLastBuildNumberAndDb(out double lastbuildNumber, out string lastDatabase)
        {
            //Use the dataset to get the last build number and Db
            if (this.buildData != null && this.buildData.Script.Rows.Count > 0)
            {
                DataView view = this.buildData.Script.DefaultView;
                view.RowFilter = this.buildData.Script.BuildOrderColumn.ColumnName + " < " + ((int)ResequenceIgnore.StartNumber).ToString();
                view.Sort = this.buildData.Script.BuildOrderColumn + " DESC";
                if (view.Count > 0)
                {
                    lastbuildNumber = ((SqlSyncBuildData.ScriptRow)view[0].Row).BuildOrder;
                    lastDatabase = ((SqlSyncBuildData.ScriptRow)view[0].Row).Database;
                }
                else
                {
                    lastbuildNumber = 0;
                    lastDatabase = "";
                }

                return;
            }

            lastbuildNumber = 0;
            lastDatabase = string.Empty;
            return;
        }

        #endregion

        #region .: Script File ListView Refresh Methods :.
        private void RefreshScriptFileList_SingleItem(SqlSyncBuildData.ScriptRow row)
        {
            this.statGeneral.Text = "Saving Changes. Updating Script List.";
            System.Threading.Thread.Sleep(50); // Give it time to update.

            try
            {
                var item = (from s in this.lstScriptFiles.Items.Cast<ListViewItem>()
                            where ((SqlSyncBuildData.ScriptRow)s.Tag).ScriptId == row.ScriptId
                            select s).ToList();

                if (item.Count > 0)
                {
                    ListViewItem firstItem = item[0];
                    SqlBuildHelper helper = new SqlBuildHelper(this.connData, this.createSqlRunLogFile, this.externalScriptLogFileName, true);
                    RefreshScriptFileList_SingleItem(row, ref firstItem, ref helper, true);
                    firstItem.EnsureVisible();
                    this.statGeneral.Text = "Changes Saved.";

                    if (this.runPolicyCheckingOnLoad)
                        SetPolicyCheckStatusIcons();

                    SetCodeReviewIcon(row);
                }
            }
            catch (Exception exe)
            {
                log.Warn("Unable to refresh list view for " + row.FileName, exe);
                this.statGeneral.Text = "Changes Saved, but unable to update script display. See error log.";
            }



        }
        private void RefreshScriptFileList_SingleItem(SqlSyncBuildData.ScriptRow row, ref ListViewItem item, ref SqlBuildHelper helper, bool updateIcon)
        {
            
            if(this.projectIsUnderSourceControl &&  !File.Exists(this.projectFilePath + row.FileName))
            {
                //This file might not be downloaded from source control.. try to get it now..
                SourceControlStatus stat = SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, this.projectFilePath + row.FileName);
            }

            long fileSize = 0;


            try
            {
                fileSize = new FileInfo(this.projectFilePath + row.FileName).Length;
            }
            catch { }

            if (item.SubItems.Count <= 1)
            {
                //Sync order of addition with the ScriptListIndex enum
                item = new ListViewItem(new string[]{
													 "",
                                                     "",
                                                     "",
													 row.BuildOrder.ToString(),
													 row.FileName,
													 row.Database,
													 row.ScriptId,
													 fileSize.ToString(),
                                                     row.Tag,
                                                     row.DateAdded.ToString(),
                (row.IsDateModifiedNull() || row.DateModified < Convert.ToDateTime("1/1/1980")) ? "" : row.DateModified.ToString()});

            }
            else
            {
                item.SubItems[(int)ScriptListIndex.SequenceNumber].Text = row.BuildOrder.ToString();
                item.SubItems[(int)ScriptListIndex.FileName].Text = row.FileName;
                item.SubItems[(int)ScriptListIndex.Database].Text = row.Database;
                item.SubItems[(int)ScriptListIndex.ScriptId].Text = row.ScriptId;
                item.SubItems[(int)ScriptListIndex.FileSize].Text = fileSize.ToString();
                item.SubItems[(int)ScriptListIndex.ScriptTag].Text = row.Tag;
                item.SubItems[(int)ScriptListIndex.DateAdded].Text = row.DateAdded.ToString();
                if (row.IsDateModifiedNull() || row.DateModified < Convert.ToDateTime("1/1/1980"))
                    item.SubItems[(int)ScriptListIndex.DateModified].Text = "";
                else
                    item.SubItems[(int)ScriptListIndex.DateModified].Text = row.DateModified.ToString();
            }

            //Set the object tag.
            item.Tag = row;
            item.ToolTipText = row.FileName +
                "\r\nDB:\t\t" + row.Database +
                "\r\nSize:\t\t" + fileSize.ToString() +
                "\r\nTag:\t\t" + row.Tag +
                "\r\nTimeOut:\t" + row.ScriptTimeOut +
                "\r\nRollback Build:\t" + row.CausesBuildFailure.ToString() +
                "\r\nRollback Script:\t" + row.RollBackOnError.ToString();


            //Determine Status ICON 
            if (updateIcon)
            {
                DateTime commitDate;
                DateTime serverChangeDate;
                ScriptStatusType status = StatusHelper.DetermineScriptRunStatus(row, this.connData, this.projectFilePath, chkScriptChanges.Checked, OverrideData.TargetDatabaseOverrides, out commitDate, out serverChangeDate);
                item.ImageIndex = (int)status;
                row.ScriptRunStatus = status;
                switch (status)
                {
                    case ScriptStatusType.NotRun:
                    case ScriptStatusType.NotRunButOlderVersion:
                        item.ToolTipText += "\r\nLast SBM Date:\t\t " + ((commitDate == DateTime.MinValue) ? "None" : commitDate.ToString()) +
                            "\r\nServer Change Date:\t " + ((serverChangeDate == DateTime.MinValue) ? "Unknown" : serverChangeDate.ToString());
                        break;
                    default:
                        item.ToolTipText += "\r\nLast Commit Date:\t " + ((commitDate == DateTime.MinValue) ? "None" : commitDate.ToString()) +
                            "\r\nServer Change Date:\t " + ((serverChangeDate == DateTime.MinValue) ? "Unknown" : serverChangeDate.ToString());
                        break;

                }
            }
            else
            {
                item.ImageIndex = (int)ScriptStatusType.Unknown;
                row.ScriptRunStatus = ScriptStatusType.Unknown;
            }

           
        

            //Reset back-color to the default
            item.BackColor = SystemColors.Window;
            item.ForeColor = SystemColors.ControlText;

            //Determine list back color
            if (row.AllowMultipleRuns == true)
                item.BackColor = colorMultipleRun;

            if (row.StripTransactionText == false)
                item.BackColor = colorLeaveTrans; ;

            if (row.AllowMultipleRuns == true && row.StripTransactionText == false)
                item.BackColor = colorBoth;

            if (row.AllowMultipleRuns == false && (item.ImageIndex == (int)ScriptStatusType.ChangedSinceCommit ||
                item.ImageIndex == (int)ScriptStatusType.Locked || item.ImageIndex == (int)ScriptStatusType.ServerChange ||
                item.ImageIndex == (int)ScriptStatusType.UpToDate))
            {
                item.BackColor = colorSkipped;
            }

            if (File.Exists(this.projectFilePath + row.FileName) && (File.GetAttributes(this.projectFilePath + row.FileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                item.BackColor = colorReadOnlyFile;
                item.ForeColor = Color.White;
            }

            //Check for bold
            if (row.IsDateModifiedNull() == false && row.DateModified != DateTime.MinValue && row.DateModified != row.DateAdded)
            {

                item.UseItemStyleForSubItems = false;
                for (int j = 0; j < item.SubItems.Count; j++)
                {
                    item.SubItems[j].BackColor = item.BackColor;
                    item.SubItems[j].ForeColor = item.ForeColor;
                }
                item.SubItems[(int)ScriptListIndex.SequenceNumber].Font = new System.Drawing.Font(item.Font, FontStyle.Bold);
            }

            //Get the contents of the file for the next couple of operations
            string scriptContents = string.Empty;
            try
            {
                scriptContents = File.ReadAllText(this.projectFilePath + row.FileName);
            }
            catch (Exception exe)
            {
                log.Error(String.Format("Unable to read file at {0} for list refresh", this.projectFilePath + row.FileName), exe);
            }

            //Determine if this script requires a build message...  
            if (SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptContents))
            {
                if (!this.scriptsRequiringBuildDescription.Contains(row.ScriptId))
                {
                    this.scriptsRequiringBuildDescription.Add(row.ScriptId);
                }
            }
            else
            {
                this.scriptsRequiringBuildDescription.Remove(row.ScriptId);
            }

        }
        private void RefreshScriptFileList()
        {
            this.RefreshScriptFileList(false);
        }
        private void RefreshScriptFileList(bool runPolicyChecks)
        {
            this.RefreshScriptFileList(-1, runPolicyChecks);
        }
        private void RefreshScriptFileList(double selectItemIndex, bool runPolicyChecks)
        {
            ListRefreshSettings s = new ListRefreshSettings()
            {
                SelectedItemIndex = selectItemIndex,
                RunPolicyChecks = runPolicyChecks
            };

            if(this.buildData != null)
                bgRefreshScriptList.RunWorkerAsync(s);
        }

 
        private class ListRefreshSettings
        {
            internal bool RunPolicyChecks { get; set; }
            internal double SelectedItemIndex { get; set; }
            internal List<ListViewItem> ListItems { get; set; }
            internal IFileStatus SourceFileStatus { get; set; }
        }
        #region .: Refresh Script List Background Worker :.
        
        private void bgRefreshScriptList_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            bg.ReportProgress(0, null);

            if (this.buildData == null)
            {
                e.Cancel = true;
                return;
            }

            ListRefreshSettings s = new ListRefreshSettings();
            if (e.Argument != null && e.Argument is ListRefreshSettings)
            {
                s = (ListRefreshSettings)e.Argument;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            try
            {
                SqlBuildHelper helper = new SqlBuildHelper(this.connData, this.createSqlRunLogFile, this.externalScriptLogFileName, true);
                DataView view = this.buildData.Script.DefaultView;
                view.RowFilter = "";
                view.Sort = "BuildOrder ASC";

                bg.ReportProgress(10, "Refreshing script list...");
                for (int i = 0; i < view.Count; i++)
                {
                    int counter = i;
                    ListViewItem item = new ListViewItem();
                    SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)view[i].Row;
                    RefreshScriptFileList_SingleItem(row, ref item, ref helper, true);
                    if (row.BuildOrder == s.SelectedItemIndex + 1) // Build order is 1 based vs 0 based
                    {
                        item.Selected = true;
                    }
                    items.Add(item);
                }
                s.ListItems = items;
 
                e.Result = s;
            }
            catch (Exception exe)
            {
                //System.Diagnostics.EventLog.WriteEntry("SQLSync", "List Refesh Error:\r\n" + exe.ToString(), EventLogEntryType.Error, 545);
            }

        }

        private void bgRefreshScriptList_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
            {
                this.lstScriptFiles.Items.Clear();
                this.progressBuild.Style = ProgressBarStyle.Marquee;
                this.Cursor = Cursors.AppStarting;
                this.statGeneral.Text = "Refreshing script list...";
                this.grbBuildScripts.Enabled = false;

            }
            else
            {
                this.statGeneral.Text = e.UserState.ToString();
            }
        }

        private void bgRefreshScriptList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null && e.Result is ListRefreshSettings)
            {
                ListRefreshSettings s = (ListRefreshSettings)e.Result;
                List<ListViewItem> items = s.ListItems;
                for (int i = 0; i < items.Count; i++)
                {
                    lstScriptFiles.Items.Add(items[i]);
                }

                lstScriptFiles.Refresh();

                //Just set them to the current value now.. the async process will set them
                SetPolicyCheckStatusIcons();
                SetDefaultCodeReviewIcons();
                if (this.runPolicyCheckingOnLoad && s.RunPolicyChecks && !bgPolicyCheck.IsBusy)
                    bgPolicyCheck.RunWorkerAsync(s);

                //Check to see if there was a source control error
                if (this.ProjectIsUnderSourceControl && s.SourceFileStatus != null)
                {
                    if (s.SourceFileStatus.SourceError.Count > 0)
                    {
                        string msg = "There was an error integrating with source control for the following files.\r\nPlease confirm that they have been handled appropriately.\r\n\r\n{0}";
                        string files = String.Join("\t", s.SourceFileStatus.SourceError.ToArray());
                        MessageBox.Show(String.Format(msg, files), "Source Control Problem", MessageBoxButtons.OK);
                    }
                }
            }
            else if (!e.Cancelled)
            {
                MessageBox.Show("Error refeshing script list. See Event log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (lstScriptFiles.SelectedItems.Count > 0)
                lstScriptFiles.SelectedItems[0].EnsureVisible();

            this.progressBuild.Style = ProgressBarStyle.Blocks;
            this.Cursor = Cursors.Default;
            this.statGeneral.Text = "List Refresh Complete.";
            this.grbBuildScripts.Enabled = true;
            this.statScriptCount.Text = String.Format("Script Count: {0}", this.buildData.Script.Rows.Count);

            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig != null && EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
            {
                bgCodeReview.RunWorkerAsync();
            }
        }
        #endregion
        #endregion

        #region .: Set Policy Icon :.

        private void bgPolicyCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            PolicyHelper policyHelp = new PolicyHelper();
            bg.ReportProgress(0, null);

            if (this.buildData == null)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                
                Script violation = null;


                if (e.Argument is ListRefreshSettings)
                {
                    ListRefreshSettings s = (ListRefreshSettings)e.Argument;
                    if (s.SelectedItemIndex == -1)
                    {
                        this.currentViolations.Clear();
                        bg.ReportProgress(0, "Performing script policy checks...");
                        PolicyHelper.GetPolicies();

                        Parallel.ForEach(this.buildData.Script, row  =>
                        {
                            try
                            {
                                bg.ReportProgress(0, string.Format("Policy checking {0}", row.FileName));
                                string scriptContents = File.ReadAllText(this.projectFilePath + row.FileName);
                                violation = policyHelp.ValidateScriptAgainstPolicies(row.FileName, row.ScriptId, scriptContents, row.Database, 80);
                                if (violation == null)
                                {
                                    row.PolicyCheckState = ScriptStatusType.PolicyPass;
                                }
                                else
                                {
                                    violation.LastChangeDate = (row.DateModified == DateTime.MinValue) ? row.DateAdded.ToString() : row.DateModified.ToString();
                                    violation.LastChangeUserId = (row.ModifiedBy.Length == 0) ? row.AddedBy : row.ModifiedBy;
                                    currentViolations.Add(violation);

                                    string highSev, medSev, reviewWarningSev,lowSev;
                                    highSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.High);
                                    medSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.Medium);
                                    lowSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.Low);
                                    reviewWarningSev = Enum.GetName(typeof (ViolationSeverity),ViolationSeverity.ReviewWarning);

                                    var high = (from v in violation.Violations
                                                where v.Severity == highSev
                                                select v.Severity).Any();

                                    var mediumLow = (from v in violation.Violations
                                                where v.Severity == medSev || v.Severity == lowSev
                                                select v.Severity).Any();

                                    var review = (from v in violation.Violations
                                                  where v.Severity == reviewWarningSev
                                                  select v.Severity).Any();
                                    if (high)
                                        row.PolicyCheckState = ScriptStatusType.PolicyFail;
                                    else if (review)
                                        row.PolicyCheckState = ScriptStatusType.PolicyReviewWarning;
                                    else if (mediumLow)
                                        row.PolicyCheckState = ScriptStatusType.PolicyWarning;
                                }
                                bg.ReportProgress(0, row);
                            }
                            catch (Exception exe)
                            {
                                log.Error(String.Format("Unable to read file '{0}' for policy check validation", this.projectFilePath + row.FileName), exe);
                            }
                        });
                    }
                    else
                    {
                        bg.ReportProgress(0, "Performing script policy check...");
                        try
                        {
                            double index = s.SelectedItemIndex;
                            var row = (from sc in this.buildData.Script
                                       where sc.BuildOrder == index
                                       select sc).First<SqlSyncBuildData.ScriptRow>();

                            string scriptContents = File.ReadAllText(this.projectFilePath + row.FileName);
                            violation = policyHelp.ValidateScriptAgainstPolicies(row.FileName, row.ScriptId, scriptContents, row.Database, 80);
                            
                            if (violation == null)
                            {
                                row.PolicyCheckState = ScriptStatusType.PolicyPass;
                            }
                            else
                            {
                                violation.LastChangeDate = (row.DateModified == DateTime.MinValue) ? row.DateAdded.ToString() : row.DateModified.ToString();
                                violation.LastChangeUserId = (row.ModifiedBy.Length == 0) ? row.AddedBy : row.ModifiedBy;

                                var vol = (from v in this.currentViolations
                                           where v.Guid == row.ScriptId
                                           select v);

                                if (vol.Count() == 0)
                                    this.currentViolations.Add(violation);
                                else
                                {
                                    this.currentViolations.Remove(vol.First());
                                    this.currentViolations.Add(violation);

                                }
                                row.PolicyCheckState = ScriptStatusType.PolicyFail;
                            }
                        }
                        catch (Exception exe)
                        {
                            log.Error("Unable to read file '{0}' for single file policy check validation", exe);
                        }

                    }
                }
                else
                {
                    log.WarnFormat("Unable to run policy check. DoWorkEventArgs Argument is not of type {0}", "ListRefreshSettings");
                }
            }catch(Exception exe)
            {
                log.WarnFormat("Error when executing policy check.", exe);
                bg.ReportProgress(0, "Problem running policy checks.");
            }
           
        }

        private void bgPolicyCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           
            if(this.Cursor != Cursors.AppStarting)
                this.Cursor = Cursors.AppStarting;

            if(this.progressBuild.Style != ProgressBarStyle.Marquee)
                this.progressBuild.Style = ProgressBarStyle.Marquee;
            
            if(e.UserState is string)
            {
                this.statGeneral.Text = e.UserState.ToString();
            }
            else if (e.UserState is SqlSyncBuildData.ScriptRow)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)e.UserState;

                var indx = from i in lstScriptFiles.Items.Cast<ListViewItem>()
                           where i.SubItems[(int)ScriptListIndex.FileName].Text == row.FileName
                           select i.Index;

                if (indx != null && indx.Count() > 0)
                {
                    OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
                    lvi.iItem = indx.First();
                    // Column
                    lvi.iSubItem = (int)ScriptListIndex.PolicyIconColumn;
                    lvi.mask = OAKListView.LVIF_IMAGE;
                    // Image index on imagelist
                    lvi.iImage = (int)row.PolicyCheckState;
                    OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
                }

            }
        }

        private void bgPolicyCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetPolicyCheckStatusIcons();
            if (!this.bgCodeReview.IsBusy)
            {
                this.Cursor = Cursors.Default;
                this.statGeneral.Text = "Ready.";
                this.progressBuild.Style = ProgressBarStyle.Blocks;
            }
            else
            {
                this.statGeneral.Text = "Checking on code review status...";
            }
        }

          
        private void SetPolicyCheckStatusIcons()
        {
            for (int x = 0; x < lstScriptFiles.Items.Count; x++)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.Items[x].Tag;
                OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
                lvi.iItem = x;
                // Column
                lvi.iSubItem = (int)ScriptListIndex.PolicyIconColumn;
                lvi.mask = OAKListView.LVIF_IMAGE;
                // Image index on imagelist
                lvi.iImage = (int)row.PolicyCheckState;
                OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
            }

        }
      
        #endregion

        #region .: Set Code Review Icon :.
        private void SetDefaultCodeReviewIcons()
        {
            for (int x = 0; x < lstScriptFiles.Items.Count; x++)
            {
                OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
                lvi.iItem = x;
                // Column
                lvi.iSubItem = (int)ScriptListIndex.CodeReviewStatusIconColumn;
                lvi.mask = OAKListView.LVIF_IMAGE;
                // Image index on imagelist
                lvi.iImage = (int)ScriptStatusType.CodeReviewStatusWaiting;
                OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
            }
        }
        private void SetCodeReviewIcon(int listViewItemIndex)
        {
            SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.Items[listViewItemIndex].Tag;
            SetCodeReviewIcon(row);
        }
        private void SetCodeReviewIcon(SqlSyncBuildData.ScriptRow row)
        {
            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
                return;


            var item = (from s in this.lstScriptFiles.Items.Cast<ListViewItem>()
                        where ((SqlSyncBuildData.ScriptRow)s.Tag).ScriptId == row.ScriptId
                        select s).ToList();

            if (item.Count > 0)
            {
                ListViewItem firstItem = item[0];
                OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
                lvi.iItem = firstItem.Index;
                // Column
                lvi.iSubItem = (int)ScriptListIndex.CodeReviewStatusIconColumn;
                lvi.mask = OAKListView.LVIF_IMAGE;
                // Image index on imagelist
                var cr = from c in row.GetCodeReviewRows()
                         orderby c.ReviewDate descending 
                         select c.ReviewStatus
                ;
                          

                //Has at least one accepted..
                if (!cr.Any())
                {
                    //no reviews yet
                    lvi.iImage = (int)ScriptStatusType.CodeReviewNotStarted;
                }
                else if (cr.Contains((short)CodeReviewStatus.Accepted) && cr.First() == (short)CodeReviewStatus.Accepted) //Is the latest one an accepted status?  // && !cr.Contains((short)CodeReviewStatus.Defect) && !cr.Contains((short)CodeReviewStatus.OutOfDate))
                {
                   



                    //Accepted by DBA?
                    var acceptedReviewers = from crr in row.GetCodeReviewRows()
                                            where crr.ReviewStatus == (short)CodeReviewStatus.Accepted
                                            select crr.ReviewBy;

                    if (acceptedReviewers.Any()) //it had better be, but just checking
                    {

                        var isDba = from c in row.GetCodeReviewRows()
                                    join d in this.codeReviewDbaMembers
                                    on c.ReviewBy equals d
                                    where c.ReviewStatus == (short)CodeReviewStatus.Accepted
                                    select d;

                        if (isDba.Any())
                        {
                            lvi.iImage = (int)ScriptStatusType.CodeReviewAcceptedDba;
                        }
                        else
                        {
                            lvi.iImage = (int)ScriptStatusType.CodeReviewAccepted;
                        }
                    }
                    else
                    {
                        lvi.iImage = (int)ScriptStatusType.CodeReviewAccepted;
                    }


                }
                else
                {
                    lvi.iImage = (int)ScriptStatusType.CodeReviewInProgress;
                }
                OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
            }
        }
        private void SetCodeReviewIcons()
        {
            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
                return;

            //Find any codereview rows that fail their validation key (i.e.someone changed it in the database or in the file)
            var fail = from c in this.buildData.CodeReview
                       where c.ValidationKey != CodeReviewManager.GetValidationKey(ref c)
                       select c;

            if (fail.Any())
            {
                foreach (SqlSyncBuildData.CodeReviewRow r in fail)
                    r.ReviewStatus = (short)CodeReviewStatus.OutOfDate;
            }

            for (int x = 0; x < lstScriptFiles.Items.Count; x++)
            {
                SetCodeReviewIcon(x);
            }

        }

        private void bgCodeReview_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            bg.ReportProgress(0);
            bool databaseSuccess;
            CodeReviewManager.LoadCodeReviewData(this.buildData, out databaseSuccess);
            //CodeReviewManager.ValidateReviewCheckSum(this.buildData, this.projectFilePath);

            e.Result = databaseSuccess;
        }
        private void bgCodeReview_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.statGeneral.Text = "Checking on code review status...";
            this.progressBuild.Style = ProgressBarStyle.Marquee;
            this.Cursor = Cursors.AppStarting;
        }
        private void bgCodeReview_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Boolean && !(bool)e.Result)
            {
                string text = "Unable to connect to code review database.\r\nResults are from local data only.\r\nUpdates will be sync'd when connection is available";
                toolTip2.BackColor = Color.GhostWhite;
                this.toolTip2.ToolTipIcon = ToolTipIcon.Warning;
                this.toolTip2.Show(text, this.menuStrip1,7000);

                codeReviewIconToolStripMenuItem.ToolTipText = text;
                codeReviewIconToolStripMenuItem.ForeColor = Color.Red;
            }
            else
            {
                codeReviewIconToolStripMenuItem.ToolTipText = "";
                codeReviewIconToolStripMenuItem.ForeColor = Color.Blue;
                this.toolTip2.Hide(this.menuStrip1);

            }
            this.SetCodeReviewIcons();

            this.Cursor = Cursors.Default;
            this.statGeneral.Text = "Ready.";
            this.progressBuild.Style = ProgressBarStyle.Blocks;
        }
        #endregion

        private void RenameUnusedFiles()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                DirectoryInfo inf = new DirectoryInfo(this.projectFilePath);
                FileInfo[] files = inf.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (this.buildData.Script.Select("FileName ='" + files[i].Name + "'").Length == 0 &&
                        files[i].Name != Path.GetFileName(this.projectFileName) &&
                        files[i].Name.StartsWith("zzzz-") == false)
                    {
                        files[i].MoveTo("zzzz-" + files[i].Name);
                    }
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }




        private void SetDatabaseMenuList()
        {
            if (this.databaseList == null || this.databaseList.Count == 0)
            {
                mnuDDActiveDatabase.Enabled = false;
                mnuDDActiveDatabase.Text = "No Databases Found";
                return;
            }
            this.databaseList.Sort(new DatabaseListComparer());
            mnuDDActiveDatabase.Items.Clear();
            mnuDDActiveDatabase.Items.Add(selectDatabaseString);
            for (int i = 0; i < this.databaseList.Count; i++)
                mnuDDActiveDatabase.Items.Add(this.databaseList[i].DatabaseName);
            mnuDDActiveDatabase.SelectedIndex = 0;

            ddOverrideLogDatabase.Items.Clear();
            ddOverrideLogDatabase.Items.Add(string.Empty);
            for (int i = 0; i < this.databaseList.Count; i++)
                ddOverrideLogDatabase.Items.Add(this.databaseList[i].DatabaseName);
            ddOverrideLogDatabase.SelectedIndex = 0;
        }
        private void GenerateUtiltityItems()
        {
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string utilityPath = executablePath + @"\Utility";
            string utiltityXmlFile = utilityPath + @"\UtilityRegistry.xml";
            if (File.Exists(utiltityXmlFile) == false)
                return;

            using (StreamReader sr = new StreamReader(utiltityXmlFile))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Utility.SqlSyncUtilityRegistry));
                object obj = serializer.Deserialize(sr);
                this.utilityRegistry = (Utility.SqlSyncUtilityRegistry)obj;
            }
            if (this.utilityRegistry != null)
            {
                for (int i = 0; i < this.utilityRegistry.Items.Length; i++)
                {
                    SetUtilityQueryFilePath(ref this.utilityRegistry.Items[i], utilityPath);
                }
            }
            else
            {
                if (SqlBuildFileHelper.UpdateObsoleteXmlNamespace(utiltityXmlFile))
                    GenerateUtiltityItems();
            }
        }
        private void SetUtilityQueryFilePath(ref object utilityRegItem, string utilityPath)
        {
            if (utilityRegItem.GetType() == typeof(Utility.UtilityQuery))
            {
                ((Utility.UtilityQuery)utilityRegItem).FileName =
                    utilityPath + @"\" + ((Utility.UtilityQuery)utilityRegItem).FileName;
            }
            else if (utilityRegItem.GetType() == typeof(Utility.SubMenu))
            {
                Utility.SubMenu sub = (Utility.SubMenu)utilityRegItem;
                if (sub.Items == null) return;
                for (int i = 0; i < sub.Items.Length; i++)
                    SetUtilityQueryFilePath(ref sub.Items[i], utilityPath);
            }
        }
        private void GenerateAutoScriptList()
        {
            this.mnuAutoScripting.DropDownItems.Clear();
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.autoXmlFile = executablePath + @"\AutoScriptList.xml";
            if (File.Exists(autoXmlFile))
            {
                using (StreamReader sr = new StreamReader(autoXmlFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SqlSync.ObjectScript.AutoScriptList));
                    object obj = serializer.Deserialize(sr);
                    this.autoScriptListRegistration = (SqlSync.ObjectScript.AutoScriptList)obj;
                }
                if (this.autoScriptListRegistration != null)
                {


                    for (int i = 0; i < this.autoScriptListRegistration.Items.Length; i++)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(this.autoScriptListRegistration.Items[i].File));
                        item.Click += new EventHandler(autoScriptListItem_Click);
                        this.mnuAutoScripting.DropDownItems.Add(item);
                    }
                }
            }
            ToolStripMenuItem newItem = new ToolStripMenuItem("<< Add New AutoScript >>");
            newItem.Click += new EventHandler(autoScriptAddRegistration_Click);
            this.mnuAutoScripting.DropDownItems.Add(newItem);

        }
        private void DatabindBuildType()
        {
            FieldInfo[] fields = new BuildType().GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                ddBuildType.Items.Add(fields[i].GetValue(null));
            }
            ddBuildType.SelectedIndex = 0;
        }


        #region ## Main Menu and Context Menu Click Handlers ##
        #region Script List Menu
        private void mnuAddScript_Click(object sender, System.EventArgs e)
        {

            DialogResult result = dlgAddScriptFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                DisplayNewScriptWindow(dlgAddScriptFile.FileName);
            }
        }
        private bool DisplayNewScriptWindow(string fullFileName)
        {
            double lastBuildNumber = 0;
            string lastDatabase = string.Empty;
            string shortFileName = Path.GetFileName(fullFileName);

            GetLastBuildNumberAndDb(out lastBuildNumber, out lastDatabase);

            NewBuildScriptForm frmNew = new NewBuildScriptForm(this.projectFilePath, fullFileName, this.databaseList, lastBuildNumber, lastDatabase, System.Environment.UserName, this.tagList);
            DialogResult result2 = frmNew.ShowDialog();
            if (result2 == DialogResult.OK)
            {
             
                SqlSyncBuildData.ScriptRow cfgRow = this.buildData.Script.NewScriptRow();
                cfgRow.BuildOrder = frmNew.BuildOrder;
                cfgRow.Description = frmNew.Description;
                cfgRow.RollBackOnError = frmNew.RollBackScript;
                cfgRow.CausesBuildFailure = frmNew.RollBackBuild;
                cfgRow.Database = frmNew.DatabaseName;
                cfgRow.StripTransactionText = frmNew.StripTransactions;
                cfgRow.AllowMultipleRuns = frmNew.AllowMultipleRuns;
                cfgRow.ScriptTimeOut = frmNew.ScriptTimeout;
                cfgRow.Tag = frmNew.ScriptTag;

                this.ProcessNewScript(fullFileName, cfgRow);
            }

            frmNew.Dispose();
            return true;
        }
        private bool ProcessNewScript(string fullFileName, SqlSyncBuildData.ScriptRow cfgRow)
        {
            string shortFileName = Path.GetFileName(fullFileName);
            bool fileAdded = true;
            //Move the file to the build file folder
            string newLocalFile = this.projectFilePath + shortFileName;
            if (File.Exists(fullFileName) && fullFileName != newLocalFile)
            {
                bool copy = true;
                if (File.Exists(newLocalFile))
                {
                    DialogResult confirm = MessageBox.Show("The script file already exists in the destination folder.\r\nDo you want to overwrite it?", "Overwrite?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Cancel)
                    {
                        copy = false;
                        fileAdded = false;
                    }
                }
                if (copy == true)
                {
                    try
                    {
                        File.Copy(fullFileName, newLocalFile, true);
                    }
                    catch
                    {
                        fileAdded = false;
                        MessageBox.Show("Unable to move the file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
            }

            //Add the script 
            if (fileAdded)
            {
                SqlBuildFileHelper.AddScriptFileToBuild(
                        ref this.buildData,
                        this.projectFileName,
                        shortFileName,
                        cfgRow.BuildOrder,
                        cfgRow.Description,
                        cfgRow.RollBackOnError,
                        cfgRow.CausesBuildFailure,
                        cfgRow.Database,
                        cfgRow.StripTransactionText,
                        this.buildZipFileName,
                        true,
                        cfgRow.AllowMultipleRuns,
                        System.Environment.UserName,
                        cfgRow.ScriptTimeOut,
                        cfgRow.Tag
                        );
                
                //
                if (this.projectIsUnderSourceControl)
                {
                    SourceControlStatus stat = SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, newLocalFile);
                    if (stat == SourceControlStatus.Error || stat == SourceControlStatus.NotUnderSourceControl || stat == SourceControlStatus.Unknown)
                    {
                        MessageBox.Show("Unable to add file to source control. Please add it manually", "Source control problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                //Refresh the file list
                RefreshScriptFileList(cfgRow.BuildOrder, true);
            }

            PopulateTagList();

            return true;

        }
        private void mnuRemoveScriptFile_Click(object sender, System.EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            this.Cursor = Cursors.WaitCursor;
            StringBuilder sb = new StringBuilder("Are you sure you want to remove the follow file(s)?\r\n\r\n"); ;
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                if (lstScriptFiles.SelectedItems[i].BackColor == colorReadOnlyFile)
                {
                    MessageBox.Show("You have selected one or more Read-only files.\r\nThese can not be deleted until they are marked as writeable.", "Read-only files selected", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    this.Cursor = Cursors.Default;
                    return;
                }
                sb.Append("  " + lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text + "\r\n");
            }

            
            if (DialogResult.No == MessageBox.Show(sb.ToString(), "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2))
            {
                this.Cursor = Cursors.Default;
                return;
            }


            //Get list of rows to remove then remove them.
            SqlSyncBuildData.ScriptRow[] rows = new SqlSyncBuildData.ScriptRow[lstScriptFiles.SelectedItems.Count];
            List<string> fileNames = new List<string>();
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                rows[i] = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;
                fileNames.Add(rows[i].FileName);
            }

            bool deleteFiles = DeleteFilesFromSbx(rows);
            if (!SqlBuildFileHelper.RemoveScriptFilesFromBuild(ref this.buildData, this.projectFileName, this.buildZipFileName, rows, deleteFiles))
            {
                MessageBox.Show("Unable to remove file from list. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           

            if (lstScriptFiles.SelectedItems.Count > 0)
            {
                this.RefreshScriptFileList();
            }
            this.Cursor = Cursors.Default;
        }

        private void mnuEditFromResults_Click(object sender, System.EventArgs e)
        {
            EditFile(this.lstBuild);
        }
        private void lstScriptFiles_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.lstScriptFiles.SelectedItems.Count == 0)
            {
                AddNewTextScript();
            }
            else
            {
                if(CheckoutScriptFromSource())
                    EditFile(this.lstScriptFiles);
            }
        }
        private void mnuEditFile_Click(object sender, System.EventArgs e)
        {
            if(CheckoutScriptFromSource())
                EditFile(this.lstScriptFiles);
        }
        private bool CheckoutScriptFromSource()
        {
            //this.sbxBuildControlFileName --> only loose files will be under source control, all others will be zipped up in the SBM and not under individual source control
            if (this.projectIsUnderSourceControl && lstScriptFiles.SelectedItems.Count > 0 && this.sbxBuildControlFileName != null && this.sbxBuildControlFileName.Length > 0)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                string fileName = this.projectFilePath + row.FileName;
                SourceControlStatus stat = SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, fileName);
                if (stat == SourceControlStatus.Error || stat == SourceControlStatus.NotUnderSourceControl || stat == SourceControlStatus.Unknown)
                {
                    MessageBox.Show(String.Format("Error checking out {0} from source control. Please see application log for details", row.FileName), "Source Control Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else if (stat == SourceControlStatus.AlreadyPending)
                {
                    RefreshScriptFileList_SingleItem(row);
                }
                else
                {
                    RefreshScriptFileList_SingleItem(row);
                    //int index = lstScriptFiles.SelectedItems[0].Index;
                    //RefreshScriptFileList(index, false);
                    //lstScriptFiles.Items[index].Selected = true;
                    //return true;
                }
            }
            return true;
        }
        private void EditFile(ListView lstView)
        {
            if (lstView.SelectedItems.Count > 0)
            {
                bool hasChanges = false;
                string fileName;
                string scriptGuid;
                bool stripTransText;
                if (lstView.SelectedItems[0].Tag is SqlSyncBuildData.ScriptRow)
                {
                    fileName = ((SqlSyncBuildData.ScriptRow)lstView.SelectedItems[0].Tag).FileName;
                    scriptGuid = ((SqlSyncBuildData.ScriptRow)lstView.SelectedItems[0].Tag).ScriptId;
                    stripTransText = ((SqlSyncBuildData.ScriptRow)lstView.SelectedItems[0].Tag).StripTransactionText;

                }
                else if (lstView.SelectedItems[0].Tag is BuildScriptEventArgs)
                {
                    fileName = ((BuildScriptEventArgs)lstView.SelectedItems[0].Tag).FileName;
                    scriptGuid = ((BuildScriptEventArgs)lstView.SelectedItems[0].Tag).ScriptId;
                    stripTransText = ((BuildScriptEventArgs)lstView.SelectedItems[0].Tag).StripTransactionText;
                }

                else
                    return;

                string fullFileName = this.projectFilePath + fileName;
                if (File.Exists(fullFileName))
                {
                    SqlSyncBuildData.ScriptRow cfgRow = this.GetScriptRow(new Guid(scriptGuid));
                    if (cfgRow == null)
                    {
                        MessageBox.Show("Error: Unable to locate script with ID of " + scriptGuid, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string fileHash;
                    string textHash;
                    SqlBuild.SqlBuildFileHelper.GetSHA1Hash(fullFileName, out fileHash, out textHash, stripTransText);
                    bool allowEdit = true;
                    if ((File.GetAttributes(this.projectFilePath + fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        allowEdit = false;

                    this.PopulateTagList();

                    //this.buildData.SqlSyncBuildProject[0].ScriptTagRequired

                    AddScriptTextForm frmEdit = new AddScriptTextForm(ref this.buildData, fileName, fullFileName, this.utilityRegistry, ref cfgRow, textHash, this.databaseList, this.tagList, SqlSync.Properties.Settings.Default.RequireScriptTags, allowEdit);
                    DialogResult result = frmEdit.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        using (StreamWriter sw = File.CreateText(fullFileName))
                        {
                            sw.WriteLine(frmEdit.SqlText);
                            sw.Flush();
                            sw.Close();
                        }
                        string originalHash = textHash;
                        SqlBuild.SqlBuildFileHelper.GetSHA1Hash(fullFileName, out fileHash, out textHash, stripTransText);

                        //Update the modified date and user if changed found
                        if (originalHash != textHash)
                        {

                            cfgRow.DateModified = DateTime.Now;
                            cfgRow.ModifiedBy = System.Environment.UserName;
                            cfgRow.AcceptChanges();
                            cfgRow.Table.AcceptChanges();
                            hasChanges = true;
                        }

                        if (hasChanges || frmEdit.ConfigurationChanged)
                            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

                        //Update override Db list if necessary.
                        if (frmEdit.ConfigurationChanged)
                        {
                            this.SetUsedDatabases();
                            //if (!this.databasesUsed.Contains(cfgRow.Database))
                            //    this.databasesUsed.Add(cfgRow.Database);

                            //this.targetDatabaseOverrideCtrl1.SetDatabaseData(this.databaseList, this.databasesUsed);
                        }

                        this.PopulateTagList();

                        //Since the file or config may have changed, we'll need to refresh to update the icon.
                        if ((chkScriptChanges.Checked && hasChanges) || frmEdit.ConfigurationChanged)
                        {
                            this.RefreshScriptFileList_SingleItem(cfgRow);
                            if (!bgPolicyCheck.IsBusy)
                                bgPolicyCheck.RunWorkerAsync(new ListRefreshSettings() { SelectedItemIndex = cfgRow.BuildOrder });
                        }

                        if (frmEdit.BuildSequenceChanged)
                        {
                            this.listSorter.CurrentColumn = -100;
                            this.listSorter.Sort = SortOrder.Ascending;
                            lstScriptFiles_ColumnClick(null, new ColumnClickEventArgs(1));
                        }
                    }
                    frmEdit.Dispose();
                }
                else
                {
                    MessageBox.Show(string.Format("Unable to find file '{0}'", fullFileName), "Where did it go?!?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void mnuEditScriptFile_Click(object sender, System.EventArgs e)
        {
            PopulateTagList();
            if (lstScriptFiles.SelectedItems.Count == 1)
            {
                EditFile(this.lstScriptFiles);
                //SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                //if (row != null)
                //{
                //    double oldSequence = row.BuildOrder;
                //    NewBuildScriptForm frmNew = new NewBuildScriptForm(ref row,
                //        this.databaseList,
                //        this.tagList);

                //    DialogResult result = frmNew.ShowDialog();

                //    if (result == DialogResult.OK)
                //    {
                //        this.Cursor = Cursors.WaitCursor;
                //        this.UpdateScriptFileDetails(
                //            frmNew.ScriptID,
                //            frmNew.BuildOrder,
                //            frmNew.Description,
                //            frmNew.RollBackScript,
                //            frmNew.RollBackBuild,
                //            frmNew.DatabaseName,
                //            frmNew.StripTransactions,
                //            frmNew.AllowMultipleRuns,
                //            frmNew.ScriptTimeout);

                //        this.RefreshScriptFileList_SingleItem(row);

                //        if (oldSequence != frmNew.BuildOrder)
                //        {
                //            this.listSorter.CurrentColumn = -100;
                //            this.listSorter.Sort = SortOrder.Ascending;
                //            lstScriptFiles_ColumnClick(null, new ColumnClickEventArgs(1));
                //        }
                //        // this.RefreshScriptFileList();
                //    }


                //}
            }
            else if (lstScriptFiles.SelectedItems.Count > 1)
            {
                this.Cursor = Cursors.WaitCursor;
                SqlSyncBuildData.ScriptRow[] rows = new SqlSyncBuildData.ScriptRow[lstScriptFiles.SelectedItems.Count];
                for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                    rows[i] = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;
                this.Cursor = Cursors.Default;

                NewBuildScriptForm frmNew = new NewBuildScriptForm(ref rows,this.projectFilePath,  this.databaseList, this.tagList);
                if (DialogResult.OK == frmNew.ShowDialog())
                {
                    this.Cursor = Cursors.WaitCursor;
                    this.buildData.AcceptChanges();
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);
                    for (int i = 0; i < rows.Length; i++)
                        this.RefreshScriptFileList_SingleItem(rows[i]);

                    this.SetUsedDatabases();
                    //this.RefreshScriptFileList();
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void mnuRenumberSequence_Click(object sender, System.EventArgs e)
        {
            this.RenumberBuildSequence();
        }

        private void mnuRenameFiles_Click(object sender, System.EventArgs e)
        {
            this.RenameUnusedFiles();
        }

        private bool DeleteFilesFromSbx(SqlSyncBuildData.ScriptRow[] rows)
        {
            if (this.sbxBuildControlFileName == null || this.sbxBuildControlFileName.Length == 0) //don't delete file system files is this is an SBM file
                return true;

            string fileName = "FileName=\"{0}\"";
            Dictionary<string, List<string>> matches = new Dictionary<string, List<string>>(); 
            if (this.projectFileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
            {
                //Check for other SBX files in the same directory and see if they include the file in question...
                string[] sbxFiles = Directory.GetFiles(Path.GetDirectoryName(this.projectFileName), "*.sbx", SearchOption.TopDirectoryOnly);
                if (sbxFiles.Length == 1)
                    return true;

                //Get list of files that are reused in other SBX files...
                for (int i = 0; i < sbxFiles.Length; i++)
                {
                    if (sbxFiles[i] == this.projectFileName)
                        continue;

                    string fileContents = File.ReadAllText(sbxFiles[i]);
                    for (int j = 0; j < rows.Length; j++)
                    {
                        if (fileContents.IndexOf(String.Format(fileName, rows[j].FileName)) > -1)
                        {
                            if (matches.ContainsKey(sbxFiles[i]))
                                matches[sbxFiles[i]].Add(rows[j].FileName);
                            else
                                matches.Add(sbxFiles[i], new List<string>(new string[] { rows[j].FileName }));
                        }
                    }
                }

                //If there are matches, compile the warning message...
                if (matches.Count > 0)
                {
                    StringBuilder sb = new StringBuilder("The file(s) selected will be removed from this SBX project, however they are also being used in another SBX file:\r\n");
                    Dictionary<string, List<string>>.Enumerator enumer =  matches.GetEnumerator();
                    while(enumer.MoveNext())
                    {
                         sb.AppendLine(Path.GetFileName( enumer.Current.Key));
                         for (int i = 0; i < enumer.Current.Value.Count; i++)
                             sb.AppendLine("\t" + enumer.Current.Value[i]);
                    }
                    sb.AppendLine("Deleting these files will cause the other SBX files to fail.");
                    sb.AppendLine("Do you want to physically delete these file(s) anyway?");

                    if (DialogResult.Yes == MessageBox.Show(sb.ToString(), "Delete warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

               
            }
            return true;
        }
        private void mnuCreateExportFile_Click(object sender, System.EventArgs e)
        {
            //Get the list of rows to action on...
            SqlSyncBuildData.ScriptRow[] rows = new SqlSyncBuildData.ScriptRow[lstScriptFiles.SelectedItems.Count];
            for (int i = 0; i < this.lstScriptFiles.SelectedItems.Count; i++)
                rows[i] = (SqlSyncBuildData.ScriptRow)this.lstScriptFiles.SelectedItems[i].Tag;

            DialogResult resultRemove = DialogResult.No;
            DialogResult result = saveExportFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = saveExportFile.FileName;
                if (SaveBuildExportFile(rows, fileName))
                {
                    resultRemove = MessageBox.Show("SQL Build Export File successfully saved!\r\nDo you want to remove the scripts from the current build file?", "Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else
                {
                    MessageBox.Show("Unable to save SQL Build Export File", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (resultRemove == DialogResult.Yes)
            {
                bool deleteFiles = DeleteFilesFromSbx(rows);
                this.Cursor = Cursors.WaitCursor;
                if (!SqlBuildFileHelper.RemoveScriptFilesFromBuild(ref this.buildData, this.projectFileName, this.buildZipFileName, rows, deleteFiles))
                {
                    MessageBox.Show("Unable to remove file from list. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.RefreshScriptFileList();
            }
            this.Cursor = Cursors.Default;
        }

        private void mnuAddSqlScriptText_Click(object sender, System.EventArgs e)
        {
            AddNewTextScript();
        }
        private void AddNewTextScript()
        {
            double lastBuildNumber = 0;
            string lastDatabase = string.Empty;

            SqlSyncBuildData.ScriptRow newRow = this.buildData.Script.NewScriptRow();
            GetLastBuildNumberAndDb(out lastBuildNumber, out lastDatabase);
            newRow.Database = lastDatabase;
            newRow.BuildOrder = Math.Floor(lastBuildNumber + 1);
            newRow.CausesBuildFailure = true;
            if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout > 0)
                newRow.ScriptTimeOut = SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout;

            // this.buildData.SqlSyncBuildProject[0].ScriptTagRequired
            AddScriptTextForm frmAdd = new AddScriptTextForm(ref this.buildData, ref newRow, this.utilityRegistry, this.databaseList, this.tagList, SqlSync.Properties.Settings.Default.RequireScriptTags);

            //TODO: handle changes.
            DialogResult result = frmAdd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string sql = frmAdd.SqlText;
                string name = frmAdd.SqlName;
                if (name.ToLower().EndsWith(".sql") == false)
                {
                    name = name + ".sql";
                }
                //bool fileAdded = false;
                //bool writeFile = true;

                string fullName = Path.GetTempPath() + name;
                if (!File.Exists(fullName))
                {
                    using (StreamWriter sw = File.CreateText(fullName))
                    {
                        sw.WriteLine(sql);
                        sw.Flush();
                        sw.Close();
                        //fileAdded = true;
                    }
                }

                ProcessNewScript(fullName, newRow);

                frmAdd.Dispose();
                this.SetUsedDatabases();
            }
        }
        private void mnuTryScript_Click(object sender, System.EventArgs e)
        {
            double[] selected = GetSelectedScriptIndexes();
            if (selected.Length > 0)
                this.RunSingleFiles(selected, true, false);

        }
        private void mnuRunScript_Click(object sender, System.EventArgs e)
        {
            double[] selected = GetSelectedScriptIndexes();
            if (selected.Length > 0)
                this.RunSingleFiles(selected, false, false);

        }

        private double[] GetSelectedScriptIndexes()
        {

            if (this.lstScriptFiles.SelectedItems.Count > 0)
            {
                double[] selected = new double[this.lstScriptFiles.SelectedItems.Count];
                for (int i = 0; i < selected.Length; i++)
                    selected[i] = double.Parse(this.lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.SequenceNumber].Text);

                return selected;
            }
            else
            {
                return new double[0];
            }

        }
        private void mnuExportBuildListToClipBoard_Click(object sender, System.EventArgs e)
        {
            string export = BuildExportString();
            Clipboard.SetDataObject(export, true);
        }




        #endregion

        #region Main Menu
        private void mnuLoadProject_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openSbmFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = openSbmFileDialog.FileName;
                //if (PassesReadOnlyProjectFileCheck(fileName))
                //{
                    if (Path.GetExtension(fileName).ToLower() == ".sbx")
                        LoadXmlControlFile(fileName);
                    else
                        LoadProject(fileName);
                //}
            }
                
        }

        private void AddDefaultScript(DefaultScripts.DefaultScript defScript, string projectFileName, DefaultScriptCopyAction copyAction)
        {
            string msg;
            string scriptName = Path.GetFileName(defScript.ScriptName);
            DefaultScriptCopyStatus status = SqlBuildFileHelper.AddDefaultScriptToBuild(ref this.buildData, defScript, copyAction, projectFileName, this.buildZipFileName);
             switch (status)
             {
                 case DefaultScriptCopyStatus.DefaultNotFound:
                     msg = "Unable to add locate default script: '" + scriptName + "'.\r\nYou may continue, but the script has not been added to your project.";
                     MessageBox.Show(msg, "Default Script Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     break;
                 case DefaultScriptCopyStatus.PreexistingDifferent:
                     msg = "There is already a different version of the default script '" + scriptName + "' in your project directory.\r\n Do you want to overwrite the existing file?" +
                         "\r\n\r\nYes to overwrite the existing file with the default version\r\nNo to add the exsiting file to your build package";
                     if (DialogResult.Yes == MessageBox.Show(msg, "Altered Default Script", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                         AddDefaultScript(defScript, projectFileName, DefaultScriptCopyAction.OverwriteExisting);
                     else
                         AddDefaultScript(defScript, projectFileName, DefaultScriptCopyAction.LeaveExisting);
                     break;
                 case DefaultScriptCopyStatus.PreexistingReadOnly:
                     msg = "There is already a read-only version of the default script '" + scriptName + "' in your project directory.\r\n This copy of the file will be added to your project, but will remain read-only.";
                     MessageBox.Show(msg, "Read-Only Default Script", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     break;
                  case DefaultScriptCopyStatus.PreexistingDifferentReadOnly:
                     msg = "There is already a read-only version of the default script '" + scriptName + "' in your project directory.\r\nThis version is different from the standard default script.\r\nThe pre-existing copy of the file will be added to your project, but you should check it for accuracy.";
                     MessageBox.Show(msg, "Altered Read-Only Default Script", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     break;
                 case DefaultScriptCopyStatus.Success:
                     break;
             }
        }
        private void LoadXmlControlFile(string fileName)
        {
            if (!PassesReadOnlyProjectFileCheck(fileName))
            {
                return;
            }
            this.Cursor = Cursors.AppStarting;
            this.sbxBuildControlFileName = fileName;
            this.mruManager.Add(fileName);
            this.projectFileName = fileName;
            this.projectFilePath = Path.GetDirectoryName(fileName) + @"\";
            this.buildZipFileName = null;
            this.settingsControl1.Project = fileName;
            if (File.Exists(fileName))
            {

                this.statGeneral.Text = "Reading Control File.";
                SqlBuildFileHelper.LoadSqlBuildProjectFile(out this.buildData, fileName, true);
            }
            else
            {
                this.statGeneral.Text = "Creating Base Build File.";
                if (this.buildData != null)
                {
                    this.buildData.Clear();
                    this.buildData.AcceptChanges();
                }
                this.buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                this.buildData.AcceptChanges();

                DefaultScriptRegistry defReg = SqlBuildFileHelper.GetDefaultScriptRegistry();
                foreach (DefaultScripts.DefaultScript defScript in defReg.Items)
                {
                    this.AddDefaultScript(defScript, this.sbxBuildControlFileName, DefaultScriptCopyAction.Undefined);
                }
                this.buildData.WriteXml(this.sbxBuildControlFileName);
            }

            this.Cursor = Cursors.WaitCursor;
            this.lstScriptFiles.Items.Clear();
            this.progressBuild.Style = ProgressBarStyle.Marquee;
            this.grbBuildScripts.Enabled = false;
            this.grpManager.Enabled = false;
            this.grpBuildResults.Enabled = false;
         

            this.SetUsedDatabases();
            this.RefreshScriptFileList(true);
            this.SetUsedDatabases();
            this.mnuListTop.Enabled = true;
            this.mnuLogging.Enabled = true;
            this.mnuScripting.Enabled = true;
            this.grbBuildScripts.Enabled = true;
            this.grpManager.Enabled = true;
            this.grpBuildResults.Enabled = true;

            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = true;

            this.Cursor = Cursors.Default;
        }
 
        
        private void LoadProject(string fileName)
        {
            this.Cursor = Cursors.AppStarting;
            if (!PassesReadOnlyProjectFileCheck(fileName))
            {
                this.Cursor = Cursors.Default;
                return;
            }

            this.sbxBuildControlFileName = null;
            this.packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = false;

            try
            {

                SqlBuildFileHelper.InitilizeWorkingDirectory(ref this.workingDirectory, ref this.projectFilePath, ref this.projectFileName);
                this.buildZipFileName = fileName;
                this.mruManager.Add(fileName);
                if (File.Exists(this.buildZipFileName))
                {
                    this.statGeneral.Text = "Reading Build File.";
                }
                else
                {
                    this.statGeneral.Text = "Creating Base Build File.";
                    if (this.buildData != null)
                    {
                        this.buildData.Clear();
                        this.buildData.AcceptChanges();
                    }
                    this.buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                    this.buildData.AcceptChanges();
                    SqlBuildFileHelper.PackageProjectFileIntoZip(this.buildData, this.projectFilePath, this.buildZipFileName);

                    DefaultScriptRegistry defReg = SqlBuildFileHelper.GetDefaultScriptRegistry();
                    if (defReg != null && defReg.Items != null)
                    {
                        foreach (DefaultScripts.DefaultScript defScript in defReg.Items)
                        {
                            this.AddDefaultScript(defScript, this.projectFilePath + XmlFileNames.MainProjectFile, DefaultScriptCopyAction.Undefined);
                        }
                    }
                }


                this.Cursor = Cursors.WaitCursor;
                this.lstScriptFiles.Items.Clear();
                this.statGeneral.Text = "Loading Build File...";
                this.progressBuild.Style = ProgressBarStyle.Marquee;
                this.grbBuildScripts.Enabled = false;
                this.grpManager.Enabled = false;
                this.grpBuildResults.Enabled = false;

                this.bgLoadZipFle.RunWorkerAsync(this.buildZipFileName);

            }
            catch(Exception exe)
            {
                log.Error("Error loading project file:" + fileName, exe);
                MessageBox.Show("Unable to load project file. The following error has been recorded in the log file: \r\n"+ exe.ToString(), "An error has occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.connData = frmConnect.SqlConnection;
                this.databaseList = frmConnect.DatabaseList;
                this.settingsControl1.InitServers(true);

                settingsControl1_ServerChanged(sender, this.connData.SQLServerName);
                //this.SqlBuildForm_Load(null, EventArgs.Empty);
            }
        }

        private void mnuExportBuildList_Click(object sender, System.EventArgs e)
        {
            string build = BuildExportString();
            if (build.Length > 0)
            {
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName, false))
                    {
                        writer.WriteLine(build);
                        writer.Flush();
                        writer.Close();
                    }
                    MessageBox.Show("Export Complete");
                }
            }
        }

        private void mnuImportScriptFromFile_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openScriptExportFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                ImportSqlBuildFiles(new string[] { openScriptExportFile.FileName });
            }
        }

        private void ImportSqlBuildFiles(string[] fileNames)
        {
            string[] addedFileNames;
            double lastBuildNumber;
            string lastDatabase;
            for (int i = 0; i < fileNames.Length; i++)
            {
                GetLastBuildNumberAndDb(out lastBuildNumber, out lastDatabase);

                double importStartNumber = ImportSqlScriptFile(fileNames[i], lastBuildNumber, out addedFileNames);
                if (importStartNumber > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Import Successful\r\n");
                    sb.Append("New file indexes start at: " + importStartNumber.ToString() + "\r\n\r\n");
                    this.RefreshScriptFileList(true);
                    MessageBox.Show(sb.ToString(), "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (importStartNumber == (double)ImportFileStatus.Canceled)
                    {
                        MessageBox.Show("Import Canceled", "Import Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (importStartNumber == (double)ImportFileStatus.UnableToImport)
                    {
                        MessageBox.Show("Unable to Import the requested file", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (importStartNumber == (double)ImportFileStatus.NoRowsImported)
                    {
                        MessageBox.Show("No rows were selected for import", "None selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void mnuBulkAdd_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openFileBulkLoad.ShowDialog();
            if (result == DialogResult.OK)
            {
                string[] fileList = openFileBulkLoad.FileNames;
                List<string> tmp = new List<string>(fileList);
                tmp.Sort();
                BulkAdd(tmp.ToArray());

            }
        }
        private void mnuBulkFromList_Click(object sender, System.EventArgs e)
        {
            BulkAddListForm frmBulk = new BulkAddListForm();
            DialogResult result = frmBulk.ShowDialog();
            if (result == DialogResult.OK)
            {
                string[] files = frmBulk.SelectedFiles;
                if (files.Length > 0)
                    BulkAdd(files);
            }
        }
        private void mnuBulkFromFile_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openFileBulkLoad.ShowDialog();
            if (result == DialogResult.OK)
            {
                string sourceFile = openFileBulkLoad.FileName;
                if (File.Exists(sourceFile))
                {
                    ArrayList lst = new ArrayList();
                    using (StreamReader sr = File.OpenText(sourceFile))
                    {
                        string fileName;
                        while ((fileName = sr.ReadLine()) != null)
                        {
                            if (File.Exists(fileName))
                            {
                                lst.Add(fileName);
                            }
                        }
                        sr.Close();
                    }
                    string[] fileList = new string[lst.Count];
                    lst.CopyTo(fileList);
                    BulkAdd(fileList);
                }
            }
        }




        private void mnuFindScript_Click(object sender, System.EventArgs e)
        {
            FindForm frmFind = new FindForm(this.searchText);
            DialogResult result = frmFind.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.searchStartIndex = 0;
                this.searchText = frmFind.ScriptName;
                SearchScriptFileList();

            }
        }

        #endregion

        #region Results Context Menu
        private void mnuDisplayRowResults_Click(object sender, System.EventArgs e)
        {
            if (lstBuild.SelectedItems.Count > 0)
            {
                if (lstBuild.SelectedItems[0].Tag is BuildScriptEventArgs)
                {
                    System.Guid buildScriptId = ((BuildScriptEventArgs)lstBuild.SelectedItems[0].Tag).BuildScriptId;
                    string buildHistFile = this.projectFilePath + XmlFileNames.HistoryFile;
                    SqlSyncBuildData buildHistData = new SqlSyncBuildData();
                    buildHistData.ReadXml(buildHistFile);

                    DataRow[] runRows = buildHistData.ScriptRun.Select("ScriptRunId ='" + buildScriptId.ToString() + "'");
                    if (runRows.Length > 0)
                    {
                        SqlSyncBuildData.ScriptRunRow row = (SqlSyncBuildData.ScriptRunRow)runRows[0];
                        BuildHistory.ScriptRunResultsForm frmResult = new BuildHistory.ScriptRunResultsForm(row);
                        frmResult.ShowDialog();
                    }
                }
            }
        }

        private void mnuOpenRunScriptFile_Click(object sender, System.EventArgs e)
        {
            if (lstBuild.SelectedItems.Count > 0)
            {
                string fileName = lstBuild.SelectedItems[0].SubItems[(int)BuildListIndex.FileName].Text;
                Process prc = new Process();
                prc.StartInfo.FileName = "notepad.exe";
                prc.StartInfo.Arguments = this.projectFilePath + fileName;
                prc.Start();
            }
        }
        #endregion
        #endregion

        private void lnkStartBuild_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {

            this.Cursor = Cursors.AppStarting;

            //Update the autocomplete source...
            if (txtBuildDesc.Text.Length > 0 && !SqlSync.Properties.Settings.Default.Description.Contains(txtBuildDesc.Text))
                SqlSync.Properties.Settings.Default.Description.Add(txtBuildDesc.Text);

            SqlSync.Properties.Settings.Default.Save();



            bool isTrial = false;
            if (ddBuildType.SelectedItem == null || ddBuildType.SelectedItem.ToString().Length == 0)
            {
                MessageBox.Show("Please select a Build Type", "Build Type Needed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (ddBuildType.SelectedItem.ToString() == BuildType.Trial ||
                ddBuildType.SelectedItem.ToString() == BuildType.TrialPartial)
                isTrial = true;

            try
            {
                this.connData.ScriptTimeout = 200;
            }
            catch { }

            SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
            runData.BuildData = this.buildData;
            runData.BuildType = ddBuildType.SelectedItem.ToString();
            runData.BuildDescription = txtBuildDesc.Text;
            runData.StartIndex = Double.Parse(txtStartIndex.Text);
            runData.ProjectFileName = this.projectFileName;
            runData.IsTrial = isTrial;
            runData.BuildFileName = this.buildZipFileName;
            runData.IsTransactional = !this.chkNotTransactional.Checked;
            runData.TargetDatabaseOverrides = this.targetDatabaseOverrideCtrl1.GetOverrideData();
            if (ddOverrideLogDatabase.SelectedItem != null && ddOverrideLogDatabase.SelectedItem.ToString().Length > 0)
                runData.LogToDatabaseName = ddOverrideLogDatabase.SelectedItem.ToString();


            if(!ConnectionHelper.ValidateDatabaseOverrides(runData.TargetDatabaseOverrides))
            {
                MessageBox.Show("One or more scripts is missing a default or target override database setting.\r\nRun has been halted. Please correct the error and try again","Missing Database setting",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            bgBuildProcess.RunWorkerAsync(runData);

        }

        private void txtBuildDesc_TextChanged(object sender, System.EventArgs e)
        {
            if (txtBuildDesc.Text.Length > 0)
            {
                this.lnkStartBuild.Enabled = true;
                this.lnkStartBuild.Text = "Start Build on \"" + this.connData.SQLServerName + "\"";
            }
            else
            {
                this.lnkStartBuild.Enabled = false;
                this.lnkStartBuild.Text = "Please Enter a Description";

            }
        }

        #region Bulk Add Method Handlers
        internal void BulkAdd(string[] fileList)
        {
            this.BulkAdd(fileList, false);
        }
        internal void BulkAdd(string[] fileList, bool deleteOriginals)
        {
            BulkAdd(fileList, deleteOriginals, "");
        }
        internal void BulkAdd(string[] fileList, bool deleteOriginals, string preSelectedDbName)
        {
            double lastBuildNumber;
            string lastDatabase;
            GetLastBuildNumberAndDb(out lastBuildNumber, out lastDatabase);

            //Get the last whole number since we initially add 1
            lastBuildNumber = Math.Floor(lastBuildNumber);


            BulkAddConfirmation frmConfirm = new BulkAddConfirmation(fileList.ToList(), this.projectFilePath, this.buildData);
            DialogResult result = frmConfirm.ShowDialog();
            if (result == DialogResult.Cancel)
                return;


            fileList = frmConfirm.SelectedFiles;

            BulkAddData b = new BulkAddData()
            {
                PreSetDatabase = preSelectedDbName,
                FileList = fileList.ToList(),
                DeleteOriginalFiles = deleteOriginals,
                CreateNewEntriesForPreExisting = frmConfirm.CreateNewEntries,
                LastBuildNumber = lastBuildNumber
            };

            while (bgBulkAdd.IsBusy)
            {
                statGeneral.Text = "Waiting for bulk add worker...";
            }

            statGeneral.Text = "Adding files..";
            progressBuild.Style = ProgressBarStyle.Marquee;
            this.Cursor = Cursors.AppStarting;
            bgBulkAdd.RunWorkerAsync(b);
        }

        private void bgBulkAdd_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is BulkAddData)
            {
                BulkAddData b = (BulkAddData)e.Argument;
                Package lstViolations = BulkAddBackgroundPolicyCheck((BackgroundWorker)sender, b.FileList, b.DeleteOriginalFiles, b.PreSetDatabase, b.CreateNewEntriesForPreExisting, b.LastBuildNumber);
                e.Result = new KeyValuePair<BulkAddData, Package>((BulkAddData)e.Argument, lstViolations);
            }
            else
            {
                e.Result = new KeyValuePair<BulkAddData, Package>((BulkAddData)e.Argument, null);
            }
            return;
        }
        private Package BulkAddBackgroundPolicyCheck(BackgroundWorker bg, List<string> fileList, bool deleteOriginals, string preSelectedDbName, bool createNewEntries, double lastBuildNumber)
        {
            if (!this.runPolicyCheckingOnLoad) //only need to run here if it's not run later..
            {
                PolicyHelper policyHelp = new PolicyHelper();
                bg.ReportProgress(0, "Validating files against policies...");
                Package lstViolations = policyHelp.ValidateFilesAgainstPolicies(fileList);
                return lstViolations;
            }
            else
            {
                return null;
            }
        }
        private void bgBulkAdd_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                statGeneral.Text = e.UserState.ToString();
            }
        }
        private void bgBulkAdd_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            KeyValuePair<BulkAddData, Package> result = (KeyValuePair<BulkAddData, Package>)e.Result;
            if (result.Value is Package && result.Value != null)
            {
                Package lstViolations = (Package)result.Value;
                Policy.PolicyViolationForm frmViolation = new SqlSync.SqlBuild.Policy.PolicyViolationForm(lstViolations, true);
                frmViolation.StartPosition = FormStartPosition.CenterParent;
                frmViolation.BringToFront();
                if (DialogResult.No == frmViolation.ShowDialog())
                    return;
            }

            BulkAddData b = result.Key;
            string lastDatabase;
            if (b.PreSetDatabase.Length != 0)
                lastDatabase = b.PreSetDatabase;
            else
                lastDatabase = string.Empty;

            statGeneral.Text = "Waiting for script run-time meta data...";

            NewBuildScriptForm frmNew = new NewBuildScriptForm(this.projectFilePath, "<bulk>", this.databaseList, b.LastBuildNumber, lastDatabase, System.Environment.UserName, this.tagList);
            frmNew.StartPosition = FormStartPosition.CenterParent;
            frmNew.BringToFront();
            DialogResult result2 = frmNew.ShowDialog();

            if (result2 != DialogResult.OK)
                return;

            b.TagInferSource = frmNew.TagInferSource;
            b.TagInferSourceRegexFormats = new List<string>(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());
            b.AllowMultipleRuns = frmNew.AllowMultipleRuns;
            b.DatabaseName = frmNew.DatabaseName;
            b.Description = frmNew.Description;
            b.RollBackBuild = frmNew.RollBackBuild;
            b.RollBackScript = frmNew.RollBackScript;
            b.ScriptTag = frmNew.ScriptTag;
            b.StripTransactions = frmNew.StripTransactions;

            bgBulkAddStep2.RunWorkerAsync(b);


        }

        private void bgBulkAddStep2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is BulkAddData)
            {
                BulkAddData b = (BulkAddData)e.Argument;
                BulkAddBackground((BackgroundWorker)sender, b.FileList, b.DeleteOriginalFiles, b.PreSetDatabase, b.CreateNewEntriesForPreExisting, b.LastBuildNumber,
                    b.TagInferSource,b.TagInferSourceRegexFormats,b.ScriptTag,b.StripTransactions,b.Description,b.RollBackScript,b.RollBackBuild,b.DatabaseName,b.AllowMultipleRuns);
            }
        }
        private void BulkAddBackground(BackgroundWorker bg, List<string> fileList, bool deleteOriginals, string preSelectedDbName, bool createNewEntries, double lastBuildNumber,
            TagInferenceSource tagInferSource, List<string> regexFormats, string enteredScriptTag, bool stripTransactions, string description,
                        bool rollBackScript,
                        bool rollBackBuild,
                        string databaseName,
                        bool allowMultipleRuns)
        {
            int increment = 0;
            
            bool fileAdded = true;
            bool addEntry;
            bool fileExists = false;



            bg.ReportProgress(0, "Adding files...");
            for (int i = 0; i < fileList.Count; i++)
            {
                fileExists = false;
                addEntry = false;

                //Move the file to the build file folder
                string newLocalFile = this.projectFilePath + Path.GetFileName(fileList[i]);
                bg.ReportProgress(0, "Adding " + Path.GetFileName(fileList[i]));
                if (File.Exists(newLocalFile))
                    fileExists = true;

                try
                {
                    if (!fileList[i].Trim().Equals(newLocalFile.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (fileExists && this.projectIsUnderSourceControl)
                        {
                            bg.ReportProgress(0, String.Format("Checking out '{0}' from source control", newLocalFile));
                            SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, newLocalFile);
                        }
                        File.Copy(fileList[i], newLocalFile, true);
                    }


                    var val = from s in this.buildData.Script.AsEnumerable().Cast<SqlSyncBuildData.ScriptRow>()
                              where s.FileName == Path.GetFileName(newLocalFile)
                              select s.FileName;

                    //If it's a new file, 
                    //or... it's an existing file but the user wants a new entry  
                    //or... if the file exists in the folder but not in the project data
                    //then... add a new entry
                    if (fileExists == false || (fileExists == true && createNewEntries == true) || (fileExists && val.Count() == 0))
                    {
                        addEntry = true;
                        increment++;
                    }

                    fileAdded = true;
                }
                catch (Exception exe)
                {
                    log.Error("Unable to move new file to project temp folder", exe);
                    fileAdded = false;
                    MessageBox.Show("Unable to move the file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //Add the script 
                if (fileAdded && addEntry)
                {
                    string scriptTag;
                    if (tagInferSource != TagInferenceSource.None)
                    {
                        string tmpTag = ScriptTagProcessing.InferScriptTag(tagInferSource, regexFormats, Path.GetFileName(fileList[i]), Path.GetDirectoryName(fileList[i]));
                        if (tmpTag.Length > 0)
                            scriptTag = tmpTag;
                        else
                            scriptTag = enteredScriptTag;
                    }
                    else
                    {
                        scriptTag = enteredScriptTag;
                    }
                    bool stripTran;
                    string fileName = Path.GetFileName(fileList[i]);


                    if (fileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                        stripTran = false;
                    else
                        stripTran = stripTransactions;

                    SqlBuildFileHelper.AddScriptFileToBuild(
                        ref this.buildData,
                        this.projectFileName,
                        Path.GetFileName(fileList[i]),
                        lastBuildNumber + increment,
                        description,
                        rollBackScript,
                        rollBackBuild,
                        databaseName,
                        stripTran,
                        this.buildZipFileName,
                        false,
                        allowMultipleRuns,
                        System.Environment.UserName,
                        EnterpriseConfigHelper.GetMinumumScriptTimeout(fileList[i], SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout),// frmNew.ScriptTimeout,
                        scriptTag);


                }
                if (fileAdded && deleteOriginals)
                {
                    try
                    {
                        File.Delete(fileList[i]);
                    }
                    catch (Exception exe)
                    {
                        log.WarnFormat("Unable to delete temporary file '{0}' when trying to add to project temp path\r\n{1}", fileList[i], exe.ToString());
                    }
                }

                //Make sure the file is added to source control
                if (this.projectIsUnderSourceControl)
                {
                    bg.ReportProgress(0, String.Format("Adding '{0}' to source control", fileExists));
                    SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, newLocalFile);
                }
            }


            //Refresh the file list
            bg.ReportProgress(0, "Saving project file with new files");
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

            


        }

        private void bgBulkAddStep2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                statGeneral.Text = e.UserState.ToString();
            }
        }
        private void bgBulkAddStep2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshScriptFileList(true);
            this.SetUsedDatabases();
            progressBuild.Style = ProgressBarStyle.Blocks;
            this.Cursor = Cursors.Default;
            if (e.Result is bool)
            {
                if ((bool)e.Result)
                {
                    statGeneral.Text = "Bulk Add Complete. Ready.";
                }
                else
                {
                    statGeneral.Text = "Issue adding files encountered. Please check log file for details";
                }
            }
        }




        #endregion

        private void ddBuildType_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            if (ddBuildType.SelectedItem.ToString() == BuildType.Partial ||
                ddBuildType.SelectedItem.ToString() == BuildType.TrialPartial)
            {
                txtStartIndex.Enabled = true;
            }
            else
            {
                txtStartIndex.Text = "0";
                txtStartIndex.Enabled = false;
            }
        }



        #region ## Script Search Methods
        private void SearchScriptFileList()
        {

            for (int j = 0; j < this.lstScriptFiles.SelectedItems.Count; j++)
            {
                this.lstScriptFiles.SelectedItems[j].Selected = false;
            }

            for (int i = this.searchStartIndex; i < this.lstScriptFiles.Items.Count; i++)
            {
                if (this.lstScriptFiles.Items[i].SubItems[(int)ScriptListIndex.FileName].Text.ToLower().IndexOf(this.searchText.ToLower()) > -1)
                {
                    this.lstScriptFiles.Focus();
                    this.lstScriptFiles.Items[i].Selected = true;
                    this.lstScriptFiles.SelectedItems[0].EnsureVisible();
                    this.searchStartIndex = i + 1;
                    if (this.searchStartIndex >= this.lstScriptFiles.Items.Count)
                    {
                        this.searchStartIndex = 0;
                    }
                    return;
                }
            }
            MessageBox.Show("Reached end of list. Unable to find a matching file", "No Match", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.searchStartIndex = 0;
        }

        private void SqlBuildForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3 && this.searchText.Length > 0)
            {
                SearchScriptFileList();
            }
        }

        private void menuItem9_Click(object sender, System.EventArgs e)
        {
            if (this.searchText.Length > 0)
            {
                SearchScriptFileList();
            }
        }

        #endregion

        private void lstScriptFiles_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop);
                var sbmFile = from f in fileList
                              where f.EndsWith(".sbm", StringComparison.CurrentCultureIgnoreCase) || f.EndsWith(".sbe", StringComparison.CurrentCultureIgnoreCase)
                              select f;

                if (sbmFile.Count() == fileList.Length)
                {
                    ImportSqlBuildFiles(fileList);
                    return;
                }
                else if (fileList.Length > 1 && sbmFile.Count() > 0)
                {
                    MessageBox.Show("Unable to import a Sql Build File at the same time as SQL text files. Please import them separately.", "No mixing allowed", MessageBoxButtons.OK);
                    return;
                }

                BulkAdd(fileList);
            }
            //else if (e.Data.GetDataPresent(typeof(ListViewItem)))
            else if (e.Data.GetDataPresent(typeof(string)))
            {
                bool isLocal = (e.Data is DataObject);
                string data = (string)e.Data.GetData(typeof(string));


                //Deserialize the string list...
                List<string> dataList = null;
                XmlSerializer xmlS = new XmlSerializer(typeof(List<string>));
                using (StringReader sr = new StringReader(data))
                {
                    object raw = xmlS.Deserialize(sr);
                    if (raw != null)
                        dataList = (List<string>)raw;
                }

                //Deserialize again to get the ScriptRow / script text combinations...
                XmlSerializer xmls = new XmlSerializer(typeof(SqlSyncBuildData.ScriptDataTable));
                List<KeyValuePair<SqlSyncBuildData.ScriptRow, string>> lstDeserialized = new List<KeyValuePair<SqlSyncBuildData.ScriptRow, string>>();
                for(int i=0;i<dataList.Count;i++)
                {
                    string item = dataList[i];
                    //extract off the ***SCRIPT***...
                    int start = item.IndexOf("***SCRIPT***") + 12;
                    string script = item.Substring(start);
                    item = item.Substring(0, start - 12);


                    SqlSyncBuildData.ScriptRow droppedRow = null;
                    //Deserialize the table and get the row...
                    using (StringReader sr = new StringReader(item))
                    {
                        object obj = xmls.Deserialize(sr);
                        if (obj != null)
                            droppedRow = ((SqlSyncBuildData.ScriptDataTable)obj)[0];
                    }

                    KeyValuePair<SqlSyncBuildData.ScriptRow, string> tmp = new KeyValuePair<SqlSyncBuildData.ScriptRow, string>(droppedRow, script);
                    lstDeserialized.Add(tmp);
                }

                //Get the index number of where to insert the dropped item
                System.Drawing.Point pnt = lstScriptFiles.PointToClient(new Point(e.X, e.Y));
                double floorIndex, ceilingIndex;
                GetDragDropScriptItemIndex(pnt, out floorIndex, out ceilingIndex);
                
                //Renumber these build rows..
                List<double> indexes = SqlBuildFileHelper.GetInsertedIndexValues(floorIndex, ceilingIndex, lstDeserialized.Count);
                for (int i = 0; i < lstDeserialized.Count; i++)
                    lstDeserialized[i].Key.BuildOrder = indexes[i];


                ListViewItem droppedItem = null;
                if (isLocal)
                {
                    foreach (KeyValuePair<SqlSyncBuildData.ScriptRow, string> value in lstDeserialized)
                    {
                        foreach (ListViewItem item in lstScriptFiles.Items)
                        {
                            if (value.Key.ScriptId == ((SqlSyncBuildData.ScriptRow)item.Tag).ScriptId)
                            {
                                item.SubItems[(int)ScriptListIndex.SequenceNumber].Text = value.Key.BuildOrder.ToString();
                                ((SqlSyncBuildData.ScriptRow)item.Tag).BuildOrder = value.Key.BuildOrder;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<SqlSyncBuildData.ScriptRow, string> value in lstDeserialized)
                    {

                        droppedItem = new ListViewItem(new string[]{
                                                     "",
                                                     value.Key.BuildOrder.ToString(),
                                                     value.Key.FileName,
                                                     value.Key.Database,
                                                     value.Key.ScriptId,
                                                     "0",
                                                     value.Key.Tag,
                                                        value.Key.DateAdded.ToString(),
                                        (value.Key.IsDateModifiedNull() || value.Key.DateModified < Convert.ToDateTime("1/1/1980")) ? "" : value.Key.DateModified.ToString()});

                        droppedItem.Tag = value.Key;
                        lstScriptFiles.Items.Add(droppedItem);

                        if (!File.Exists(this.projectFilePath + value.Key.FileName))
                            File.WriteAllText(this.projectFilePath + value.Key.FileName, value.Value);

                        this.buildData.Script.ImportRow(value.Key);
                    }
                }


                this.buildData.AcceptChanges();
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

                this.RefreshScriptFileList(true);

                lstScriptFiles.ListViewItemSorter = listSorter;
                lstScriptFiles.Sort();

            }

        }
        private void GetDragDropScriptItemIndex(System.Drawing.Point pnt, out double floorIndex, out double ceilingIndex)
        {
            floorIndex = 0;
            ceilingIndex = (int)ResequenceIgnore.StartNumber;

            ListViewItem droppedOnItem = lstScriptFiles.GetItemAt(pnt.X, pnt.Y);
            if (droppedOnItem == null)
            {
                for (int i = lstScriptFiles.Items.Count - 1; i >= 0; i = i - 1)
                {
                    if (Double.Parse(lstScriptFiles.Items[i].SubItems[(int)ScriptListIndex.SequenceNumber].Text) < (int)ResequenceIgnore.StartNumber)
                    {
                        floorIndex = Double.Parse(lstScriptFiles.Items[i].SubItems[(int)ScriptListIndex.SequenceNumber].Text);
                        break;
                    }
                }
            }
            else
            {
                double droppedOnScriptOrder = Double.Parse(droppedOnItem.SubItems[(int)ScriptListIndex.SequenceNumber].Text);

                //Determine if we insert above or below the current item
                bool addAbove = true;
                System.Drawing.Rectangle rec = droppedOnItem.GetBounds(ItemBoundsPortion.Entire);
                int top = rec.Top;
                int half = rec.Height / 2;
                ListViewItem aboveItem = lstScriptFiles.GetItemAt(pnt.X, pnt.Y - half);
                if (aboveItem.SubItems[(int)ScriptListIndex.SequenceNumber].Text == droppedOnItem.SubItems[(int)ScriptListIndex.SequenceNumber].Text)
                    addAbove = false;


                if (droppedOnItem.Index == 0)
                {
                    if (double.Parse(lstScriptFiles.Items[0].SubItems[(int)ScriptListIndex.SequenceNumber].Text) > 1)
                    {
                        floorIndex = 0;
                        ceilingIndex = double.Parse(lstScriptFiles.Items[0].SubItems[(int)ScriptListIndex.SequenceNumber].Text);
                    }
                    else
                    {
                        floorIndex = 0;
                        ceilingIndex = 0.9;
                    }
                }
                else
                {
                    if (addAbove)
                    {
                        ListViewItem itemAbove = lstScriptFiles.Items[droppedOnItem.Index - 1];
                        double aboveScriptOrder = Double.Parse(itemAbove.SubItems[(int)ScriptListIndex.SequenceNumber].Text);
                        //floorIndex = droppedOnScriptOrder - ((droppedOnScriptOrder - aboveScriptOrder) / 2);
                        ceilingIndex = droppedOnScriptOrder;
                        floorIndex = aboveScriptOrder;
                    }
                    else
                    {
                        ListViewItem itemBelow = lstScriptFiles.Items[droppedOnItem.Index + 1];
                        double belowScriptOrder = Double.Parse(itemBelow.SubItems[(int)ScriptListIndex.SequenceNumber].Text);
                        //floorIndex = droppedOnScriptOrder + ((belowScriptOrder - droppedOnScriptOrder) / 2);
                        ceilingIndex = belowScriptOrder;
                        floorIndex = droppedOnScriptOrder;
                    }
                }
            }
            //return floorIndex;

        }
       
        private void lstScriptFiles_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else //if (e.Data.GetDataPresent(DataFormats.Serializable))
            {
                e.Effect = DragDropEffects.Move;
            }
        }


        private void RunSingleFiles(double[] runItemIndexes, bool isTrial, bool scriptOnly)
        {
            this.Cursor = Cursors.WaitCursor;
            string type;
            if (isTrial)
                type = "Trial";
            else
                type = "Partial";


            SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
            runData.BuildDescription = "Single File";
            //Determine if a build description is required..
            try
            {
                List<string> ids = (from s in this.buildData.Script 
                        join r in runItemIndexes on s.BuildOrder equals r
                        join d in this.scriptsRequiringBuildDescription on s.ScriptId equals d
                        select s.ScriptId).ToList();


                if (ids.Count > 0)
                {
                    BuildDescriptionForm frmDesc = new BuildDescriptionForm();
                    if (DialogResult.OK == frmDesc.ShowDialog())
                    {
                        runData.BuildDescription = frmDesc.BuildDescription;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    frmDesc.Dispose();
                }

            }
            catch(Exception exe)
            {
                log.Warn("Unable to determine if build description is required", exe);
            }

            
            runData.BuildData = this.buildData;
            runData.BuildType = type;
            
            runData.StartIndex = Double.Parse(txtStartIndex.Text);
            runData.ProjectFileName = this.projectFileName;
            runData.IsTrial = isTrial;
            runData.RunItemIndexes = runItemIndexes;
            runData.RunScriptOnly = scriptOnly;
            runData.BuildFileName = this.buildZipFileName;
            runData.TargetDatabaseOverrides = this.targetDatabaseOverrideCtrl1.GetOverrideData();
            runData.IsTransactional = true; //for context menu driven runs, always use transactions!!!
            bgBuildProcess.RunWorkerAsync(runData);
        }

        /// <summary>
        /// Executes the run via a non-interactive mode...
        /// </summary>
        private void ProcessBuildUnattended()
        {
            SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
            runData.BuildData = this.buildData;
            runData.BuildType = "Other";
            runData.BuildDescription = "Unattended Build";
            runData.StartIndex = 0;
            runData.ProjectFileName = this.projectFileName;
            runData.IsTrial = false;
            runData.RunScriptOnly = false;
            runData.BuildFileName = this.buildZipFileName;
            runData.IsTransactional = true; //For now, "/Transaction" flag is only for MultiDb runs

            bgBuildProcess.RunWorkerAsync(runData);
            this.txtBuildDesc.Text = string.Empty;

        }
        private void ProcessMultiDbBuildUnattended()
        {
            this.multiDbRunData.BuildData = this.buildData;
            this.multiDbRunData.ProjectFileName = this.projectFileName;
            bgBuildProcess.RunWorkerAsync(this.multiDbRunData);

        }


        private void mnuClearPreviouslyRunBlocks_Click(object sender, System.EventArgs e)
        {
            string[] selectedScriptIds = new string[this.lstScriptFiles.SelectedItems.Count];
            for (int i = 0; i < this.lstScriptFiles.SelectedItems.Count; i++)
                selectedScriptIds[i] = this.lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.ScriptId].Text;

            ClearScriptData scrData = new ClearScriptData(selectedScriptIds, this.buildData, this.projectFileName, this.buildZipFileName);
            bgBuildProcess.RunWorkerAsync(scrData);
            this.txtBuildDesc.Text = string.Empty;


        }

        private void mnuShowBuildLogs_Click(object sender, System.EventArgs e)
        {
            SqlSync.BuildHistory.LogFileHistoryForm frmLog = new SqlSync.BuildHistory.LogFileHistoryForm(this.projectFilePath, this.buildZipFileName);
            frmLog.LogFilesArchvied += new EventHandler(frmLog_LogFilesArchvied);
            frmLog.ShowDialog();
            frmLog.Dispose();
        }
        private void frmLog_LogFilesArchvied(object sender, EventArgs e)
        {
            this.statGeneral.Text = "Updating Project File";
            SqlBuildFileHelper.PackageProjectFileIntoZip(this.buildData, this.projectFilePath, this.buildZipFileName);
            this.statGeneral.Text = "Project File Updated";
        }

        private void mnuScriptToLogFile_Click(object sender, System.EventArgs e)
        {
            this.RunSingleFiles(new double[0], false, true);
        }

        private void mnuShowBuildHistory_Click(object sender, System.EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            SqlBuild.SqlBuildFileHelper.ConvertLegacyProjectHistory(ref this.buildData, this.projectFilePath, this.buildZipFileName);
            string buildHistXml = this.projectFilePath + XmlFileNames.HistoryFile;
            if (!File.Exists(buildHistXml))
            {
                MessageBox.Show("No Build Run History is Available", "No History", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SqlSyncBuildData histdata = new SqlSyncBuildData();
                histdata.ReadXml(buildHistXml);
                SqlSync.BuildHistory.PastBuildReviewForm frmPast = new SqlSync.BuildHistory.PastBuildReviewForm(histdata);
                frmPast.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }

        private void lstScriptFiles_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            try
            {
                List<string> draggedItems = new List<string>();
                XmlSerializer xmls = new XmlSerializer(typeof(SqlSyncBuildData.ScriptDataTable));
                for (int i = 0; i < this.lstScriptFiles.SelectedItems.Count; i++)
                {

                    SqlSyncBuildData.ScriptRow item = (SqlSyncBuildData.ScriptRow)this.lstScriptFiles.SelectedItems[i].Tag;
                    SqlSyncBuildData.ScriptDataTable tbl = new SqlSyncBuildData.ScriptDataTable();
                    tbl.ImportRow(item);
                    tbl.AcceptChanges();
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        xmls.Serialize(sw, tbl);
                    }

                    string fullFileName = this.projectFilePath + item.FileName;
                    if (File.Exists(fullFileName))
                    {
                        string script = File.ReadAllText(fullFileName);
                        sb.AppendLine("");
                        sb.Append("***SCRIPT***");
                        sb.Append(script);
                    }
                    draggedItems.Add(sb.ToString());
                }

                xmls = new XmlSerializer(typeof(List<string>));
                StringBuilder sbAll = new StringBuilder();
                using (StringWriter sw = new StringWriter(sbAll))
                {
                    xmls.Serialize(sw, draggedItems);
                }
                this.DoDragDrop(sbAll.ToString(), DragDropEffects.Move);
            }
            catch
            {
            }

        }

        private void mnuResortByFileType_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Resequencing by file type will resort in ordering of the files by extension in the following order:\r\n");
            for (int i = 0; i < ResortBuildType.SortOrder.Length; i++)
                sb.Append("." + ResortBuildType.SortOrder[i] + "\r\n");
            sb.Append(".*\r\n");
            sb.Append("Are you sure you want to re-sequence the build?");

            if (DialogResult.OK == MessageBox.Show(sb.ToString(), "Verify Resequence", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                SqlBuildFileHelper.ResortBuildByFileType(ref this.buildData, this.projectFileName, this.buildZipFileName);
                this.LoadSqlBuildProjectFileData(ref buildData, projectFileName, false);
                this.RefreshScriptFileList();
            }

        }

        //http://www.pinvoke.net/default.aspx/user32/GetKeyState.html
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetKeyState(short nVirtKey);

        private void settingsControl1_Click(object sender, System.EventArgs e)
        {
            if (this.settingsControl1.Project.ToLower() == "(select / create project)")
                return;

            string dir;
            int val = GetKeyState((short)Keys.ControlKey);
            if (val >= 0)
            {
                dir = Path.GetDirectoryName(this.settingsControl1.Project);
            }
            else
            {
                dir = this.projectFilePath;
            }

            if (Directory.Exists(dir))
            {
                System.Diagnostics.Process prc = new Process();
                prc.StartInfo.FileName = "explorer";
                prc.StartInfo.Arguments = dir;
                prc.Start();
            }

        }

        private void settingsControl1_DoubleClick(object sender, System.EventArgs e)
        {
            mnuLoadProject_Click(sender, e);
        }


        public event UnattendedProcessingCompleteEventHandler UnattendedProcessingCompleteEvent;

        private void mnuAbout_Click(object sender, System.EventArgs e)
        {
            //About frmAbout = new About(this.impersonatedUser);
            //frmAbout.ShowDialog();
            //this.impersonatedUser = frmAbout.impersonatedUser;

            this.Cursor = Cursors.AppStarting;
            bgCheckForUpdates.RunWorkerAsync(true);
        }
        #region IMRUClient Members

        public void OpenMRUFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                if (Path.GetExtension(fileName).ToLower() == ".sbx")
                    LoadXmlControlFile(fileName);
                else
                    LoadProject(fileName);
            }
        }

        #endregion

        #region .: Add Objects Scripts Menu Handlers :.
        private void AddObjects(string objectTypeConst)
        {
            if (this.activeDatabase == string.Empty)
            {
                MessageBox.Show("Please select an Active Database first", "No Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                this.connData.DatabaseName = this.activeDatabase;
            }

            this.Cursor = Cursors.AppStarting;
            this.statGeneral.Text = "Retrieving list of selected objects from database...";
            this.progressBuild.Style = ProgressBarStyle.Marquee;
            this.bgGetObjectList.RunWorkerAsync(objectTypeConst);
        }
        private void bgGetObjectList_DoWork(object sender, DoWorkEventArgs e)
        {
            string objectTypeConst = e.Argument.ToString();
            List<SqlSync.DbInformation.ObjectData> objData = new List<SqlSync.DbInformation.ObjectData>();
            string desc = string.Empty;
            try
            {
                switch (objectTypeConst)
                {
                    case SqlSync.Constants.DbObjectType.StoredProcedure:
                        objData = SqlSync.DbInformation.InfoHelper.GetStoredProcedureList(this.connData);
                        desc = SqlSync.Constants.DbScriptDescription.StoredProcedure;
                        break;
                    case SqlSync.Constants.DbObjectType.UserDefinedFunction:
                        objData = SqlSync.DbInformation.InfoHelper.GetFunctionList(this.connData);
                        desc = SqlSync.Constants.DbScriptDescription.UserDefinedFunction;
                        break;
                    case SqlSync.Constants.DbObjectType.View:
                        objData = SqlSync.DbInformation.InfoHelper.GetViewList(this.connData);
                        desc = SqlSync.Constants.DbScriptDescription.View;
                        break;
                    case SqlSync.Constants.DbObjectType.Table:
                        objData = SqlSync.DbInformation.InfoHelper.GetTableObjectList(this.connData);
                        desc = SqlSync.Constants.DbScriptDescription.Table;
                        break;
                    case SqlSync.Constants.DbObjectType.Trigger:
                        objData = SqlSync.DbInformation.InfoHelper.GetTriggerObjectList(this.connData);
                        desc = SqlSync.Constants.DbScriptDescription.Trigger;
                        break;
                }
            }
            catch(Exception exe)
            {
                if (exe.Message.Trim().IndexOf("Timeout") > -1)
                {
                    e.Result = "Timeout error trying to connect to " + this.connData.DatabaseName + ". Please try again or contact your database administrator.";
                }
                else
                {
                    e.Result = "Error retrieving list:\r\n" + exe.Message;
                }
            }

            e.Result = new KeyValuePair<string[], List<SqlSync.DbInformation.ObjectData>>(new string[]{desc,objectTypeConst}, objData);
        }

        private void bgGetObjectList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            this.Cursor = Cursors.AppStarting;
            this.statGeneral.Text = "Ready.";
            this.progressBuild.Style = ProgressBarStyle.Blocks;

            if (e.Result is KeyValuePair<string[], List<SqlSync.DbInformation.ObjectData>>)
            {
                KeyValuePair<string[], List<ObjectData>> result = (KeyValuePair<string[], List<ObjectData>>)e.Result;
                List<ObjectData> objData = result.Value;
                string desc = result.Key[0];
                string objectTypeConst = result.Key[1];
                if (objData.Count > 0)
                {
                    Objects.AddObjectForm frmAdd = new SqlSync.SqlBuild.Objects.AddObjectForm(objData, desc, objectTypeConst, this.connData);
                    if (DialogResult.OK == frmAdd.ShowDialog())
                    {
                        this.Cursor = Cursors.WaitCursor;
                        List<DbInformation.ObjectData> objectsToScript = frmAdd.SelectedObjects;
                        if (objectsToScript.Count > 0)
                        {
                            this.Cursor = Cursors.AppStarting;
                            this.progressBuild.Style = ProgressBarStyle.Marquee;
                            bgObjectScripting.RunWorkerAsync(new KeyValuePair<string, List<DbInformation.ObjectData>>(objectTypeConst, objectsToScript));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No " + desc + "s found for the selected database. Does this database exist on this server?", "Nothing Found", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
            }
            else
            {
                MessageBox.Show(e.Result.ToString(),"Error Retrieving List", MessageBoxButtons.OK, MessageBoxIcon.Question);

            }
        }
        
        private void mnuAddStoredProcs_Click(object sender, System.EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.StoredProcedure);
        }
        private void mnuAddFunctions_Click(object sender, System.EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.UserDefinedFunction);
        }
        private void mnuAddViews_Click(object sender, System.EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.View);
        }
        private void mnuAddTables_Click(object sender, System.EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.Table);
        }
        private void mnuAddTriggers_Click(object sender, EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.Trigger);
        }
        private void mnuAddRoles_Click(object sender, EventArgs e)
        {
            this.AddObjects(SqlSync.Constants.DbObjectType.DatabaseRole);
        }
        
        private void bgObjectScripting_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            KeyValuePair<string, List<DbInformation.ObjectData>> data = (KeyValuePair<string, List<DbInformation.ObjectData>>)e.Argument;
            List<DbInformation.ObjectData> objectsToScript = data.Value;
            string objectTypeConst = data.Key;

            string script = string.Empty;
            string d = string.Empty;
            string path = Path.GetTempPath();
            string[] newFiles = new string[objectsToScript.Count];
            for (int i = 0; i < objectsToScript.Count; i++)
            {
                bg.ReportProgress(-1, "Scripting " + objectsToScript[i].SchemaOwner + @"." + objectsToScript[i].ObjectName);
                SqlSync.ObjectScript.ObjectScriptHelper scriptHelper = new ObjectScriptHelper(this.connData, SqlSync.Properties.Settings.Default.ScriptAsAlter, SqlSync.Properties.Settings.Default.ScriptPermissions,SqlSync.Properties.Settings.Default.ScriptPkWithTables);
                scriptHelper.ScriptDatabaseObjectWithHeader(objectTypeConst, objectsToScript[i].ObjectName, objectsToScript[i].SchemaOwner, ref script, ref d);
                if (script.Length > 0)
                {
                    newFiles[i] = path + objectsToScript[i].SchemaOwner + @"." + objectsToScript[i].ObjectName + objectTypeConst;
                    using (StreamWriter sw = File.CreateText(newFiles[i]))
                    {
                        sw.WriteLine(script.ToCharArray());
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                    newFiles[i] = "Error. Unable to script requested object";
            }

            e.Result = newFiles;
        }
        private void bgObjectScripting_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statGeneral.Text = e.UserState.ToString();
            statGeneral.BackColor = Color.LightGreen;
        }
        private void bgObjectScripting_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statGeneral.Text = "Scripting Complete. Ready.";
            statGeneral.BackColor = SystemColors.Control;
            this.progressBuild.Style = ProgressBarStyle.Blocks;
            this.Cursor = Cursors.Default;
            if (e.Result is string[])
                this.BulkAdd((string[])e.Result, true, this.activeDatabase);

        }
        #endregion

        //private void mnuActiveDatabase_Click(object sender, System.EventArgs e)
        //{
        //    this.mnuActiveDatabase.Text = "Active Database :: "+((ToolStripMenuItem)sender).Text;
        //    this.activeDatabase = ((ToolStripMenuItem)sender).Text;
        //}

        private void mnuAddCodeTablePop_Click(object sender, System.EventArgs e)
        {
            CodeTableScriptingForm frmLookUp = new CodeTableScriptingForm(this.connData, true);
            frmLookUp.SqlBuildManagerFileExport += new SqlBuildManagerFileExportHandler(frmLookUp_SqlBuildManagerFileExport);
            frmLookUp.Show();
        }

        private void frmLookUp_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
        {
            BulkAdd(e.FileNames, true);
        }

        private void mnuSchemaScripting_Click(object sender, System.EventArgs e)
        {
            SyncForm frmSchema = new SyncForm(this.connData);
            frmSchema.Show();
        }

        private void mnuCodeTableScripting_Click(object sender, System.EventArgs e)
        {
            CodeTableScriptingForm frmLookUp = new CodeTableScriptingForm(this.connData, false);
            frmLookUp.Show();
        }

        private void mnuDataExtraction_Click(object sender, System.EventArgs e)
        {
            DataDump.DataDumpForm frmDump = new SqlSync.DataDump.DataDumpForm(this.connData);
            frmDump.Show();
        }

        private void autoScriptListItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.autoScriptListRegistration.Items.Length; i++)
            {
                if (((ToolStripMenuItem)sender).Text == Path.GetFileName(this.autoScriptListRegistration.Items[i].File))
                {
                    if (File.Exists(this.autoScriptListRegistration.Items[i].File))
                    {
                        System.Diagnostics.Process prc = new System.Diagnostics.Process();
                        prc.StartInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                        prc.StartInfo.Arguments = " \"" + this.autoScriptListRegistration.Items[i].File + "\"";
                        prc.Start();
                    }
                }
            }
        }
        public void autoScriptAddRegistration_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileAutoScript.ShowDialog())
            {
                AutoScriptListRegistration reg = new AutoScriptListRegistration();
                reg.File = openFileAutoScript.FileName;
                ArrayList lst = new ArrayList();
                if (this.autoScriptListRegistration != null && this.autoScriptListRegistration.Items != null)
                    lst.AddRange(this.autoScriptListRegistration.Items);

                lst.Add(reg);

                if (this.autoScriptListRegistration == null)
                    this.autoScriptListRegistration = new AutoScriptList();

                this.autoScriptListRegistration.Items = new AutoScriptListRegistration[lst.Count];
                lst.CopyTo(this.autoScriptListRegistration.Items);

                System.Xml.XmlTextWriter tw = null;
                try
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(AutoScriptList));
                    tw = new System.Xml.XmlTextWriter(this.autoXmlFile, Encoding.UTF8);
                    xmlS.Serialize(tw, this.autoScriptListRegistration);
                }
                finally
                {
                    if (tw != null)
                        tw.Close();
                }
                GenerateAutoScriptList();

            }

        }

        private void mnuDatabaseSize_Click(object sender, System.EventArgs e)
        {
            SqlSync.Analysis.DatabaseSizeSummaryForm frmSize = new SqlSync.Analysis.DatabaseSizeSummaryForm(this.connData);
            frmSize.Show();
        }

        private void mnuDataAuditScripting_Click(object sender, System.EventArgs e)
        {
            DataAuditForm frmAudit = new DataAuditForm(this.connData);
            if(this.buildData != null)
                frmAudit.SqlBuildManagerFileExport += new SqlBuildManagerFileExportHandler(frmAudit_SqlBuildManagerFileExport);
            
            frmAudit.Show();
        }

        void frmAudit_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
        {
            BulkAdd(e.FileNames);
        }

        private void chkScriptChanges_Click(object sender, System.EventArgs e)
        {
            this.RefreshScriptFileList();
        }




        #region .: Code Table Populate Script Updates :.
        private void mnuUpdatePopulate_Click(object sender, System.EventArgs e)
        {
            CodeTable.ScriptUpdates[] updates = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(ref this.buildData, this.projectFileName);
            UpdatePopulateScripts(updates);
        }
        private void UpdatePopulateScripts(CodeTable.ScriptUpdates[] updates)
        {
            if (updates == null)
            {
                MessageBox.Show("There aren't any populate script files available", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            PopulateUpdates frmUpdates = new PopulateUpdates(updates);
            DialogResult result = frmUpdates.ShowDialog();
            if (result == DialogResult.OK)
            {
                CodeTable.ScriptUpdates[] selectedUpdates = frmUpdates.SelectedUpdates;
                for (int i = 0; i < selectedUpdates.Length; i++)
                {
                    statGeneral.Text = "Updating " + selectedUpdates[i].ShortFileName + " from " + selectedUpdates[i].SourceServer;
                    ConnectionData tmpData = new ConnectionData();
                    tmpData.DatabaseName = selectedUpdates[i].SourceDatabase;
                    tmpData.SQLServerName = selectedUpdates[i].SourceServer;
                    tmpData.Password = this.connData.Password;
                    tmpData.UserId = this.connData.UserId;
                    tmpData.UseWindowAuthentication = this.connData.UseWindowAuthentication;
                    PopulateHelper helper = new PopulateHelper(tmpData);
                    string updatedScript;
                    helper.GenerateUpdatedPopulateScript(selectedUpdates[i], out updatedScript);

                    if (updatedScript.Length > 0)
                    {
                        using (StreamWriter sw = File.CreateText(Path.GetDirectoryName(this.projectFileName) + @"\" + selectedUpdates[i].ShortFileName))
                        {
                            sw.Write(updatedScript);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error Updating " + selectedUpdates[i].ShortFileName + "\r\nThe script was NOT updated", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                statGeneral.Text = "Updating project file";
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

                statGeneral.Text = "Ready. Populate Scripts Updated";
            }
        }

        private void mnuUpdatePopulates_Click(object sender, System.EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            SqlBuild.CodeTable.ScriptUpdates[] popUpdates = new ScriptUpdates[lstScriptFiles.SelectedItems.Count];

            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                popUpdates[i] = SqlBuild.SqlBuildFileHelper.GetFileDataForCodeTableUpdates(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text, this.projectFileName);

            UpdatePopulateScripts(popUpdates);
        }

        #endregion

        #region .: DB Object Updates :.

        private void ConfigureObjectScripts(bool useCurrentSettings)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            Objects.ObjectUpdates[] objUpdates = new Objects.ObjectUpdates[lstScriptFiles.SelectedItems.Count];
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                objUpdates[i] = SqlBuild.SqlBuildFileHelper.GetFileDataForObjectUpdates(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text, this.projectFileName);
                if (useCurrentSettings && objUpdates[i] != null)
                {
                   // objUpdates[i].SourceDatabase = ConnectionHelper.GetTargetDatabase(objUpdates[i].SourceDatabase, OverrideData.TargetDatabaseOverrides);
                    objUpdates[i].SourceDatabase = ConnectionHelper.GetTargetDatabase(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.Database].Text, OverrideData.TargetDatabaseOverrides);
                    objUpdates[i].SourceServer = this.connData.SQLServerName;
                }

            }
            ObjectUpdateScripts(objUpdates);
        }
        private void mnuObjectScripts_FileDefault_Click(object sender, EventArgs e)
        {
            ConfigureObjectScripts(false);
        }

        private void mnuObjectScripts_CurrentSettings_Click(object sender, EventArgs e)
        {
            ConfigureObjectScripts(true);
        }
        private void mnuObjectUpdates_Click(object sender, System.EventArgs e)
        {
            Objects.ObjectUpdates[] updates = SqlBuildFileHelper.GetFileDataForObjectUpdates(ref this.buildData, this.projectFileName);
            if (updates == null)
            {
                MessageBox.Show("There aren't any object script files available", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ObjectUpdateScripts(updates);



        }
        private void ObjectUpdateScripts(Objects.ObjectUpdates[] objUpdates)
        {
            //List<KeyValuePair<string, string>> lstScripts = new List<KeyValuePair<string, string>>();
            PolicyHelper policyHelp = new PolicyHelper();
            List<string> lstScriptsNotUpdated = new List<string>();
            Objects.ObjectUpdatesForm frmUpdates = new Objects.ObjectUpdatesForm(objUpdates);
            if (DialogResult.OK == frmUpdates.ShowDialog())
            {
                this.Cursor = Cursors.WaitCursor;
                List<Objects.ObjectUpdates> selectedUpdates = frmUpdates.SelectedUpdates.ToList();
                List<Objects.UpdatedObject> lstScripts =  ObjectScriptHelper.ScriptDatabaseObjects(selectedUpdates, this.connData);
                

                if (lstScripts.Count > 0)
                {
                    Package lstViolations = policyHelp.ValidateScriptsAgainstPolicies(lstScripts);
                    if (lstViolations != null && lstViolations.Count > 0)
                    {
                        Policy.PolicyViolationForm frmVio = new SqlSync.SqlBuild.Policy.PolicyViolationForm(lstViolations, true);
                        if (DialogResult.No == frmVio.ShowDialog())
                        {
                            statGeneral.Text = "Ready. Scripting update cancelled. No files changed";
                            this.Cursor = Cursors.Default;
                            return;
                        }
                    }


                   

                    statGeneral.Text = "Saving updated scripts";
                    DateTime updateTime = DateTime.Now;
                    foreach (Objects.UpdatedObject obj in lstScripts)
                    {

                        if (this.projectIsUnderSourceControl)
                        {
                            SourceControlStatus stat =
                                SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, Path.GetDirectoryName(this.projectFileName) + @"\" + obj.ScriptName);
                            if (stat == SourceControlStatus.Error || stat == SourceControlStatus.Unknown)
                            {
                                MessageBox.Show(
                                    String.Format(
                                        "Error checking out {0} from source control. Please see application log for details",
                                        obj.ScriptName), "Source Control Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }



                        using (StreamWriter sw = File.CreateText(Path.GetDirectoryName(this.projectFileName) + @"\" + obj.ScriptName))
                        {
                            sw.Write(obj.ScriptContents);
                        }

                        //Update the buildData object with the update date/time and user;
                        foreach (SqlSyncBuildData.ScriptRow row in this.buildData.Script.Rows)
                        {
                            if (row.FileName == obj.ScriptName)
                            {
                                row.DateModified = updateTime;
                                row.ModifiedBy = System.Environment.UserName;
                            }
                        }
                    }
                    this.buildData.AcceptChanges();



                    statGeneral.Text = "Updating project file";
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);

                    this.RefreshScriptFileList(true);
                }

                //get the scripts that were not updated
                var notUpdated = from s in selectedUpdates
                        where !(from u in lstScripts select s.ShortFileName).Contains(s.ShortFileName)
                        select s.ShortFileName;

                if (notUpdated.Count() > 0)
                {
                    MessageBox.Show("Error Updating the following files:\r\n\r\n\t" + String.Join("\r\n\t", notUpdated.ToArray()) + "\r\n\r\n\t\t** These scripts were NOT updated **\r\n\r\nDo you need to set a Database Override Target?", "Warning - Some files not updated!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    statGeneral.Text = "Ready. Error scripting - not all scripts have been updated.";
                }
                else if (lstScripts.Count > 0)
                {
                    statGeneral.Text = "Ready. Object Scripts Updated";
                }
                else
                {
                    statGeneral.Text = "Ready. No scripts were updated.";
                }
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        private void mnuObjectValidation_Click(object sender, System.EventArgs e)
        {
            SqlSync.Validate.ObjectValidation frmObjValidation = new SqlSync.Validate.ObjectValidation(this.connData);
            frmObjValidation.Show();
        }

        private void mnuIndividualFiles_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == fdrSaveScripts.ShowDialog())
            {
                this.statGeneral.Text = "Saving Scripts to destination";
                bool saved = SqlSync.SqlBuild.SqlBuildFileHelper.CopyIndividualScriptsToFolder(ref this.buildData, fdrSaveScripts.SelectedPath, this.projectFilePath, this.mnuIncludeUSE.Checked, this.mnuIncludeSequence.Checked);
                if (saved)
                    this.statGeneral.Text = "Scripts successfully exported";
                else
                    this.statGeneral.Text = "Error: NOT successfully exported";

            }
            fdrSaveScripts.Dispose();
        }

        private void mnuCombinedFile_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == saveCombinedScript.ShowDialog())
            {
                this.statGeneral.Text = "Saving combined scripts";
                bool saved = SqlSync.SqlBuild.SqlBuildFileHelper.CopyScriptsToSingleFile(ref this.buildData, saveCombinedScript.FileName, this.projectFilePath, this.buildZipFileName, this.mnuIncludeUSE.Checked);
                if (saved)
                    this.statGeneral.Text = "Scripts successfully exported";
                else
                    this.statGeneral.Text = "Error: NOT successfully exported";
            }
            saveCombinedScript.Dispose();
        }

        private void lstScriptFiles_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            ListViewItem item = null;

            if (lstScriptFiles.SelectedItems.Count > 0)
                item = lstScriptFiles.SelectedItems[0];

            listSorter.CurrentColumn = e.Column;
            lstScriptFiles.ListViewItemSorter = listSorter;
            lstScriptFiles.Sort();
            if (item != null)
                item.EnsureVisible();
        }

        private void mnuViewRunHistory_Click(object sender, System.EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
            string baseFileWithPath = this.projectFilePath + row.FileName;
            if (!File.Exists(baseFileWithPath))
            {
                MessageBox.Show("Unable to find base file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileHash;
            string textHash;

            SqlBuild.SqlBuildFileHelper.GetSHA1Hash(baseFileWithPath, out fileHash, out textHash, row.StripTransactionText);
            string targetDatabase = ConnectionHelper.GetTargetDatabase(row.Database, OverrideData.TargetDatabaseOverrides);
            string currentFileContents = File.ReadAllText(baseFileWithPath);
            BuildHistory.ScriptRunHistoryForm frmHist = new BuildHistory.ScriptRunHistoryForm(this.connData, targetDatabase, new System.Guid(row.ScriptId), currentFileContents, textHash);
            frmHist.Show();
        }


        #region .: Build Process BackGroundWorker :.
        private void bgBuildProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            this.bgBuildProcess.ReportProgress(0, new BuildStartedEventArgs());
            if (e.Argument is SqlBuildRunData)
                RunBuild(sender as BackgroundWorker, e);
            else if (e.Argument is ClearScriptData)
                RunScriptClear(sender as BackgroundWorker, e);
            else if (e.Argument is MultiDbData)
                RunMultiDbBuild(sender as BackgroundWorker, e);

        }
        private void RunBuild(BackgroundWorker sender, DoWorkEventArgs e)
        {
            SqlBuildRunData runData = (SqlBuildRunData)e.Argument;
            SqlBuildHelper helper = new SqlBuildHelper(this.connData, this.createSqlRunLogFile, this.externalScriptLogFileName, runData.IsTransactional);
            helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
            helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
            helper.ProcessBuild(runData, 0, sender as BackgroundWorker, e);

        }

        /// <summary>
        /// Clears out the script block setting in the SqlBuild_Logging table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunScriptClear(BackgroundWorker sender, DoWorkEventArgs e)
        {
            SqlBuildHelper helper = new SqlBuildHelper(this.connData);
            helper.ClearScriptBlocks(e.Argument as ClearScriptData, sender as BackgroundWorker, e);
        }
        private void RunMultiDbBuild(BackgroundWorker sender, DoWorkEventArgs e)
        {
            MultiDbData multiRunData = (MultiDbData)e.Argument;
            
            //Create an ID for this build to tie all the entries together
            string id = Guid.NewGuid().ToString();
            id = id.Substring(0, 1) + id.Substring(7, 1) + id.Substring(10, 1) + id.Substring(26, 1) + id.Substring(30, 1);
            multiRunData.MultiRunId = id;
            SqlBuildHelper helper = new SqlBuildHelper(this.connData, this.createSqlRunLogFile, this.externalScriptLogFileName, multiRunData.IsTransactional);
            helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
            helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
            helper.ProcessMultiDbBuild(multiRunData, sender as BackgroundWorker, e);

        }
        private void bgBuildProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
                this.Cursor = Cursors.Default;

            if (e.UserState == null)
                return;

            if (e.UserState is BuildStartedEventArgs) // Update the UI now that the build has started.
            {
                this.lstBuild.Items.Clear();
                this.statBuildTime.Text = "Build Duration: ";
                this.statScriptTime.Text = "Script Duration (sec): ";
                this.lnkStartBuild.Enabled = false;
                this.txtBuildDesc.Text = string.Empty;
                this.txtBuildDesc.Enabled = false;
                this.ddBuildType.Enabled = false;
                this.progressBuild.Style = ProgressBarStyle.Marquee;
                this.btnCancel.Enabled = true;
                this.btnCancel.Visible = true;
                this.tmrBuild.Start();
            }
            else if (e.UserState is GeneralStatusEventArgs) //Update the general run status
            {
                if (this.runningUnattended)
                    Program.WriteLog(((GeneralStatusEventArgs)e.UserState).StatusMessage);

                statGeneral.Text = ((GeneralStatusEventArgs)e.UserState).StatusMessage;
            }
            else if (e.UserState is ScriptRunStatusEventArgs) //Update the status on a currently running script.
            {
                ScriptRunStatusEventArgs args = (ScriptRunStatusEventArgs)e.UserState;
                if (lstBuild.Items.Count > 0)
                {
                    ListViewItem top = lstBuild.Items[0];
                    top.SubItems[(int)BuildListIndex.Status].Text = args.Status;
                    if (args.Duration > TimeSpan.Zero)
                    {
                        string duration = args.Duration.ToString();
                        if (duration.StartsWith("00:"))
                            duration = duration.Substring(3);
                        if (duration.IndexOf('.') > -1 && duration.Substring(duration.IndexOf('.')).Length > 2)
                            duration = duration.Substring(0, duration.IndexOf('.') + 3);

                        top.SubItems[(int)BuildListIndex.Duration].Text = duration;
                    }
                    else
                    {
                        top.SubItems[(int)BuildListIndex.Duration].Text = "00:00.00";
                    }

                }


                if (args.Status.IndexOf("Error") == 0 ||
                    args.Status.IndexOf("Rolled Back") > -1)
                    lstBuild.Items[0].BackColor = Color.Red;
            }
            else if (e.UserState is BuildScriptEventArgs)
            {
                this.statScriptTime.Text = "Script Duration (sec): 0";
                this.scriptDuration = 0;
                this.tmrScript.Start();

                BuildScriptEventArgs args = e.UserState as BuildScriptEventArgs;
                ListViewItem newItem = new ListViewItem(new string[]{
												 args.BuildOrder.ToString(),
												 args.FileName,
												 args.Database,
												 args.OriginalBuildOrder.ToString(),
												 args.Status,
												 ""});
                newItem.Tag = args;
                lstBuild.Items.Insert(0, newItem);
            }
            else if (e.UserState is CommitFailureEventArgs)
            {
                if (!this.runningUnattended)
                    MessageBox.Show(((CommitFailureEventArgs)e.UserState).ErrorMessage, "Failed to Commit Build", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    Program.WriteLog("Failed to Commit Build " + ((CommitFailureEventArgs)e.UserState).ErrorMessage);
            }
            else if (e.UserState is ScriptRunProjectFileSavedEventArgs)
            {
                if (this.runningUnattended)
                    Program.WriteLog("ScriptRunProjectFileSavedEventArgs captured");

                //Reload the file
                if (this.runningUnattended)
                    Program.WriteLog("Reloading updated build XML");
                LoadSqlBuildProjectFileData(ref this.buildData, this.projectFileName, true);

                if (this.runningUnattended)
                    Program.WriteLog("Saving updated build file to disk");

                try
                {
                    SqlBuildFileHelper.PackageProjectFileIntoZip(this.buildData, this.projectFilePath, this.buildZipFileName);
                    Program.WriteLog("Build file saved to disk");

                }
                catch (Exception exe)
                {
                    if (this.runningUnattended)
                        Program.WriteLog("ERROR!" + exe.ToString());
                    else
                        MessageBox.Show(e.ToString(), "Error Saving Build File", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.returnCode = 789;
                }

                if (this.runningUnattended)
                {
                    Program.WriteLog("Completed with return code " + this.returnCode.ToString());
                    Program.WriteLog("************************************************");
                }

                if (this.runningUnattended && this.returnCode != -1 && this.UnattendedProcessingCompleteEvent != null)
                    this.UnattendedProcessingCompleteEvent(this.returnCode);

                // this.RefreshScriptFileList();
            }
            else if (e.UserState is Exception)
            {
                if (!this.runningUnattended)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Run Error:\r\n" + ((Exception)e.UserState).Message);
                }
                else
                {
                    Program.WriteLog("ERROR!" + ((Exception)e.UserState).Message);
                }
            }



        }
        private void bgBuildProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.tmrScript.Stop();

            if (!this.runningUnattended)
            {
                if (e.Cancelled)
                {
                    statGeneral.Text = "Build Cancelled and Rolled Back.";
                }
                else if (e.Result != null)
                {
                    BuildResultStatus stat = (BuildResultStatus)e.Result;
                    switch (stat)
                    {
                        case BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK:
                            statGeneral.Text = "Build Failed and Rolled Back";
                            break;
                        case BuildResultStatus.BUILD_COMMITTED:
                            this.RefreshScriptFileList(true);
                            statGeneral.Text = "Build Committed";
                            break;
                        case BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL:
                            statGeneral.Text = "Build Successful. Rolled Back for Trial Build";
                            break;
                        case BuildResultStatus.SCRIPT_GENERATION_COMPLETE:
                            statGeneral.Text = "Script Generation Complete";
                            break;
                        case BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK:
                            statGeneral.Text = "Build Cancelled and Rolled Back.";
                            break;
                        case BuildResultStatus.BUILD_FAILED_NO_TRANSACTION:
                            statGeneral.Text = "Build Failed. No Transaction set.";
                            break;
                        case BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION:
                            statGeneral.Text = "Build Cancelled. No Transaction set.";
                            break;
                        default:
                            statGeneral.Text = "Unknown Status.";
                            break;

                    }

                }

                this.txtBuildDesc.Enabled = true;
                this.ddBuildType.Enabled = true;
                this.progressBuild.Style = ProgressBarStyle.Blocks;
                this.btnCancel.Visible = false;
                this.btnCancel.Enabled = true;

                //Stop and reset the timer.
                this.tmrBuild.Stop();
                this.buildDuration = 0;
            }
        }

        #region ## Build Helper Event Handlers ##

        private void helper_BuildCommittedEvent(object sender, RunnerReturn rr)
        {
            this.returnCode = 0;
        }

        private void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            this.returnCode = 999;
        }
        #endregion

        private void mnuDDActiveDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mnuDDActiveDatabase.SelectedItem.ToString() != selectDatabaseString)
            {
                this.activeDatabase = mnuDDActiveDatabase.SelectedItem.ToString();
                mnuAddObjectCreate.Enabled = true;
            }
            else
            {
                mnuAddObjectCreate.Enabled = false;
            }

        }
        #endregion

        private void ctxScriptFile_Opening(object sender, CancelEventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            //Loop through to see if there are all populate scripts or all object scripts. If so, enable the links
            bool enablePop = true;
            bool enableObject = true;
            bool found;
            int fileNameIndex = (int)ScriptListIndex.FileName;
            string[] objectTypes = SqlSync.Constants.DbObjectType.GetObjectTypes();
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                if (!lstScriptFiles.SelectedItems[i].SubItems[fileNameIndex].Text.ToUpper().EndsWith(SqlSync.Constants.DbObjectType.PopulateScript))
                    enablePop = false;


                found = false;
                for (int j = 0; j < objectTypes.Length; j++)
                {
                    if ((lstScriptFiles.SelectedItems[i].SubItems[fileNameIndex].Text.ToUpper().EndsWith(objectTypes[j])))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) enableObject = false;
            }


            mnuUpdatePopulates.Enabled = enablePop;
            mnuUpdateObjectScripts.Enabled = enableObject;

            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Enabled = enableObject && (lstScriptFiles.SelectedItems.Count == 1);
            mnuCompareObject.Enabled = enableObject && (lstScriptFiles.SelectedItems.Count == 1);

            if (lstScriptFiles.SelectedItems.Count == 1 && lstScriptFiles.SelectedItems[0].ImageIndex != 0) //see if it's been run before
                mnuViewRunHistory.Enabled = true;
            else
                mnuViewRunHistory.Enabled = false;

            if (lstScriptFiles.SelectedItems.Count == 1)
            {
                mnuEditFile.Enabled = true;
                renameScriptFIleToolStripMenuItem.Enabled = true;
                mnuEditScriptFile.Enabled = false;
            }
            else
            {
                mnuEditFile.Enabled = false;
                renameScriptFIleToolStripMenuItem.Enabled = false;
                mnuEditScriptFile.Enabled = true;
            }

            //Check for read-only to decide if we are editing or view only...
            bool haveReadOnly = false;
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                if (lstScriptFiles.SelectedItems[i].BackColor == colorReadOnlyFile)
                {
                    haveReadOnly = true;
                    break;
                }
            }
            if (haveReadOnly && !this.projectIsUnderSourceControl)
            {
                renameScriptFIleToolStripMenuItem.Enabled = false;
                mnuUpdatePopulates.Enabled = false;
                mnuUpdateObjectScripts.Enabled = false;
                makeFileWriteableremoveToolStripMenuItem.Enabled = true;
                mnuEditFile.Image = SqlSync.Properties.Resources.Script_Edit;
                mnuEditFile.Text = "View File";
            }
            else if (this.projectIsUnderSourceControl)
            {
                mnuEditFile.Text = "Check Out for Edit";
                mnuEditFile.Image = SqlSync.Properties.Resources.checkout;
            }
            else
            {

                makeFileWriteableremoveToolStripMenuItem.Enabled = false;
                mnuEditFile.Image = SqlSync.Properties.Resources.Script_Edit;
                mnuEditFile.Text = "Edit File";
            }

            //Check to see if this is SBX or SBM and change "Remove vs. Delete"
            string plural = (lstScriptFiles.SelectedItems.Count > 1) ? "s" : "";
            if (this.sbxBuildControlFileName != null && this.sbxBuildControlFileName.Length > 0)
            {
                mnuRemoveScriptFile.Text = String.Format("Delete file{0} from project and directory", plural);
            }
            else
            {
                mnuRemoveScriptFile.Text = String.Format("Remove file{0} from project", plural) ;
            }

            //Check to see if we are marking files as read or checking them out
            if (this.projectIsUnderSourceControl)
            {
                makeFileWriteableremoveToolStripMenuItem.Enabled = false;
            }
            //else
            //{
            //    makeFileWriteableremoveToolStripMenuItem.Text = "Make file writeable (remove Read Only attribute)";
            //    makeFileWriteableremoveToolStripMenuItem.Image = SqlSync.Properties.Resources.Key;
            //    makeFileWriteableremoveToolStripMenuItem.Font = new System.Drawing.Font(makeFileWriteableremoveToolStripMenuItem.Font, FontStyle.Regular);
            //}
        }

        private void settingsControl1_ServerChanged(object sender, string serverName)
        {
            this.Cursor = Cursors.WaitCursor;
            string oldServer = this.connData.SQLServerName;
            this.connData.SQLServerName = this.settingsControl1.Server;
            this.connData.ScriptTimeout = 5;
            try
            {
                this.databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
                this.targetDatabaseOverrideCtrl1.SetDatabaseData(this.databaseList, this.databasesUsed);
                InfoHelper.UpdateRoutineAndViewChangeDates(this.connData, this.targetDatabaseOverrideCtrl1.GetOverrideData());
                SetDatabaseMenuList();
                this.RefreshScriptFileList();
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.connData.SQLServerName = oldServer;
                this.settingsControl1.Server = oldServer;
            }


            this.Cursor = Cursors.Default;

        }

        private void rebuildPreviouslyCommitedBuildFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RebuildForm frmRebuild = new RebuildForm(this.connData, this.databaseList);
            frmRebuild.Show();
        }

        private void SqlBuildForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (!SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(this.workingDirectory))
            {
                DialogResult result = MessageBox.Show("Unable to clean-up working directory. Do you want to navigate to it to remove manually?", "Unable to clean-up", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process prc = new Process();
                    prc.StartInfo.FileName = this.projectFilePath;
                    prc.Start();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

        }

        private void mnuActionMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.buildData != null)
            {
                this.mnuCompare.Enabled = true;
                this.mnuExportBuildList.Enabled = true;
                this.mnuExportScriptText.Enabled = true;
                this.mnuImportScriptFromFile.Enabled = true;
                this.mnuMainAddNewFile.Enabled = true;
                this.mnuMainAddSqlScript.Enabled = true;
                this.startConfigureMultiServerDatabaseRunToolStripMenuItem.Enabled = true;
            }

            if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout > 0)
                this.mnuDefaultScriptTimeout.Text = SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout.ToString();
            else
                this.mnuDefaultScriptTimeout.Text = "90";

            
            this.scriptTagsRequiredToolStripMenuItem.Checked = SqlSync.Properties.Settings.Default.RequireScriptTags;

            this.sourceControlServerURLTextboxMenuItem.Text = SqlSync.Properties.Settings.Default.SourceControlServerUrl;

            


        }

        private void mnuCompare_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openSbmFileDialog.ShowDialog())
            {
                string rightFile = openSbmFileDialog.FileName;
                openSbmFileDialog.Dispose();

                double lastBuildNumber;
                string lastDb;
                GetLastBuildNumberAndDb(out lastBuildNumber, out lastDb);

                SqlSync.Analysis.ComparisonForm frmCompare = new SqlSync.Analysis.ComparisonForm(ref this.buildData, lastBuildNumber, this.buildZipFileName, this.projectFilePath, rightFile, true);
                frmCompare.ShowDialog();

                if (frmCompare.RefreshProjectList)
                    this.RefreshScriptFileList();
            }
        }
        private void showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSyncBuildData.ScriptRow row = null;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                //We can only script for compare one object at a time
                if (lstScriptFiles.SelectedItems.Count != 1)
                    return;

                //Make sure we can find the base file. 
                row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                string baseFileWithPath = this.projectFilePath + row.FileName;
                if (!File.Exists(baseFileWithPath))
                {
                    MessageBox.Show("Unable to find base file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                string script = File.ReadAllText(baseFileWithPath);
                this.connData.DatabaseName = ConnectionHelper.GetTargetDatabase(row.Database, OverrideData.TargetDatabaseOverrides);
                ScriptRunHistoryForm frmHist = new ScriptRunHistoryForm(connData, this.connData.DatabaseName, row.FileName, script, "");
                frmHist.ShowDialog();
                frmHist.Dispose();
            }
            catch (Exception exe)
            {
                string fileName = (row == null ? "unknown" : row.FileName);
                log.Error(String.Format("Error trying to get object history for {0}", fileName), exe);
                MessageBox.Show(String.Format("Unable to get object history.\r\n{0}", exe.Message), "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void mnuCompareObject_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                //We can only script for compare one object at a time
                if (lstScriptFiles.SelectedItems.Count != 1)
                    return;

                //Make sure we can find the base file. 
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                string baseFileWithPath = this.projectFilePath + row.FileName;
                if (!File.Exists(baseFileWithPath))
                {
                    MessageBox.Show("Unable to find base file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string dbTypeConst = string.Empty;
                string schemaOwner;
                string objectName = Path.GetFileNameWithoutExtension(row.FileName);
                InfoHelper.ExtractNameAndSchema(objectName, out objectName, out schemaOwner);

                //find the correct type constant based on the file extension
                string[] types = SqlSync.Constants.DbObjectType.GetComparableObjectTypes();
                for (int i = 0; i < types.Length; i++)
                    if (baseFileWithPath.EndsWith(types[i]))
                        dbTypeConst = types[i];

                //Make sure it's a scriptable type
                if (dbTypeConst == string.Empty)
                {
                    MessageBox.Show("Selected object is not scriptable for comparison!", "Bad File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //Generate the script and save the temp file
                string script = string.Empty;
                string desc = string.Empty;
                this.connData.DatabaseName = ConnectionHelper.GetTargetDatabase(row.Database, OverrideData.TargetDatabaseOverrides);
                SqlBuild.Objects.ObjectUpdates objSetting = SqlBuild.SqlBuildFileHelper.GetFileDataForObjectUpdates(lstScriptFiles.SelectedItems[0].SubItems[(int)ScriptListIndex.FileName].Text, this.projectFileName);

                SqlSync.ObjectScript.ObjectScriptHelper scriptHelper = new ObjectScriptHelper(this.connData, objSetting.ScriptAsAlter, objSetting.IncludePermissions, objSetting.ScriptPkWithTable);
                bool success = scriptHelper.ScriptDatabaseObjectWithHeader(dbTypeConst, objectName,schemaOwner, ref script, ref desc);
                if (!success)
                {
                    MessageBox.Show("Unable to Script object from database.\r\nDoes a Target Database Override need to be set?", "Error Scripting from Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string newFile = Path.GetTempPath() + schemaOwner+ "." + objectName + dbTypeConst;
                File.WriteAllText(newFile, script);


                FileCompareResults comp = new FileCompareResults();
                comp.LeftScriptRow = row;
                comp.LeftScriptPath = baseFileWithPath;
                comp.RightScriptPath = newFile;
                comp.RightSciptText = script;

                comp = SqlSync.Compare.SqlUnifiedDiff.ProcessUnifiedDiff(comp);

                SqlSync.Analysis.SimpleDiffForm frmDiff = new SqlSync.Analysis.SimpleDiffForm(comp, objectName, this.connData.DatabaseName, this.connData.SQLServerName);
                frmDiff.ShowDialog();

                //If the user changed the file, update the zip.
                if (frmDiff.FileChanged)
                {
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, this.projectFileName, buildZipFileName);
                    RefreshScriptFileList(true);
                }

                try
                { File.Delete(newFile); }
                catch { }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Unable to Script object from database.\r\nDoes a Target Database Override need to be set?", "Error Scripting from Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception exe)
            {
                //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Unable to Script object from database.\r\n" + exe.ToString(), System.Diagnostics.EventLogEntryType.Error, 522);
                MessageBox.Show("Unable to Script object from database.\r\nCheck Event Log for details", "Error Scripting from Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.connData.DatabaseName = string.Empty;
            }




        }

        private void archiveBuildHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(this.projectFilePath + XmlFileNames.HistoryFile))
            {
                MessageBox.Show("No Build Run History is Available", "No History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to archive the XML build History?", "Confirm Archiving", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                try
                {
                    string buildHistFileName = string.Empty;
                    string newZipName = Path.GetDirectoryName(this.buildZipFileName) + @"\" + Path.GetFileNameWithoutExtension(this.buildZipFileName) + " - Build Archive.zip";
                    if (File.Exists(this.projectFilePath + XmlFileNames.HistoryFile))
                    {
                        buildHistFileName = this.projectFilePath +
                            DateTime.Now.Year.ToString() + "-" +
                            DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" +
                            DateTime.Now.Day.ToString().PadLeft(2, '0') + " at " +
                            DateTime.Now.Hour.ToString().PadLeft(2, '0') +
                            DateTime.Now.Minute.ToString().PadLeft(2, '0') + " " +
                            XmlFileNames.HistoryFile;
                        File.Move(this.projectFilePath + XmlFileNames.HistoryFile, buildHistFileName);

                        if (ZipHelper.AppendZipPackage(new string[] { Path.GetFileName(buildHistFileName) }, this.projectFilePath, newZipName, false))
                        {
                            SqlBuildFileHelper.PackageProjectFileIntoZip(this.buildData, this.projectFilePath, this.buildZipFileName);
                            MessageBox.Show("Build History has been successfully archived to:\r\n" + newZipName, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            try
                            {
                                File.Move(buildHistFileName, this.projectFilePath + XmlFileNames.HistoryFile);
                            }
                            catch { }

                            MessageBox.Show("There was an error archiving the build history", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }


                    }
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Unable to archive the build history.\r\n" + exe.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void renameScriptFIleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                //We can only script for compare one object at a time
                if (lstScriptFiles.SelectedItems.Count != 1)
                    return;

                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                RenameScriptForm frmRename = new RenameScriptForm(ref row);
                if (DialogResult.OK == frmRename.ShowDialog())
                {
                    if (!File.Exists(this.projectFilePath + frmRename.OldName))
                    {
                        MessageBox.Show("Unable to find old file!", "Whoops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (File.Exists(this.projectFilePath + frmRename.NewName))
                    {
                        MessageBox.Show("A file be the name of " + frmRename.NewName + " was already found!\r\nRename can not proceed.", "Sorry, can't overwrite", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Rename the File
                    File.Move(this.projectFilePath + frmRename.OldName, this.projectFilePath + frmRename.NewName);

                    //Rename in the config
                    ((SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag).FileName = frmRename.NewName;
                    this.buildData.AcceptChanges();
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, this.projectFileName, buildZipFileName);
                    RefreshScriptFileList();


                }

            }
            catch (Exception exe)
            {
                MessageBox.Show("Unable to rename the file\r\n" + exe.ToString(), "Sorry, it didn't work.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void PopulateTagList()
        {
            if (this.buildData == null)
                return;

            if (this.buildData.Script == null || this.buildData.Script.Count == 0)
                return;

            this.tagList.Clear();

            var lst = (from r in buildData.Script
                       where r.Tag.Trim().Length > 0
                       select r.Tag.Trim()).Distinct();

            if (lst.Count() > 0)
                this.tagList = lst.ToList();

            this.tagList.Sort();

        }

        private void scriptTagsRequiredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.buildData == null)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                this.statGeneral.Text = "Updating Build File with new setting";
                if (this.buildData.SqlSyncBuildProject.Rows.Count == 0)
                    this.buildData.SqlSyncBuildProject.AddSqlSyncBuildProjectRow("", scriptTagsRequiredToolStripMenuItem.Checked);
                else
                    this.buildData.SqlSyncBuildProject[0].ScriptTagRequired = scriptTagsRequiredToolStripMenuItem.Checked;

                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, this.projectFileName, this.buildZipFileName);
            }
            finally
            {
                this.statGeneral.Text = "Build File Updated. Ready.";
                this.Cursor = Cursors.Default;
            }
        }
        private void createSQLLogOfBuildRunsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.createSqlRunLogFile = createSQLLogOfBuildRunsToolStripMenuItem.Checked;
        }
        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            bgCheckForUpdates.RunWorkerAsync(true);
        }

        private void bgCheckForUpdates_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.runningUnattended)
                return;

            char[] delims = new char[] { ',', ';', '|' };
            VersionData verData = new VersionData();
            try
            {
                verData.ManualCheck = (bool)e.Argument;
                //Get the path to the update text file
                System.Configuration.AppSettingsReader appReader = new System.Configuration.AppSettingsReader();
                string[] fileURL = ((string)appReader.GetValue("ProgramVersionCheckURL", typeof(string))).Split(delims);
                verData.UpdateFolder = (string)appReader.GetValue("ProgramUpdateFolderURL", typeof(string));
                verData.Contact = (string)appReader.GetValue("ProgramUpdateContact", typeof(string));
                verData.ContactEMail = (string)appReader.GetValue("ProgramUpdateContactEMail", typeof(string));
                verData.YourVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
                SqlSyncConfig config = new SqlSyncConfig();
                if (File.Exists(homePath + SqlSync.Utility.ConfigFileName))
                    config.ReadXml(homePath + SqlSync.Utility.ConfigFileName);

                if (config.LastProgramUpdateCheck != null)
                {
                    if (verData.ManualCheck || config.LastProgramUpdateCheck.Count == 0 || config.LastProgramUpdateCheck[0].IsCheckTimeNull() ||
                        (config.LastProgramUpdateCheck[0].CheckTime < DateTime.Now.AddHours(-2)))
                    {
                        verData.CheckIntervalElapsed = true;
                        bool contacted = false;
                        string errorMessage = string.Empty;
                       
                            for (int i = 0; i < fileURL.Length; i++)
                            {
                                try
                                {
                                    System.Net.WebRequest req = System.Net.WebRequest.Create(fileURL[i]);
                                    req.Proxy = System.Net.WebRequest.DefaultWebProxy;
                                    req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                                    System.Net.WebResponse resp = req.GetResponse();
                                    System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

                                    string versionFile = sr.ReadToEnd();
                                    sr.Close();

                                    string[] versions = versionFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    verData.LatestVersion = new Version(versions[0]);
                                    verData.LastCompatableVersion = new Version(versions[1]);
                                    if (versions.Length > 3)
                                    {
                                        //This is the "classic" versions file with the change notes in-line
                                        verData.ReleaseNotes = String.Join("\r\n", versions, 2, versions.Length - 2);
                                    }
                                    else if(versions.Length == 3)
                                    {
                                        //This is the new versions file that has a file pointer to the change notes file...
                                        string changeNotesFilePath = versions[2];
                                        req = System.Net.WebRequest.Create(changeNotesFilePath);
                                        req.Proxy = System.Net.WebRequest.DefaultWebProxy;
                                        req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                                        resp = req.GetResponse();
                                        StreamReader srChangeNotes = new StreamReader(resp.GetResponseStream());
                                        string changeNotesFileContents = srChangeNotes.ReadToEnd();
                                        srChangeNotes.Close();

                                        //The file contents should be XML, so lets transform them 
                                        string xsltFilePath = homePath + SqlSync.VersionData.XsltTransformFileName;

                                        if (File.Exists(xsltFilePath))
                                        {
                                            XmlTextReader xsltReader;

                                            XslCompiledTransform trans = new XslCompiledTransform();
                                            StringReader srContents = new StringReader(changeNotesFileContents);
                                            StringReader xsltText;
                                            XmlTextReader xmlReader = new XmlTextReader(srContents);
                                            XPathDocument xPathDoc = new XPathDocument(xmlReader);


                                            StringBuilder sbHtml = new StringBuilder();
                                            StringWriter swHtml = new StringWriter(sbHtml);
                                            xsltText = new StringReader(File.ReadAllText(xsltFilePath));
                                            xsltReader = new XmlTextReader(xsltText);
                                            trans.Load(xsltReader);
                                            trans.Transform(xPathDoc, null, swHtml);
                                            verData.ReleaseNotes = sbHtml.ToString();
                                            verData.ReleaseNotesAreHtml = true;
                                        }
                                        else
                                        {
                                            verData.ReleaseNotes = changeNotesFileContents;
                                        }
                                    }


                                    if (verData.UpdateFolder.Split(delims).Length == fileURL.Length)
                                    {
                                        verData.UpdateFolder = verData.UpdateFolder.Split(delims)[i];
                                    }
                                    else
                                    {
                                        verData.UpdateFolder = verData.UpdateFolder.Split(delims)[0];
                                    }
                                    contacted = true;
                                    break;
                                }
                                catch(Exception exe)
                                {
                                    errorMessage = exe.ToString();
                                }
                            }
                        
                        if(!contacted)
                        {
                            verData.UpdateFileReadError = true;
                            //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Unable to read update file.\r\n" + errorMessage, EventLogEntryType.Error, 901);
                        }

                        if (config.LastProgramUpdateCheck.Count == 0)
                            config.LastProgramUpdateCheck.AddLastProgramUpdateCheckRow(DateTime.Now);
                        else
                            config.LastProgramUpdateCheck[0].CheckTime = DateTime.Now;

                        config.AcceptChanges();
                        config.WriteXml(homePath + SqlSync.Utility.ConfigFileName);
                    }
                    else
                    {
                        verData.CheckIntervalElapsed = false;
                    }
                }

                e.Result = verData;
            }
            catch (Exception exe)
            {
                verData.UpdateFileReadError = true;
                verData.CheckIntervalElapsed = true;
                //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Error Checking for updates.\r\n" + exe.ToString(), EventLogEntryType.Error, 901);

            }
            finally
            {
            }
        }

        private void bgCheckForUpdates_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            if (e.Result != null)
            {
                try
                {
                    VersionData verData = (VersionData)e.Result;
                    if (verData.CheckIntervalElapsed)
                    {
                        if (verData.LatestVersion == null || verData.LatestVersion > verData.YourVersion || verData.ManualCheck)
                        {
                            bool isBreaking = false;
                            if (verData.LastCompatableVersion != null)
                                isBreaking = (verData.YourVersion < verData.LastCompatableVersion);
                            NewVersion frmVersion = new NewVersion(verData, isBreaking, this.impersonatedUser);
                            frmVersion.ShowDialog();
                            this.impersonatedUser = frmVersion.impersonatedUser;
                        }
                    }
                }
                catch (Exception exe)
                {
                    //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Unable to display New Version alert window.\r\n" + exe.ToString(), EventLogEntryType.Error, 901);

                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            bgBuildProcess.CancelAsync();
            statGeneral.Text = "Attempting Cancellation";
        }

        private void tmrBuild_Tick(object sender, EventArgs e)
        {
            buildDuration++;
            TimeSpan t = new TimeSpan(0, 0, buildDuration);
            statBuildTime.Text = "Build Duration: " + t.ToString();
        }

        private void tmrScript_Tick(object sender, EventArgs e)
        {
            scriptDuration++;
            statScriptTime.Text = "Script Duration (sec): " + scriptDuration.ToString();
        }

        private void storedProcedureTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Test.SprocTestConfigForm frmTest = new SqlSync.Test.SprocTestConfigForm(this.connData);
            frmTest.Show();
        }

        #region .: Tracking :.
        //private void bgUsageTrack_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
        //    SqlSync.SqlSyncUsageMeter.UsageRecord rec = null;
        //    try
        //    {
        //        List<SqlSync.SqlSyncUsageMeter.UsageRecord> usage = this.GetUsageRecords(homePath);
        //        SqlSync.SqlSyncUsageMeter.UsageMeter meterWs = new SqlSync.SqlSyncUsageMeter.UsageMeter();
        //        rec = new SqlSync.SqlSyncUsageMeter.UsageRecord();
        //        usage.Add(rec);
        //        rec.BuildFileName = this.buildZipFileName;
        //        rec.FileOpenUTC = DateTime.Now.ToUniversalTime();
        //        rec.HostMachineName = System.Environment.MachineName;
        //        rec.SqlSyncVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //        rec.User = System.Environment.UserName;
        //        meterWs.RecordUsageRecord(usage.ToArray());
        //        this.DeleteRecordedUsageFiles(homePath);
        //    }
        //    catch (Exception exe)
        //    {
        //        string fileName = homePath + System.Guid.NewGuid().ToString() + ".trk";
        //        System.Xml.XmlTextWriter tw = null;
        //        try
        //        {
        //            XmlSerializer xmlS = new XmlSerializer(typeof(SqlSync.SqlSyncUsageMeter.UsageRecord));
        //            tw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8);
        //            tw.Formatting = System.Xml.Formatting.Indented;
        //            tw.Indentation = 3;
        //            xmlS.Serialize(tw, rec);
        //            if (File.Exists(fileName))
        //                File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Hidden);
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //        finally
        //        {
        //            if (tw != null)
        //                tw.Close();
        //        }
        //    }
        //}
        //private List<SqlSync.SqlSyncUsageMeter.UsageRecord> GetUsageRecords(string homePath)
        //{
        //    List<SqlSync.SqlSyncUsageMeter.UsageRecord> usage = new List<SqlSync.SqlSyncUsageMeter.UsageRecord>();
        //    if (Directory.Exists(homePath))
        //    {
        //        DirectoryInfo inf = new DirectoryInfo(homePath);
        //        FileInfo[] files = inf.GetFiles("*.trk");
        //        if (files.Length > 0)
        //        {
        //            for (int i = 0; i < files.Length; i++)
        //            {
        //                using (StreamReader sr = new StreamReader(files[i].Name))
        //                {
        //                    try
        //                    {
        //                        SqlSync.SqlSyncUsageMeter.UsageRecord cfg = null;
        //                        XmlSerializer serializer = new XmlSerializer(typeof(SqlSync.SqlSyncUsageMeter.UsageRecord));
        //                        object obj = serializer.Deserialize(sr);
        //                        cfg = (SqlSync.SqlSyncUsageMeter.UsageRecord)obj;
        //                        if (cfg != null) usage.Add(cfg);
        //                    }
        //                    catch (Exception exe)
        //                    { }

        //                }
        //            }
        //        }
        //    }
        //    return usage;
        //}
        //private void DeleteRecordedUsageFiles(string homePath)
        //{
        //    if (Directory.Exists(homePath))
        //    {
        //        DirectoryInfo inf = new DirectoryInfo(homePath);
        //        FileInfo[] files = inf.GetFiles("*.trk");
        //        for (int i = 0; i < files.Length; i++)
        //        {
        //            try
        //            {
        //                files[i].Delete();
        //            }
        //            catch { }
        //        }
        //    }

        //}
        #endregion

        private void maintainManualDatabaseEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManualDatabaseForm frmManual = new ManualDatabaseForm();
            if (DialogResult.OK == frmManual.ShowDialog())
            {
                this.databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
            }
            frmManual.Dispose();
        }

        private void targetDatabaseOverrideCtrl1_TargetChanged(object sender, TargetChangedEventArgs e)
        {
            OverrideData.TargetDatabaseOverrides = targetDatabaseOverrideCtrl1.GetOverrideData();
            if (chkUpdateOnOverride.Checked)
            {
                InfoHelper.UpdateRoutineAndViewChangeDates(this.connData, targetDatabaseOverrideCtrl1.GetOverrideData());

                bgRefreshScriptList.RunWorkerAsync(new ListRefreshSettings() { SelectedItemIndex = -1, RunPolicyChecks = false });
                //DataRow[] rows = this.buildData.Script.Select(this.buildData.Script.DatabaseColumn.ColumnName + "='" + e.DefaultDatabase + "'");
                //for (int i = 0; i < rows.Length; i++)
                //{
                //    RefreshScriptFileList_SingleItem((SqlSyncBuildData.ScriptRow)rows[i]);
                //}
            }
        }

        private void chkUpdateOnOverride_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUpdateOnOverride.Checked)
            {
                this.RefreshScriptFileList();
            }
        }

        private void SetUsedDatabases()
        {
            this.databasesUsed.Clear();
            if (this.buildData != null && this.buildData.Script != null)
            {
                foreach (SqlSyncBuildData.ScriptRow row in this.buildData.Script)
                {
                    bool found = false;
                    //Can't use "contains" because it is case sensitive
                    for (int i = 0; i < this.databasesUsed.Count; i++)
                    {
                        if (this.databasesUsed[i].ToLower() == row.Database.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        this.databasesUsed.Add(row.Database);
                }
            }
            this.targetDatabaseOverrideCtrl1.SetDatabaseData(this.databaseList, this.databasesUsed);
        }

        private int pnlBuildScriptsStdWidth;
        private void btnSlideBuildScripts_Click(object sender, EventArgs e)
        {
            if (btnSlideBuildScripts.ImageIndex == 0) // currently collapsed.
            {
                pnlBuildScriptsStdWidth = pnlBuildScripts.Width;
                pnlBuildScripts.Width = 740;
                colDateAdded.Width = 130;
                colDateModified.Width = 130;
                btnSlideBuildScripts.ImageIndex = 1;
            }
            else
            {
                pnlBuildScripts.Width = pnlBuildScriptsStdWidth;
                colDateAdded.Width = 0;
                colDateModified.Width = 0;
                btnSlideBuildScripts.ImageIndex = 0;
            }
        }

        private void maintainDefaultScriptRegistryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.Default_Scripts.DefaultScriptMaintenanceForm frm = new SqlSync.SqlBuild.Default_Scripts.DefaultScriptMaintenanceForm(this.databaseList);
            frm.ShowDialog();
        }

        private void startConfigureMultiServerDatabaseRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MultiDbRunForm frm = new MultiDbRunForm(this.connData, this.databasesUsed, this.databaseList,this.buildZipFileName,this.projectFilePath,ref this.buildData);
            if(DialogResult.OK == frm.ShowDialog())
            {
                MultiDbData runData = frm.RunConfiguration;
                runData.BuildData = this.buildData;
                runData.BuildFileName = this.buildZipFileName;
                runData.ProjectFileName = this.projectFileName;

                List<string> desc = (from s in this.buildData.Script
                         join d in this.scriptsRequiringBuildDescription on s.ScriptId equals d
                         select d).ToList();
                
                if (desc.Count > 0)
                {
                    BuildDescriptionForm frmDesc = new BuildDescriptionForm();
                    if (DialogResult.OK == frmDesc.ShowDialog())
                    {
                        runData.BuildDescription = frmDesc.BuildDescription;
                    }
                    else
                    {
                        frmDesc.Dispose();
                        return;
                    }
                    frmDesc.Dispose();
                }

                bgBuildProcess.RunWorkerAsync(runData);
            }
            frm.Dispose();
        }

        private void projectSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://" + ((ToolStripMenuItem)sender).Text);
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataDump.DataExtractScriptCreateForm frmCreate = new SqlSync.DataDump.DataExtractScriptCreateForm();
            frmCreate.Show();
        }

        private void loadNewDirectoryControlFilesbxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openSbxFileDialog.ShowDialog())
            {
                if (Path.GetExtension(openSbxFileDialog.FileName).ToLower() == ".sbm")
                    LoadProject(openSbxFileDialog.FileName);
                else
                    LoadXmlControlFile(openSbxFileDialog.FileName);
                    
            }

        }

        private void packageScriptsIntoProjectFilesbmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveScriptsToPackage.FileName = Path.GetFileNameWithoutExtension(this.sbxBuildControlFileName) + ".sbm";
            if (DialogResult.OK == saveScriptsToPackage.ShowDialog())
            {
                bool success = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(this.sbxBuildControlFileName, saveScriptsToPackage.FileName);
                //bool copied = false;
                //string path = Path.GetDirectoryName(this.sbxBuildControlFileName) +@"\";
                ////Just in case that file already exists...
                //if(File.Exists(path+XmlFileNames.MainProjectFile))
                //{
                //    File.Copy(path+XmlFileNames.MainProjectFile,path+"~~"+XmlFileNames.MainProjectFile,true);
                //    copied = true;
                //}

                //File.Copy(this.sbxBuildControlFileName,path+XmlFileNames.MainProjectFile,true);

                //SqlBuildFileHelper.SaveSqlBuildProjectFile(ref this.buildData, path + XmlFileNames.MainProjectFile, saveScriptsToPackage.FileName);


                //if (copied)
                //{
                //    File.Copy(path + "~~" + XmlFileNames.MainProjectFile, path + XmlFileNames.MainProjectFile, true);
                //    File.Delete(path + "~~" + XmlFileNames.MainProjectFile);
                //}
                //else
                //{
                //    File.Delete(path + XmlFileNames.MainProjectFile);
                //}

                if (success)
                {
                    if (DialogResult.Yes == MessageBox.Show("Package creation successful. Open new build package project file?", "Open New File?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        LoadProject(saveScriptsToPackage.FileName);
                }
                else
                {
                    MessageBox.Show("There was an error creating the SBM package. Please check the log file for details.", "Sorry about that!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //SqlBuildFileHelper.SaveSqlBuildProjectFile(
        }

        private void includeObjectPermissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Properties.Settings.Default.ScriptPermissions = includeObjectPermissionsToolStripMenuItem1.Checked;
            SqlSync.Properties.Settings.Default.Save();
        }

        private void scriptALTERVsCREATEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Properties.Settings.Default.ScriptAsAlter = scriptALTERAndCREATEToolStripMenuItem.Checked;
            SqlSync.Properties.Settings.Default.Save();
        }

        private void lstScriptFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstScriptFiles.SelectedItems.Count > 0)
            {
                mnuRemoveScriptFile_Click(sender, e);
            }
        }

        private void makeFileWriteableremoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;
                string fileName = this.projectFilePath + row.FileName;
                if (this.projectIsUnderSourceControl)
                {
                    SourceControlStatus stat =  SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, fileName);
                    if (stat == SourceControlStatus.Error || stat == SourceControlStatus.NotUnderSourceControl || stat == SourceControlStatus.Unknown)
                    {
                        MessageBox.Show(String.Format("Error checking out {0} from source control. Please see application log for details", row.FileName), "Source Control Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        int index = lstScriptFiles.SelectedItems[0].Index;
                        RefreshScriptFileList(index, false);
                        lstScriptFiles.Items[index].Selected = true;
                        EditFile(lstScriptFiles);
                    }
                }
                else
                {
                    if (!SqlBuildFileHelper.MakeFileWriteable(fileName))
                    {
                        MessageBox.Show("Unable to make " + row.FileName + " writeable", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        RefreshScriptFileList(lstScriptFiles.SelectedItems[0].Index, false);
                    }
                }

                
            }

        }

        private void constructCommandLineStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommandLineBuilderForm frmCmd;
            if (sbxBuildControlFileName != null && sbxBuildControlFileName.Length > 0)
                frmCmd = new CommandLineBuilderForm(this.sbxBuildControlFileName);
            else
                frmCmd = new CommandLineBuilderForm(this.buildZipFileName);

            frmCmd.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void scriptPolicyCheckingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Policy.PolicyForm frmPolicy = new SqlSync.SqlBuild.Policy.PolicyForm(ref this.buildData, this.projectFilePath);
            frmPolicy.ScriptSelected += new SqlSync.SqlBuild.Policy.PolicyForm.ScriptSelectedHandler(frmPolicy_ScriptSelected);
            frmPolicy.Show();
        }

        void frmPolicy_ScriptSelected(object sender, SqlSync.SqlBuild.Policy.ScriptSelectedEventArgs e)
        {
           
            for (int i = 0; i < this.lstScriptFiles.Items.Count; i++)
            {
                if (e.SelectedRow == (SqlSyncBuildData.ScriptRow)this.lstScriptFiles.Items[i].Tag)
                {
                    this.lstScriptFiles.Items[i].Selected = true;
                    EditFile(this.lstScriptFiles);
                    break;
                }
                else
                    this.lstScriptFiles.Items[i].Selected = false;
            }
        }

        private void chkNotTransactional_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkNotTransactional.Checked)
            {
                string message = "WARNING!\r\nBy checking this box, you are disabling the transaction handling of Sql Build Manager.\r\nIn the event of a script failure, your scripts will NOT be rolled back\r\nand your database will be left in an inconsistent state!\r\n\r\nAre you certain you want this checked?";
                if (DialogResult.No == MessageBox.Show(message, "Are you sure you want this?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    this.chkNotTransactional.Checked = false;
                    return;
                }

                if(ddBuildType.SelectedItem != null && 
                    ( ddBuildType.SelectedItem.ToString() == BuildType.Trial || ddBuildType.SelectedItem.ToString() == BuildType.TrialPartial))
                {
                    this.chkNotTransactional.Checked = false;
                    message = "You can not have a Trial run without transactions. Please select a different build type then re-check the transaction box";
                    MessageBox.Show(message, "Invalid Build/Transaction combination", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
               
            }

         
        }

        private void ddOverrideLogDatabase_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (ddOverrideLogDatabase.SelectedItem != null && ddOverrideLogDatabase.SelectedItem.ToString().Length > 0 )
            {
                if (DialogResult.No == MessageBox.Show("The committed scripts log for ALL scripts will be saved to '" + ddOverrideLogDatabase.SelectedItem.ToString() + "'.\r\nAre you certain you want this?", "Alternate Logging Database Selected", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    ddOverrideLogDatabase.SelectedIndex = 0;
                }

            }
        }

        private void lblAdvanced_Click(object sender, EventArgs e)
        {
            if (pnlAdvanced.Height <30)
            {
                pnlAdvanced.Height = 48;
                splitContainer1.SplitterDistance += 33;
                lblAdvanced.Text = "Advanced Runtime Settings (use with caution) <<";
            }
            else
            {
                pnlAdvanced.Height = 15;
                splitContainer1.SplitterDistance -= 33;
                lblAdvanced.Text = "Advanced Runtime Settings (use with caution) >>";
            }
        }

        private void howToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual(string.Empty);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("RunTimeBuildSettings");
        }

        private void remoteExecutionServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoteServiceForm frmRemote;
            if(this.buildZipFileName != null && this.buildZipFileName.Length > 0)
                frmRemote = new RemoteServiceForm(this.connData, this.databaseList,this.buildZipFileName);
            else
                frmRemote = new RemoteServiceForm(this.connData,this.databaseList);

            frmRemote.Show();
        }

        private void calculateScriptPackageHashSignatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.buildData != null)
            {

                string hashSignature = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(this.projectFilePath, this.buildData);
                PackageSignatureForm sigForm = new PackageSignatureForm(hashSignature);
                sigForm.ShowDialog();
            }
        }

        private void mnuDefaultScriptTimeout_TextChanged(object sender, EventArgs e)
        {
            int timeout;
            if (Int32.TryParse(mnuDefaultScriptTimeout.Text, out timeout))
            {
                SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout = timeout;
                SqlSync.Properties.Settings.Default.Save();
            }

        }

        private void createBackoutPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Objects.ObjectUpdates> canUpdate;
            List<string> canNotUpdate;
            SqlBuildFileHelper.GetFileDataForObjectUpdates(ref this.buildData, this.projectFileName, out canUpdate, out canNotUpdate);
            if (canUpdate == null)
            {
                MessageBox.Show("There aren't any object script files available for updating", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BackoutPackageForm frmBack = new BackoutPackageForm(this.connData, this.buildData, this.buildZipFileName, this.projectFilePath, this.projectFileName);
            frmBack.ShowDialog();

        }

        private void InferScriptTag_Click(object sender, EventArgs e)
        {
            string typeValue = string.Empty;
            if (sender is ToolStripMenuItem)
            {
                typeValue = ((ToolStripMenuItem)sender).Tag.ToString();
            }

            TagInferenceSource source = (TagInferenceSource) Enum.Parse(typeof(TagInferenceSource), typeValue, true);
            //if (source == null) source = TagInferenceSource.TextOverName;

            List<string> regexFormats = new List<string>();
            regexFormats.AddRange(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());

            if (ScriptTagProcessing.InferScriptTags(ref this.buildData, this.projectFilePath, regexFormats, source))
            {
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
                this.RefreshScriptFileList();
                this.statGeneral.Text = "Script tags updated";
            }
            else
            {
                this.statGeneral.Text = "No script tags could be inferred";
            }
        }

        private void runPolicyCheckingonloadtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.runPolicyCheckingOnLoad = runPolicyCheckingonloadtoolStripMenuItem.Checked;
            if (!this.runPolicyCheckingOnLoad)
            {
                this.lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 0;
                this.policyCheckIconHelpToolStripMenuItem.Visible = false;
            }
            else
            {
                this.lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 20;
                this.policyCheckIconHelpToolStripMenuItem.Visible = true;

            }
        }

        private void savePolicyResultsInCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavePolicyResults(PolicySaveType.CSV);
        }

        private void savePolicyResultsAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavePolicyResults(PolicySaveType.XML);
        }

        private void SavePolicyResults(PolicySaveType type)
        {

            if ((this.buildZipFileName == null || this.buildZipFileName.Length == 0) &&
                (this.sbxBuildControlFileName == null || this.sbxBuildControlFileName.Length == 0))
            {
                MessageBox.Show("Please load an SBM file prior to trying to save violations", "Can't do that yet", MessageBoxButtons.OK);
                return;
            }

            string rootBuildFileName;
            string buildFileName;
            if (this.buildZipFileName != null || this.buildZipFileName.Length > 0)
            {
                rootBuildFileName = Path.GetFileNameWithoutExtension(this.buildZipFileName);
                buildFileName = Path.GetFileName(this.buildZipFileName);
            }
            else
            {
                rootBuildFileName = Path.GetFileNameWithoutExtension(this.sbxBuildControlFileName);
                buildFileName = Path.GetFileName(this.sbxBuildControlFileName);
            }

            this.currentViolations.PackageName = buildFileName;

            string format;
            if (type == PolicySaveType.CSV)
            {
                format = "{0} Violations {1}-{2}-{3} {4}{5}.csv";
                savePolicyViolationCsv.Filter = "CSV *.csv|*.csv|All Files *.*|*.*";
                savePolicyViolationCsv.Title = "Save Policy Violations as CSV";
            }
            else
            {
                format = "{0} Violations {1}-{2}-{3} {4}{5}.xml";
                savePolicyViolationCsv.Filter = "XML *.xml|*.xml|All Files *.*|*.*";
                savePolicyViolationCsv.Title = "Save Policy Violations as XML";
            }

            savePolicyViolationCsv.FileName = String.Format(format, rootBuildFileName, DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString().PadLeft(2, '0'),
                DateTime.Now.Day.ToString().PadLeft(2, '0'),
                DateTime.Now.Hour.ToString().PadLeft(2, '0'),
                DateTime.Now.Minute.ToString().PadLeft(2, '0'));

            if (savePolicyViolationCsv.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string fileName = savePolicyViolationCsv.FileName;

            if (type == PolicySaveType.CSV)
            {
                if (!PolicyHelper.TransformViolationstoCsv(fileName, this.currentViolations))
                {
                    log.ErrorFormat("Error saving violations to file {0}", fileName);
                }
            }
            else
            {
                try
                {
                    string xml = PolicyHelper.TransformViolationstoXml(this.currentViolations);
                    File.WriteAllText(fileName, xml);
                }
                catch(Exception exe)
                {
                    log.Error("Unable to write violations XML to file",exe);
                }
            }
        }


        private void sourceControlServerURLTextboxMenuItem_TextChanged(object sender, EventArgs e)
        {
            SqlSync.Properties.Settings.Default.SourceControlServerUrl = this.sourceControlServerURLTextboxMenuItem.Text;
            SqlSync.Properties.Settings.Default.Save();
        }

        private void mnuListTop_DropDownOpening(object sender, EventArgs e)
        {
            if (this.buildData != null)
            {
                this.mnuExportBuildList.Enabled = true;
            }
        }

     


    }
    public delegate void UnattendedProcessingCompleteEventHandler(int returnCode);
    internal enum PolicySaveType
    {
        CSV, 
        XML
    }
}
