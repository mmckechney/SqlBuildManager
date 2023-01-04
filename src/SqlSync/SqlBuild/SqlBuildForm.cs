using Microsoft.Extensions.Logging;
using SqlBuildManager.Enterprise;
using SqlBuildManager.Enterprise.ActiveDirectory;
using SqlBuildManager.Enterprise.Feature;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlBuildManager.Interfaces.SourceControl;
using SqlBuildManager.ScriptHandling;
using SqlSync.BuildHistory;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.Controls.OAKControls;
using SqlSync.DbInformation;
using SqlSync.MRU;
using SqlSync.ObjectScript;
using SqlSync.SqlBuild.CodeTable;
using SqlSync.SqlBuild.DefaultScripts;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Status;
using SqlSync.TableScript;
using SqlSync.Validator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using sb = SqlSync.SqlBuild;
//using SqlBuildManager.Enterprise.CodeReview;
namespace SqlSync.SqlBuild

{
    /// <summary>
    /// Summary description for SqlBuildForm.
    /// </summary>
    public class SqlBuildForm : System.Windows.Forms.Form, IMRUClient
    {

        private List<string> codeReviewDbaMembers = new List<string>();
        Package currentViolations = new Package();
        private bool scriptPkWithTables = true;
        List<string> adGroupMemberships = new List<string>();
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        //private System.Security.Principal.ImpersonationLevel  impersonatedUser = null;


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
        private ToolStripMenuItem mnuDacpacDelta;
        private ToolStripMenuItem mnuMainAddSqlScript;
        private ToolStripMenuItem mnuMainAddNewFile;
        private ToolStripSeparator toolStripSeparator4;
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
        //private ToolStripMenuItem sourceControlServerURLToolStripMenuItem;
        //private ToolStripTextBox sourceControlServerURLTextboxMenuItem;
        private BackgroundWorker bgBulkAddStep2;
        private ToolStripMenuItem policyWarningActionMayBeRequiredToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator23;
        private ColumnHeader colCodeReviewIcon;
        //private ToolStripMenuItem codeReviewIconToolStripMenuItem;
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
            connData = data;
        }
        public SqlBuildForm(string buildZipFileName)
            : this()
        {
            if (Path.GetExtension(buildZipFileName).ToLower() == ".sbx")
            {
                sbxBuildControlFileName = buildZipFileName;
            }
            else
            {
                this.buildZipFileName = buildZipFileName;
            }

        }
        public SqlBuildForm(string buildZipFileName, string serverName, string scriptLogFileName)
            : this()
        {
            this.buildZipFileName = buildZipFileName;
            connData = new ConnectionData();
            connData.SQLServerName = serverName;
            connData.AuthenticationType = AuthenticationType.Windows;

            if (scriptLogFileName.Length > 0)
            {
                externalScriptLogFileName = scriptLogFileName;
                createSqlRunLogFile = true;
            }

            runningUnattended = true;
        }
        public SqlBuildForm(string buildZipFileName, ConnectionData data)
            : this()
        {
            this.buildZipFileName = buildZipFileName;
            connData = data;
        }
        public SqlBuildForm(string buildZipFileName, MultiDb.MultiDbData multiDbData, string scriptLogFileName) : this()
        {
            //Initalize the multiDbData object here. Final initializtion will happen just before the build is run.
            //The build will get kicked off after the build file is loaded in the "bgLoadZipFle_RunWorkerCompleted" method
            multiDbRunData = multiDbData;
            multiDbRunData.BuildFileName = buildZipFileName;

            if (scriptLogFileName.Length > 0)
            {
                externalScriptLogFileName = scriptLogFileName;
                createSqlRunLogFile = true;
            }

            this.buildZipFileName = buildZipFileName;

            runningUnattended = true;
            connData = new ConnectionData(); //initialize, but don't need to populate it...
            connData.AuthenticationType = AuthenticationType.Windows;
        }
        private void SqlBuildForm_Load(object sender, System.EventArgs e)
        {
            bgEnterpriseSettings.RunWorkerAsync();


            try
            {
                //If no connection, will need to pop up the connection dialog. 
                if (connData == null)
                {
                    ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
                    DialogResult result = frmConnect.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        connData = frmConnect.SqlConnection;
                        databaseList = frmConnect.DatabaseList;
                    }
                    else
                    {
                        MessageBox.Show("Sql Build manager can not continue without a valid SQL Server Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        Close();
                        Application.Exit();
                        return;
                    }
                }

                InitMRU();

                //Set the color coding for the script list...

                leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptDontStripTransactions;
                allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptAllowMultipleRuns;
                scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptWillBeSkippedMarkedAsRunOnce;
                allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptAllowMultipleRunsAndLeaveTransactions;
                scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor = SqlSync.Properties.Settings.Default.ScriptReadOnly;

                colorLeaveTrans = leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor;
                colorMultipleRun = allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor;
                colorSkipped = scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor;
                colorBoth = allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor;
                colorReadOnlyFile = scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor;

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
                if (databaseList == null)
                    databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);


                //If the connection object is initalized, which it should be, set the current server and bind the build type dropdown box
                if (connData != null)
                {
                    settingsControl1.Server = connData.SQLServerName;
                    DatabindBuildType();
                }

                if (PassesReadOnlyProjectFileCheck())
                {

                    //If we have a build file name already on Load, go ahead an load it up via asyc method. This will get the form to the user quicker for better response.
                    //If this is an unattended/non-interactive build, processing will get kicked off in the "RunWorkerCompleted" method of the async call
                    if (!String.IsNullOrEmpty(buildZipFileName))
                    {
                        bgLoadZipFle.RunWorkerAsync(buildZipFileName);
                    }
                    else if (!String.IsNullOrEmpty(sbxBuildControlFileName))
                    {
                        LoadXmlControlFile(sbxBuildControlFileName);
                    }
                }


                //Show the form, but minimized if non-interactive.
                if (runningUnattended)
                    WindowState = FormWindowState.Minimized;
                Show();

                //Retrieve the helper queries.
                GenerateUtiltityItems();

                //Get auto scripting configurations
                GenerateAutoScriptList();



                //If not running in an unattended mode, check for updates via an asyc call.
                if (!runningUnattended)
                    bgCheckForUpdates.RunWorkerAsync(false);

            }
            catch (Exception exe)
            {
                log.LogError(exe, "Sorry...There was an error loading. Returing with code 998.");

                //Any errors?... Close out with a non-Zero exit code
                if (UnattendedProcessingCompleteEvent != null)
                    UnattendedProcessingCompleteEvent(998);
            }

