namespace SqlSync.Controls
{
    partial class RemoteExecutionLogsContextMenuStrip
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.viewExecutionErrorsLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewExecutionCommitsLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLocalLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtDetailedLogTarget = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retrieveAllApplicableErrorLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.setLoggingLevelMenuItem1 = new SetLoggingLevelMenuItem();
            this.viewLogFileMenuItem1 = new ViewLogFileMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.SuspendLayout();
            // 
            // viewLocalLogs
            // 
            this.viewLocalLogs.Name = "viewLocalLogs";
            this.viewLocalLogs.Size = new System.Drawing.Size(429, 22);
            this.viewLocalLogs.Text = "View Local Application Log...";
            // 
            // viewExecutionErrorsLogToolStripMenuItem
            // 
            this.viewExecutionErrorsLogToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Receipt_Delete;
            this.viewExecutionErrorsLogToolStripMenuItem.Name = "viewExecutionErrorsLogToolStripMenuItem";
            this.viewExecutionErrorsLogToolStripMenuItem.Size = new System.Drawing.Size(429, 22);
            this.viewExecutionErrorsLogToolStripMenuItem.Text = "View Last Execution \"Errors\" log";
            // 
            // viewExecutionCommitsLogToolStripMenuItem
            // 
            this.viewExecutionCommitsLogToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Receipt_New;
            this.viewExecutionCommitsLogToolStripMenuItem.Name = "viewExecutionCommitsLogToolStripMenuItem";
            this.viewExecutionCommitsLogToolStripMenuItem.Size = new System.Drawing.Size(429, 22);
            this.viewExecutionCommitsLogToolStripMenuItem.Text = "View Last Execution \"Commits\" log";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Receipt;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(429, 22);
            this.pasteToolStripMenuItem.Text = "Paste Server/Database value below to retrieve detailed log for last run:";
            // 
            // txtDetailedLogTarget
            // 
            this.txtDetailedLogTarget.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.txtDetailedLogTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDetailedLogTarget.Name = "txtDetailedLogTarget";
            this.txtDetailedLogTarget.Size = new System.Drawing.Size(350, 21);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(426, 6);
            // 
            // viewRemoteServiceExecutableLogFileToolStripMenuItem
            // 
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server_News;
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem.Name = "viewRemoteServiceExecutableLogFileToolStripMenuItem";
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem.Size = new System.Drawing.Size(429, 22);
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem.Text = "View Remote Service Executable Log file";
            // 
            // retrieveAllApplicableErrorLogsToolStripMenuItem
            // 
            this.retrieveAllApplicableErrorLogsToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Report_2;
            this.retrieveAllApplicableErrorLogsToolStripMenuItem.Name = "retrieveAllApplicableErrorLogsToolStripMenuItem";
            this.retrieveAllApplicableErrorLogsToolStripMenuItem.Size = new System.Drawing.Size(429, 22);
            this.retrieveAllApplicableErrorLogsToolStripMenuItem.Text = "Retrieve all applicable error logs for this run (zip file)";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "zip";
            this.saveFileDialog1.Title = "Save error log files (zip)";
            this.viewLocalLogs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]{
                this.viewLogFileMenuItem1,
                this.setLoggingLevelMenuItem1});
            // 
            // RemoteExecutionLogsContextMenuStrip
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewExecutionErrorsLogToolStripMenuItem,
            this.viewExecutionCommitsLogToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.txtDetailedLogTarget,
            this.retrieveAllApplicableErrorLogsToolStripMenuItem,
            this.toolStripSeparator3,
            this.viewRemoteServiceExecutableLogFileToolStripMenuItem,
            this.toolStripSeparator4,
            viewLocalLogs});
            this.Size = new System.Drawing.Size(430, 121);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem viewExecutionErrorsLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewExecutionCommitsLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox txtDetailedLogTarget;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem viewRemoteServiceExecutableLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retrieveAllApplicableErrorLogsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private SqlSync.Controls.SetLoggingLevelMenuItem setLoggingLevelMenuItem1;
        private SqlSync.Controls.ViewLogFileMenuItem viewLogFileMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem viewLocalLogs;
        //private System.Windows.Forms.ToolStripMenuItem viewBuildRequestHistoryForThisRemoteServiceToolStripMenuItem;
    }
}