            try
            {

                //Set the database list as found in the main menu "Scripting" selection
                SetDatabaseMenuList();
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
            }




        }
        private void bgEnterpriseSettings_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                bool tagsRequiredEnabled, defaultScriptEnabled, remoteExecutionEnabled, policyCheckOnLoadEnabled, scriptPkWithTable;
                SetPropertiesFromEnterpriseConfiguration(out tagsRequiredEnabled, out defaultScriptEnabled, out remoteExecutionEnabled, out policyCheckOnLoadEnabled, out scriptPkWithTable);
                e.Result = new List<bool>(new bool[] { tagsRequiredEnabled, defaultScriptEnabled, remoteExecutionEnabled, policyCheckOnLoadEnabled, scriptPkWithTable });
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                throw;
            }
        }

        private void bgEnterpriseSettings_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result is List<bool>)
                {
                    List<bool> x = (List<bool>)e.Result;
                    scriptTagsRequiredToolStripMenuItem.Enabled = x[0];
                    maintainDefaultScriptRegistryToolStripMenuItem.Enabled = x[1];
                    //remoteExecutionServiceToolStripMenuItem.Enabled = x[2];
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

                if (!runPolicyCheckingOnLoad)
                {
                    lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 0;
                    policyCheckIconHelpToolStripMenuItem.Visible = false;
                    runPolicyCheckingonloadtoolStripMenuItem.Checked = true;
                }
                else
                {
                    runPolicyCheckingonloadtoolStripMenuItem.Checked = false;
                }

                //Code Review status
                if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null)
                    EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig = new CodeReviewConfig();

                if (!EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
                {
                    lstScriptFiles.Columns[(int)ScriptListIndex.CodeReviewStatusIconColumn].Width = 0;
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                throw;
            }
        }
        private void SetPropertiesFromEnterpriseConfiguration(out bool tagsRequiredEnabled, out bool defaultScriptEnabled, out bool remoteExecutionEnabled, out bool policyCheckOnLoadEnabled, out bool scriptPkWithTable)
        {
            scriptPkWithTable = true;

            adGroupMemberships = AdHelper.GetGroupMemberships(System.Environment.UserName).ToList();
            if (adGroupMemberships.Count == 0)
            {
                log.LogWarning($"No Group Memberships found for {System.Environment.UserName}");
            }
            else if (SqlBuildManager.Logging.ApplicationLogging.IsDebug())
            {
                log.LogDebug($"Group memberships for {System.Environment.UserName}: {String.Join("; ", adGroupMemberships.ToArray())}");
            }

            tagsRequiredEnabled = true;
            defaultScriptEnabled = true;
            bool saveSettings = false;
            EnterpriseConfiguration entConfig = EnterpriseConfigHelper.EnterpriseConfig;

            //Get the Feature control sets..
            if (entConfig.FeatureAccess != null)
                remoteExecutionEnabled = FeatureAccessHelper.IsFeatureEnabled(FeatureKey.RemoteExecution, System.Environment.UserName, adGroupMemberships, entConfig.FeatureAccess);
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
                SqlBuildManager.Enterprise.DefaultScripts.DefaultScriptHelper.SetEnterpriseDefaultScripts(entConfig.DefaultScriptConfiguration.ToList(), adGroupMemberships);
            }

            //Get enterprise level Tag regex
            if (entConfig.ScriptTagInference != null && entConfig.ScriptTagInference.Count() > 0)
            {
                List<string> regexList = SqlBuildManager.Enterprise.Tag.EnterpriseTagHelper.GetEnterpriseTagRegexValues(entConfig.ScriptTagInference.ToList(), adGroupMemberships);
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
                    var d = (from g in adGroupMemberships
                             join a in entConfig.SciptPolicyRunOnLoad.ApplyToGroup on g.ToLower() equals a.GroupName.ToLower()
                             select g);

                    if (d.Count() > 0)
                        runPolicyCheckingOnLoad = true;
                }
                else
                {
                    runPolicyCheckingOnLoad = true;
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
                    var ss = (from g in adGroupMemberships
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
                            scriptPkWithTables = pk.ToList()[0];
                            scriptPkWithTable = scriptPkWithTables;
                            SqlSync.Properties.Settings.Default.ScriptPkWithTables = scriptPkWithTables;
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
                if (!File.Exists(fileName))
                    return true;

                FileAttributes attrib = File.GetAttributes(fileName);

                if ((attrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    string msg = "Your selected file " + Path.GetFileName(fileName) + " is marked as Read-Only. To continue, this file needs to be writable.\r\nDo you want to make the file writable and continue loading?";
                    if (DialogResult.Yes == MessageBox.Show(msg, "File is Read Only!", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal);
                        if (fileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string xml = Path.Combine(Path.GetDirectoryName(fileName), "SqlSyncBuildHistory.xml");
                            if (File.Exists(xml))
                                File.SetAttributes(xml, FileAttributes.Normal);
                        }
                        return true;
                    }
                    else
                    {
                        buildZipFileName = null;
                        sbxBuildControlFileName = null;
                    }
                }
                else if (fileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
                {
                    string xml = Path.Combine(Path.GetDirectoryName(fileName), "SqlSyncBuildHistory.xml");
                    if (File.Exists(xml))
                    {
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
            if (!String.IsNullOrEmpty(buildZipFileName))
                tmp = buildZipFileName;
            else if (!String.IsNullOrEmpty(sbxBuildControlFileName))
                tmp = sbxBuildControlFileName;

            return PassesReadOnlyProjectFileCheck(tmp);
        }

        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            mruManager = new MRUManager();
            mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Sql Build Manager"); // Registry path to keep MRU list
            mruManager.MaxDisplayNameLength = 40;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlBuildForm));
            grbBuildScripts = new System.Windows.Forms.GroupBox();
            btnSlideBuildScripts = new System.Windows.Forms.Button();
            imageListSlide = new System.Windows.Forms.ImageList(components);
            chkUpdateOnOverride = new System.Windows.Forms.CheckBox();
            chkScriptChanges = new System.Windows.Forms.CheckBox();
            lstScriptFiles = new SqlSync.Controls.OAKControls.OAKListView();
            colImage = new System.Windows.Forms.ColumnHeader();
            colPolicyIcon = new System.Windows.Forms.ColumnHeader();
            colCodeReviewIcon = new System.Windows.Forms.ColumnHeader();
            colSequence = new System.Windows.Forms.ColumnHeader();
            colScriptFile = new System.Windows.Forms.ColumnHeader();
            colDatabaseName = new System.Windows.Forms.ColumnHeader();
            colScriptId = new System.Windows.Forms.ColumnHeader();
            colScriptSize = new System.Windows.Forms.ColumnHeader();
            colTag = new System.Windows.Forms.ColumnHeader();
            colDateAdded = new System.Windows.Forms.ColumnHeader();
            colDateModified = new System.Windows.Forms.ColumnHeader();
            ctxScriptFile = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEditFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            menuItem4 = new System.Windows.Forms.ToolStripSeparator();
            makeFileWriteableremoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            mnuAddSqlScriptText = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddScript = new System.Windows.Forms.ToolStripMenuItem();
            menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            mnuUpdatePopulates = new System.Windows.Forms.ToolStripMenuItem();
            mnuUpdateObjectScripts = new System.Windows.Forms.ToolStripMenuItem();
            mnuObjectScripts_FileDefault = new System.Windows.Forms.ToolStripMenuItem();
            mnuObjectScripts_CurrentSettings = new System.Windows.Forms.ToolStripMenuItem();
            menuItem2 = new System.Windows.Forms.ToolStripSeparator();
            mnuCreateExportFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuRemoveScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            mnuTryScript = new System.Windows.Forms.ToolStripMenuItem();
            mnuRunScript = new System.Windows.Forms.ToolStripMenuItem();
            mnuViewRunHistorySep = new System.Windows.Forms.ToolStripSeparator();
            mnuViewRunHistory = new System.Windows.Forms.ToolStripMenuItem();
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renameScriptFIleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            imageListBuildScripts = new System.Windows.Forms.ImageList(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            statusHelpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            iconLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptNotRunOnCurrentServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            serverVersionIsNewerThanSBMVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            runOnCurrentServerdatabaseCombinationAndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            fileMissingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            policyCheckIconHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            policyChecksNotRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            policyCheckFailedActionRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            policyWarningActionMayBeRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            passesPolicyChecksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            backgrounLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            statusHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            waitingOnStatusCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reviewNotStartedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reviewInProgressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reviewAcceptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reviewAcceptedByDBAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openSbmFileDialog = new System.Windows.Forms.OpenFileDialog();
            dlgAddScriptFile = new System.Windows.Forms.OpenFileDialog();
            grpManager = new System.Windows.Forms.GroupBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            targetDatabaseOverrideCtrl1 = new SqlSync.TargetDatabaseOverrideCtrl();
            label2 = new System.Windows.Forms.Label();
            txtBuildDesc = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            txtStartIndex = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lnkStartBuild = new System.Windows.Forms.LinkLabel();
            label1 = new System.Windows.Forms.Label();
            ddBuildType = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            ddOverrideLogDatabase = new System.Windows.Forms.ComboBox();
            chkNotTransactional = new System.Windows.Forms.CheckBox();
            lstBuild = new System.Windows.Forms.ListView();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            columnHeader6 = new System.Windows.Forms.ColumnHeader();
            columnHeader8 = new System.Windows.Forms.ColumnHeader();
            columnHeader7 = new System.Windows.Forms.ColumnHeader();
            columnHeader13 = new System.Windows.Forms.ColumnHeader();
            ctxResults = new System.Windows.Forms.ContextMenuStrip(components);
            mnuDisplayRowResults = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditFromResults = new System.Windows.Forms.ToolStripMenuItem();
            mnuOpenRunScriptFile = new System.Windows.Forms.ToolStripMenuItem();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveExportFile = new System.Windows.Forms.SaveFileDialog();
            openScriptExportFile = new System.Windows.Forms.OpenFileDialog();
            mainMenu1 = new System.Windows.Forms.MenuStrip();
            mnuActionMain = new System.Windows.Forms.ToolStripMenuItem();
            mnuLoadProject = new System.Windows.Forms.ToolStripMenuItem();
            loadNewDirectoryControlFilesbxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            packageScriptsIntoProjectFilesbmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            mnuChangeSqlServer = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            maintainManualDatabaseEntriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            maintainDefaultScriptRegistryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            scriptTagsRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            createSQLLogOfBuildRunsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runPolicyCheckingonloadtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptPrimaryKeyWithTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            defaultScriptTimeoutsecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuDefaultScriptTimeout = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            mnuMainAddSqlScript = new System.Windows.Forms.ToolStripMenuItem();
            mnuMainAddNewFile = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            mnuImportScriptFromFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuDacpacDelta = new System.Windows.Forms.ToolStripMenuItem();
            menuItem12 = new System.Windows.Forms.ToolStripSeparator();
            mnuExportScriptText = new System.Windows.Forms.ToolStripMenuItem();
            mnuIndividualFiles = new System.Windows.Forms.ToolStripMenuItem();
            mnuCombinedFile = new System.Windows.Forms.ToolStripMenuItem();
            menuItem22 = new System.Windows.Forms.ToolStripSeparator();
            mnuIncludeUSE = new System.Windows.Forms.ToolStripMenuItem();
            mnuIncludeSequence = new System.Windows.Forms.ToolStripMenuItem();
            menuItem15 = new System.Windows.Forms.ToolStripSeparator();
            startConfigureMultiServerDatabaseRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuListTop = new System.Windows.Forms.ToolStripMenuItem();
            mnuFindScript = new System.Windows.Forms.ToolStripMenuItem();
            menuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            menuItem8 = new System.Windows.Forms.ToolStripSeparator();
            mnuRenumberSequence = new System.Windows.Forms.ToolStripMenuItem();
            mnuResortByFileType = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportBuildList = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportBuildListToFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportBuildListToClipBoard = new System.Windows.Forms.ToolStripMenuItem();
            menuItem10 = new System.Windows.Forms.ToolStripSeparator();
            mnuBulkAdd = new System.Windows.Forms.ToolStripMenuItem();
            mnuBulkFromList = new System.Windows.Forms.ToolStripMenuItem();
            mnuBulkFromFile = new System.Windows.Forms.ToolStripMenuItem();
            menuItem13 = new System.Windows.Forms.ToolStripSeparator();
            mnuClearPreviouslyRunBlocks = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            inferScriptTagFromFileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptContentsFirstThenFileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileNameFirstThenSciptContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            scriptContentsOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileNamesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuScripting = new System.Windows.Forms.ToolStripMenuItem();
            mnuDDActiveDatabase = new System.Windows.Forms.ToolStripComboBox();
            mnuAddObjectCreate = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddStoredProcs = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddFunctions = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddViews = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddTables = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddTriggers = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            mnuAddRoles = new System.Windows.Forms.ToolStripMenuItem();
            scriptingOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptALTERAndCREATEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            includeObjectPermissionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            mnuAddCodeTablePop = new System.Windows.Forms.ToolStripMenuItem();
            menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            mnuUpdatePopulate = new System.Windows.Forms.ToolStripMenuItem();
            mnuObjectUpdates = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            createBackoutPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuLogging = new System.Windows.Forms.ToolStripMenuItem();
            mnuShowBuildLogs = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptToLogFile = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mnuShowBuildHistory = new System.Windows.Forms.ToolStripMenuItem();
            archiveBuildHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuItem16 = new System.Windows.Forms.ToolStripMenuItem();
            mnuObjectValidation = new System.Windows.Forms.ToolStripMenuItem();
            storedProcedureTestingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuItem21 = new System.Windows.Forms.ToolStripSeparator();
            mnuSchemaScripting = new System.Windows.Forms.ToolStripMenuItem();
            mnuCodeTableScripting = new System.Windows.Forms.ToolStripMenuItem();
            menuItem18 = new System.Windows.Forms.ToolStripSeparator();
            mnuDataAuditScripting = new System.Windows.Forms.ToolStripMenuItem();
            mnuDataExtraction = new System.Windows.Forms.ToolStripMenuItem();
            createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            mnuDatabaseSize = new System.Windows.Forms.ToolStripMenuItem();
            menuItem19 = new System.Windows.Forms.ToolStripSeparator();
            mnuAutoScripting = new System.Windows.Forms.ToolStripMenuItem();
            menuItem11 = new System.Windows.Forms.ToolStripSeparator();
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            scriptPolicyCheckingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runPolicyChecksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            savePolicyResultsInCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            savePolicyResultsAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            calculateScriptPackageHashSignatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            howToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            projectSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewLogFileMenuItem2 = new SqlSync.Controls.ViewLogFileMenuItem();
            setLoggingLevelMenuItem2 = new SqlSync.Controls.SetLoggingLevelMenuItem();
            remoteExecutionServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            constructCommandLineStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openFileBulkLoad = new System.Windows.Forms.OpenFileDialog();
            pnlManager = new System.Windows.Forms.Panel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            pnlAdvanced = new System.Windows.Forms.Panel();
            lblAdvanced = new System.Windows.Forms.Label();
            grpAdvanced = new System.Windows.Forms.GroupBox();
            grpBuildResults = new System.Windows.Forms.GroupBox();
            pnlBuildScripts = new System.Windows.Forms.Panel();
            splitter1 = new System.Windows.Forms.Splitter();
            openFileAutoScript = new System.Windows.Forms.OpenFileDialog();
            fdrSaveScripts = new System.Windows.Forms.FolderBrowserDialog();
            saveCombinedScript = new System.Windows.Forms.SaveFileDialog();
            bgBuildProcess = new System.ComponentModel.BackgroundWorker();
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
            settingsControl1 = new SqlSync.SettingsControl();
            bgCheckForUpdates = new System.ComponentModel.BackgroundWorker();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            statScriptCount = new System.Windows.Forms.ToolStripStatusLabel();
            statBuildTime = new System.Windows.Forms.ToolStripStatusLabel();
            statScriptTime = new System.Windows.Forms.ToolStripStatusLabel();
            progressBuild = new System.Windows.Forms.ToolStripProgressBar();
            tmrBuild = new System.Windows.Forms.Timer(components);
            tmrScript = new System.Windows.Forms.Timer(components);
            bgLoadZipFle = new System.ComponentModel.BackgroundWorker();
            bgRefreshScriptList = new System.ComponentModel.BackgroundWorker();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            bgDatabaseRoutine = new System.ComponentModel.BackgroundWorker();
            bgObjectScripting = new System.ComponentModel.BackgroundWorker();
            bgGetObjectList = new System.ComponentModel.BackgroundWorker();
            openFileDataExtract = new System.Windows.Forms.OpenFileDialog();
            openSbxFileDialog = new System.Windows.Forms.OpenFileDialog();
            saveScriptsToPackage = new System.Windows.Forms.SaveFileDialog();
            bgEnterpriseSettings = new System.ComponentModel.BackgroundWorker();
            bgBulkAdd = new System.ComponentModel.BackgroundWorker();
            bgPolicyCheck = new System.ComponentModel.BackgroundWorker();
            savePolicyViolationCsv = new System.Windows.Forms.SaveFileDialog();
            bgBulkAddStep2 = new System.ComponentModel.BackgroundWorker();
            bgCodeReview = new System.ComponentModel.BackgroundWorker();
            toolTip2 = new System.Windows.Forms.ToolTip(components);
            grbBuildScripts.SuspendLayout();
            ctxScriptFile.SuspendLayout();
            menuStrip1.SuspendLayout();
            grpManager.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            ctxResults.SuspendLayout();
            mainMenu1.SuspendLayout();
            pnlManager.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            pnlAdvanced.SuspendLayout();
            grpAdvanced.SuspendLayout();
            grpBuildResults.SuspendLayout();
            pnlBuildScripts.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            toolStripContainer2.ContentPanel.SuspendLayout();
            toolStripContainer2.TopToolStripPanel.SuspendLayout();
            toolStripContainer2.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // grbBuildScripts
            // 
            grbBuildScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            grbBuildScripts.Controls.Add(btnSlideBuildScripts);
            grbBuildScripts.Controls.Add(chkUpdateOnOverride);
            grbBuildScripts.Controls.Add(chkScriptChanges);
            grbBuildScripts.Controls.Add(lstScriptFiles);
            grbBuildScripts.Controls.Add(menuStrip1);
            grbBuildScripts.Enabled = false;
            grbBuildScripts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grbBuildScripts.Location = new System.Drawing.Point(2, 2);
            grbBuildScripts.Name = "grbBuildScripts";
            grbBuildScripts.Size = new System.Drawing.Size(518, 540);
            grbBuildScripts.TabIndex = 14;
            grbBuildScripts.TabStop = false;
            grbBuildScripts.Text = "Build Scripts";
            // 
            // btnSlideBuildScripts
            // 
            btnSlideBuildScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnSlideBuildScripts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSlideBuildScripts.FlatAppearance.BorderSize = 0;
            btnSlideBuildScripts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSlideBuildScripts.ImageIndex = 0;
            btnSlideBuildScripts.ImageList = imageListSlide;
            btnSlideBuildScripts.Location = new System.Drawing.Point(497, 485);
            btnSlideBuildScripts.Margin = new System.Windows.Forms.Padding(0);
            btnSlideBuildScripts.Name = "btnSlideBuildScripts";
            btnSlideBuildScripts.Size = new System.Drawing.Size(15, 15);
            btnSlideBuildScripts.TabIndex = 17;
            toolTip1.SetToolTip(btnSlideBuildScripts, "Toggle Date displays");
            btnSlideBuildScripts.UseVisualStyleBackColor = true;
            btnSlideBuildScripts.Click += new System.EventHandler(btnSlideBuildScripts_Click);
            // 
            // imageListSlide
            // 
            imageListSlide.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListSlide.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSlide.ImageStream")));
            imageListSlide.TransparentColor = System.Drawing.Color.Transparent;
            imageListSlide.Images.SetKeyName(0, "rightarrow_white.GIF");
            imageListSlide.Images.SetKeyName(1, "leftarrow_white.GIF");
            // 
            // chkUpdateOnOverride
            // 
            chkUpdateOnOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            chkUpdateOnOverride.Checked = true;
            chkUpdateOnOverride.CheckState = System.Windows.Forms.CheckState.Checked;
            chkUpdateOnOverride.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkUpdateOnOverride.Location = new System.Drawing.Point(6, 494);
            chkUpdateOnOverride.Name = "chkUpdateOnOverride";
            chkUpdateOnOverride.Size = new System.Drawing.Size(420, 16);
            chkUpdateOnOverride.TabIndex = 16;
            chkUpdateOnOverride.Text = "Update Icons on Override Target Change (will slow list refresh)";
            chkUpdateOnOverride.CheckedChanged += new System.EventHandler(chkUpdateOnOverride_CheckedChanged);
            // 
            // chkScriptChanges
            // 
            chkScriptChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            chkScriptChanges.Checked = true;
            chkScriptChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptChanges.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkScriptChanges.Location = new System.Drawing.Point(6, 478);
            chkScriptChanges.Name = "chkScriptChanges";
            chkScriptChanges.Size = new System.Drawing.Size(420, 16);
            chkScriptChanges.TabIndex = 15;
            chkScriptChanges.Text = "Check for script changes (Pre-run scripts only, may slow list refresh)";
            chkScriptChanges.Click += new System.EventHandler(chkScriptChanges_Click);
            // 
            // lstScriptFiles
            // 
            lstScriptFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstScriptFiles.AllowDrop = true;
            lstScriptFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstScriptFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstScriptFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colImage,
            colPolicyIcon,
            colCodeReviewIcon,
            colSequence,
            colScriptFile,
            colDatabaseName,
            colScriptId,
            colScriptSize,
            colTag,
            colDateAdded,
            colDateModified});
            lstScriptFiles.ContextMenuStrip = ctxScriptFile;
            lstScriptFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lstScriptFiles.FullRowSelect = true;
            lstScriptFiles.GridLines = true;
            lstScriptFiles.Location = new System.Drawing.Point(5, 17);
            lstScriptFiles.Name = "lstScriptFiles";
            lstScriptFiles.ShowItemToolTips = true;
            lstScriptFiles.Size = new System.Drawing.Size(508, 455);
            lstScriptFiles.SmallImageList = imageListBuildScripts;
            lstScriptFiles.TabIndex = 0;
            lstScriptFiles.UseCompatibleStateImageBehavior = false;
            lstScriptFiles.View = System.Windows.Forms.View.Details;
            lstScriptFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lstScriptFiles_ColumnClick);
            lstScriptFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(lstScriptFiles_ItemDrag);
            lstScriptFiles.DragDrop += new System.Windows.Forms.DragEventHandler(lstScriptFiles_DragDrop);
            lstScriptFiles.DragEnter += new System.Windows.Forms.DragEventHandler(lstScriptFiles_DragEnter);
            lstScriptFiles.DoubleClick += new System.EventHandler(lstScriptFiles_DoubleClick);
            lstScriptFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(lstScriptFiles_KeyDown);
            // 
            // colImage
            // 
            colImage.Text = "";
            colImage.Width = 21;
            // 
            // colPolicyIcon
            // 
            colPolicyIcon.Text = "";
            colPolicyIcon.Width = 16;
            // 
            // colCodeReviewIcon
            // 
            colCodeReviewIcon.Text = "";
            colCodeReviewIcon.Width = 16;
            // 
            // colSequence
            // 
            colSequence.Text = "Seq #";
            colSequence.Width = 47;
            // 
            // colScriptFile
            // 
            colScriptFile.Text = "Script File";
            colScriptFile.Width = 217;
            // 
            // colDatabaseName
            // 
            colDatabaseName.Text = "Database ";
            colDatabaseName.Width = 80;
            // 
            // colScriptId
            // 
            colScriptId.Text = "Script Id";
            colScriptId.Width = 0;
            // 
            // colScriptSize
            // 
            colScriptSize.Text = "Size";
            colScriptSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            colScriptSize.Width = 0;
            // 
            // colTag
            // 
            colTag.Text = "Tag";
            colTag.Width = 85;
            // 
            // colDateAdded
            // 
            colDateAdded.Text = "Date Added";
            colDateAdded.Width = 0;
            // 
            // colDateModified
            // 
            colDateModified.Text = "Date Modified";
            colDateModified.Width = 0;
            // 
            // ctxScriptFile
            // 
            ctxScriptFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuEditFile,
            mnuEditScriptFile,
            menuItem4,
            makeFileWriteableremoveToolStripMenuItem,
            toolStripSeparator23,
            mnuAddSqlScriptText,
            mnuAddScript,
            menuItem1,
            mnuUpdatePopulates,
            mnuUpdateObjectScripts,
            menuItem2,
            mnuCreateExportFile,
            mnuRemoveScriptFile,
            menuItem6,
            mnuTryScript,
            mnuRunScript,
            mnuViewRunHistorySep,
            mnuViewRunHistory,
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem,
            renameScriptFIleToolStripMenuItem,
            toolStripSeparator12});
            ctxScriptFile.Name = "ctxScriptFile";
            ctxScriptFile.Size = new System.Drawing.Size(367, 354);
            ctxScriptFile.Opening += new System.ComponentModel.CancelEventHandler(ctxScriptFile_Opening);
            // 
            // mnuEditFile
            // 
            mnuEditFile.MergeIndex = 1;
            mnuEditFile.Name = "mnuEditFile";
            mnuEditFile.Size = new System.Drawing.Size(366, 22);
            mnuEditFile.Text = "Edit File";
            mnuEditFile.Click += new System.EventHandler(mnuEditFile_Click);
            // 
            // mnuEditScriptFile
            // 
            mnuEditScriptFile.MergeIndex = 0;
            mnuEditScriptFile.Name = "mnuEditScriptFile";
            mnuEditScriptFile.Size = new System.Drawing.Size(366, 22);
            mnuEditScriptFile.Text = "Edit/View Script Build Detail";
            mnuEditScriptFile.Click += new System.EventHandler(mnuEditScriptFile_Click);
            // 
            // menuItem4
            // 
            menuItem4.MergeIndex = 2;
            menuItem4.Name = "menuItem4";
            menuItem4.Size = new System.Drawing.Size(363, 6);
            // 
            // makeFileWriteableremoveToolStripMenuItem
            // 
            makeFileWriteableremoveToolStripMenuItem.Name = "makeFileWriteableremoveToolStripMenuItem";
            makeFileWriteableremoveToolStripMenuItem.Size = new System.Drawing.Size(366, 22);
            makeFileWriteableremoveToolStripMenuItem.Text = "Make file writeable (remove Read Only attribute)";
            makeFileWriteableremoveToolStripMenuItem.Click += new System.EventHandler(makeFileWriteableremoveToolStripMenuItem_Click);
            // 
            // toolStripSeparator23
            // 
            toolStripSeparator23.Name = "toolStripSeparator23";
            toolStripSeparator23.Size = new System.Drawing.Size(363, 6);
            // 
            // mnuAddSqlScriptText
            // 
            mnuAddSqlScriptText.MergeIndex = 4;
            mnuAddSqlScriptText.Name = "mnuAddSqlScriptText";
            mnuAddSqlScriptText.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            mnuAddSqlScriptText.Size = new System.Drawing.Size(366, 22);
            mnuAddSqlScriptText.Text = "Add New Sql Script (Text)";
            mnuAddSqlScriptText.Click += new System.EventHandler(mnuAddSqlScriptText_Click);
            // 
            // mnuAddScript
            // 
            mnuAddScript.MergeIndex = 3;
            mnuAddScript.Name = "mnuAddScript";
            mnuAddScript.Size = new System.Drawing.Size(366, 22);
            mnuAddScript.Text = "Add New File";
            mnuAddScript.Click += new System.EventHandler(mnuAddScript_Click);
            // 
            // menuItem1
            // 
            menuItem1.MergeIndex = 5;
            menuItem1.Name = "menuItem1";
            menuItem1.Size = new System.Drawing.Size(363, 6);
            // 
            // mnuUpdatePopulates
            // 
            mnuUpdatePopulates.Enabled = false;
            mnuUpdatePopulates.MergeIndex = 6;
            mnuUpdatePopulates.Name = "mnuUpdatePopulates";
            mnuUpdatePopulates.Size = new System.Drawing.Size(366, 22);
            mnuUpdatePopulates.Text = "Update Code Table Populate Scripts";
            mnuUpdatePopulates.Click += new System.EventHandler(mnuUpdatePopulates_Click);
            // 
            // mnuUpdateObjectScripts
            // 
            mnuUpdateObjectScripts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuObjectScripts_FileDefault,
            mnuObjectScripts_CurrentSettings});
            mnuUpdateObjectScripts.Enabled = false;
            mnuUpdateObjectScripts.MergeIndex = 7;
            mnuUpdateObjectScripts.Name = "mnuUpdateObjectScripts";
            mnuUpdateObjectScripts.Size = new System.Drawing.Size(366, 22);
            mnuUpdateObjectScripts.Text = "Update Object Create Scripts";
            // 
            // mnuObjectScripts_FileDefault
            // 
            mnuObjectScripts_FileDefault.Name = "mnuObjectScripts_FileDefault";
            mnuObjectScripts_FileDefault.Size = new System.Drawing.Size(346, 22);
            mnuObjectScripts_FileDefault.Text = "Using File Default Setting";
            mnuObjectScripts_FileDefault.Click += new System.EventHandler(mnuObjectScripts_FileDefault_Click);
            // 
            // mnuObjectScripts_CurrentSettings
            // 
            mnuObjectScripts_CurrentSettings.Name = "mnuObjectScripts_CurrentSettings";
            mnuObjectScripts_CurrentSettings.Size = new System.Drawing.Size(346, 22);
            mnuObjectScripts_CurrentSettings.Text = "Using Current Server and Database/Override Setting";
            mnuObjectScripts_CurrentSettings.Click += new System.EventHandler(mnuObjectScripts_CurrentSettings_Click);
            // 
            // menuItem2
            // 
            menuItem2.MergeIndex = 8;
            menuItem2.Name = "menuItem2";
            menuItem2.Size = new System.Drawing.Size(363, 6);
            // 
            // mnuCreateExportFile
            // 
            mnuCreateExportFile.MergeIndex = 9;
            mnuCreateExportFile.Name = "mnuCreateExportFile";
            mnuCreateExportFile.Size = new System.Drawing.Size(366, 22);
            mnuCreateExportFile.Text = "Export Selected Script Entries";
            mnuCreateExportFile.Click += new System.EventHandler(mnuCreateExportFile_Click);
            // 
            // mnuRemoveScriptFile
            // 
            mnuRemoveScriptFile.MergeIndex = 10;
            mnuRemoveScriptFile.Name = "mnuRemoveScriptFile";
            mnuRemoveScriptFile.Size = new System.Drawing.Size(366, 22);
            mnuRemoveScriptFile.Text = "Remove File(s)";
            mnuRemoveScriptFile.Click += new System.EventHandler(mnuRemoveScriptFile_Click);
            // 
            // menuItem6
            // 
            menuItem6.MergeIndex = 11;
            menuItem6.Name = "menuItem6";
            menuItem6.Size = new System.Drawing.Size(363, 6);
            // 
            // mnuTryScript
            // 
            mnuTryScript.MergeIndex = 12;
            mnuTryScript.Name = "mnuTryScript";
            mnuTryScript.Size = new System.Drawing.Size(366, 22);
            mnuTryScript.Text = "Try Script against Database (Rollback)";
            mnuTryScript.Click += new System.EventHandler(mnuTryScript_Click);
            // 
            // mnuRunScript
            // 
            mnuRunScript.MergeIndex = 13;
            mnuRunScript.Name = "mnuRunScript";
            mnuRunScript.Size = new System.Drawing.Size(366, 22);
            mnuRunScript.Text = "Run Script against Database (Commit)";
            mnuRunScript.Click += new System.EventHandler(mnuRunScript_Click);
            // 
            // mnuViewRunHistorySep
            // 
            mnuViewRunHistorySep.MergeIndex = 14;
            mnuViewRunHistorySep.Name = "mnuViewRunHistorySep";
            mnuViewRunHistorySep.Size = new System.Drawing.Size(363, 6);
            // 
            // mnuViewRunHistory
            // 
            mnuViewRunHistory.MergeIndex = 15;
            mnuViewRunHistory.Name = "mnuViewRunHistory";
            mnuViewRunHistory.Size = new System.Drawing.Size(366, 22);
            mnuViewRunHistory.Text = "View packaged script run history against current server";
            mnuViewRunHistory.Click += new System.EventHandler(mnuViewRunHistory_Click);
            // 
            // showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem
            // 
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Enabled = false;
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Name = "showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem";
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Size = new System.Drawing.Size(366, 22);
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Text = "View object change history as run by Sql Build Manager";
            showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem.Click += new System.EventHandler(showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem_Click);
            // 
            // renameScriptFIleToolStripMenuItem
            // 
            renameScriptFIleToolStripMenuItem.Name = "renameScriptFIleToolStripMenuItem";
            renameScriptFIleToolStripMenuItem.Size = new System.Drawing.Size(366, 22);
            renameScriptFIleToolStripMenuItem.Text = "Rename Script File";
            renameScriptFIleToolStripMenuItem.Click += new System.EventHandler(renameScriptFIleToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new System.Drawing.Size(363, 6);
            // 
            // imageListBuildScripts
            // 
            imageListBuildScripts.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageListBuildScripts.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBuildScripts.ImageStream")));
            imageListBuildScripts.TransparentColor = System.Drawing.Color.Transparent;
            imageListBuildScripts.Images.SetKeyName(0, "");
            imageListBuildScripts.Images.SetKeyName(1, "lock.png");
            imageListBuildScripts.Images.SetKeyName(2, "");
            imageListBuildScripts.Images.SetKeyName(3, "");
            imageListBuildScripts.Images.SetKeyName(4, "");
            imageListBuildScripts.Images.SetKeyName(5, "question.ico");
            imageListBuildScripts.Images.SetKeyName(6, "Delete.png");
            imageListBuildScripts.Images.SetKeyName(7, "Document-Protect.png");
            imageListBuildScripts.Images.SetKeyName(8, "Help-2.png");
            imageListBuildScripts.Images.SetKeyName(9, "Tick.png");
            imageListBuildScripts.Images.SetKeyName(10, "Exclamation-square.png");
            imageListBuildScripts.Images.SetKeyName(11, "exclamation-shield-frame.png");
            imageListBuildScripts.Images.SetKeyName(12, "Hand.ico");
            imageListBuildScripts.Images.SetKeyName(13, "Clock.ico");
            imageListBuildScripts.Images.SetKeyName(14, "Ok-blueSquare.ico");
            imageListBuildScripts.Images.SetKeyName(15, "Ok-greenSquare.ico");
            imageListBuildScripts.Images.SetKeyName(16, "Wait.png");
            imageListBuildScripts.Images.SetKeyName(17, "Discuss.ico");
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = System.Drawing.Color.Gainsboro;
            menuStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statusHelpToolStripMenuItem1,
            iconLegendToolStripMenuItem,
            policyCheckIconHelpToolStripMenuItem,
            backgrounLegendToolStripMenuItem,
            toolStripMenuItem1});
            menuStrip1.Location = new System.Drawing.Point(3, 513);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(512, 24);
            menuStrip1.TabIndex = 18;
            menuStrip1.Text = "menuStrip1";
            // 
            // statusHelpToolStripMenuItem1
            // 
            statusHelpToolStripMenuItem1.Name = "statusHelpToolStripMenuItem1";
            statusHelpToolStripMenuItem1.Size = new System.Drawing.Size(47, 20);
            statusHelpToolStripMenuItem1.Text = "Help:";
            // 
            // iconLegendToolStripMenuItem
            // 
            iconLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem,
            scriptNotRunOnCurrentServerToolStripMenuItem,
            serverVersionIsNewerThanSBMVersionToolStripMenuItem,
            toolStripSeparator6,
            runOnCurrentServerdatabaseCombinationAndToolStripMenuItem,
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem,
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem,
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem,
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem,
            toolStripSeparator22,
            fileMissingToolStripMenuItem});
            iconLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            iconLegendToolStripMenuItem.Name = "iconLegendToolStripMenuItem";
            iconLegendToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            iconLegendToolStripMenuItem.Text = "Status Icon";
            // 
            // notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem
            // 
            notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Name = "notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem";
            notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            notYetRunOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Text = "Not yet run on current server/database combination and...";
            // 
            // scriptNotRunOnCurrentServerToolStripMenuItem
            // 
            scriptNotRunOnCurrentServerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("scriptNotRunOnCurrentServerToolStripMenuItem.Image")));
            scriptNotRunOnCurrentServerToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            scriptNotRunOnCurrentServerToolStripMenuItem.Name = "scriptNotRunOnCurrentServerToolStripMenuItem";
            scriptNotRunOnCurrentServerToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            scriptNotRunOnCurrentServerToolStripMenuItem.Text = "...SBM version is newer than server version (OK)";
            // 
            // serverVersionIsNewerThanSBMVersionToolStripMenuItem
            // 
            serverVersionIsNewerThanSBMVersionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("serverVersionIsNewerThanSBMVersionToolStripMenuItem.Image")));
            serverVersionIsNewerThanSBMVersionToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            serverVersionIsNewerThanSBMVersionToolStripMenuItem.Name = "serverVersionIsNewerThanSBMVersionToolStripMenuItem";
            serverVersionIsNewerThanSBMVersionToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            serverVersionIsNewerThanSBMVersionToolStripMenuItem.Text = "...server version is newer than SBM version (possible conflict, compare before ru" +
    "nning)";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(596, 6);
            // 
            // runOnCurrentServerdatabaseCombinationAndToolStripMenuItem
            // 
            runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Name = "runOnCurrentServerdatabaseCombinationAndToolStripMenuItem";
            runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            runOnCurrentServerdatabaseCombinationAndToolStripMenuItem.Text = "Run on current server/database combination and... ";
            // 
            // runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem
            // 
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Image")));
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Name = "runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem";
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            runOnCurrentServerAndUnchangedSinceLastRunToolStripMenuItem.Text = "...unchanged since last run";
            // 
            // alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem
            // 
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Image")));
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Name = "alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem";
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            alreadyRunOnCurrentServerAndMarkedAsRunOnceToolStripMenuItem.Text = "...marked as \"Run Once\" (will be skipped if run again)";
            // 
            // runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem
            // 
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Image")));
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Name = "runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem";
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            runOnCurrentServerAndChangedInBuildFileSinceLastRunToolStripMenuItem.Text = "...changed in build file since last run";
            // 
            // runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem
            // 
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.I" +
        "mage")));
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Name = "runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem";
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.Text = "...manually changed in database (out of sync with SBM and should be compared befo" +
    "re re-running)";
            runOnCurrentServerButManuallyChangedInDatabaseoutOfSyncWithSBMToolStripMenuItem.ToolTipText = "This status is valid for Stored Procedures and Functions and is\r\nbased on the las" +
    "t commit date from the SBM file vs. the change date\r\nof the Procedure or Functio" +
    "n.";
            // 
            // toolStripSeparator22
            // 
            toolStripSeparator22.Name = "toolStripSeparator22";
            toolStripSeparator22.Size = new System.Drawing.Size(596, 6);
            // 
            // fileMissingToolStripMenuItem
            // 
            fileMissingToolStripMenuItem.Name = "fileMissingToolStripMenuItem";
            fileMissingToolStripMenuItem.Size = new System.Drawing.Size(599, 22);
            fileMissingToolStripMenuItem.Text = "File is missing from file system or build file. This must be corrected prior to e" +
    "xecution. ";
            // 
            // policyCheckIconHelpToolStripMenuItem
            // 
            policyCheckIconHelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            policyChecksNotRunToolStripMenuItem,
            policyCheckFailedActionRequiredToolStripMenuItem,
            reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem,
            policyWarningActionMayBeRequiredToolStripMenuItem,
            passesPolicyChecksToolStripMenuItem});
            policyCheckIconHelpToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            policyCheckIconHelpToolStripMenuItem.Name = "policyCheckIconHelpToolStripMenuItem";
            policyCheckIconHelpToolStripMenuItem.Size = new System.Drawing.Size(113, 20);
            policyCheckIconHelpToolStripMenuItem.Text = "Policy Check Icon";
            // 
            // policyChecksNotRunToolStripMenuItem
            // 
            policyChecksNotRunToolStripMenuItem.Name = "policyChecksNotRunToolStripMenuItem";
            policyChecksNotRunToolStripMenuItem.Size = new System.Drawing.Size(433, 22);
            policyChecksNotRunToolStripMenuItem.Text = "Policy checks not run";
            // 
            // policyCheckFailedActionRequiredToolStripMenuItem
            // 
            policyCheckFailedActionRequiredToolStripMenuItem.Name = "policyCheckFailedActionRequiredToolStripMenuItem";
            policyCheckFailedActionRequiredToolStripMenuItem.Size = new System.Drawing.Size(433, 22);
            policyCheckFailedActionRequiredToolStripMenuItem.Text = "Policy checks failed - ACTION REQUIRED!";
            // 
            // reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem
            // 
            reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Name = "reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem";
            reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Size = new System.Drawing.Size(433, 22);
            reviewWarningThisScriptShouldBeExaminedBeforeDeploymentToolStripMenuItem.Text = "Review warning - this script should be examined before deployment";
            // 
            // policyWarningActionMayBeRequiredToolStripMenuItem
            // 
            policyWarningActionMayBeRequiredToolStripMenuItem.Name = "policyWarningActionMayBeRequiredToolStripMenuItem";
            policyWarningActionMayBeRequiredToolStripMenuItem.Size = new System.Drawing.Size(433, 22);
            policyWarningActionMayBeRequiredToolStripMenuItem.Text = "Policy warning - action may be required.";
            // 
            // passesPolicyChecksToolStripMenuItem
            // 
            passesPolicyChecksToolStripMenuItem.Name = "passesPolicyChecksToolStripMenuItem";
            passesPolicyChecksToolStripMenuItem.Size = new System.Drawing.Size(433, 22);
            passesPolicyChecksToolStripMenuItem.Text = "Policy checks passed";
            // 
            // backgrounLegendToolStripMenuItem
            // 
            backgrounLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem,
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem,
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem,
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem,
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem});
            backgrounLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            backgrounLegendToolStripMenuItem.Name = "backgrounLegendToolStripMenuItem";
            backgrounLegendToolStripMenuItem.Size = new System.Drawing.Size(115, 20);
            backgrounLegendToolStripMenuItem.Text = "Background Color";
            // 
            // allowMultipleRunsOfScriptOnSameServerToolStripMenuItem
            // 
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = System.Drawing.Color.DarkSalmon;
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Name = "allowMultipleRunsOfScriptOnSameServerToolStripMenuItem";
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Size = new System.Drawing.Size(459, 22);
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Text = "Allow multiple runs of script on same server";
            // 
            // leaveTransactionTextInScriptsdontStripOutToolStripMenuItem
            // 
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = System.Drawing.Color.LightBlue;
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Name = "leaveTransactionTextInScriptsdontStripOutToolStripMenuItem";
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Size = new System.Drawing.Size(459, 22);
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Text = "Leave transaction text in scripts (don\'t strip out transaction references)";
            // 
            // allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem
            // 
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.BackColor = System.Drawing.Color.Thistle;
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Name = "allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem";
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Size = new System.Drawing.Size(459, 22);
            allowMultipleRunsANDLeaveTransactionTextToolStripMenuItem.Text = "Allow multiple runs AND Leave transaction text";
            // 
            // scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem
            // 
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.BackColor = System.Drawing.Color.BlanchedAlmond;
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Name = "scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem";
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Size = new System.Drawing.Size(459, 22);
            scriptWillBeSkippedOnServeralreadyRunAndMarkedAsRunOnceToolStripMenuItem.Text = "Script will be skipped on server (already run and marked as \"run once\")";
            // 
            // scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem
            // 
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.BackColor = System.Drawing.Color.Gray;
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Name = "scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem";
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Size = new System.Drawing.Size(459, 22);
            scriptFileIsReadonlyYouWillNeedToMakeWriteableBeforeModifyingToolStripMenuItem.Text = "Script file is read-only! You will need to make writeable before modifying.";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statusHelpToolStripMenuItem});
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // statusHelpToolStripMenuItem
            // 
            statusHelpToolStripMenuItem.Name = "statusHelpToolStripMenuItem";
            statusHelpToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            statusHelpToolStripMenuItem.Text = "Status Help:";
            // 
            // waitingOnStatusCheckToolStripMenuItem
            // 
            waitingOnStatusCheckToolStripMenuItem.Name = "waitingOnStatusCheckToolStripMenuItem";
            waitingOnStatusCheckToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            waitingOnStatusCheckToolStripMenuItem.Text = "Waiting on status check";
            // 
            // reviewNotStartedToolStripMenuItem
            // 
            reviewNotStartedToolStripMenuItem.Name = "reviewNotStartedToolStripMenuItem";
            reviewNotStartedToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            reviewNotStartedToolStripMenuItem.Text = "Review Not Started";
            // 
            // reviewInProgressToolStripMenuItem
            // 
            reviewInProgressToolStripMenuItem.Name = "reviewInProgressToolStripMenuItem";
            reviewInProgressToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            reviewInProgressToolStripMenuItem.Text = "Review in Progress";
            // 
            // reviewAcceptedToolStripMenuItem
            // 
            reviewAcceptedToolStripMenuItem.Name = "reviewAcceptedToolStripMenuItem";
            reviewAcceptedToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            reviewAcceptedToolStripMenuItem.Text = "Review Accepted";
            // 
            // reviewAcceptedByDBAToolStripMenuItem
            // 
            reviewAcceptedByDBAToolStripMenuItem.Name = "reviewAcceptedByDBAToolStripMenuItem";
            reviewAcceptedByDBAToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            reviewAcceptedByDBAToolStripMenuItem.Text = "Review Accepted by DBA";
            // 
            // openSbmFileDialog
            // 
            openSbmFileDialog.CheckFileExists = false;
            openSbmFileDialog.DefaultExt = "xml";
            openSbmFileDialog.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
    "iles (*.zip)|*.zip|All Files|*.*";
            openSbmFileDialog.Title = "Open or Create New Sql Build Manager Project File";
            // 
            // dlgAddScriptFile
            // 
            dlgAddScriptFile.Filter = resources.GetString("dlgAddScriptFile.Filter");
            dlgAddScriptFile.Title = "Add Script File to Build";
            // 
            // grpManager
            // 
            grpManager.Controls.Add(pictureBox1);
            grpManager.Controls.Add(targetDatabaseOverrideCtrl1);
            grpManager.Controls.Add(label2);
            grpManager.Controls.Add(txtBuildDesc);
            grpManager.Controls.Add(btnCancel);
            grpManager.Controls.Add(txtStartIndex);
            grpManager.Controls.Add(label4);
            grpManager.Controls.Add(label3);
            grpManager.Controls.Add(lnkStartBuild);
            grpManager.Controls.Add(label1);
            grpManager.Controls.Add(ddBuildType);
            grpManager.Dock = System.Windows.Forms.DockStyle.Fill;
            grpManager.Enabled = false;
            grpManager.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpManager.Location = new System.Drawing.Point(3, 3);
            grpManager.MinimumSize = new System.Drawing.Size(533, 154);
            grpManager.Name = "grpManager";
            grpManager.Size = new System.Drawing.Size(547, 167);
            grpManager.TabIndex = 15;
            grpManager.TabStop = false;
            grpManager.Text = "Build Manager / Run Settings";
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            pictureBox1.Location = new System.Drawing.Point(525, 16);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(17, 16);
            pictureBox1.TabIndex = 28;
            pictureBox1.TabStop = false;
            toolTip1.SetToolTip(pictureBox1, "Click for Help on Build Settings");
            pictureBox1.Click += new System.EventHandler(pictureBox1_Click);
            pictureBox1.DoubleClick += new System.EventHandler(pictureBox1_Click);
            // 
            // targetDatabaseOverrideCtrl1
            // 
            targetDatabaseOverrideCtrl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            targetDatabaseOverrideCtrl1.Location = new System.Drawing.Point(99, 41);
            targetDatabaseOverrideCtrl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            targetDatabaseOverrideCtrl1.Name = "targetDatabaseOverrideCtrl1";
            targetDatabaseOverrideCtrl1.Size = new System.Drawing.Size(443, 76);
            targetDatabaseOverrideCtrl1.TabIndex = 1;
            targetDatabaseOverrideCtrl1.TargetChanged += new SqlSync.TargetChangedEventHandler(targetDatabaseOverrideCtrl1_TargetChanged);
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(6, 42);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(92, 65);
            label2.TabIndex = 27;
            label2.Text = "Target Database\r\nOverride:\r\n";
            // 
            // txtBuildDesc
            // 
            txtBuildDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            txtBuildDesc.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            txtBuildDesc.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            txtBuildDesc.Location = new System.Drawing.Point(99, 123);
            txtBuildDesc.Name = "txtBuildDesc";
            txtBuildDesc.Size = new System.Drawing.Size(370, 21);
            txtBuildDesc.TabIndex = 2;
            toolTip1.SetToolTip(txtBuildDesc, "Enter a Description for the build. This is required before you can execute.\r\n\r\nNO" +
        "TE: This value will be inserted whereever the token #BuildDescription# is found " +
        "in scripts in the package.");
            txtBuildDesc.TextChanged += new System.EventHandler(txtBuildDesc_TextChanged);
            // 
            // btnCancel
            // 
            btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnCancel.BackColor = System.Drawing.SystemColors.Control;
            btnCancel.ForeColor = System.Drawing.Color.Red;
            btnCancel.Location = new System.Drawing.Point(475, 123);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(55, 23);
            btnCancel.TabIndex = 24;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Visible = false;
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // txtStartIndex
            // 
            txtStartIndex.Enabled = false;
            txtStartIndex.Location = new System.Drawing.Point(460, 16);
            txtStartIndex.Name = "txtStartIndex";
            txtStartIndex.Size = new System.Drawing.Size(40, 21);
            txtStartIndex.TabIndex = 2;
            txtStartIndex.Text = "0";
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(295, 16);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(151, 20);
            label4.TabIndex = 13;
            label4.Text = "Partial Run Start Index:";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label3.Location = new System.Drawing.Point(8, 126);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(92, 16);
            label3.TabIndex = 12;
            label3.Text = "Description *:";
            // 
            // lnkStartBuild
            // 
            lnkStartBuild.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lnkStartBuild.Enabled = false;
            lnkStartBuild.Location = new System.Drawing.Point(154, 145);
            lnkStartBuild.Name = "lnkStartBuild";
            lnkStartBuild.Size = new System.Drawing.Size(261, 16);
            lnkStartBuild.TabIndex = 3;
            lnkStartBuild.TabStop = true;
            lnkStartBuild.Text = "Please Enter a Description above to run build";
            lnkStartBuild.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lnkStartBuild.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkStartBuild_LinkClicked);
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(9, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(75, 16);
            label1.TabIndex = 1;
            label1.Text = "Build Type:";
            toolTip1.SetToolTip(label1, "NOTE: A \"Trial\" will always Roll Back the build");
            // 
            // ddBuildType
            // 
            ddBuildType.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ddBuildType.ItemHeight = 13;
            ddBuildType.Location = new System.Drawing.Point(115, 16);
            ddBuildType.Name = "ddBuildType";
            ddBuildType.Size = new System.Drawing.Size(173, 21);
            ddBuildType.TabIndex = 0;
            toolTip1.SetToolTip(ddBuildType, "NOTE: A \"Trial\" will always Roll Back the build");
            ddBuildType.SelectionChangeCommitted += new System.EventHandler(ddBuildType_SelectionChangeCommitted);
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(9, 28);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(130, 13);
            label5.TabIndex = 30;
            label5.Text = "Log Target Database:";
            // 
            // ddOverrideLogDatabase
            // 
            ddOverrideLogDatabase.FormattingEnabled = true;
            ddOverrideLogDatabase.Location = new System.Drawing.Point(146, 24);
            ddOverrideLogDatabase.Name = "ddOverrideLogDatabase";
            ddOverrideLogDatabase.Size = new System.Drawing.Size(142, 21);
            ddOverrideLogDatabase.TabIndex = 29;
            toolTip1.SetToolTip(ddOverrideLogDatabase, "Sets an alternate database to log the changes to.\r\nMost of the time, this will be" +
        " left blank.");
            ddOverrideLogDatabase.SelectionChangeCommitted += new System.EventHandler(ddOverrideLogDatabase_SelectionChangeCommitted);
            // 
            // chkNotTransactional
            // 
            chkNotTransactional.AutoSize = true;
            chkNotTransactional.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            chkNotTransactional.Location = new System.Drawing.Point(301, 26);
            chkNotTransactional.Name = "chkNotTransactional";
            chkNotTransactional.Size = new System.Drawing.Size(205, 17);
            chkNotTransactional.TabIndex = 28;
            chkNotTransactional.Text = "Run Build without a Transaction";
            toolTip1.SetToolTip(chkNotTransactional, "CAUTION!!\r\nSets whether or not to run this build in a transaction. In most cases " +
        "this should be left unchecked.\r\n** When checked, your scripts will not be rolled" +
        " back in the event of a failure! **");
            chkNotTransactional.UseVisualStyleBackColor = true;
            chkNotTransactional.CheckedChanged += new System.EventHandler(chkNotTransactional_CheckedChanged);
            // 
            // lstBuild
            // 
            lstBuild.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstBuild.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstBuild.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader4,
            columnHeader5,
            columnHeader6,
            columnHeader8,
            columnHeader7,
            columnHeader13});
            lstBuild.ContextMenuStrip = ctxResults;
            lstBuild.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lstBuild.FullRowSelect = true;
            lstBuild.GridLines = true;
            lstBuild.Location = new System.Drawing.Point(5, 15);
            lstBuild.Name = "lstBuild";
            lstBuild.Size = new System.Drawing.Size(537, 304);
            lstBuild.TabIndex = 10;
            lstBuild.UseCompatibleStateImageBehavior = false;
            lstBuild.View = System.Windows.Forms.View.Details;
            lstBuild.DoubleClick += new System.EventHandler(mnuEditFromResults_Click);
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Seq #";
            columnHeader4.Width = 44;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Script File";
            columnHeader5.Width = 170;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "Database Name";
            columnHeader6.Width = 91;
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "Orig #";
            columnHeader8.Width = 45;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Status";
            columnHeader7.Width = 100;
            // 
            // columnHeader13
            // 
            columnHeader13.Text = "Time";
            columnHeader13.Width = 56;
            // 
            // ctxResults
            // 
            ctxResults.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuDisplayRowResults,
            mnuEditFromResults,
            mnuOpenRunScriptFile});
            ctxResults.Name = "ctxResults";
            ctxResults.Size = new System.Drawing.Size(153, 70);
            // 
            // mnuDisplayRowResults
            // 
            mnuDisplayRowResults.MergeIndex = 0;
            mnuDisplayRowResults.Name = "mnuDisplayRowResults";
            mnuDisplayRowResults.Size = new System.Drawing.Size(152, 22);
            mnuDisplayRowResults.Text = "Display Results";
            mnuDisplayRowResults.Click += new System.EventHandler(mnuDisplayRowResults_Click);
            // 
            // mnuEditFromResults
            // 
            mnuEditFromResults.MergeIndex = 2;
            mnuEditFromResults.Name = "mnuEditFromResults";
            mnuEditFromResults.Size = new System.Drawing.Size(152, 22);
            mnuEditFromResults.Text = "Edit File";
            mnuEditFromResults.Click += new System.EventHandler(mnuEditFromResults_Click);
            // 
            // mnuOpenRunScriptFile
            // 
            mnuOpenRunScriptFile.MergeIndex = 1;
            mnuOpenRunScriptFile.Name = "mnuOpenRunScriptFile";
            mnuOpenRunScriptFile.Size = new System.Drawing.Size(152, 22);
            mnuOpenRunScriptFile.Text = "Open File";
            mnuOpenRunScriptFile.Click += new System.EventHandler(mnuOpenRunScriptFile_Click);
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            saveFileDialog1.Title = "Save Build Export";
            // 
            // saveExportFile
            // 
            saveExportFile.DefaultExt = "sbe";
            saveExportFile.Filter = "Sql Build Export Files (*.sbe)|*.sbe|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            saveExportFile.Title = "Save File";
            // 
            // openScriptExportFile
            // 
            openScriptExportFile.Filter = "Sql Build Files (*.sbm)|*.sbm|Sql Build Export Files (*.sbe)|*.sbe|XML Files (*.x" +
    "ml)|*.xml|All Files (*.*)|*.*";
            openScriptExportFile.Title = "Import Sql Build Script File";
            // 
            // mainMenu1
            // 
            mainMenu1.Dock = System.Windows.Forms.DockStyle.None;
            mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuActionMain,
            mnuListTop,
            mnuScripting,
            mnuLogging,
            menuItem16,
            mnuHelp});
            mainMenu1.Location = new System.Drawing.Point(0, 0);
            mainMenu1.Name = "mainMenu1";
            mainMenu1.Size = new System.Drawing.Size(1079, 24);
            mainMenu1.TabIndex = 0;
            // 
            // mnuActionMain
            // 
            mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuLoadProject,
            loadNewDirectoryControlFilesbxToolStripMenuItem,
            toolStripSeparator11,
            packageScriptsIntoProjectFilesbmToolStripMenuItem,
            toolStripSeparator10,
            mnuChangeSqlServer,
            toolStripSeparator1,
            settingsToolStripMenuItem,
            mnuMainAddSqlScript,
            mnuMainAddNewFile,
            toolStripSeparator4,
            mnuImportScriptFromFile,
            mnuDacpacDelta,
            menuItem12,
            mnuExportScriptText,
            menuItem15,
            startConfigureMultiServerDatabaseRunToolStripMenuItem,
            toolStripSeparator7,
            mnuFileMRU,
            toolStripSeparator13,
            exitToolStripMenuItem});
            mnuActionMain.MergeIndex = 0;
            mnuActionMain.Name = "mnuActionMain";
            mnuActionMain.Size = new System.Drawing.Size(54, 20);
            mnuActionMain.Text = "&Action";
            mnuActionMain.MouseUp += new System.Windows.Forms.MouseEventHandler(mnuActionMain_MouseUp);
            // 
            // mnuLoadProject
            // 
            mnuLoadProject.MergeIndex = 0;
            mnuLoadProject.Name = "mnuLoadProject";
            mnuLoadProject.Size = new System.Drawing.Size(345, 22);
            mnuLoadProject.Text = "&Load/New Project File (*.sbm)";
            mnuLoadProject.ToolTipText = "Open existing or create new self contained build project file (.sbm)";
            mnuLoadProject.Click += new System.EventHandler(mnuLoadProject_Click);
            // 
            // loadNewDirectoryControlFilesbxToolStripMenuItem
            // 
            loadNewDirectoryControlFilesbxToolStripMenuItem.Name = "loadNewDirectoryControlFilesbxToolStripMenuItem";
            loadNewDirectoryControlFilesbxToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            loadNewDirectoryControlFilesbxToolStripMenuItem.Text = "Load/New Directory Based Build Control File (*.sbx)";
            loadNewDirectoryControlFilesbxToolStripMenuItem.ToolTipText = "Open existing or create new directory control file (.sbx) to manage \"loose\" scrip" +
    "ts ";
            loadNewDirectoryControlFilesbxToolStripMenuItem.Click += new System.EventHandler(loadNewDirectoryControlFilesbxToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(342, 6);
            // 
            // packageScriptsIntoProjectFilesbmToolStripMenuItem
            // 
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = false;
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Name = "packageScriptsIntoProjectFilesbmToolStripMenuItem";
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Text = "Package scripts into project file (.sbm)";
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Click += new System.EventHandler(packageScriptsIntoProjectFilesbmToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(342, 6);
            // 
            // mnuChangeSqlServer
            // 
            mnuChangeSqlServer.MergeIndex = 1;
            mnuChangeSqlServer.Name = "mnuChangeSqlServer";
            mnuChangeSqlServer.Size = new System.Drawing.Size(345, 22);
            mnuChangeSqlServer.Text = "&Change Sql Server Connection";
            mnuChangeSqlServer.Click += new System.EventHandler(mnuChangeSqlServer_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(342, 6);
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            maintainManualDatabaseEntriesToolStripMenuItem,
            maintainDefaultScriptRegistryToolStripMenuItem,
            toolStripSeparator5,
            scriptTagsRequiredToolStripMenuItem,
            createSQLLogOfBuildRunsToolStripMenuItem,
            runPolicyCheckingonloadtoolStripMenuItem,
            scriptPrimaryKeyWithTableToolStripMenuItem,
            toolStripSeparator15,
            defaultScriptTimeoutsecondsToolStripMenuItem,
            mnuDefaultScriptTimeout,
            toolStripSeparator16});
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // maintainManualDatabaseEntriesToolStripMenuItem
            // 
            maintainManualDatabaseEntriesToolStripMenuItem.Name = "maintainManualDatabaseEntriesToolStripMenuItem";
            maintainManualDatabaseEntriesToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            maintainManualDatabaseEntriesToolStripMenuItem.Text = "Maintain Manual Database Entries";
            maintainManualDatabaseEntriesToolStripMenuItem.Click += new System.EventHandler(maintainManualDatabaseEntriesToolStripMenuItem_Click);
            // 
            // maintainDefaultScriptRegistryToolStripMenuItem
            // 
            maintainDefaultScriptRegistryToolStripMenuItem.Name = "maintainDefaultScriptRegistryToolStripMenuItem";
            maintainDefaultScriptRegistryToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            maintainDefaultScriptRegistryToolStripMenuItem.Text = "Maintain Default Script Registry";
            maintainDefaultScriptRegistryToolStripMenuItem.Click += new System.EventHandler(maintainDefaultScriptRegistryToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(304, 6);
            // 
            // scriptTagsRequiredToolStripMenuItem
            // 
            scriptTagsRequiredToolStripMenuItem.CheckOnClick = true;
            scriptTagsRequiredToolStripMenuItem.Name = "scriptTagsRequiredToolStripMenuItem";
            scriptTagsRequiredToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            scriptTagsRequiredToolStripMenuItem.Text = "Script Tags Required";
            scriptTagsRequiredToolStripMenuItem.ToolTipText = "Check as to whether or not this build file requires script tags for all of its sc" +
    "ripts.";
            scriptTagsRequiredToolStripMenuItem.Click += new System.EventHandler(scriptTagsRequiredToolStripMenuItem_Click);
            // 
            // createSQLLogOfBuildRunsToolStripMenuItem
            // 
            createSQLLogOfBuildRunsToolStripMenuItem.CheckOnClick = true;
            createSQLLogOfBuildRunsToolStripMenuItem.Name = "createSQLLogOfBuildRunsToolStripMenuItem";
            createSQLLogOfBuildRunsToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            createSQLLogOfBuildRunsToolStripMenuItem.Text = "Create SQL Log of Build Runs";
            createSQLLogOfBuildRunsToolStripMenuItem.ToolTipText = "Check as to whether or not to create a SQL script log \r\nof builds as they are run" +
    ".";
            createSQLLogOfBuildRunsToolStripMenuItem.CheckedChanged += new System.EventHandler(createSQLLogOfBuildRunsToolStripMenuItem_Click);
            createSQLLogOfBuildRunsToolStripMenuItem.Click += new System.EventHandler(createSQLLogOfBuildRunsToolStripMenuItem_Click);
            // 
            // runPolicyCheckingonloadtoolStripMenuItem
            // 
            runPolicyCheckingonloadtoolStripMenuItem.CheckOnClick = true;
            runPolicyCheckingonloadtoolStripMenuItem.Name = "runPolicyCheckingonloadtoolStripMenuItem";
            runPolicyCheckingonloadtoolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            runPolicyCheckingonloadtoolStripMenuItem.Text = "Run Policy Checking on load";
            runPolicyCheckingonloadtoolStripMenuItem.ToolTipText = "Check as to whether or not to create a SQL script log \r\nof builds as they are run" +
    ".";
            runPolicyCheckingonloadtoolStripMenuItem.Click += new System.EventHandler(runPolicyCheckingonloadtoolStripMenuItem_Click);
            // 
            // scriptPrimaryKeyWithTableToolStripMenuItem
            // 
            scriptPrimaryKeyWithTableToolStripMenuItem.Name = "scriptPrimaryKeyWithTableToolStripMenuItem";
            scriptPrimaryKeyWithTableToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            scriptPrimaryKeyWithTableToolStripMenuItem.Text = "Script Primary Key with Table";
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new System.Drawing.Size(304, 6);
            // 
            // defaultScriptTimeoutsecondsToolStripMenuItem
            // 
            defaultScriptTimeoutsecondsToolStripMenuItem.Name = "defaultScriptTimeoutsecondsToolStripMenuItem";
            defaultScriptTimeoutsecondsToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            defaultScriptTimeoutsecondsToolStripMenuItem.Text = "Default/Minimum Script Timeout (seconds):";
            // 
            // mnuDefaultScriptTimeout
            // 
            mnuDefaultScriptTimeout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            mnuDefaultScriptTimeout.BackColor = System.Drawing.SystemColors.Window;
            mnuDefaultScriptTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            mnuDefaultScriptTimeout.Name = "mnuDefaultScriptTimeout";
            mnuDefaultScriptTimeout.Size = new System.Drawing.Size(50, 23);
            mnuDefaultScriptTimeout.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            mnuDefaultScriptTimeout.TextChanged += new System.EventHandler(mnuDefaultScriptTimeout_TextChanged);
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new System.Drawing.Size(304, 6);
            // 
            // mnuMainAddSqlScript
            // 
            mnuMainAddSqlScript.Enabled = false;
            mnuMainAddSqlScript.MergeIndex = 4;
            mnuMainAddSqlScript.Name = "mnuMainAddSqlScript";
            mnuMainAddSqlScript.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            mnuMainAddSqlScript.Size = new System.Drawing.Size(345, 22);
            mnuMainAddSqlScript.Text = "Add New Sql Script (Text)";
            mnuMainAddSqlScript.Click += new System.EventHandler(mnuAddSqlScriptText_Click);
            // 
            // mnuMainAddNewFile
            // 
            mnuMainAddNewFile.Enabled = false;
            mnuMainAddNewFile.MergeIndex = 3;
            mnuMainAddNewFile.Name = "mnuMainAddNewFile";
            mnuMainAddNewFile.Size = new System.Drawing.Size(345, 22);
            mnuMainAddNewFile.Text = "Add New File";
            mnuMainAddNewFile.Click += new System.EventHandler(mnuAddScript_Click);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(342, 6);
            // 
            // mnuImportScriptFromFile
            // 
            mnuImportScriptFromFile.Enabled = false;
            mnuImportScriptFromFile.MergeIndex = 3;
            mnuImportScriptFromFile.Name = "mnuImportScriptFromFile";
            mnuImportScriptFromFile.Size = new System.Drawing.Size(345, 22);
            mnuImportScriptFromFile.Text = "&Import Scripts from Sql Build File";
            mnuImportScriptFromFile.Click += new System.EventHandler(mnuImportScriptFromFile_Click);
            // 
            // mnuDacpacDelta
            // 
            mnuDacpacDelta.Name = "mnuDacpacDelta";
            mnuDacpacDelta.Size = new System.Drawing.Size(345, 22);
            mnuDacpacDelta.Text = "Sql Build File from DACPAC/DB Delta";
            mnuDacpacDelta.Click += new System.EventHandler(mnuDacpacDelta_Click);
            // 
            // menuItem12
            // 
            menuItem12.MergeIndex = 4;
            menuItem12.Name = "menuItem12";
            menuItem12.Size = new System.Drawing.Size(342, 6);
            // 
            // mnuExportScriptText
            // 
            mnuExportScriptText.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuIndividualFiles,
            mnuCombinedFile,
            menuItem22,
            mnuIncludeUSE,
            mnuIncludeSequence});
            mnuExportScriptText.Enabled = false;
            mnuExportScriptText.MergeIndex = 6;
            mnuExportScriptText.Name = "mnuExportScriptText";
            mnuExportScriptText.Size = new System.Drawing.Size(345, 22);
            mnuExportScriptText.Text = "Export Scripts To";
            // 
            // mnuIndividualFiles
            // 
            mnuIndividualFiles.MergeIndex = 0;
            mnuIndividualFiles.Name = "mnuIndividualFiles";
            mnuIndividualFiles.Size = new System.Drawing.Size(247, 22);
            mnuIndividualFiles.Text = "Individual Script Files";
            mnuIndividualFiles.Click += new System.EventHandler(mnuIndividualFiles_Click);
            // 
            // mnuCombinedFile
            // 
            mnuCombinedFile.MergeIndex = 1;
            mnuCombinedFile.Name = "mnuCombinedFile";
            mnuCombinedFile.Size = new System.Drawing.Size(247, 22);
            mnuCombinedFile.Text = "Consolidated Script File";
            mnuCombinedFile.Click += new System.EventHandler(mnuCombinedFile_Click);
            // 
            // menuItem22
            // 
            menuItem22.MergeIndex = 2;
            menuItem22.Name = "menuItem22";
            menuItem22.Size = new System.Drawing.Size(244, 6);
            // 
            // mnuIncludeUSE
            // 
            mnuIncludeUSE.Checked = true;
            mnuIncludeUSE.CheckOnClick = true;
            mnuIncludeUSE.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuIncludeUSE.MergeIndex = 3;
            mnuIncludeUSE.Name = "mnuIncludeUSE";
            mnuIncludeUSE.Size = new System.Drawing.Size(247, 22);
            mnuIncludeUSE.Text = "Include \"USE\" Statements";
            // 
            // mnuIncludeSequence
            // 
            mnuIncludeSequence.Checked = true;
            mnuIncludeSequence.CheckOnClick = true;
            mnuIncludeSequence.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuIncludeSequence.MergeIndex = 4;
            mnuIncludeSequence.Name = "mnuIncludeSequence";
            mnuIncludeSequence.Size = new System.Drawing.Size(247, 22);
            mnuIncludeSequence.Text = "Include Sequence Number Prefix";
            // 
            // menuItem15
            // 
            menuItem15.MergeIndex = 7;
            menuItem15.Name = "menuItem15";
            menuItem15.Size = new System.Drawing.Size(342, 6);
            // 
            // startConfigureMultiServerDatabaseRunToolStripMenuItem
            // 
            startConfigureMultiServerDatabaseRunToolStripMenuItem.Name = "startConfigureMultiServerDatabaseRunToolStripMenuItem";
            startConfigureMultiServerDatabaseRunToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            startConfigureMultiServerDatabaseRunToolStripMenuItem.Text = "Configure Multi Server/Database Run";
            startConfigureMultiServerDatabaseRunToolStripMenuItem.Click += new System.EventHandler(startConfigureMultiServerDatabaseRunToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(342, 6);
            // 
            // mnuFileMRU
            // 
            mnuFileMRU.MergeIndex = 8;
            mnuFileMRU.Name = "mnuFileMRU";
            mnuFileMRU.Size = new System.Drawing.Size(345, 22);
            mnuFileMRU.Text = "Recent Files";
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new System.Drawing.Size(342, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
            // 
            // mnuListTop
            // 
            mnuListTop.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuFindScript,
            menuItem9,
            menuItem8,
            mnuRenumberSequence,
            mnuResortByFileType,
            mnuExportBuildList,
            menuItem10,
            mnuBulkAdd,
            mnuBulkFromList,
            mnuBulkFromFile,
            menuItem13,
            mnuClearPreviouslyRunBlocks,
            toolStripSeparator18,
            inferScriptTagFromFileNameToolStripMenuItem});
            mnuListTop.Enabled = false;
            mnuListTop.MergeIndex = 1;
            mnuListTop.Name = "mnuListTop";
            mnuListTop.Size = new System.Drawing.Size(37, 20);
            mnuListTop.Text = "&List";
            mnuListTop.DropDownOpening += new System.EventHandler(mnuListTop_DropDownOpening);
            // 
            // mnuFindScript
            // 
            mnuFindScript.MergeIndex = 0;
            mnuFindScript.Name = "mnuFindScript";
            mnuFindScript.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            mnuFindScript.Size = new System.Drawing.Size(327, 22);
            mnuFindScript.Text = "&Find Script by Name";
            mnuFindScript.Click += new System.EventHandler(mnuFindScript_Click);
            // 
            // menuItem9
            // 
            menuItem9.MergeIndex = 1;
            menuItem9.Name = "menuItem9";
            menuItem9.ShortcutKeys = System.Windows.Forms.Keys.F3;
            menuItem9.Size = new System.Drawing.Size(327, 22);
            menuItem9.Text = "Find &Again";
            menuItem9.Click += new System.EventHandler(menuItem9_Click);
            // 
            // menuItem8
            // 
            menuItem8.MergeIndex = 2;
            menuItem8.Name = "menuItem8";
            menuItem8.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuRenumberSequence
            // 
            mnuRenumberSequence.MergeIndex = 3;
            mnuRenumberSequence.Name = "mnuRenumberSequence";
            mnuRenumberSequence.Size = new System.Drawing.Size(327, 22);
            mnuRenumberSequence.Text = "&Re-number Build Sequence";
            mnuRenumberSequence.Click += new System.EventHandler(mnuRenumberSequence_Click);
            // 
            // mnuResortByFileType
            // 
            mnuResortByFileType.MergeIndex = 4;
            mnuResortByFileType.Name = "mnuResortByFileType";
            mnuResortByFileType.Size = new System.Drawing.Size(327, 22);
            mnuResortByFileType.Text = "Re&sort Build By File Type";
            mnuResortByFileType.Click += new System.EventHandler(mnuResortByFileType_Click);
            // 
            // mnuExportBuildList
            // 
            mnuExportBuildList.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuExportBuildListToFile,
            mnuExportBuildListToClipBoard});
            mnuExportBuildList.Enabled = false;
            mnuExportBuildList.MergeIndex = 5;
            mnuExportBuildList.Name = "mnuExportBuildList";
            mnuExportBuildList.Size = new System.Drawing.Size(327, 22);
            mnuExportBuildList.Text = "&Export Build List for Documentation";
            // 
            // mnuExportBuildListToFile
            // 
            mnuExportBuildListToFile.MergeIndex = 0;
            mnuExportBuildListToFile.Name = "mnuExportBuildListToFile";
            mnuExportBuildListToFile.Size = new System.Drawing.Size(141, 22);
            mnuExportBuildListToFile.Text = "To &File";
            mnuExportBuildListToFile.Click += new System.EventHandler(mnuExportBuildList_Click);
            // 
            // mnuExportBuildListToClipBoard
            // 
            mnuExportBuildListToClipBoard.MergeIndex = 1;
            mnuExportBuildListToClipBoard.Name = "mnuExportBuildListToClipBoard";
            mnuExportBuildListToClipBoard.Size = new System.Drawing.Size(141, 22);
            mnuExportBuildListToClipBoard.Text = "To &Clipboard";
            mnuExportBuildListToClipBoard.Click += new System.EventHandler(mnuExportBuildListToClipBoard_Click);
            // 
            // menuItem10
            // 
            menuItem10.MergeIndex = 5;
            menuItem10.Name = "menuItem10";
            menuItem10.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuBulkAdd
            // 
            mnuBulkAdd.MergeIndex = 6;
            mnuBulkAdd.Name = "mnuBulkAdd";
            mnuBulkAdd.Size = new System.Drawing.Size(327, 22);
            mnuBulkAdd.Text = "&Bulk Add";
            mnuBulkAdd.Click += new System.EventHandler(mnuBulkAdd_Click);
            // 
            // mnuBulkFromList
            // 
            mnuBulkFromList.MergeIndex = 7;
            mnuBulkFromList.Name = "mnuBulkFromList";
            mnuBulkFromList.Size = new System.Drawing.Size(327, 22);
            mnuBulkFromList.Text = "Bulk Add From &List";
            mnuBulkFromList.Click += new System.EventHandler(mnuBulkFromList_Click);
            // 
            // mnuBulkFromFile
            // 
            mnuBulkFromFile.MergeIndex = 8;
            mnuBulkFromFile.Name = "mnuBulkFromFile";
            mnuBulkFromFile.Size = new System.Drawing.Size(327, 22);
            mnuBulkFromFile.Text = "Bulk Add From &Text File";
            mnuBulkFromFile.Click += new System.EventHandler(mnuBulkFromFile_Click);
            // 
            // menuItem13
            // 
            menuItem13.MergeIndex = 9;
            menuItem13.Name = "menuItem13";
            menuItem13.Size = new System.Drawing.Size(324, 6);
            // 
            // mnuClearPreviouslyRunBlocks
            // 
            mnuClearPreviouslyRunBlocks.MergeIndex = 10;
            mnuClearPreviouslyRunBlocks.Name = "mnuClearPreviouslyRunBlocks";
            mnuClearPreviouslyRunBlocks.Size = new System.Drawing.Size(327, 22);
            mnuClearPreviouslyRunBlocks.Text = "&Clear \"Previously Run\" Blocks for Selected Script";
            mnuClearPreviouslyRunBlocks.Click += new System.EventHandler(mnuClearPreviouslyRunBlocks_Click);
            // 
            // toolStripSeparator18
            // 
            toolStripSeparator18.Name = "toolStripSeparator18";
            toolStripSeparator18.Size = new System.Drawing.Size(324, 6);
            // 
            // inferScriptTagFromFileNameToolStripMenuItem
            // 
            inferScriptTagFromFileNameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            scriptContentsFirstThenFileNameToolStripMenuItem,
            fileNameFirstThenSciptContentsToolStripMenuItem,
            toolStripSeparator20,
            scriptContentsOnlyToolStripMenuItem,
            fileNamesOnlyToolStripMenuItem});
            inferScriptTagFromFileNameToolStripMenuItem.Name = "inferScriptTagFromFileNameToolStripMenuItem";
            inferScriptTagFromFileNameToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            inferScriptTagFromFileNameToolStripMenuItem.Text = "Infer Script Tag from...";
            // 
            // scriptContentsFirstThenFileNameToolStripMenuItem
            // 
            scriptContentsFirstThenFileNameToolStripMenuItem.Name = "scriptContentsFirstThenFileNameToolStripMenuItem";
            scriptContentsFirstThenFileNameToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            scriptContentsFirstThenFileNameToolStripMenuItem.Tag = "TextOverName";
            scriptContentsFirstThenFileNameToolStripMenuItem.Text = "Script Contents first, then File Name";
            scriptContentsFirstThenFileNameToolStripMenuItem.ToolTipText = "Scans the script for a tag value in a header comment block. If not found it will " +
    "check the file name.";
            scriptContentsFirstThenFileNameToolStripMenuItem.Click += new System.EventHandler(InferScriptTag_Click);
            // 
            // fileNameFirstThenSciptContentsToolStripMenuItem
            // 
            fileNameFirstThenSciptContentsToolStripMenuItem.Name = "fileNameFirstThenSciptContentsToolStripMenuItem";
            fileNameFirstThenSciptContentsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            fileNameFirstThenSciptContentsToolStripMenuItem.Tag = "NameOverText";
            fileNameFirstThenSciptContentsToolStripMenuItem.Text = "File Name First, then Scipt Contents";
            fileNameFirstThenSciptContentsToolStripMenuItem.ToolTipText = "Checks the file name for a script tag value. If not found, it scans the script fo" +
    "r a tag value in a header comment block.";
            fileNameFirstThenSciptContentsToolStripMenuItem.Click += new System.EventHandler(InferScriptTag_Click);
            // 
            // toolStripSeparator20
            // 
            toolStripSeparator20.Name = "toolStripSeparator20";
            toolStripSeparator20.Size = new System.Drawing.Size(261, 6);
            // 
            // scriptContentsOnlyToolStripMenuItem
            // 
            scriptContentsOnlyToolStripMenuItem.Name = "scriptContentsOnlyToolStripMenuItem";
            scriptContentsOnlyToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            scriptContentsOnlyToolStripMenuItem.Tag = "ScriptText";
            scriptContentsOnlyToolStripMenuItem.Text = "Script Contents Only";
            scriptContentsOnlyToolStripMenuItem.ToolTipText = "Scans the script for a tag value in a header comment block.";
            scriptContentsOnlyToolStripMenuItem.Click += new System.EventHandler(InferScriptTag_Click);
            // 
            // fileNamesOnlyToolStripMenuItem
            // 
            fileNamesOnlyToolStripMenuItem.Name = "fileNamesOnlyToolStripMenuItem";
            fileNamesOnlyToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            fileNamesOnlyToolStripMenuItem.Tag = "ScriptName";
            fileNamesOnlyToolStripMenuItem.Text = "File Names Only";
            fileNamesOnlyToolStripMenuItem.ToolTipText = "Checks the file name for a script tag value. ";
            fileNamesOnlyToolStripMenuItem.Click += new System.EventHandler(InferScriptTag_Click);
            // 
            // mnuScripting
            // 
            mnuScripting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuDDActiveDatabase,
            mnuAddObjectCreate,
            scriptingOptionsToolStripMenuItem,
            scriptALTERAndCREATEToolStripMenuItem,
            includeObjectPermissionsToolStripMenuItem1,
            toolStripSeparator3,
            mnuAddCodeTablePop,
            menuItem3,
            mnuUpdatePopulate,
            mnuObjectUpdates,
            toolStripSeparator19,
            createBackoutPackageToolStripMenuItem});
            mnuScripting.Enabled = false;
            mnuScripting.MergeIndex = 2;
            mnuScripting.Name = "mnuScripting";
            mnuScripting.Size = new System.Drawing.Size(66, 20);
            mnuScripting.Text = "Scripting";
            // 
            // mnuDDActiveDatabase
            // 
            mnuDDActiveDatabase.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            mnuDDActiveDatabase.Name = "mnuDDActiveDatabase";
            mnuDDActiveDatabase.Size = new System.Drawing.Size(200, 22);
            mnuDDActiveDatabase.Text = "<< Select Active Database >>";
            mnuDDActiveDatabase.ToolTipText = "Select the database to target for scripting";
            mnuDDActiveDatabase.SelectedIndexChanged += new System.EventHandler(mnuDDActiveDatabase_SelectedIndexChanged);
            // 
            // mnuAddObjectCreate
            // 
            mnuAddObjectCreate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuAddStoredProcs,
            mnuAddFunctions,
            mnuAddViews,
            mnuAddTables,
            mnuAddTriggers,
            toolStripSeparator21,
            mnuAddRoles});
            mnuAddObjectCreate.Enabled = false;
            mnuAddObjectCreate.MergeIndex = 1;
            mnuAddObjectCreate.Name = "mnuAddObjectCreate";
            mnuAddObjectCreate.Size = new System.Drawing.Size(261, 22);
            mnuAddObjectCreate.Text = "Add Object Create Scripts";
            // 
            // mnuAddStoredProcs
            // 
            mnuAddStoredProcs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuAddStoredProcs.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            mnuAddStoredProcs.MergeIndex = 0;
            mnuAddStoredProcs.Name = "mnuAddStoredProcs";
            mnuAddStoredProcs.Size = new System.Drawing.Size(170, 22);
            mnuAddStoredProcs.Text = "Stored Procedures";
            mnuAddStoredProcs.Click += new System.EventHandler(mnuAddStoredProcs_Click);
            // 
            // mnuAddFunctions
            // 
            mnuAddFunctions.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuAddFunctions.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            mnuAddFunctions.MergeIndex = 1;
            mnuAddFunctions.Name = "mnuAddFunctions";
            mnuAddFunctions.Size = new System.Drawing.Size(170, 22);
            mnuAddFunctions.Text = "Functions";
            mnuAddFunctions.Click += new System.EventHandler(mnuAddFunctions_Click);
            // 
            // mnuAddViews
            // 
            mnuAddViews.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuAddViews.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            mnuAddViews.MergeIndex = 2;
            mnuAddViews.Name = "mnuAddViews";
            mnuAddViews.Size = new System.Drawing.Size(170, 22);
            mnuAddViews.Text = "Views";
            mnuAddViews.Click += new System.EventHandler(mnuAddViews_Click);
            // 
            // mnuAddTables
            // 
            mnuAddTables.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuAddTables.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(224)))));
            mnuAddTables.MergeIndex = 3;
            mnuAddTables.Name = "mnuAddTables";
            mnuAddTables.Size = new System.Drawing.Size(170, 22);
            mnuAddTables.Text = "Tables";
            mnuAddTables.Click += new System.EventHandler(mnuAddTables_Click);
            // 
            // mnuAddTriggers
            // 
            mnuAddTriggers.Name = "mnuAddTriggers";
            mnuAddTriggers.Size = new System.Drawing.Size(170, 22);
            mnuAddTriggers.Text = "Triggers";
            mnuAddTriggers.Click += new System.EventHandler(mnuAddTriggers_Click);
            // 
            // toolStripSeparator21
            // 
            toolStripSeparator21.Name = "toolStripSeparator21";
            toolStripSeparator21.Size = new System.Drawing.Size(167, 6);
            toolStripSeparator21.Visible = false;
            // 
            // mnuAddRoles
            // 
            mnuAddRoles.Name = "mnuAddRoles";
            mnuAddRoles.Size = new System.Drawing.Size(170, 22);
            mnuAddRoles.Text = "Roles";
            mnuAddRoles.Visible = false;
            mnuAddRoles.Click += new System.EventHandler(mnuAddRoles_Click);
            // 
            // scriptingOptionsToolStripMenuItem
            // 
            scriptingOptionsToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            scriptingOptionsToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            scriptingOptionsToolStripMenuItem.Name = "scriptingOptionsToolStripMenuItem";
            scriptingOptionsToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            scriptingOptionsToolStripMenuItem.Text = "Scripting Options";
            // 
            // scriptALTERAndCREATEToolStripMenuItem
            // 
            scriptALTERAndCREATEToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            scriptALTERAndCREATEToolStripMenuItem.CheckOnClick = true;
            scriptALTERAndCREATEToolStripMenuItem.Name = "scriptALTERAndCREATEToolStripMenuItem";
            scriptALTERAndCREATEToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            scriptALTERAndCREATEToolStripMenuItem.Text = "Script Object ALTER and CREATE";
            scriptALTERAndCREATEToolStripMenuItem.Click += new System.EventHandler(scriptALTERVsCREATEToolStripMenuItem_Click);
            // 
            // includeObjectPermissionsToolStripMenuItem1
            // 
            includeObjectPermissionsToolStripMenuItem1.BackColor = System.Drawing.SystemColors.Control;
            includeObjectPermissionsToolStripMenuItem1.CheckOnClick = true;
            includeObjectPermissionsToolStripMenuItem1.Name = "includeObjectPermissionsToolStripMenuItem1";
            includeObjectPermissionsToolStripMenuItem1.Size = new System.Drawing.Size(261, 22);
            includeObjectPermissionsToolStripMenuItem1.Text = "Include Object Permissions";
            includeObjectPermissionsToolStripMenuItem1.Click += new System.EventHandler(includeObjectPermissionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.BackColor = System.Drawing.SystemColors.Control;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuAddCodeTablePop
            // 
            mnuAddCodeTablePop.MergeIndex = 2;
            mnuAddCodeTablePop.Name = "mnuAddCodeTablePop";
            mnuAddCodeTablePop.Size = new System.Drawing.Size(261, 22);
            mnuAddCodeTablePop.Text = "Add Code Table Populate Scripts";
            mnuAddCodeTablePop.Click += new System.EventHandler(mnuAddCodeTablePop_Click);
            // 
            // menuItem3
            // 
            menuItem3.MergeIndex = 3;
            menuItem3.Name = "menuItem3";
            menuItem3.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuUpdatePopulate
            // 
            mnuUpdatePopulate.MergeIndex = 4;
            mnuUpdatePopulate.Name = "mnuUpdatePopulate";
            mnuUpdatePopulate.Size = new System.Drawing.Size(261, 22);
            mnuUpdatePopulate.Text = "Update Code Table Populate Scripts";
            mnuUpdatePopulate.Click += new System.EventHandler(mnuUpdatePopulate_Click);
            // 
            // mnuObjectUpdates
            // 
            mnuObjectUpdates.MergeIndex = 5;
            mnuObjectUpdates.Name = "mnuObjectUpdates";
            mnuObjectUpdates.Size = new System.Drawing.Size(261, 22);
            mnuObjectUpdates.Text = "Update Object Create Scripts";
            mnuObjectUpdates.Click += new System.EventHandler(mnuObjectUpdates_Click);
            // 
            // toolStripSeparator19
            // 
            toolStripSeparator19.Name = "toolStripSeparator19";
            toolStripSeparator19.Size = new System.Drawing.Size(258, 6);
            // 
            // createBackoutPackageToolStripMenuItem
            // 
            createBackoutPackageToolStripMenuItem.Name = "createBackoutPackageToolStripMenuItem";
            createBackoutPackageToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            createBackoutPackageToolStripMenuItem.Text = "Create back out package";
            createBackoutPackageToolStripMenuItem.Click += new System.EventHandler(createBackoutPackageToolStripMenuItem_Click);
            // 
            // mnuLogging
            // 
            mnuLogging.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuShowBuildLogs,
            mnuScriptToLogFile,
            toolStripSeparator2,
            mnuShowBuildHistory,
            archiveBuildHistoryToolStripMenuItem});
            mnuLogging.Enabled = false;
            mnuLogging.MergeIndex = 3;
            mnuLogging.Name = "mnuLogging";
            mnuLogging.Size = new System.Drawing.Size(63, 20);
            mnuLogging.Text = "Lo&gging";
            // 
            // mnuShowBuildLogs
            // 
            mnuShowBuildLogs.MergeIndex = 0;
            mnuShowBuildLogs.Name = "mnuShowBuildLogs";
            mnuShowBuildLogs.Size = new System.Drawing.Size(192, 22);
            mnuShowBuildLogs.Text = "&Show Build Logs";
            mnuShowBuildLogs.Click += new System.EventHandler(mnuShowBuildLogs_Click);
            // 
            // mnuScriptToLogFile
            // 
            mnuScriptToLogFile.MergeIndex = 1;
            mnuScriptToLogFile.Name = "mnuScriptToLogFile";
            mnuScriptToLogFile.Size = new System.Drawing.Size(192, 22);
            mnuScriptToLogFile.Text = "Script Build to Log &File";
            mnuScriptToLogFile.Click += new System.EventHandler(mnuScriptToLogFile_Click);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(189, 6);
            // 
            // mnuShowBuildHistory
            // 
            mnuShowBuildHistory.MergeIndex = 3;
            mnuShowBuildHistory.Name = "mnuShowBuildHistory";
            mnuShowBuildHistory.Size = new System.Drawing.Size(192, 22);
            mnuShowBuildHistory.Text = "Show Build &History";
            mnuShowBuildHistory.Click += new System.EventHandler(mnuShowBuildHistory_Click);
            // 
            // archiveBuildHistoryToolStripMenuItem
            // 
            archiveBuildHistoryToolStripMenuItem.Name = "archiveBuildHistoryToolStripMenuItem";
            archiveBuildHistoryToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            archiveBuildHistoryToolStripMenuItem.Text = "Archive Build History";
            archiveBuildHistoryToolStripMenuItem.Click += new System.EventHandler(archiveBuildHistoryToolStripMenuItem_Click);
            // 
            // menuItem16
            // 
            menuItem16.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuObjectValidation,
            storedProcedureTestingToolStripMenuItem,
            menuItem21,
            mnuSchemaScripting,
            mnuCodeTableScripting,
            menuItem18,
            mnuDataAuditScripting,
            mnuDataExtraction,
            createToolStripMenuItem,
            toolStripSeparator8,
            mnuDatabaseSize,
            menuItem19,
            mnuAutoScripting,
            menuItem11,
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem,
            toolStripSeparator14,
            scriptPolicyCheckingToolStripMenuItem,
            calculateScriptPackageHashSignatureToolStripMenuItem});
            menuItem16.MergeIndex = 4;
            menuItem16.Name = "menuItem16";
            menuItem16.Size = new System.Drawing.Size(46, 20);
            menuItem16.Text = "Tools";
            // 
            // mnuObjectValidation
            // 
            mnuObjectValidation.MergeIndex = 0;
            mnuObjectValidation.Name = "mnuObjectValidation";
            mnuObjectValidation.Size = new System.Drawing.Size(286, 22);
            mnuObjectValidation.Text = "Database Object Validation";
            mnuObjectValidation.Click += new System.EventHandler(mnuObjectValidation_Click);
            // 
            // storedProcedureTestingToolStripMenuItem
            // 
            storedProcedureTestingToolStripMenuItem.Name = "storedProcedureTestingToolStripMenuItem";
            storedProcedureTestingToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            storedProcedureTestingToolStripMenuItem.Text = "Stored Procedure Testing";
            storedProcedureTestingToolStripMenuItem.Click += new System.EventHandler(storedProcedureTestingToolStripMenuItem_Click);
            // 
            // menuItem21
            // 
            menuItem21.MergeIndex = 1;
            menuItem21.Name = "menuItem21";
            menuItem21.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuSchemaScripting
            // 
            mnuSchemaScripting.MergeIndex = 2;
            mnuSchemaScripting.Name = "mnuSchemaScripting";
            mnuSchemaScripting.Size = new System.Drawing.Size(286, 22);
            mnuSchemaScripting.Text = "Database Schema Scripting";
            mnuSchemaScripting.Click += new System.EventHandler(mnuSchemaScripting_Click);
            // 
            // mnuCodeTableScripting
            // 
            mnuCodeTableScripting.MergeIndex = 3;
            mnuCodeTableScripting.Name = "mnuCodeTableScripting";
            mnuCodeTableScripting.Size = new System.Drawing.Size(286, 22);
            mnuCodeTableScripting.Text = "Code Table Scripting and Auditing";
            mnuCodeTableScripting.Click += new System.EventHandler(mnuCodeTableScripting_Click);
            // 
            // menuItem18
            // 
            menuItem18.MergeIndex = 4;
            menuItem18.Name = "menuItem18";
            menuItem18.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuDataAuditScripting
            // 
            mnuDataAuditScripting.MergeIndex = 5;
            mnuDataAuditScripting.Name = "mnuDataAuditScripting";
            mnuDataAuditScripting.Size = new System.Drawing.Size(286, 22);
            mnuDataAuditScripting.Text = "User Data History and Audit Scripting";
            mnuDataAuditScripting.Click += new System.EventHandler(mnuDataAuditScripting_Click);
            // 
            // mnuDataExtraction
            // 
            mnuDataExtraction.MergeIndex = 6;
            mnuDataExtraction.Name = "mnuDataExtraction";
            mnuDataExtraction.Size = new System.Drawing.Size(286, 22);
            mnuDataExtraction.Text = "Data Extraction";
            mnuDataExtraction.Click += new System.EventHandler(mnuDataExtraction_Click);
            // 
            // createToolStripMenuItem
            // 
            createToolStripMenuItem.Name = "createToolStripMenuItem";
            createToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            createToolStripMenuItem.Text = "Create Scripts from Extracted Data";
            createToolStripMenuItem.Click += new System.EventHandler(createToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuDatabaseSize
            // 
            mnuDatabaseSize.MergeIndex = 7;
            mnuDatabaseSize.Name = "mnuDatabaseSize";
            mnuDatabaseSize.Size = new System.Drawing.Size(286, 22);
            mnuDatabaseSize.Text = "Database Analysis";
            mnuDatabaseSize.Click += new System.EventHandler(mnuDatabaseSize_Click);
            // 
            // menuItem19
            // 
            menuItem19.MergeIndex = 8;
            menuItem19.Name = "menuItem19";
            menuItem19.Size = new System.Drawing.Size(283, 6);
            // 
            // mnuAutoScripting
            // 
            mnuAutoScripting.MergeIndex = 9;
            mnuAutoScripting.Name = "mnuAutoScripting";
            mnuAutoScripting.Size = new System.Drawing.Size(286, 22);
            mnuAutoScripting.Text = "Auto Scripting ";
            // 
            // menuItem11
            // 
            menuItem11.MergeIndex = 10;
            menuItem11.Name = "menuItem11";
            menuItem11.Size = new System.Drawing.Size(283, 6);
            // 
            // rebuildPreviouslyCommitedBuildFileToolStripMenuItem
            // 
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Name = "rebuildPreviouslyCommitedBuildFileToolStripMenuItem";
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Text = "Rebuild Previously Commited Build File";
            rebuildPreviouslyCommitedBuildFileToolStripMenuItem.Click += new System.EventHandler(rebuildPreviouslyCommitedBuildFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(283, 6);
            // 
            // scriptPolicyCheckingToolStripMenuItem
            // 
            scriptPolicyCheckingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            runPolicyChecksToolStripMenuItem,
            toolStripSeparator9,
            savePolicyResultsInCSVToolStripMenuItem,
            savePolicyResultsAsXMLToolStripMenuItem});
            scriptPolicyCheckingToolStripMenuItem.Name = "scriptPolicyCheckingToolStripMenuItem";
            scriptPolicyCheckingToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            scriptPolicyCheckingToolStripMenuItem.Text = "Script Policy Checking";
            // 
            // runPolicyChecksToolStripMenuItem
            // 
            runPolicyChecksToolStripMenuItem.Name = "runPolicyChecksToolStripMenuItem";
            runPolicyChecksToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            runPolicyChecksToolStripMenuItem.Text = "Run Policy Checks";
            runPolicyChecksToolStripMenuItem.Click += new System.EventHandler(scriptPolicyCheckingToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(211, 6);
            // 
            // savePolicyResultsInCSVToolStripMenuItem
            // 
            savePolicyResultsInCSVToolStripMenuItem.Name = "savePolicyResultsInCSVToolStripMenuItem";
            savePolicyResultsInCSVToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            savePolicyResultsInCSVToolStripMenuItem.Text = "Save Policy Results as CSV";
            savePolicyResultsInCSVToolStripMenuItem.Click += new System.EventHandler(savePolicyResultsInCSVToolStripMenuItem_Click);
            // 
            // savePolicyResultsAsXMLToolStripMenuItem
            // 
            savePolicyResultsAsXMLToolStripMenuItem.Name = "savePolicyResultsAsXMLToolStripMenuItem";
            savePolicyResultsAsXMLToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            savePolicyResultsAsXMLToolStripMenuItem.Text = "Save Policy Results as XML";
            savePolicyResultsAsXMLToolStripMenuItem.Click += new System.EventHandler(savePolicyResultsAsXMLToolStripMenuItem_Click);
            // 
            // calculateScriptPackageHashSignatureToolStripMenuItem
            // 
            calculateScriptPackageHashSignatureToolStripMenuItem.Name = "calculateScriptPackageHashSignatureToolStripMenuItem";
            calculateScriptPackageHashSignatureToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            calculateScriptPackageHashSignatureToolStripMenuItem.Text = "Calculate Script Package Hash Signature";
            calculateScriptPackageHashSignatureToolStripMenuItem.Click += new System.EventHandler(calculateScriptPackageHashSignatureToolStripMenuItem_Click);
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuAbout,
            howToToolStripMenuItem,
            toolStripSeparator17,
            projectSiteToolStripMenuItem,
            viewLogFileMenuItem2,
            setLoggingLevelMenuItem2});
            mnuHelp.MergeIndex = 5;
            mnuHelp.Name = "mnuHelp";
            mnuHelp.Size = new System.Drawing.Size(44, 20);
            mnuHelp.Text = "Help";
            // 
            // mnuAbout
            // 
            mnuAbout.MergeIndex = 0;
            mnuAbout.Name = "mnuAbout";
            mnuAbout.Size = new System.Drawing.Size(221, 22);
            mnuAbout.Text = "About Sql Build Manager";
            mnuAbout.Click += new System.EventHandler(mnuAbout_Click);
            // 
            // howToToolStripMenuItem
            // 
            howToToolStripMenuItem.Name = "howToToolStripMenuItem";
            howToToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            howToToolStripMenuItem.Text = "How Do I?";
            howToToolStripMenuItem.Click += new System.EventHandler(howToToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new System.Drawing.Size(218, 6);
            // 
            // projectSiteToolStripMenuItem
            // 
            projectSiteToolStripMenuItem.Name = "projectSiteToolStripMenuItem";
            projectSiteToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            projectSiteToolStripMenuItem.Text = "www.SqlBuildManager.com";
            projectSiteToolStripMenuItem.Visible = false;
            projectSiteToolStripMenuItem.Click += new System.EventHandler(projectSiteToolStripMenuItem_Click);
            // 
            // viewLogFileMenuItem2
            // 
            viewLogFileMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("viewLogFileMenuItem2.Image")));
            viewLogFileMenuItem2.Name = "viewLogFileMenuItem2";
            viewLogFileMenuItem2.Size = new System.Drawing.Size(221, 22);
            viewLogFileMenuItem2.Text = "View Application Log File";
            // 
            // setLoggingLevelMenuItem2
            // 
            setLoggingLevelMenuItem2.Name = "setLoggingLevelMenuItem2";
            setLoggingLevelMenuItem2.Size = new System.Drawing.Size(221, 22);
            setLoggingLevelMenuItem2.Text = "Set Logging Level";
            // 
            // remoteExecutionServiceToolStripMenuItem
            // 
            remoteExecutionServiceToolStripMenuItem.Name = "remoteExecutionServiceToolStripMenuItem";
            remoteExecutionServiceToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            // 
            // constructCommandLineStringToolStripMenuItem
            // 
            constructCommandLineStringToolStripMenuItem.Name = "constructCommandLineStringToolStripMenuItem";
            constructCommandLineStringToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            constructCommandLineStringToolStripMenuItem.Text = "Construct Command Line String";
            constructCommandLineStringToolStripMenuItem.Click += new System.EventHandler(constructCommandLineStringToolStripMenuItem_Click);
            // 
            // openFileBulkLoad
            // 
            openFileBulkLoad.AddExtension = false;
            openFileBulkLoad.Filter = "All Files (*.*)|*.*";
            openFileBulkLoad.Multiselect = true;
            openFileBulkLoad.Title = "Select Files to Add";
            // 
            // pnlManager
            // 
            pnlManager.Controls.Add(splitContainer1);
            pnlManager.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlManager.Location = new System.Drawing.Point(526, 56);
            pnlManager.Name = "pnlManager";
            pnlManager.Size = new System.Drawing.Size(553, 544);
            pnlManager.TabIndex = 19;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(grpManager);
            splitContainer1.Panel1.Controls.Add(pnlAdvanced);
            splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(grpBuildResults);
            splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3);
            splitContainer1.Size = new System.Drawing.Size(553, 544);
            splitContainer1.SplitterDistance = 188;
            splitContainer1.TabIndex = 17;
            // 
            // pnlAdvanced
            // 
            pnlAdvanced.Controls.Add(lblAdvanced);
            pnlAdvanced.Controls.Add(grpAdvanced);
            pnlAdvanced.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlAdvanced.Location = new System.Drawing.Point(3, 170);
            pnlAdvanced.Name = "pnlAdvanced";
            pnlAdvanced.Size = new System.Drawing.Size(547, 15);
            pnlAdvanced.TabIndex = 17;
            // 
            // lblAdvanced
            // 
            lblAdvanced.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            lblAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
            lblAdvanced.ForeColor = System.Drawing.SystemColors.ControlText;
            lblAdvanced.Location = new System.Drawing.Point(0, 0);
            lblAdvanced.Name = "lblAdvanced";
            lblAdvanced.Size = new System.Drawing.Size(547, 13);
            lblAdvanced.TabIndex = 0;
            lblAdvanced.Text = "Advanced Runtime Settings (use with caution) >>";
            lblAdvanced.Click += new System.EventHandler(lblAdvanced_Click);
            // 
            // grpAdvanced
            // 
            grpAdvanced.Controls.Add(label5);
            grpAdvanced.Controls.Add(ddOverrideLogDatabase);
            grpAdvanced.Controls.Add(chkNotTransactional);
            grpAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            grpAdvanced.Location = new System.Drawing.Point(0, 0);
            grpAdvanced.Name = "grpAdvanced";
            grpAdvanced.Size = new System.Drawing.Size(547, 15);
            grpAdvanced.TabIndex = 16;
            grpAdvanced.TabStop = false;
            // 
            // grpBuildResults
            // 
            grpBuildResults.Controls.Add(lstBuild);
            grpBuildResults.Dock = System.Windows.Forms.DockStyle.Fill;
            grpBuildResults.Enabled = false;
            grpBuildResults.Location = new System.Drawing.Point(3, 3);
            grpBuildResults.Margin = new System.Windows.Forms.Padding(6);
            grpBuildResults.Name = "grpBuildResults";
            grpBuildResults.Size = new System.Drawing.Size(547, 346);
            grpBuildResults.TabIndex = 16;
            grpBuildResults.TabStop = false;
            grpBuildResults.Text = "Build Results";
            // 
            // pnlBuildScripts
            // 
            pnlBuildScripts.Controls.Add(grbBuildScripts);
            pnlBuildScripts.Dock = System.Windows.Forms.DockStyle.Left;
            pnlBuildScripts.Location = new System.Drawing.Point(0, 56);
            pnlBuildScripts.Name = "pnlBuildScripts";
            pnlBuildScripts.Size = new System.Drawing.Size(522, 544);
            pnlBuildScripts.TabIndex = 20;
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(522, 56);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(4, 544);
            splitter1.TabIndex = 21;
            splitter1.TabStop = false;
            // 
            // openFileAutoScript
            // 
            openFileAutoScript.Filter = "AutoScript *.sqlauto|*.sqlauto|All Files *.*|*.*";
            openFileAutoScript.Title = "Add Auto Script Registration";
            // 
            // fdrSaveScripts
            // 
            fdrSaveScripts.Description = "Save Script Files";
            // 
            // saveCombinedScript
            // 
            saveCombinedScript.DefaultExt = "sql";
            saveCombinedScript.Filter = "SQL Files *.sql|*.sql|All Files *.*|*.*";
            saveCombinedScript.Title = "Save Combined Scripts";
            // 
            // bgBuildProcess
            // 
            bgBuildProcess.WorkerReportsProgress = true;
            bgBuildProcess.WorkerSupportsCancellation = true;
            bgBuildProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(bgBuildProcess_DoWork);
            bgBuildProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgBuildProcess_ProgressChanged);
            bgBuildProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgBuildProcess_RunWorkerCompleted);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(150, 150);
            toolStripContainer1.Location = new System.Drawing.Point(8, 8);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new System.Drawing.Size(150, 175);
            toolStripContainer1.TabIndex = 22;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer2
            // 
            // 
            // toolStripContainer2.ContentPanel
            // 
            toolStripContainer2.ContentPanel.AutoScroll = true;
            toolStripContainer2.ContentPanel.Controls.Add(pnlManager);
            toolStripContainer2.ContentPanel.Controls.Add(splitter1);
            toolStripContainer2.ContentPanel.Controls.Add(pnlBuildScripts);
            toolStripContainer2.ContentPanel.Controls.Add(settingsControl1);
            toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(1079, 600);
            toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer2.Location = new System.Drawing.Point(0, 0);
            toolStripContainer2.Name = "toolStripContainer2";
            toolStripContainer2.Size = new System.Drawing.Size(1079, 624);
            toolStripContainer2.TabIndex = 23;
            toolStripContainer2.Text = "toolStripContainer2";
            // 
            // toolStripContainer2.TopToolStripPanel
            // 
            toolStripContainer2.TopToolStripPanel.Controls.Add(mainMenu1);
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            settingsControl1.Location = new System.Drawing.Point(0, 0);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "(select / create project)";
            settingsControl1.ProjectLabelText = "Project File:";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(1079, 56);
            settingsControl1.TabIndex = 17;
            settingsControl1.Click += new System.EventHandler(settingsControl1_Click);
            settingsControl1.DoubleClick += new System.EventHandler(settingsControl1_DoubleClick);
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            // 
            // bgCheckForUpdates
            // 
            bgCheckForUpdates.DoWork += new System.ComponentModel.DoWorkEventHandler(bgCheckForUpdates_DoWork);
            bgCheckForUpdates.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgCheckForUpdates_RunWorkerCompleted);
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statGeneral,
            statScriptCount,
            statBuildTime,
            statScriptTime,
            progressBuild});
            statusStrip1.Location = new System.Drawing.Point(0, 624);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1079, 24);
            statusStrip1.TabIndex = 24;
            statusStrip1.Text = "Build Duration: ";
            // 
            // statGeneral
            // 
            statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statGeneral.Name = "statGeneral";
            statGeneral.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            statGeneral.Size = new System.Drawing.Size(463, 19);
            statGeneral.Spring = true;
            statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statScriptCount
            // 
            statScriptCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statScriptCount.Name = "statScriptCount";
            statScriptCount.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            statScriptCount.Size = new System.Drawing.Size(99, 19);
            statScriptCount.Text = "Script Count: 0";
            statScriptCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statBuildTime
            // 
            statBuildTime.AutoSize = false;
            statBuildTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statBuildTime.Name = "statBuildTime";
            statBuildTime.Size = new System.Drawing.Size(175, 19);
            statBuildTime.Text = "Build Duration: ";
            statBuildTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statScriptTime
            // 
            statScriptTime.AutoSize = false;
            statScriptTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statScriptTime.Name = "statScriptTime";
            statScriptTime.Padding = new System.Windows.Forms.Padding(0, 0, 100, 0);
            statScriptTime.Size = new System.Drawing.Size(175, 19);
            statScriptTime.Text = "Script Duration (sec):";
            statScriptTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBuild
            // 
            progressBuild.Name = "progressBuild";
            progressBuild.Size = new System.Drawing.Size(150, 18);
            // 
            // tmrBuild
            // 
            tmrBuild.Interval = 1000;
            tmrBuild.Tick += new System.EventHandler(tmrBuild_Tick);
            // 
            // tmrScript
            // 
            tmrScript.Interval = 1000;
            tmrScript.Tick += new System.EventHandler(tmrScript_Tick);
            // 
            // bgLoadZipFle
            // 
            bgLoadZipFle.WorkerReportsProgress = true;
            bgLoadZipFle.DoWork += new System.ComponentModel.DoWorkEventHandler(bgLoadZipFle_DoWork);
            bgLoadZipFle.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgLoadZipFle_RunWorkerCompleted);
            // 
            // bgRefreshScriptList
            // 
            bgRefreshScriptList.WorkerReportsProgress = true;
            bgRefreshScriptList.DoWork += new System.ComponentModel.DoWorkEventHandler(bgRefreshScriptList_DoWork);
            bgRefreshScriptList.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgRefreshScriptList_ProgressChanged);
            bgRefreshScriptList.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgRefreshScriptList_RunWorkerCompleted);
            // 
            // bgObjectScripting
            // 
            bgObjectScripting.WorkerReportsProgress = true;
            bgObjectScripting.DoWork += new System.ComponentModel.DoWorkEventHandler(bgObjectScripting_DoWork);
            bgObjectScripting.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgObjectScripting_ProgressChanged);
            bgObjectScripting.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgObjectScripting_RunWorkerCompleted);
            // 
            // bgGetObjectList
            // 
            bgGetObjectList.DoWork += new System.ComponentModel.DoWorkEventHandler(bgGetObjectList_DoWork);
            bgGetObjectList.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgGetObjectList_RunWorkerCompleted);
            // 
            // openFileDataExtract
            // 
            openFileDataExtract.Filter = "Data Extract *.data|*.data|All Files *.*|*.*";
            openFileDataExtract.RestoreDirectory = true;
            openFileDataExtract.Title = "Open Data Extract File";
            // 
            // openSbxFileDialog
            // 
            openSbxFileDialog.CheckFileExists = false;
            openSbxFileDialog.DefaultExt = "xml";
            openSbxFileDialog.Filter = "Sql Build Manager Control File (*.sbx)|*.sbx|Xml Files (*.xml)|*.xml|All Files|*." +
    "*";
            openSbxFileDialog.Title = "Open or Create New Sql Build Manager Control File";
            // 
            // saveScriptsToPackage
            // 
            saveScriptsToPackage.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
    "iles (*.zip)|*.zip|All Files|*.*";
            saveScriptsToPackage.Title = "Save scripts to build package";
            // 
            // bgEnterpriseSettings
            // 
            bgEnterpriseSettings.DoWork += new System.ComponentModel.DoWorkEventHandler(bgEnterpriseSettings_DoWork);
            bgEnterpriseSettings.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgEnterpriseSettings_RunWorkerCompleted);
            // 
            // bgBulkAdd
            // 
            bgBulkAdd.WorkerReportsProgress = true;
            bgBulkAdd.DoWork += new System.ComponentModel.DoWorkEventHandler(bgBulkAdd_DoWork);
            bgBulkAdd.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgBulkAdd_ProgressChanged);
            bgBulkAdd.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgBulkAdd_RunWorkerCompleted);
            // 
            // bgPolicyCheck
            // 
            bgPolicyCheck.WorkerReportsProgress = true;
            bgPolicyCheck.WorkerSupportsCancellation = true;
            bgPolicyCheck.DoWork += new System.ComponentModel.DoWorkEventHandler(bgPolicyCheck_DoWork);
            bgPolicyCheck.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgPolicyCheck_ProgressChanged);
            bgPolicyCheck.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgPolicyCheck_RunWorkerCompleted);
            // 
            // savePolicyViolationCsv
            // 
            savePolicyViolationCsv.DefaultExt = "csv";
            savePolicyViolationCsv.Filter = "CSV *.csv|*.csv|All Files *.*|*.*";
            savePolicyViolationCsv.Title = "Save Policy Violations as CSV";
            // 
            // bgBulkAddStep2
            // 
            bgBulkAddStep2.WorkerReportsProgress = true;
            bgBulkAddStep2.DoWork += new System.ComponentModel.DoWorkEventHandler(bgBulkAddStep2_DoWork);
            bgBulkAddStep2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgBulkAddStep2_ProgressChanged);
            bgBulkAddStep2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgBulkAddStep2_RunWorkerCompleted);
            // 
            // SqlBuildForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(1079, 648);
            Controls.Add(toolStripContainer2);
            Controls.Add(toolStripContainer1);
            Controls.Add(statusStrip1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ForeColor = System.Drawing.SystemColors.ControlText;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Location = new System.Drawing.Point(1, 1);
            MainMenuStrip = mainMenu1;
            Name = "SqlBuildForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager";
            FormClosing += new System.Windows.Forms.FormClosingEventHandler(SqlBuildForm_FormClosing);
            Load += new System.EventHandler(SqlBuildForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(SqlBuildForm_KeyDown);
            grbBuildScripts.ResumeLayout(false);
            grbBuildScripts.PerformLayout();
            ctxScriptFile.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            grpManager.ResumeLayout(false);
            grpManager.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ctxResults.ResumeLayout(false);
            mainMenu1.ResumeLayout(false);
            mainMenu1.PerformLayout();
            pnlManager.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            pnlAdvanced.ResumeLayout(false);
            grpAdvanced.ResumeLayout(false);
            grpAdvanced.PerformLayout();
            grpBuildResults.ResumeLayout(false);
            pnlBuildScripts.ResumeLayout(false);
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            toolStripContainer2.ContentPanel.ResumeLayout(false);
            toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer2.TopToolStripPanel.PerformLayout();
            toolStripContainer2.ResumeLayout(false);
            toolStripContainer2.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        #region .: Build Project File Methods :.
        internal void LoadSqlBuildProjectFileData(ref SqlSyncBuildData buildData, string projFileName, bool validateSchema)
        {
            bool successfulLoad = SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projFileName, validateSchema);
            if (successfulLoad)
            {
                projectFileName = projFileName;
                if (!settingsControl1.InvokeRequired)
                {
                    if (String.IsNullOrEmpty(buildZipFileName) && !String.IsNullOrEmpty(sbxBuildControlFileName))
                        settingsControl1.Project = projectFileName;
                    else
                        settingsControl1.Project = buildZipFileName;
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
            bool success = SqlBuildFileHelper.ExtractSqlBuildZipFile(zipFileName, ref workingDirectory, ref projectFilePath, ref projectFileName, out result);
            if (success)
            {
                LoadSqlBuildProjectFileData(ref buildData, projectFileName, false);
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
                if (!runningUnattended)
                {
                    MessageBox.Show(e.Result.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statGeneral.Text = "Build File Load Error.";
                }
                else
                {
                    //EventLog.WriteEntry("SqlSync", "Build File Load Error.", EventLogEntryType.Error, 867);
                    log.LogError("Build File Load Error");
                    if (UnattendedProcessingCompleteEvent != null)
                        UnattendedProcessingCompleteEvent(867);
                }
            }
            else
            {
                //All must have loaded ok!
                statGeneral.Text = "Ready.";
                if (String.IsNullOrEmpty(buildZipFileName) && !string.IsNullOrEmpty(sbxBuildControlFileName))
                    settingsControl1.Project = projectFileName;
                else
                    settingsControl1.Project = buildZipFileName;
            }

            //If this was loaded with the "runningUnattended" flag as false (i.e. an interactive session) enable the form's controls
            if (!runningUnattended)
            {
                Cursor = Cursors.Default;
                progressBuild.Style = ProgressBarStyle.Blocks;

                SetUsedDatabases();
                RefreshScriptFileList(true);
                mnuListTop.Enabled = true;
                mnuLogging.Enabled = true;
                mnuScripting.Enabled = true;
                grbBuildScripts.Enabled = true;
                grpManager.Enabled = true;
                grpBuildResults.Enabled = true;


                //If the log file is bloaded, altert the user and suggest that it be archived
                Int64 logSize = SqlBuild.SqlBuildFileHelper.GetTotalLogFilesSize(projectFilePath);
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
                log.LogInformation("Build File Loaded Successfully.");

                //What kind of unattended run-- single or multiDb?
                if (multiDbRunData != null)
                {
                    multiDbRunData.BuildData = buildData;
                    multiDbRunData.ProjectFileName = projectFileName;
                    ProcessMultiDbBuildUnattended();
                }
                else
                {
                    ProcessBuildUnattended();
                }
            }
        }
        #endregion

        private bool PackageExportIntoZip(SqlSyncBuildData exportData, string zipFileName)
        {

            exportData.WriteXml(Path.Combine(projectFilePath, XmlFileNames.ExportFile)); ;

            string[] fileList = new string[exportData.Script.Rows.Count + 1];
            for (int i = 0; i < exportData.Script.Rows.Count; i++)
            {
                fileList[i] = ((SqlSyncBuildData.ScriptRow)exportData.Script.Rows[i]).FileName;
            }
            fileList[exportData.Script.Rows.Count] = XmlFileNames.ExportFile;
            bool val = ZipHelper.CreateZipPackage(fileList, projectFilePath, zipFileName);
            File.Delete(Path.Combine(projectFilePath, XmlFileNames.ExportFile));
            return val;

        }

        private double ImportSqlScriptFile(string fileName, double lastBuildNumber, out string[] addedFileNames)
        {
            double startBuildNumber = lastBuildNumber + 1;
            //bool haveImportedRows = false;
            string tmpDir = System.IO.Path.GetTempPath();
            string workingDir = Path.Combine(tmpDir, @"SqlsyncImport-" + System.Guid.NewGuid().ToString());
            if (Directory.Exists(workingDir) == false)
            {
                Directory.CreateDirectory(workingDir);
            }
            if (File.Exists(fileName))
            {
                if (ZipHelper.UnpackZipPackage(workingDir, fileName, false) == false)
                {
                    addedFileNames = new string[0];
                    return (double)ImportFileStatus.UnableToImport;
                }
                try
                {
                    //Save any outstanding changes
                    buildData.AcceptChanges();
                    bool isValid = true;

                    //Validate that the selected file is a Sql build file
                    string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    SchemaValidator validator = new SchemaValidator();
                    string importFileXml = string.Empty;
                    if (File.Exists(Path.Combine(workingDir, XmlFileNames.ExportFile)))
                    {
                        importFileXml = Path.Combine(workingDir, XmlFileNames.ExportFile);
                    }
                    else if (File.Exists(Path.Combine(workingDir, XmlFileNames.MainProjectFile)))
                    {
                        importFileXml = Path.Combine(workingDir, XmlFileNames.MainProjectFile);
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

                    ImportListForm frmImport = new ImportListForm(importData, workingDir, buildData, new string[0]);
                    if (DialogResult.OK != frmImport.ShowDialog())
                    {
                        addedFileNames = new string[0];
                        return 0;
                    }

                    SqlBuild.SqlBuildFileHelper.ImportSqlScriptFile(ref buildData,
                         importData, workingDir, lastBuildNumber, projectFilePath, projectFileName, buildZipFileName, true, out addedFileNames);

                    if (addedFileNames.Length > 0)
                        return startBuildNumber + 1;

                }
                catch { }



            }
            addedFileNames = new string[0];
            return -1;
        }
        #endregion

        private void UpdateScriptFileDetails(System.Guid scriptID, double buildOrder, string description, bool rollBackScript, bool rollBackBuild, string databaseName, bool stripTransactions, bool allowMultipleRuns, int scriptTimeOut)
        {
            Cursor = Cursors.AppStarting;
            SqlSyncBuildData.ScriptRow row = GetScriptRow(scriptID);
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

                UpdateScriptFileDetails(row);
            }
            Cursor = Cursors.Default;
        }
        private void UpdateScriptFileDetails(SqlSyncBuildData.ScriptRow row)
        {
            row.AcceptChanges();
            row.Table.AcceptChanges();
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);

        }
        public void RenumberBuildSequence()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                SqlBuildFileHelper.RenumberBuildSequence(ref buildData, projectFileName, buildZipFileName);
                LoadSqlBuildProjectFileData(ref buildData, projectFileName, false);
                RefreshScriptFileList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private SqlSyncBuildData.ScriptRow GetScriptRow(System.Guid scriptId)
        {
            DataRow[] rows = buildData.Script.Select("ScriptId ='" + scriptId.ToString() + "'");
            if (rows.Length > 0)
            {
                return (SqlSyncBuildData.ScriptRow)rows[0];
            }
            return null;
        }
        private string BuildExportString()
        {
            if (buildData.Script.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder("Seq #\tScript File Name\tDefault Database\tComments\tTag\r\n");
                DataView view = buildData.Script.DefaultView;
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


        #endregion

        #region .: Script File ListView Refresh Methods :.
        private void RefreshScriptFileList_SingleItem(SqlSyncBuildData.ScriptRow row)
        {
            statGeneral.Text = "Saving Changes. Updating Script List.";
            System.Threading.Thread.Sleep(50); // Give it time to update.

            try
            {
                var item = (from s in lstScriptFiles.Items.Cast<ListViewItem>()
                            where ((SqlSyncBuildData.ScriptRow)s.Tag).ScriptId == row.ScriptId
                            select s).ToList();

                if (item.Count > 0)
                {
                    ListViewItem firstItem = item[0];
                    SqlBuildHelper helper = new SqlBuildHelper(connData, createSqlRunLogFile, externalScriptLogFileName, true);
                    RefreshScriptFileList_SingleItem(row, ref firstItem, ref helper, true);
                    firstItem.EnsureVisible();
                    statGeneral.Text = "Changes Saved.";

                    if (runPolicyCheckingOnLoad)
                        SetPolicyCheckStatusIcons();

                    //SetCodeReviewIcon(row);
                }
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to refresh list view for {row.FileName}");
                statGeneral.Text = "Changes Saved, but unable to update script display. See error log.";
            }



        }
        private void RefreshScriptFileList_SingleItem(SqlSyncBuildData.ScriptRow row, ref ListViewItem item, ref SqlBuildHelper helper, bool updateIcon)
        {

            long fileSize = 0;


            try
            {
                fileSize = new FileInfo(Path.Combine(projectFilePath, row.FileName)).Length;
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
                ScriptStatusType status = StatusHelper.DetermineScriptRunStatus(row, connData, projectFilePath, chkScriptChanges.Checked, OverrideData.TargetDatabaseOverrides, out commitDate, out serverChangeDate);
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

            if (File.Exists(projectFilePath + row.FileName) && (File.GetAttributes(projectFilePath + row.FileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
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
                scriptContents = File.ReadAllText(Path.Combine(projectFilePath, row.FileName));
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Unable to read file at {Path.Combine(projectFilePath, row.FileName)} for list refresh");
            }

            //Determine if this script requires a build message...  
            if (SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptContents))
            {
                if (!scriptsRequiringBuildDescription.Contains(row.ScriptId))
                {
                    scriptsRequiringBuildDescription.Add(row.ScriptId);
                }
            }
            else
            {
                scriptsRequiringBuildDescription.Remove(row.ScriptId);
            }

        }
        private void RefreshScriptFileList()
        {
            RefreshScriptFileList(false);
        }
        private void RefreshScriptFileList(bool runPolicyChecks)
        {
            RefreshScriptFileList(-1, runPolicyChecks);
        }
        private void RefreshScriptFileList(double selectItemIndex, bool runPolicyChecks)
        {
            ListRefreshSettings s = new ListRefreshSettings()
            {
                SelectedItemIndex = selectItemIndex,
                RunPolicyChecks = runPolicyChecks
            };

            if (buildData != null)
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

            if (buildData == null)
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
                SqlBuildHelper helper = new SqlBuildHelper(connData, createSqlRunLogFile, externalScriptLogFileName, true);
                DataView view = buildData.Script.DefaultView;
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
                log.LogError(exe, "List refresh error");
            }

        }

        private void bgRefreshScriptList_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
            {
                lstScriptFiles.Items.Clear();
                progressBuild.Style = ProgressBarStyle.Marquee;
                Cursor = Cursors.AppStarting;
                statGeneral.Text = "Refreshing script list...";
                grbBuildScripts.Enabled = false;

            }
            else
            {
                statGeneral.Text = e.UserState.ToString();
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
                // SetDefaultCodeReviewIcons();
                if (runPolicyCheckingOnLoad && s.RunPolicyChecks && !bgPolicyCheck.IsBusy)
                {
                    bgPolicyCheck.RunWorkerAsync(s);
                }


            }
            else if (!e.Cancelled)
            {
                MessageBox.Show("Error refeshing script list. See Event log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (lstScriptFiles.SelectedItems.Count > 0)
                lstScriptFiles.SelectedItems[0].EnsureVisible();

            progressBuild.Style = ProgressBarStyle.Blocks;
            Cursor = Cursors.Default;
            statGeneral.Text = "List Refresh Complete.";
            grbBuildScripts.Enabled = true;
            statScriptCount.Text = String.Format("Script Count: {0}", buildData.Script.Rows.Count);

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

            if (buildData == null)
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
                        currentViolations.Clear();
                        bg.ReportProgress(0, "Performing script policy checks...");
                        PolicyHelper.GetPolicies();

                        Parallel.ForEach(buildData.Script, row =>
                        {
                            try
                            {
                                bg.ReportProgress(0, string.Format("Policy checking {0}", row.FileName));
                                string scriptContents = File.ReadAllText(Path.Combine(projectFilePath, row.FileName));
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

                                    string highSev, medSev, reviewWarningSev, lowSev;
                                    highSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.High);
                                    medSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.Medium);
                                    lowSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.Low);
                                    reviewWarningSev = Enum.GetName(typeof(ViolationSeverity), ViolationSeverity.ReviewWarning);

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
                                log.LogError(exe, $"Unable to read file '{Path.Combine(projectFilePath, row.FileName)}' for policy check validation");
                            }
                        });
                    }
                    else
                    {
                        bg.ReportProgress(0, "Performing script policy check...");
                        try
                        {
                            double index = s.SelectedItemIndex;
                            var row = (from sc in buildData.Script
                                       where sc.BuildOrder == index
                                       select sc).First<SqlSyncBuildData.ScriptRow>();

                            string scriptContents = File.ReadAllText(Path.Combine(projectFilePath, row.FileName));
                            violation = policyHelp.ValidateScriptAgainstPolicies(row.FileName, row.ScriptId, scriptContents, row.Database, 80);

                            if (violation == null)
                            {
                                row.PolicyCheckState = ScriptStatusType.PolicyPass;
                            }
                            else
                            {
                                violation.LastChangeDate = (row.DateModified == DateTime.MinValue) ? row.DateAdded.ToString() : row.DateModified.ToString();
                                violation.LastChangeUserId = (row.ModifiedBy.Length == 0) ? row.AddedBy : row.ModifiedBy;

                                var vol = (from v in currentViolations
                                           where v.Guid == row.ScriptId
                                           select v);

                                if (vol.Count() == 0)
                                    currentViolations.Add(violation);
                                else
                                {
                                    currentViolations.Remove(vol.First());
                                    currentViolations.Add(violation);

                                }
                                row.PolicyCheckState = ScriptStatusType.PolicyFail;
                            }
                        }
                        catch (Exception exe)
                        {
                            log.LogWarning(exe, $"Unable to read file for single file policy check validation");
                        }

                    }
                }
                else
                {
                    log.LogWarning($"Unable to run policy check. DoWorkEventArgs Argument is not of type ListRefreshSettings");
                }
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, "Error when executing policy check.");
                bg.ReportProgress(0, "Problem running policy checks.");
            }

        }

        private void bgPolicyCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (Cursor != Cursors.AppStarting)
                Cursor = Cursors.AppStarting;

            if (progressBuild.Style != ProgressBarStyle.Marquee)
                progressBuild.Style = ProgressBarStyle.Marquee;

            if (e.UserState is string)
            {
                statGeneral.Text = e.UserState.ToString();
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
            if (!bgCodeReview.IsBusy)
            {
                Cursor = Cursors.Default;
                statGeneral.Text = "Ready.";
                progressBuild.Style = ProgressBarStyle.Blocks;
            }
            else
            {
                statGeneral.Text = "Checking on code review status...";
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
        //private void SetDefaultCodeReviewIcons()
        //{
        //    for (int x = 0; x < lstScriptFiles.Items.Count; x++)
        //    {
        //        OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
        //        lvi.iItem = x;
        //        // Column
        //        lvi.iSubItem = (int)ScriptListIndex.CodeReviewStatusIconColumn;
        //        lvi.mask = OAKListView.LVIF_IMAGE;
        //        // Image index on imagelist
        //        lvi.iImage = (int)ScriptStatusType.CodeReviewStatusWaiting;
        //        OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
        //    }
        //}
        //private void SetCodeReviewIcon(int listViewItemIndex)
        //{
        //    SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.Items[listViewItemIndex].Tag;
        //    SetCodeReviewIcon(row);
        //}
        //private void SetCodeReviewIcon(SqlSyncBuildData.ScriptRow row)
        //{
        //    if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
        //        return;


        //    var item = (from s in this.lstScriptFiles.Items.Cast<ListViewItem>()
        //                where ((SqlSyncBuildData.ScriptRow)s.Tag).ScriptId == row.ScriptId
        //                select s).ToList();

        //    if (item.Count > 0)
        //    {
        //        ListViewItem firstItem = item[0];
        //        OAKListView.LV_ITEM lvi = new OAKListView.LV_ITEM();
        //        lvi.iItem = firstItem.Index;
        //        // Column
        //        lvi.iSubItem = (int)ScriptListIndex.CodeReviewStatusIconColumn;
        //        lvi.mask = OAKListView.LVIF_IMAGE;
        //        // Image index on imagelist
        //        var cr = from c in row.GetCodeReviewRows()
        //                 orderby c.ReviewDate descending 
        //                 select c.ReviewStatus
        //        ;


        //        //Has at least one accepted..
        //        if (!cr.Any())
        //        {
        //            //no reviews yet
        //            lvi.iImage = (int)ScriptStatusType.CodeReviewNotStarted;
        //        }
        //        else if (cr.Contains((short)CodeReviewStatus.Accepted) && cr.First() == (short)CodeReviewStatus.Accepted) //Is the latest one an accepted status?  // && !cr.Contains((short)CodeReviewStatus.Defect) && !cr.Contains((short)CodeReviewStatus.OutOfDate))
        //        {




        //            //Accepted by DBA?
        //            var acceptedReviewers = from crr in row.GetCodeReviewRows()
        //                                    where crr.ReviewStatus == (short)CodeReviewStatus.Accepted
        //                                    select crr.ReviewBy;

        //            if (acceptedReviewers.Any()) //it had better be, but just checking
        //            {

        //                var isDba = from c in row.GetCodeReviewRows()
        //                            join d in this.codeReviewDbaMembers
        //                            on c.ReviewBy equals d
        //                            where c.ReviewStatus == (short)CodeReviewStatus.Accepted
        //                            select d;

        //                if (isDba.Any())
        //                {
        //                    lvi.iImage = (int)ScriptStatusType.CodeReviewAcceptedDba;
        //                }
        //                else
        //                {
        //                    lvi.iImage = (int)ScriptStatusType.CodeReviewAccepted;
        //                }
        //            }
        //            else
        //            {
        //                lvi.iImage = (int)ScriptStatusType.CodeReviewAccepted;
        //            }


        //        }
        //        else
        //        {
        //            lvi.iImage = (int)ScriptStatusType.CodeReviewInProgress;
        //        }
        //        OAKListView.SendMessage(lstScriptFiles.Handle, OAKListView.LVM_SETITEM, 0, ref lvi);
        //    }
        //}
        //private void SetCodeReviewIcons()
        //{
        //    if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
        //        return;

        //    //Find any codereview rows that fail their validation key (i.e.someone changed it in the database or in the file)
        //    var fail = from c in this.buildData.CodeReview
        //               where c.ValidationKey != CodeReviewManager.GetValidationKey(c)
        //               select c;

        //    if (fail.Any())
        //    {
        //        foreach (SqlSyncBuildData.CodeReviewRow r in fail)
        //            r.ReviewStatus = (short)CodeReviewStatus.OutOfDate;
        //    }

        //    for (int x = 0; x < lstScriptFiles.Items.Count; x++)
        //    {
        //        SetCodeReviewIcon(x);
        //    }

        //}

        //private void bgCodeReview_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    BackgroundWorker bg = (BackgroundWorker)sender;
        //    bg.ReportProgress(0);
        //    bool databaseSuccess;
        //   // CodeReviewManager.LoadCodeReviewData(this.buildData, out databaseSuccess);
        //    //CodeReviewManager.ValidateReviewCheckSum(this.buildData, this.projectFilePath);

        //    e.Result = databaseSuccess;
        //}
        //private void bgCodeReview_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    this.statGeneral.Text = "Checking on code review status...";
        //    this.progressBuild.Style = ProgressBarStyle.Marquee;
        //    this.Cursor = Cursors.AppStarting;
        //}
        //private void bgCodeReview_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Result is Boolean && !(bool)e.Result)
        //    {
        //        string text = "Unable to connect to code review database.\r\nResults are from local data only.\r\nUpdates will be sync'd when connection is available";
        //        toolTip2.BackColor = Color.GhostWhite;
        //        this.toolTip2.ToolTipIcon = ToolTipIcon.Warning;
        //        this.toolTip2.Show(text, this.menuStrip1,7000);

        //        codeReviewIconToolStripMenuItem.ToolTipText = text;
        //        codeReviewIconToolStripMenuItem.ForeColor = Color.Red;
        //    }
        //    else
        //    {
        //        codeReviewIconToolStripMenuItem.ToolTipText = "";
        //        codeReviewIconToolStripMenuItem.ForeColor = Color.Blue;
        //        this.toolTip2.Hide(this.menuStrip1);

        //    }
        //    this.SetCodeReviewIcons();

        //    this.Cursor = Cursors.Default;
        //    this.statGeneral.Text = "Ready.";
        //    this.progressBuild.Style = ProgressBarStyle.Blocks;
        //}
        #endregion

        private void RenameUnusedFiles()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                DirectoryInfo inf = new DirectoryInfo(projectFilePath);
                FileInfo[] files = inf.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (buildData.Script.Select("FileName ='" + files[i].Name + "'").Length == 0 &&
                        files[i].Name != Path.GetFileName(projectFileName) &&
                        files[i].Name.StartsWith("zzzz-") == false)
                    {
                        files[i].MoveTo("zzzz-" + files[i].Name);
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }




        private void SetDatabaseMenuList()
        {
            if (databaseList == null || databaseList.Count == 0)
            {
                mnuDDActiveDatabase.Enabled = false;
                mnuDDActiveDatabase.Text = "No Databases Found";
                return;
            }
            databaseList.Sort(new DatabaseListComparer());
            mnuDDActiveDatabase.Items.Clear();
            mnuDDActiveDatabase.Items.Add(selectDatabaseString);
            for (int i = 0; i < databaseList.Count; i++)
                mnuDDActiveDatabase.Items.Add(databaseList[i].DatabaseName);
            mnuDDActiveDatabase.SelectedIndex = 0;

            ddOverrideLogDatabase.Items.Clear();
            ddOverrideLogDatabase.Items.Add(string.Empty);
            for (int i = 0; i < databaseList.Count; i++)
                ddOverrideLogDatabase.Items.Add(databaseList[i].DatabaseName);
            ddOverrideLogDatabase.SelectedIndex = 0;
        }
        private void GenerateUtiltityItems()
        {
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string utilityPath = Path.Combine(executablePath, "Utility");
            string utiltityXmlFile = Path.Combine(utilityPath, "UtilityRegistry.xml");
            if (File.Exists(utiltityXmlFile) == false)
                return;

            using (StreamReader sr = new StreamReader(utiltityXmlFile))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Utility.SqlSyncUtilityRegistry));
                object obj = serializer.Deserialize(sr);
                utilityRegistry = (Utility.SqlSyncUtilityRegistry)obj;
            }
            if (utilityRegistry != null)
            {
                for (int i = 0; i < utilityRegistry.Items.Length; i++)
                {
                    SetUtilityQueryFilePath(ref utilityRegistry.Items[i], utilityPath);
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
                ((Utility.UtilityQuery)utilityRegItem).FileName = Path.Combine(utilityPath, ((Utility.UtilityQuery)utilityRegItem).FileName);
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
            mnuAutoScripting.DropDownItems.Clear();
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            autoXmlFile = Path.Combine(executablePath, "AutoScriptList.xml");
            if (File.Exists(autoXmlFile))
            {
                using (StreamReader sr = new StreamReader(autoXmlFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SqlSync.ObjectScript.AutoScriptList));
                    object obj = serializer.Deserialize(sr);
                    autoScriptListRegistration = (SqlSync.ObjectScript.AutoScriptList)obj;
                }
                if (autoScriptListRegistration != null)
                {


                    for (int i = 0; i < autoScriptListRegistration.Items.Length; i++)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(autoScriptListRegistration.Items[i].File));
                        item.Click += new EventHandler(autoScriptListItem_Click);
                        mnuAutoScripting.DropDownItems.Add(item);
                    }
                }
            }
            ToolStripMenuItem newItem = new ToolStripMenuItem("<< Add New AutoScript >>");
            newItem.Click += new EventHandler(autoScriptAddRegistration_Click);
            mnuAutoScripting.DropDownItems.Add(newItem);

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

            BuildDataHelper.GetLastBuildNumberAndDb(buildData, out lastBuildNumber, out lastDatabase);

            NewBuildScriptForm frmNew = new NewBuildScriptForm(projectFilePath, fullFileName, databaseList, lastBuildNumber, lastDatabase, System.Environment.UserName, tagList);
            DialogResult result2 = frmNew.ShowDialog();
            if (result2 == DialogResult.OK)
            {

                SqlSyncBuildData.ScriptRow cfgRow = buildData.Script.NewScriptRow();
                cfgRow.BuildOrder = frmNew.BuildOrder;
                cfgRow.Description = frmNew.Description;
                cfgRow.RollBackOnError = frmNew.RollBackScript;
                cfgRow.CausesBuildFailure = frmNew.RollBackBuild;
                cfgRow.Database = frmNew.DatabaseName;
                cfgRow.StripTransactionText = frmNew.StripTransactions;
                cfgRow.AllowMultipleRuns = frmNew.AllowMultipleRuns;
                cfgRow.ScriptTimeOut = frmNew.ScriptTimeout;
                cfgRow.Tag = frmNew.ScriptTag;

                ProcessNewScript(fullFileName, cfgRow);
            }

            frmNew.Dispose();
            return true;
        }
        private bool ProcessNewScript(string fullFileName, SqlSyncBuildData.ScriptRow cfgRow)
        {
            string shortFileName = Path.GetFileName(fullFileName);
            bool fileAdded = true;
            //Move the file to the build file folder
            string newLocalFile = Path.Combine(projectFilePath, shortFileName);
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
                        ref buildData,
                        projectFileName,
                        shortFileName,
                        cfgRow.BuildOrder,
                        cfgRow.Description,
                        cfgRow.RollBackOnError,
                        cfgRow.CausesBuildFailure,
                        cfgRow.Database,
                        cfgRow.StripTransactionText,
                        buildZipFileName,
                        true,
                        cfgRow.AllowMultipleRuns,
                        System.Environment.UserName,
                        cfgRow.ScriptTimeOut,
                        cfgRow.Tag
                        );

                //
                //if (this.projectIsUnderSourceControl)
                //{
                //    SourceControlStatus stat = SqlBuildFileHelper.CheckoutFileFromSourceControl(SqlSync.Properties.Settings.Default.SourceControlServerUrl, newLocalFile);
                //    if (stat == SourceControlStatus.Error || stat == SourceControlStatus.NotUnderSourceControl || stat == SourceControlStatus.Unknown)
                //    {
                //        MessageBox.Show("Unable to add file to source control. Please add it manually", "Source control problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    }
                //}

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

            Cursor = Cursors.WaitCursor;
            StringBuilder sb = new StringBuilder("Are you sure you want to remove the follow file(s)?\r\n\r\n"); ;
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
            {
                if (lstScriptFiles.SelectedItems[i].BackColor == colorReadOnlyFile)
                {
                    MessageBox.Show("You have selected one or more Read-only files.\r\nThese can not be deleted until they are marked as writeable.", "Read-only files selected", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Cursor = Cursors.Default;
                    return;
                }
                sb.Append("  " + lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text + "\r\n");
            }


            if (DialogResult.No == MessageBox.Show(sb.ToString(), "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
            {
                Cursor = Cursors.Default;
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
            if (!SqlBuildFileHelper.RemoveScriptFilesFromBuild(ref buildData, projectFileName, buildZipFileName, rows, deleteFiles))
            {
                MessageBox.Show("Unable to remove file from list. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            if (lstScriptFiles.SelectedItems.Count > 0)
            {
                RefreshScriptFileList();
            }
            Cursor = Cursors.Default;
        }

        private void mnuEditFromResults_Click(object sender, System.EventArgs e)
        {
            EditFile(lstBuild);
        }
        private void lstScriptFiles_DoubleClick(object sender, System.EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
            {
                AddNewTextScript();
            }
            else
            {
                EditFile(lstScriptFiles);
            }
        }
        private void mnuEditFile_Click(object sender, System.EventArgs e)
        {
            EditFile(lstScriptFiles);
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

                string fullFileName = Path.Combine(projectFilePath, fileName);
                if (File.Exists(fullFileName))
                {
                    SqlSyncBuildData.ScriptRow cfgRow = GetScriptRow(new Guid(scriptGuid));
                    if (cfgRow == null)
                    {
                        MessageBox.Show("Error: Unable to locate script with ID of " + scriptGuid, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string fileHash;
                    string textHash;
                    SqlBuild.SqlBuildFileHelper.GetSHA1Hash(fullFileName, out fileHash, out textHash, stripTransText);
                    bool allowEdit = true;
                    if ((File.GetAttributes(fullFileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        allowEdit = false;

                    PopulateTagList();

                    //this.buildData.SqlSyncBuildProject[0].ScriptTagRequired

                    AddScriptTextForm frmEdit = new AddScriptTextForm(ref buildData, fileName, fullFileName, utilityRegistry, ref cfgRow, textHash, databaseList, tagList, SqlSync.Properties.Settings.Default.RequireScriptTags, allowEdit);
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
                            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);

                        //Update override Db list if necessary.
                        if (frmEdit.ConfigurationChanged)
                        {
                            SetUsedDatabases();
                            //if (!this.databasesUsed.Contains(cfgRow.Database))
                            //    this.databasesUsed.Add(cfgRow.Database);

                            //this.targetDatabaseOverrideCtrl1.SetDatabaseData(this.databaseList, this.databasesUsed);
                        }

                        PopulateTagList();

                        //Since the file or config may have changed, we'll need to refresh to update the icon.
                        if ((chkScriptChanges.Checked && hasChanges) || frmEdit.ConfigurationChanged)
                        {
                            RefreshScriptFileList_SingleItem(cfgRow);
                            if (!bgPolicyCheck.IsBusy)
                                bgPolicyCheck.RunWorkerAsync(new ListRefreshSettings() { SelectedItemIndex = cfgRow.BuildOrder });
                        }

                        if (frmEdit.BuildSequenceChanged)
                        {
                            listSorter.CurrentColumn = -100;
                            listSorter.Sort = SortOrder.Ascending;
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
                EditFile(lstScriptFiles);
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
                Cursor = Cursors.WaitCursor;
                SqlSyncBuildData.ScriptRow[] rows = new SqlSyncBuildData.ScriptRow[lstScriptFiles.SelectedItems.Count];
                for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                    rows[i] = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;
                Cursor = Cursors.Default;

                NewBuildScriptForm frmNew = new NewBuildScriptForm(ref rows, projectFilePath, databaseList, tagList);
                if (DialogResult.OK == frmNew.ShowDialog())
                {
                    Cursor = Cursors.WaitCursor;
                    buildData.AcceptChanges();
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
                    for (int i = 0; i < rows.Length; i++)
                        RefreshScriptFileList_SingleItem(rows[i]);

                    SetUsedDatabases();
                    //this.RefreshScriptFileList();
                }
            }
            Cursor = Cursors.Default;
        }

        private void mnuRenumberSequence_Click(object sender, System.EventArgs e)
        {
            RenumberBuildSequence();
        }

        private void mnuRenameFiles_Click(object sender, System.EventArgs e)
        {
            RenameUnusedFiles();
        }

        private bool DeleteFilesFromSbx(SqlSyncBuildData.ScriptRow[] rows)
        {
            if (sbxBuildControlFileName == null || sbxBuildControlFileName.Length == 0) //don't delete file system files is this is an SBM file
                return true;

            string fileName = "FileName=\"{0}\"";
            Dictionary<string, List<string>> matches = new Dictionary<string, List<string>>();
            if (projectFileName.EndsWith(".sbx", StringComparison.CurrentCultureIgnoreCase))
            {
                //Check for other SBX files in the same directory and see if they include the file in question...
                string[] sbxFiles = Directory.GetFiles(Path.GetDirectoryName(projectFileName), "*.sbx", SearchOption.TopDirectoryOnly);
                if (sbxFiles.Length == 1)
                    return true;

                //Get list of files that are reused in other SBX files...
                for (int i = 0; i < sbxFiles.Length; i++)
                {
                    if (sbxFiles[i] == projectFileName)
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
                    Dictionary<string, List<string>>.Enumerator enumer = matches.GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        sb.AppendLine(Path.GetFileName(enumer.Current.Key));
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
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                rows[i] = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;

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
                Cursor = Cursors.WaitCursor;
                if (!SqlBuildFileHelper.RemoveScriptFilesFromBuild(ref buildData, projectFileName, buildZipFileName, rows, deleteFiles))
                {
                    MessageBox.Show("Unable to remove file from list. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                RefreshScriptFileList();
            }
            Cursor = Cursors.Default;
        }

        private void mnuAddSqlScriptText_Click(object sender, System.EventArgs e)
        {
            AddNewTextScript();
        }
        private void AddNewTextScript()
        {
            double lastBuildNumber = 0;
            string lastDatabase = string.Empty;

            SqlSyncBuildData.ScriptRow newRow = buildData.Script.NewScriptRow();
            sb.BuildDataHelper.GetLastBuildNumberAndDb(buildData, out lastBuildNumber, out lastDatabase);
            newRow.Database = lastDatabase;
            newRow.BuildOrder = Math.Floor(lastBuildNumber + 1);
            newRow.CausesBuildFailure = true;
            if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout > 0)
                newRow.ScriptTimeOut = SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout;

            // this.buildData.SqlSyncBuildProject[0].ScriptTagRequired
            AddScriptTextForm frmAdd = new AddScriptTextForm(ref buildData, ref newRow, utilityRegistry, databaseList, tagList, SqlSync.Properties.Settings.Default.RequireScriptTags);

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
                SetUsedDatabases();
            }
        }
        private void mnuTryScript_Click(object sender, System.EventArgs e)
        {
            double[] selected = GetSelectedScriptIndexes();
            if (selected.Length > 0)
                RunSingleFiles(selected, true, false);

        }
        private void mnuRunScript_Click(object sender, System.EventArgs e)
        {
            double[] selected = GetSelectedScriptIndexes();
            if (selected.Length > 0)
                RunSingleFiles(selected, false, false);

        }

        private double[] GetSelectedScriptIndexes()
        {

            if (lstScriptFiles.SelectedItems.Count > 0)
            {
                double[] selected = new double[lstScriptFiles.SelectedItems.Count];
                for (int i = 0; i < selected.Length; i++)
                    selected[i] = double.Parse(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.SequenceNumber].Text);

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
            DefaultScriptCopyStatus status = SqlBuildFileHelper.AddDefaultScriptToBuild(ref buildData, defScript, copyAction, projectFileName, buildZipFileName);
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
            Cursor = Cursors.AppStarting;
            sbxBuildControlFileName = fileName;
            mruManager.Add(fileName);
            projectFileName = fileName;
            projectFilePath = Path.GetDirectoryName(fileName) + @"\";
            buildZipFileName = null;
            settingsControl1.Project = fileName;
            if (File.Exists(fileName))
            {

                statGeneral.Text = "Reading Control File.";
                SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, fileName, true);
            }
            else
            {
                statGeneral.Text = "Creating Base Build File.";
                if (buildData != null)
                {
                    buildData.Clear();
                    buildData.AcceptChanges();
                }
                buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                buildData.AcceptChanges();

                DefaultScriptRegistry defReg = SqlBuildFileHelper.GetDefaultScriptRegistry();
                foreach (DefaultScripts.DefaultScript defScript in defReg.Items)
                {
                    AddDefaultScript(defScript, sbxBuildControlFileName, DefaultScriptCopyAction.Undefined);
                }
                buildData.WriteXml(sbxBuildControlFileName);
            }

            Cursor = Cursors.WaitCursor;
            lstScriptFiles.Items.Clear();
            progressBuild.Style = ProgressBarStyle.Marquee;
            grbBuildScripts.Enabled = false;
            grpManager.Enabled = false;
            grpBuildResults.Enabled = false;


            SetUsedDatabases();
            RefreshScriptFileList(true);
            SetUsedDatabases();
            mnuListTop.Enabled = true;
            mnuLogging.Enabled = true;
            mnuScripting.Enabled = true;
            grbBuildScripts.Enabled = true;
            grpManager.Enabled = true;
            grpBuildResults.Enabled = true;

            packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = true;

            Cursor = Cursors.Default;
        }


        private void LoadProject(string fileName)
        {
            Cursor = Cursors.AppStarting;
            if (!PassesReadOnlyProjectFileCheck(fileName))
            {
                Cursor = Cursors.Default;
                return;
            }

            sbxBuildControlFileName = null;
            packageScriptsIntoProjectFilesbmToolStripMenuItem.Enabled = false;

            try
            {

                SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);
                buildZipFileName = fileName;
                mruManager.Add(fileName);
                if (File.Exists(buildZipFileName))
                {
                    statGeneral.Text = "Reading Build File.";
                }
                else
                {
                    statGeneral.Text = "Creating Base Build File.";
                    if (buildData != null)
                    {
                        buildData.Clear();
                        buildData.AcceptChanges();
                    }
                    buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                    buildData.AcceptChanges();
                    SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, projectFilePath, buildZipFileName);

                    DefaultScriptRegistry defReg = SqlBuildFileHelper.GetDefaultScriptRegistry();
                    if (defReg != null && defReg.Items != null)
                    {
                        foreach (DefaultScripts.DefaultScript defScript in defReg.Items)
                        {
                            AddDefaultScript(defScript, Path.Combine(projectFilePath, XmlFileNames.MainProjectFile), DefaultScriptCopyAction.Undefined);
                        }
                    }
                }


                Cursor = Cursors.WaitCursor;
                lstScriptFiles.Items.Clear();
                statGeneral.Text = "Loading Build File...";
                progressBuild.Style = ProgressBarStyle.Marquee;
                grbBuildScripts.Enabled = false;
                grpManager.Enabled = false;
                grpBuildResults.Enabled = false;

                bgLoadZipFle.RunWorkerAsync(buildZipFileName);

            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error loading project file: {fileName}");
                MessageBox.Show("Unable to load project file. The following error has been recorded in the log file: \r\n" + exe.ToString(), "An error has occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                databaseList = frmConnect.DatabaseList;
                settingsControl1.InitServers(true);

                settingsControl1_ServerChanged(sender, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType);
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
                sb.BuildDataHelper.GetLastBuildNumberAndDb(buildData, out lastBuildNumber, out lastDatabase);

                double importStartNumber = ImportSqlScriptFile(fileNames[i], lastBuildNumber, out addedFileNames);
                if (importStartNumber > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Import Successful\r\n");
                    sb.Append("New file indexes start at: " + importStartNumber.ToString() + "\r\n\r\n");
                    RefreshScriptFileList(true);
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
            FindForm frmFind = new FindForm(searchText);
            DialogResult result = frmFind.ShowDialog();
            if (result == DialogResult.OK)
            {
                searchStartIndex = 0;
                searchText = frmFind.ScriptName;
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
                    string buildHistFile = Path.Combine(projectFilePath, XmlFileNames.HistoryFile);
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
                prc.StartInfo.Arguments = Path.Combine(projectFilePath, fileName);
                prc.Start();
            }
        }
        #endregion
        #endregion

        private void lnkStartBuild_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {

            Cursor = Cursors.AppStarting;

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
                connData.ScriptTimeout = 200;
            }
            catch { }

            SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
            runData.BuildData = buildData;
            runData.BuildType = ddBuildType.SelectedItem.ToString();
            runData.BuildDescription = txtBuildDesc.Text;
            runData.StartIndex = Double.Parse(txtStartIndex.Text);
            runData.ProjectFileName = projectFileName;
            runData.IsTrial = isTrial;
            runData.BuildFileName = buildZipFileName;
            runData.IsTransactional = !chkNotTransactional.Checked;
            runData.TargetDatabaseOverrides = targetDatabaseOverrideCtrl1.GetOverrideData();
            if (ddOverrideLogDatabase.SelectedItem != null && ddOverrideLogDatabase.SelectedItem.ToString().Length > 0)
                runData.LogToDatabaseName = ddOverrideLogDatabase.SelectedItem.ToString();


            if (!ConnectionHelper.ValidateDatabaseOverrides(runData.TargetDatabaseOverrides))
            {
                MessageBox.Show("One or more scripts is missing a default or target override database setting.\r\nRun has been halted. Please correct the error and try again", "Missing Database setting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bgBuildProcess.RunWorkerAsync(runData);

        }

        private void txtBuildDesc_TextChanged(object sender, System.EventArgs e)
        {
            if (txtBuildDesc.Text.Length > 0)
            {
                lnkStartBuild.Enabled = true;
                lnkStartBuild.Text = "Start Build on \"" + connData.SQLServerName + "\"";
            }
            else
            {
                lnkStartBuild.Enabled = false;
                lnkStartBuild.Text = "Please Enter a Description";

            }
        }

        #region Bulk Add Method Handlers
        internal void BulkAdd(string[] fileList)
        {
            BulkAdd(fileList, false);
        }
        internal void BulkAdd(string[] fileList, bool deleteOriginals)
        {
            BulkAdd(fileList, deleteOriginals, "");
        }
        internal void BulkAdd(string[] fileList, bool deleteOriginals, string preSelectedDbName)
        {
            double lastBuildNumber;
            string lastDatabase;
            sb.BuildDataHelper.GetLastBuildNumberAndDb(buildData, out lastBuildNumber, out lastDatabase);

            //Get the last whole number since we initially add 1
            lastBuildNumber = Math.Floor(lastBuildNumber);


            BulkAddConfirmation frmConfirm = new BulkAddConfirmation(fileList.ToList(), projectFilePath, buildData);
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
            Cursor = Cursors.AppStarting;
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
            if (!runPolicyCheckingOnLoad) //only need to run here if it's not run later..
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

            NewBuildScriptForm frmNew = new NewBuildScriptForm(projectFilePath, "<bulk>", databaseList, b.LastBuildNumber, lastDatabase, System.Environment.UserName, tagList);
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
                    b.TagInferSource, b.TagInferSourceRegexFormats, b.ScriptTag, b.StripTransactions, b.Description, b.RollBackScript, b.RollBackBuild, b.DatabaseName, b.AllowMultipleRuns);
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
                string newLocalFile = Path.Combine(projectFilePath, Path.GetFileName(fileList[i]));
                bg.ReportProgress(0, "Adding " + Path.GetFileName(fileList[i]));
                if (File.Exists(newLocalFile))
                    fileExists = true;

                try
                {
                    if (!fileList[i].Trim().Equals(newLocalFile.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        File.Copy(fileList[i], newLocalFile, true);
                    }


                    var val = from s in buildData.Script.AsEnumerable().Cast<SqlSyncBuildData.ScriptRow>()
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
                    log.LogError(exe, "Unable to move new file to project temp folder");
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
                        ref buildData,
                        projectFileName,
                        Path.GetFileName(fileList[i]),
                        lastBuildNumber + increment,
                        description,
                        rollBackScript,
                        rollBackBuild,
                        databaseName,
                        stripTran,
                        buildZipFileName,
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
                        log.LogWarning($"Unable to delete temporary file '{fileList[i]}' when trying to add to project temp path\r\n{exe.ToString()}");
                    }
                }

            }


            //Refresh the file list
            bg.ReportProgress(0, "Saving project file with new files");
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);




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
            SetUsedDatabases();
            progressBuild.Style = ProgressBarStyle.Blocks;
            Cursor = Cursors.Default;
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

            for (int j = 0; j < lstScriptFiles.SelectedItems.Count; j++)
            {
                lstScriptFiles.SelectedItems[j].Selected = false;
            }

            for (int i = searchStartIndex; i < lstScriptFiles.Items.Count; i++)
            {
                if (lstScriptFiles.Items[i].SubItems[(int)ScriptListIndex.FileName].Text.ToLower().IndexOf(searchText.ToLower()) > -1)
                {
                    lstScriptFiles.Focus();
                    lstScriptFiles.Items[i].Selected = true;
                    lstScriptFiles.SelectedItems[0].EnsureVisible();
                    searchStartIndex = i + 1;
                    if (searchStartIndex >= lstScriptFiles.Items.Count)
                    {
                        searchStartIndex = 0;
                    }
                    return;
                }
            }
            MessageBox.Show("Reached end of list. Unable to find a matching file", "No Match", MessageBoxButtons.OK, MessageBoxIcon.Information);
            searchStartIndex = 0;
        }

        private void SqlBuildForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3 && searchText.Length > 0)
            {
                SearchScriptFileList();
            }
        }

        private void menuItem9_Click(object sender, System.EventArgs e)
        {
            if (searchText.Length > 0)
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
                for (int i = 0; i < dataList.Count; i++)
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

                        if (!File.Exists(Path.Combine(projectFilePath, value.Key.FileName)))
                        {
                            File.WriteAllText(Path.Combine(projectFilePath, value.Key.FileName), value.Value);
                        }

                        buildData.Script.ImportRow(value.Key);
                    }
                }


                buildData.AcceptChanges();
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);

                RefreshScriptFileList(true);

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
            Cursor = Cursors.WaitCursor;
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
                List<string> ids = (from s in buildData.Script
                                    join r in runItemIndexes on s.BuildOrder equals r
                                    join d in scriptsRequiringBuildDescription on s.ScriptId equals d
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
                        Cursor = Cursors.Default;
                        return;
                    }
                    frmDesc.Dispose();
                }

            }
            catch (Exception exe)
            {
                log.LogWarning(exe, "Unable to determine if build description is required");
            }


            runData.BuildData = buildData;
            runData.BuildType = type;

            runData.StartIndex = Double.Parse(txtStartIndex.Text);
            runData.ProjectFileName = projectFileName;
            runData.IsTrial = isTrial;
            runData.RunItemIndexes = runItemIndexes;
            runData.RunScriptOnly = scriptOnly;
            runData.BuildFileName = buildZipFileName;
            runData.TargetDatabaseOverrides = targetDatabaseOverrideCtrl1.GetOverrideData();
            runData.IsTransactional = true; //for context menu driven runs, always use transactions!!!
            bgBuildProcess.RunWorkerAsync(runData);
        }

        /// <summary>
        /// Executes the run via a non-interactive mode...
        /// </summary>
        private void ProcessBuildUnattended()
        {
            SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData();
            runData.BuildData = buildData;
            runData.BuildType = "Other";
            runData.BuildDescription = "Unattended Build";
            runData.StartIndex = 0;
            runData.ProjectFileName = projectFileName;
            runData.IsTrial = false;
            runData.RunScriptOnly = false;
            runData.BuildFileName = buildZipFileName;
            runData.IsTransactional = true; //For now, "/Transaction" flag is only for MultiDb runs

            bgBuildProcess.RunWorkerAsync(runData);
            txtBuildDesc.Text = string.Empty;

        }
        private void ProcessMultiDbBuildUnattended()
        {
            multiDbRunData.BuildData = buildData;
            multiDbRunData.ProjectFileName = projectFileName;
            bgBuildProcess.RunWorkerAsync(multiDbRunData);

        }


        private void mnuClearPreviouslyRunBlocks_Click(object sender, System.EventArgs e)
        {
            string[] selectedScriptIds = new string[lstScriptFiles.SelectedItems.Count];
            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                selectedScriptIds[i] = lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.ScriptId].Text;

            ClearScriptData scrData = new ClearScriptData(selectedScriptIds, buildData, projectFileName, buildZipFileName);
            bgBuildProcess.RunWorkerAsync(scrData);
            txtBuildDesc.Text = string.Empty;


        }

        private void mnuShowBuildLogs_Click(object sender, System.EventArgs e)
        {
            SqlSync.BuildHistory.LogFileHistoryForm frmLog = new SqlSync.BuildHistory.LogFileHistoryForm(projectFilePath, buildZipFileName);
            frmLog.LogFilesArchvied += new EventHandler(frmLog_LogFilesArchvied);
            frmLog.ShowDialog();
            frmLog.Dispose();
        }
        private void frmLog_LogFilesArchvied(object sender, EventArgs e)
        {
            statGeneral.Text = "Updating Project File";
            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, projectFilePath, buildZipFileName);
            statGeneral.Text = "Project File Updated";
        }

        private void mnuScriptToLogFile_Click(object sender, System.EventArgs e)
        {
            RunSingleFiles(new double[0], false, true);
        }

        private void mnuShowBuildHistory_Click(object sender, System.EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            SqlBuild.SqlBuildFileHelper.ConvertLegacyProjectHistory(ref buildData, projectFilePath, buildZipFileName);
            string buildHistXml = Path.Combine(projectFilePath, XmlFileNames.HistoryFile);
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
            Cursor = Cursors.Default;
        }

        private void lstScriptFiles_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            try
            {
                List<string> draggedItems = new List<string>();
                XmlSerializer xmls = new XmlSerializer(typeof(SqlSyncBuildData.ScriptDataTable));
                for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                {

                    SqlSyncBuildData.ScriptRow item = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[i].Tag;
                    SqlSyncBuildData.ScriptDataTable tbl = new SqlSyncBuildData.ScriptDataTable();
                    tbl.ImportRow(item);
                    tbl.AcceptChanges();
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        xmls.Serialize(sw, tbl);
                    }

                    string fullFileName = Path.Combine(projectFilePath, item.FileName);
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
                DoDragDrop(sbAll.ToString(), DragDropEffects.Move);
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
                SqlBuildFileHelper.ResortBuildByFileType(ref buildData, projectFileName, buildZipFileName);
                LoadSqlBuildProjectFileData(ref buildData, projectFileName, false);
                RefreshScriptFileList();
            }

        }

        //http://www.pinvoke.net/default.aspx/user32/GetKeyState.html
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetKeyState(short nVirtKey);

        private void settingsControl1_Click(object sender, System.EventArgs e)
        {
            if (settingsControl1.Project.ToLower() == "(select / create project)")
                return;

            string dir;
            int val = GetKeyState((short)Keys.ControlKey);
            if (val >= 0)
            {
                dir = Path.GetDirectoryName(settingsControl1.Project);
            }
            else
            {
                dir = projectFilePath;
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

            Cursor = Cursors.AppStarting;
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
            if (activeDatabase == string.Empty)
            {
                MessageBox.Show("Please select an Active Database first", "No Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                connData.DatabaseName = activeDatabase;
            }

            Cursor = Cursors.AppStarting;
            statGeneral.Text = "Retrieving list of selected objects from database...";
            progressBuild.Style = ProgressBarStyle.Marquee;
            bgGetObjectList.RunWorkerAsync(objectTypeConst);
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
                        objData = SqlSync.DbInformation.InfoHelper.GetStoredProcedureList(connData);
                        desc = SqlSync.Constants.DbScriptDescription.StoredProcedure;
                        break;
                    case SqlSync.Constants.DbObjectType.UserDefinedFunction:
                        objData = SqlSync.DbInformation.InfoHelper.GetFunctionList(connData);
                        desc = SqlSync.Constants.DbScriptDescription.UserDefinedFunction;
                        break;
                    case SqlSync.Constants.DbObjectType.View:
                        objData = SqlSync.DbInformation.InfoHelper.GetViewList(connData);
                        desc = SqlSync.Constants.DbScriptDescription.View;
                        break;
                    case SqlSync.Constants.DbObjectType.Table:
                        objData = SqlSync.DbInformation.InfoHelper.GetTableObjectList(connData);
                        desc = SqlSync.Constants.DbScriptDescription.Table;
                        break;
                    case SqlSync.Constants.DbObjectType.Trigger:
                        objData = SqlSync.DbInformation.InfoHelper.GetTriggerObjectList(connData);
                        desc = SqlSync.Constants.DbScriptDescription.Trigger;
                        break;
                }
            }
            catch (Exception exe)
            {
                if (exe.Message.Trim().IndexOf("Timeout") > -1)
                {
                    e.Result = "Timeout error trying to connect to " + connData.DatabaseName + ". Please try again or contact your database administrator.";
                }
                else
                {
                    e.Result = "Error retrieving list:\r\n" + exe.Message;
                }
            }

            e.Result = new KeyValuePair<string[], List<SqlSync.DbInformation.ObjectData>>(new string[] { desc, objectTypeConst }, objData);
        }

        private void bgGetObjectList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            Cursor = Cursors.AppStarting;
            statGeneral.Text = "Ready.";
            progressBuild.Style = ProgressBarStyle.Blocks;

            if (e.Result is KeyValuePair<string[], List<SqlSync.DbInformation.ObjectData>>)
            {
                KeyValuePair<string[], List<ObjectData>> result = (KeyValuePair<string[], List<ObjectData>>)e.Result;
                List<ObjectData> objData = result.Value;
                string desc = result.Key[0];
                string objectTypeConst = result.Key[1];
                if (objData.Count > 0)
                {
                    Objects.AddObjectForm frmAdd = new SqlSync.SqlBuild.Objects.AddObjectForm(objData, desc, objectTypeConst, connData);
                    if (DialogResult.OK == frmAdd.ShowDialog())
                    {
                        Cursor = Cursors.WaitCursor;
                        List<DbInformation.ObjectData> objectsToScript = frmAdd.SelectedObjects;
                        if (objectsToScript.Count > 0)
                        {
                            Cursor = Cursors.AppStarting;
                            progressBuild.Style = ProgressBarStyle.Marquee;
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
                MessageBox.Show(e.Result.ToString(), "Error Retrieving List", MessageBoxButtons.OK, MessageBoxIcon.Question);

            }
        }

        private void mnuAddStoredProcs_Click(object sender, System.EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.StoredProcedure);
        }
        private void mnuAddFunctions_Click(object sender, System.EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.UserDefinedFunction);
        }
        private void mnuAddViews_Click(object sender, System.EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.View);
        }
        private void mnuAddTables_Click(object sender, System.EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.Table);
        }
        private void mnuAddTriggers_Click(object sender, EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.Trigger);
        }
        private void mnuAddRoles_Click(object sender, EventArgs e)
        {
            AddObjects(SqlSync.Constants.DbObjectType.DatabaseRole);
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
                SqlSync.ObjectScript.ObjectScriptHelper scriptHelper = new ObjectScriptHelper(connData, SqlSync.Properties.Settings.Default.ScriptAsAlter, SqlSync.Properties.Settings.Default.ScriptPermissions, SqlSync.Properties.Settings.Default.ScriptPkWithTables);
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
            progressBuild.Style = ProgressBarStyle.Blocks;
            Cursor = Cursors.Default;
            if (e.Result is string[])
                BulkAdd((string[])e.Result, true, activeDatabase);

        }
        #endregion

        //private void mnuActiveDatabase_Click(object sender, System.EventArgs e)
        //{
        //    this.mnuActiveDatabase.Text = "Active Database :: "+((ToolStripMenuItem)sender).Text;
        //    this.activeDatabase = ((ToolStripMenuItem)sender).Text;
        //}

        private void mnuAddCodeTablePop_Click(object sender, System.EventArgs e)
        {
            CodeTableScriptingForm frmLookUp = new CodeTableScriptingForm(connData, true);
            frmLookUp.SqlBuildManagerFileExport += new SqlBuildManagerFileExportHandler(frmLookUp_SqlBuildManagerFileExport);
            frmLookUp.Show();
        }

        private void frmLookUp_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
        {
            BulkAdd(e.FileNames, true);
        }

        private void mnuSchemaScripting_Click(object sender, System.EventArgs e)
        {
            SyncForm frmSchema = new SyncForm(connData);
            frmSchema.Show();
        }

        private void mnuCodeTableScripting_Click(object sender, System.EventArgs e)
        {
            CodeTableScriptingForm frmLookUp = new CodeTableScriptingForm(connData, false);
            frmLookUp.Show();
        }

        private void mnuDataExtraction_Click(object sender, System.EventArgs e)
        {
            DataDump.DataDumpForm frmDump = new SqlSync.DataDump.DataDumpForm(connData);
            frmDump.Show();
        }

        private void autoScriptListItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < autoScriptListRegistration.Items.Length; i++)
            {
                if (((ToolStripMenuItem)sender).Text == Path.GetFileName(autoScriptListRegistration.Items[i].File))
                {
                    if (File.Exists(autoScriptListRegistration.Items[i].File))
                    {
                        System.Diagnostics.Process prc = new System.Diagnostics.Process();
                        prc.StartInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        prc.StartInfo.Arguments = " \"" + autoScriptListRegistration.Items[i].File + "\"";
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
                if (autoScriptListRegistration != null && autoScriptListRegistration.Items != null)
                    lst.AddRange(autoScriptListRegistration.Items);

                lst.Add(reg);

                if (autoScriptListRegistration == null)
                    autoScriptListRegistration = new AutoScriptList();

                autoScriptListRegistration.Items = new AutoScriptListRegistration[lst.Count];
                lst.CopyTo(autoScriptListRegistration.Items);

                System.Xml.XmlTextWriter tw = null;
                try
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(AutoScriptList));
                    tw = new System.Xml.XmlTextWriter(autoXmlFile, Encoding.UTF8);
                    xmlS.Serialize(tw, autoScriptListRegistration);
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
            SqlSync.Analysis.DatabaseSizeSummaryForm frmSize = new SqlSync.Analysis.DatabaseSizeSummaryForm(connData);
            frmSize.Show();
        }

        private void mnuDataAuditScripting_Click(object sender, System.EventArgs e)
        {
            DataAuditForm frmAudit = new DataAuditForm(connData);
            if (buildData != null)
                frmAudit.SqlBuildManagerFileExport += new SqlBuildManagerFileExportHandler(frmAudit_SqlBuildManagerFileExport);

            frmAudit.Show();
        }

        void frmAudit_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
        {
            BulkAdd(e.FileNames);
        }

        private void chkScriptChanges_Click(object sender, System.EventArgs e)
        {
            RefreshScriptFileList();
        }




        #region .: Code Table Populate Script Updates :.
        private void mnuUpdatePopulate_Click(object sender, System.EventArgs e)
        {
            CodeTable.ScriptUpdates[] updates = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(ref buildData, projectFileName);
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
                    tmpData.Password = connData.Password;
                    tmpData.UserId = connData.UserId;
                    tmpData.AuthenticationType = connData.AuthenticationType;
                    PopulateHelper helper = new PopulateHelper(tmpData);
                    string updatedScript;
                    helper.GenerateUpdatedPopulateScript(selectedUpdates[i], out updatedScript);

                    if (updatedScript.Length > 0)
                    {
                        using (StreamWriter sw = File.CreateText(Path.Combine(Path.GetDirectoryName(projectFileName), selectedUpdates[i].ShortFileName)))
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
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);

                statGeneral.Text = "Ready. Populate Scripts Updated";
            }
        }

        private void mnuUpdatePopulates_Click(object sender, System.EventArgs e)
        {
            if (lstScriptFiles.SelectedItems.Count == 0)
                return;

            SqlBuild.CodeTable.ScriptUpdates[] popUpdates = new ScriptUpdates[lstScriptFiles.SelectedItems.Count];

            for (int i = 0; i < lstScriptFiles.SelectedItems.Count; i++)
                popUpdates[i] = SqlBuild.SqlBuildFileHelper.GetFileDataForCodeTableUpdates(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text, projectFileName);

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
                objUpdates[i] = SqlBuild.SqlBuildFileHelper.GetFileDataForObjectUpdates(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.FileName].Text, projectFileName);
                if (useCurrentSettings && objUpdates[i] != null)
                {
                    // objUpdates[i].SourceDatabase = ConnectionHelper.GetTargetDatabase(objUpdates[i].SourceDatabase, OverrideData.TargetDatabaseOverrides);
                    objUpdates[i].SourceDatabase = ConnectionHelper.GetTargetDatabase(lstScriptFiles.SelectedItems[i].SubItems[(int)ScriptListIndex.Database].Text, OverrideData.TargetDatabaseOverrides);
                    objUpdates[i].SourceServer = connData.SQLServerName;
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
            Objects.ObjectUpdates[] updates = SqlBuildFileHelper.GetFileDataForObjectUpdates(ref buildData, projectFileName);
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
                Cursor = Cursors.WaitCursor;
                List<Objects.ObjectUpdates> selectedUpdates = frmUpdates.SelectedUpdates.ToList();
                List<Objects.UpdatedObject> lstScripts = ObjectScriptHelper.ScriptDatabaseObjects(selectedUpdates, connData);


                if (lstScripts.Count > 0)
                {
                    Package lstViolations = policyHelp.ValidateScriptsAgainstPolicies(lstScripts);
                    if (lstViolations != null && lstViolations.Count > 0)
                    {
                        Policy.PolicyViolationForm frmVio = new SqlSync.SqlBuild.Policy.PolicyViolationForm(lstViolations, true);
                        if (DialogResult.No == frmVio.ShowDialog())
                        {
                            statGeneral.Text = "Ready. Scripting update cancelled. No files changed";
                            Cursor = Cursors.Default;
                            return;
                        }
                    }




                    statGeneral.Text = "Saving updated scripts";
                    DateTime updateTime = DateTime.Now;
                    foreach (Objects.UpdatedObject obj in lstScripts)
                    {

                        using (StreamWriter sw = File.CreateText(Path.Combine(Path.GetDirectoryName(projectFileName), obj.ScriptName)))
                        {
                            sw.Write(obj.ScriptContents);
                        }

                        //Update the buildData object with the update date/time and user;
                        foreach (SqlSyncBuildData.ScriptRow row in buildData.Script.Rows)
                        {
                            if (row.FileName == obj.ScriptName)
                            {
                                row.DateModified = updateTime;
                                row.ModifiedBy = System.Environment.UserName;
                            }
                        }
                    }
                    buildData.AcceptChanges();



                    statGeneral.Text = "Updating project file";
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);

                    RefreshScriptFileList(true);
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
                Cursor = Cursors.Default;
            }
        }
        #endregion

        private void mnuObjectValidation_Click(object sender, System.EventArgs e)
        {
            SqlSync.Validate.ObjectValidation frmObjValidation = new SqlSync.Validate.ObjectValidation(connData);
            frmObjValidation.Show();
        }

        private void mnuIndividualFiles_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == fdrSaveScripts.ShowDialog())
            {
                statGeneral.Text = "Saving Scripts to destination";
                bool saved = SqlSync.SqlBuild.SqlBuildFileHelper.CopyIndividualScriptsToFolder(ref buildData, fdrSaveScripts.SelectedPath, projectFilePath, mnuIncludeUSE.Checked, mnuIncludeSequence.Checked);
                if (saved)
                    statGeneral.Text = "Scripts successfully exported";
                else
                    statGeneral.Text = "Error: NOT successfully exported";

            }
            fdrSaveScripts.Dispose();
        }

        private void mnuCombinedFile_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == saveCombinedScript.ShowDialog())
            {
                statGeneral.Text = "Saving combined scripts";
                bool saved = SqlSync.SqlBuild.SqlBuildFileHelper.CopyScriptsToSingleFile(ref buildData, saveCombinedScript.FileName, projectFilePath, buildZipFileName, mnuIncludeUSE.Checked);
                if (saved)
                    statGeneral.Text = "Scripts successfully exported";
                else
                    statGeneral.Text = "Error: NOT successfully exported";
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
            string baseFileWithPath = Path.Combine(projectFilePath, row.FileName);
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
            BuildHistory.ScriptRunHistoryForm frmHist = new BuildHistory.ScriptRunHistoryForm(connData, targetDatabase, new System.Guid(row.ScriptId), currentFileContents, textHash);
            frmHist.Show();
        }


        #region .: Build Process BackGroundWorker :.
        private void bgBuildProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            bgBuildProcess.ReportProgress(0, new BuildStartedEventArgs());
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
            SqlBuildHelper helper = new SqlBuildHelper(connData, createSqlRunLogFile, externalScriptLogFileName, runData.IsTransactional);
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
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            helper.ClearScriptBlocks(e.Argument as ClearScriptData, sender as BackgroundWorker, e);
        }
        private void RunMultiDbBuild(BackgroundWorker sender, DoWorkEventArgs e)
        {
            MultiDbData multiRunData = (MultiDbData)e.Argument;

            //Create an ID for this build to tie all the entries together
            string id = Guid.NewGuid().ToString();
            id = id.Substring(0, 1) + id.Substring(7, 1) + id.Substring(10, 1) + id.Substring(26, 1) + id.Substring(30, 1);
            multiRunData.MultiRunId = id;
            SqlBuildHelper helper = new SqlBuildHelper(connData, createSqlRunLogFile, externalScriptLogFileName, multiRunData.IsTransactional);
            helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
            helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
            helper.ProcessMultiDbBuild(multiRunData, sender as BackgroundWorker, e);

        }
        private void bgBuildProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
                Cursor = Cursors.Default;

            if (e.UserState == null)
                return;

            if (e.UserState is BuildStartedEventArgs) // Update the UI now that the build has started.
            {
                lstBuild.Items.Clear();
                statBuildTime.Text = "Build Duration: ";
                statScriptTime.Text = "Script Duration (sec): ";
                lnkStartBuild.Enabled = false;
                txtBuildDesc.Text = string.Empty;
                txtBuildDesc.Enabled = false;
                ddBuildType.Enabled = false;
                progressBuild.Style = ProgressBarStyle.Marquee;
                btnCancel.Enabled = true;
                btnCancel.Visible = true;
                tmrBuild.Start();
            }
            else if (e.UserState is GeneralStatusEventArgs) //Update the general run status
            {
                if (runningUnattended)
                {
                    log.LogInformation(((GeneralStatusEventArgs)e.UserState).StatusMessage);
                }
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
                statScriptTime.Text = "Script Duration (sec): 0";
                scriptDuration = 0;
                tmrScript.Start();

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
                if (!runningUnattended)
                    MessageBox.Show(((CommitFailureEventArgs)e.UserState).ErrorMessage, "Failed to Commit Build", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    log.LogError("Failed to Commit Build " + ((CommitFailureEventArgs)e.UserState).ErrorMessage);
            }
            else if (e.UserState is ScriptRunProjectFileSavedEventArgs)
            {
                if (runningUnattended)
                    log.LogInformation("ScriptRunProjectFileSavedEventArgs captured");

                //Reload the file
                if (runningUnattended)
                    log.LogInformation("Reloading updated build XML");

                LoadSqlBuildProjectFileData(ref buildData, projectFileName, true);

                if (runningUnattended)
                    log.LogInformation("Saving updated build file to disk");

                try
                {
                    SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, projectFilePath, buildZipFileName);
                    log.LogInformation("Build file saved to disk");

                }
                catch (Exception exe)
                {
                    if (runningUnattended)
                        log.LogError(exe.ToString());
                    else
                        MessageBox.Show(e.ToString(), "Error Saving Build File", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    returnCode = 789;
                }

                if (runningUnattended)
                {
                    log.LogInformation("Completed with return code " + returnCode.ToString());
                    log.LogInformation("************************************************");
                }

                if (runningUnattended && returnCode != -1 && UnattendedProcessingCompleteEvent != null)
                    UnattendedProcessingCompleteEvent(returnCode);

                // this.RefreshScriptFileList();
            }
            else if (e.UserState is Exception)
            {
                if (!runningUnattended)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Run Error:\r\n" + ((Exception)e.UserState).Message);
                }
                else
                {
                    log.LogError("ERROR!" + ((Exception)e.UserState).Message);
                }
            }



        }
        private void bgBuildProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tmrScript.Stop();

            if (!runningUnattended)
            {
                try
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
                                RefreshScriptFileList(true);
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
                }
                catch (Exception exe)
                {
                    MessageBox.Show(exe.Message, "Error during build", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                txtBuildDesc.Enabled = true;
                ddBuildType.Enabled = true;
                progressBuild.Style = ProgressBarStyle.Blocks;
                btnCancel.Visible = false;
                btnCancel.Enabled = true;

                //Stop and reset the timer.
                tmrBuild.Stop();
                buildDuration = 0;
            }
        }

        #region ## Build Helper Event Handlers ##

        private void helper_BuildCommittedEvent(object sender, RunnerReturn rr)
        {
            returnCode = 0;
        }

        private void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            returnCode = 999;
        }
        #endregion

        private void mnuDDActiveDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mnuDDActiveDatabase.SelectedItem.ToString() != selectDatabaseString)
            {
                activeDatabase = mnuDDActiveDatabase.SelectedItem.ToString();
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
            if (haveReadOnly)
            {
                renameScriptFIleToolStripMenuItem.Enabled = false;
                mnuUpdatePopulates.Enabled = false;
                mnuUpdateObjectScripts.Enabled = false;
                makeFileWriteableremoveToolStripMenuItem.Enabled = true;
                mnuEditFile.Image = SqlSync.Properties.Resources.Script_Edit;
                mnuEditFile.Text = "View File";
            }
            else
            {

                makeFileWriteableremoveToolStripMenuItem.Enabled = false;
                mnuEditFile.Image = SqlSync.Properties.Resources.Script_Edit;
                mnuEditFile.Text = "Edit File";
            }

            //Check to see if this is SBX or SBM and change "Remove vs. Delete"
            string plural = (lstScriptFiles.SelectedItems.Count > 1) ? "s" : "";
            if (sbxBuildControlFileName != null && sbxBuildControlFileName.Length > 0)
            {
                mnuRemoveScriptFile.Text = String.Format("Delete file{0} from project and directory", plural);
            }
            else
            {
                mnuRemoveScriptFile.Text = String.Format("Remove file{0} from project", plural);
            }

        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Cursor = Cursors.WaitCursor;
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            connData.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }
            connData.AuthenticationType = authType;
            connData.ScriptTimeout = 5;

            try
            {
                databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
                targetDatabaseOverrideCtrl1.SetDatabaseData(databaseList, databasesUsed);
                InfoHelper.UpdateRoutineAndViewChangeDates(connData, targetDatabaseOverrideCtrl1.GetOverrideData());
                SetDatabaseMenuList();
                RefreshScriptFileList();
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connData = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;

        }

        private void rebuildPreviouslyCommitedBuildFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RebuildForm frmRebuild = new RebuildForm(connData, databaseList);
            frmRebuild.Show();
        }

        private void SqlBuildForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (!SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDirectory))
            {
                DialogResult result = MessageBox.Show("Unable to clean-up working directory. Do you want to navigate to it to remove manually?", "Unable to clean-up", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process prc = new Process();
                    prc.StartInfo.FileName = projectFilePath;
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
            if (buildData != null)
            {
                mnuExportBuildList.Enabled = true;
                mnuExportScriptText.Enabled = true;
                mnuImportScriptFromFile.Enabled = true;
                mnuMainAddNewFile.Enabled = true;
                mnuMainAddSqlScript.Enabled = true;
                startConfigureMultiServerDatabaseRunToolStripMenuItem.Enabled = true;
            }

            if (SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout > 0)
                mnuDefaultScriptTimeout.Text = SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout.ToString();
            else
                mnuDefaultScriptTimeout.Text = "90";


            scriptTagsRequiredToolStripMenuItem.Checked = SqlSync.Properties.Settings.Default.RequireScriptTags;

            // this.sourceControlServerURLTextboxMenuItem.Text = SqlSync.Properties.Settings.Default.SourceControlServerUrl;




        }

        // mnuDacpacDelta_Click

        private void mnuDacpacDelta_Click(object sender, EventArgs e)
        {
            FromDacpacForm form = new FromDacpacForm();
            form.ShowDialog();
        }
        private void showObjectHistoryAsUpdatedViaSqlBuildManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSyncBuildData.ScriptRow row = null;
            try
            {
                Cursor = Cursors.WaitCursor;
                //We can only script for compare one object at a time
                if (lstScriptFiles.SelectedItems.Count != 1)
                    return;

                //Make sure we can find the base file. 
                row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                string baseFileWithPath = Path.Combine(projectFilePath, row.FileName);
                if (!File.Exists(baseFileWithPath))
                {
                    MessageBox.Show("Unable to find base file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                string script = File.ReadAllText(baseFileWithPath);
                connData.DatabaseName = ConnectionHelper.GetTargetDatabase(row.Database, OverrideData.TargetDatabaseOverrides);
                ScriptRunHistoryForm frmHist = new ScriptRunHistoryForm(connData, connData.DatabaseName, row.FileName, script, "");
                frmHist.ShowDialog();
                frmHist.Dispose();
            }
            catch (Exception exe)
            {
                string fileName = (row == null ? "unknown" : row.FileName);
                log.LogError(exe, $"Error trying to get object history for {fileName}");
                MessageBox.Show(String.Format("Unable to get object history.\r\n{0}", exe.Message), "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        private void archiveBuildHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(projectFilePath, XmlFileNames.HistoryFile)))
            {
                MessageBox.Show("No Build Run History is Available", "No History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to archive the XML build History?", "Confirm Archiving", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                try
                {
                    string buildHistFileName = string.Empty;
                    string newZipName = Path.GetDirectoryName(Path.Combine(buildZipFileName, Path.GetFileNameWithoutExtension(buildZipFileName) + " - Build Archive.zip"));
                    if (File.Exists(Path.Combine(projectFilePath, XmlFileNames.HistoryFile)))
                    {
                        buildHistFileName = Path.Combine(projectFilePath,
                                                            DateTime.Now.Year.ToString() + "-" +
                                                            DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" +
                                                            DateTime.Now.Day.ToString().PadLeft(2, '0') + " at " +
                                                            DateTime.Now.Hour.ToString().PadLeft(2, '0') +
                                                            DateTime.Now.Minute.ToString().PadLeft(2, '0') + " " +
                                                            XmlFileNames.HistoryFile);

                        File.Move(Path.Combine(projectFilePath + XmlFileNames.HistoryFile), buildHistFileName);

                        if (ZipHelper.AppendZipPackage(new string[] { Path.GetFileName(buildHistFileName) }, projectFilePath, newZipName, false))
                        {
                            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, projectFilePath, buildZipFileName);
                            MessageBox.Show("Build History has been successfully archived to:\r\n" + newZipName, "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            try
                            {
                                File.Move(buildHistFileName, Path.Combine(projectFilePath + XmlFileNames.HistoryFile));
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
                Cursor = Cursors.WaitCursor;
                //We can only script for compare one object at a time
                if (lstScriptFiles.SelectedItems.Count != 1)
                    return;

                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag;
                RenameScriptForm frmRename = new RenameScriptForm(ref row);
                if (DialogResult.OK == frmRename.ShowDialog())
                {
                    if (!File.Exists(Path.Combine(projectFilePath, frmRename.OldName)))
                    {
                        MessageBox.Show("Unable to find old file!", "Whoops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (File.Exists(Path.Combine(projectFilePath, frmRename.NewName)))
                    {
                        MessageBox.Show("A file be the name of " + frmRename.NewName + " was already found!\r\nRename can not proceed.", "Sorry, can't overwrite", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Rename the File
                    File.Move(Path.Combine(projectFilePath, frmRename.OldName), Path.Combine(projectFilePath, frmRename.NewName));

                    //Rename in the config
                    ((SqlSyncBuildData.ScriptRow)lstScriptFiles.SelectedItems[0].Tag).FileName = frmRename.NewName;
                    buildData.AcceptChanges();
                    SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
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
                Cursor = Cursors.Default;
            }
        }

        private void PopulateTagList()
        {
            if (buildData == null)
                return;

            if (buildData.Script == null || buildData.Script.Count == 0)
                return;

            tagList.Clear();

            var lst = (from r in buildData.Script
                       where r.Tag.Trim().Length > 0
                       select r.Tag.Trim()).Distinct();

            if (lst.Count() > 0)
                tagList = lst.ToList();

            tagList.Sort();

        }

        private void scriptTagsRequiredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (buildData == null)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                statGeneral.Text = "Updating Build File with new setting";
                if (buildData.SqlSyncBuildProject.Rows.Count == 0)
                    buildData.SqlSyncBuildProject.AddSqlSyncBuildProjectRow("", scriptTagsRequiredToolStripMenuItem.Checked);
                else
                    buildData.SqlSyncBuildProject[0].ScriptTagRequired = scriptTagsRequiredToolStripMenuItem.Checked;

                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
            }
            finally
            {
                statGeneral.Text = "Build File Updated. Ready.";
                Cursor = Cursors.Default;
            }
        }
        private void createSQLLogOfBuildRunsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createSqlRunLogFile = createSQLLogOfBuildRunsToolStripMenuItem.Checked;
        }
        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            bgCheckForUpdates.RunWorkerAsync(true);
        }

        private void bgCheckForUpdates_DoWork(object sender, DoWorkEventArgs e)
        {
            if (runningUnattended)
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

                string configFileFullPath = Path.Combine(SqlBuildManager.Logging.Configure.AppDataPath, SqlSync.SqlBuild.UtilityHelper.ConfigFileName);
                ServerConnectConfig config = new ServerConnectConfig();
                if (File.Exists(configFileFullPath))
                {
                    config.ReadXml(configFileFullPath);
                }

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
                                var httpClient = new HttpClient();
                                var resp = httpClient.GetAsync(fileURL[i]).GetAwaiter().GetResult();
                                var verFull = resp.RequestMessage.RequestUri.AbsolutePath;
                                var ver = verFull.Substring(verFull.LastIndexOf("/v") + 2);

                                verData.LatestVersion = new Version(ver);
                                verData.LastCompatableVersion = new Version(ver);

                                string changeNotesFilePath = "https://raw.githubusercontent.com/mmckechney/SqlBuildManager/master/CHANGELOG.md";
                                var changeNotesFileContents = httpClient.GetStringAsync(changeNotesFilePath).GetAwaiter().GetResult();


                                verData.ReleaseNotes = changeNotesFileContents;

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
                            catch (Exception exe)
                            {
                                errorMessage = exe.ToString();
                            }
                        }

                        if (!contacted)
                        {
                            verData.UpdateFileReadError = true;
                        }

                        if (config.LastProgramUpdateCheck.Count == 0)
                            config.LastProgramUpdateCheck.AddLastProgramUpdateCheckRow(DateTime.Now);
                        else
                            config.LastProgramUpdateCheck[0].CheckTime = DateTime.Now;

                        config.AcceptChanges();
                        config.WriteXml(configFileFullPath);
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
                log.LogWarning(exe, "Error Checking for updates");
                //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Error Checking for updates.\r\n" + exe.ToString(), EventLogEntryType.Error, 901);

            }
            finally
            {
            }
        }

        private void bgCheckForUpdates_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
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
                            NewVersion frmVersion = new NewVersion(verData, isBreaking); //, this.impersonatedUser);
                            frmVersion.ShowDialog();
                            //this.impersonatedUser = frmVersion.impersonatedUser;
                        }
                    }
                }
                catch (Exception exe)
                {
                    log.LogWarning(exe, "Unable to display New Version alert window");
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
            Test.SprocTestConfigForm frmTest = new SqlSync.Test.SprocTestConfigForm(connData);
            frmTest.Show();
        }

        private void maintainManualDatabaseEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManualDatabaseForm frmManual = new ManualDatabaseForm();
            if (DialogResult.OK == frmManual.ShowDialog())
            {
                databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
            }
            frmManual.Dispose();
        }

        private void targetDatabaseOverrideCtrl1_TargetChanged(object sender, TargetChangedEventArgs e)
        {
            OverrideData.TargetDatabaseOverrides = targetDatabaseOverrideCtrl1.GetOverrideData();
            if (chkUpdateOnOverride.Checked)
            {
                InfoHelper.UpdateRoutineAndViewChangeDates(connData, targetDatabaseOverrideCtrl1.GetOverrideData());

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
                RefreshScriptFileList();
            }
        }

        private void SetUsedDatabases()
        {
            databasesUsed.Clear();
            if (buildData != null && buildData.Script != null)
            {
                foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                {
                    bool found = false;
                    //Can't use "contains" because it is case sensitive
                    for (int i = 0; i < databasesUsed.Count; i++)
                    {
                        if (databasesUsed[i].ToLower() == row.Database.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        databasesUsed.Add(row.Database);
                }
            }
            targetDatabaseOverrideCtrl1.SetDatabaseData(databaseList, databasesUsed);
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
            SqlSync.SqlBuild.Default_Scripts.DefaultScriptMaintenanceForm frm = new SqlSync.SqlBuild.Default_Scripts.DefaultScriptMaintenanceForm(databaseList);
            frm.ShowDialog();
        }

        private void startConfigureMultiServerDatabaseRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(buildZipFileName))
            {
                MessageBox.Show("Please load a project file prior creating a multi-database configuration");
                return;
            }

            if (databasesUsed.Count > 1)
            {
                MessageBox.Show("Unable to configure multi-database run when more than one default database is specified.\r\n\r\nInstead, please use the command line (sbm.exe build) with a configuration file formatted as:\r\n\r\n<server>:<default1>,<override1>;<default2>,<override2>", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MultiDbRunForm frm = new MultiDbRunForm(connData, databasesUsed, databaseList, buildZipFileName, projectFilePath, ref buildData);
            if (DialogResult.OK == frm.ShowDialog())
            {
                MultiDbData runData = frm.RunConfiguration;
                runData.BuildData = buildData;
                runData.BuildFileName = buildZipFileName;
                runData.ProjectFileName = projectFileName;

                List<string> desc = (from s in buildData.Script
                                     join d in scriptsRequiringBuildDescription on s.ScriptId equals d
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
            saveScriptsToPackage.FileName = Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm";
            if (DialogResult.OK == saveScriptsToPackage.ShowDialog())
            {
                bool success = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName, saveScriptsToPackage.FileName);

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
                string fileName = Path.Combine(projectFilePath, row.FileName);

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

        private void constructCommandLineStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommandLineBuilderForm frmCmd;
            if (sbxBuildControlFileName != null && sbxBuildControlFileName.Length > 0)
                frmCmd = new CommandLineBuilderForm(sbxBuildControlFileName);
            else
                frmCmd = new CommandLineBuilderForm(buildZipFileName);

            frmCmd.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void scriptPolicyCheckingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Policy.PolicyForm frmPolicy = new SqlSync.SqlBuild.Policy.PolicyForm(ref buildData, projectFilePath);
            frmPolicy.ScriptSelected += new SqlSync.SqlBuild.Policy.PolicyForm.ScriptSelectedHandler(frmPolicy_ScriptSelected);
            frmPolicy.Show();
        }

        void frmPolicy_ScriptSelected(object sender, SqlSync.SqlBuild.Policy.ScriptSelectedEventArgs e)
        {

            for (int i = 0; i < lstScriptFiles.Items.Count; i++)
            {
                if (e.SelectedRow == (SqlSyncBuildData.ScriptRow)lstScriptFiles.Items[i].Tag)
                {
                    lstScriptFiles.Items[i].Selected = true;
                    EditFile(lstScriptFiles);
                    break;
                }
                else
                    lstScriptFiles.Items[i].Selected = false;
            }
        }

        private void chkNotTransactional_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNotTransactional.Checked)
            {
                string message = "WARNING!\r\nBy checking this box, you are disabling the transaction handling of Sql Build Manager.\r\nIn the event of a script failure, your scripts will NOT be rolled back\r\nand your database will be left in an inconsistent state!\r\n\r\nAre you certain you want this checked?";
                if (DialogResult.No == MessageBox.Show(message, "Are you sure you want this?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    chkNotTransactional.Checked = false;
                    return;
                }

                if (ddBuildType.SelectedItem != null &&
                    (ddBuildType.SelectedItem.ToString() == BuildType.Trial || ddBuildType.SelectedItem.ToString() == BuildType.TrialPartial))
                {
                    chkNotTransactional.Checked = false;
                    message = "You can not have a Trial run without transactions. Please select a different build type then re-check the transaction box";
                    MessageBox.Show(message, "Invalid Build/Transaction combination", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

            }


        }

        private void ddOverrideLogDatabase_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (ddOverrideLogDatabase.SelectedItem != null && ddOverrideLogDatabase.SelectedItem.ToString().Length > 0)
            {
                if (DialogResult.No == MessageBox.Show("The committed scripts log for ALL scripts will be saved to '" + ddOverrideLogDatabase.SelectedItem.ToString() + "'.\r\nAre you certain you want this?", "Alternate Logging Database Selected", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    ddOverrideLogDatabase.SelectedIndex = 0;
                }

            }
        }

        private void lblAdvanced_Click(object sender, EventArgs e)
        {
            if (pnlAdvanced.Height < 30)
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
            SqlSync.SqlBuild.UtilityHelper.OpenManual(string.Empty);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("RunTimeBuildSettings");
        }

        //private void remoteExecutionServiceToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    RemoteServiceForm frmRemote;
        //    if(this.buildZipFileName != null && this.buildZipFileName.Length > 0)
        //        frmRemote = new RemoteServiceForm(this.connData, this.databaseList,this.buildZipFileName);
        //    else
        //        frmRemote = new RemoteServiceForm(this.connData,this.databaseList);

        //    frmRemote.Show();
        //}

        private void calculateScriptPackageHashSignatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (buildData != null)
            {

                string hashSignature = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFilePath, buildData);
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
            SqlBuildFileHelper.GetFileDataForObjectUpdates(ref buildData, projectFileName, out canUpdate, out canNotUpdate);
            if (canUpdate == null)
            {
                MessageBox.Show("There aren't any object script files available for updating", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BackoutPackageForm frmBack = new BackoutPackageForm(connData, buildData, buildZipFileName, projectFilePath, projectFileName);
            frmBack.ShowDialog();

        }

        private void InferScriptTag_Click(object sender, EventArgs e)
        {
            string typeValue = string.Empty;
            if (sender is ToolStripMenuItem)
            {
                typeValue = ((ToolStripMenuItem)sender).Tag.ToString();
            }

            TagInferenceSource source = (TagInferenceSource)Enum.Parse(typeof(TagInferenceSource), typeValue, true);
            //if (source == null) source = TagInferenceSource.TextOverName;

            List<string> regexFormats = new List<string>();
            regexFormats.AddRange(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());

            if (ScriptTagProcessing.InferScriptTags(ref buildData, projectFilePath, regexFormats, source))
            {
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projectFileName, buildZipFileName);
                RefreshScriptFileList();
                statGeneral.Text = "Script tags updated";
            }
            else
            {
                statGeneral.Text = "No script tags could be inferred";
            }
        }

        private void runPolicyCheckingonloadtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            runPolicyCheckingOnLoad = runPolicyCheckingonloadtoolStripMenuItem.Checked;
            if (!runPolicyCheckingOnLoad)
            {
                lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 0;
                policyCheckIconHelpToolStripMenuItem.Visible = false;
            }
            else
            {
                lstScriptFiles.Columns[(int)ScriptListIndex.PolicyIconColumn].Width = 20;
                policyCheckIconHelpToolStripMenuItem.Visible = true;

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

            if ((buildZipFileName == null || buildZipFileName.Length == 0) &&
                (sbxBuildControlFileName == null || sbxBuildControlFileName.Length == 0))
            {
                MessageBox.Show("Please load an SBM file prior to trying to save violations", "Can't do that yet", MessageBoxButtons.OK);
                return;
            }

            string rootBuildFileName;
            string buildFileName;
            if (buildZipFileName != null || buildZipFileName.Length > 0)
            {
                rootBuildFileName = Path.GetFileNameWithoutExtension(buildZipFileName);
                buildFileName = Path.GetFileName(buildZipFileName);
            }
            else
            {
                rootBuildFileName = Path.GetFileNameWithoutExtension(sbxBuildControlFileName);
                buildFileName = Path.GetFileName(sbxBuildControlFileName);
            }

            currentViolations.PackageName = buildFileName;

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
                if (!PolicyHelper.TransformViolationstoCsv(fileName, currentViolations))
                {
                    log.LogError($"Error saving violations to file {fileName}");
                }
            }
            else
            {
                try
                {
                    string xml = PolicyHelper.TransformViolationstoXml(currentViolations);
                    File.WriteAllText(fileName, xml);
                }
                catch (Exception exe)
                {
                    log.LogError(exe, "Unable to write violations XML to file");
                }
            }
        }


        //private void sourceControlServerURLTextboxMenuItem_TextChanged(object sender, EventArgs e)
        //{
        //    SqlSync.Properties.Settings.Default.SourceControlServerUrl = this.sourceControlServerURLTextboxMenuItem.Text;
        //    SqlSync.Properties.Settings.Default.Save();
        //}

        private void mnuListTop_DropDownOpening(object sender, EventArgs e)
        {
            if (buildData != null)
            {
                mnuExportBuildList.Enabled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
            }
            catch (Exception)
            {
                Invalidate();  //attempt to redraw the control

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
