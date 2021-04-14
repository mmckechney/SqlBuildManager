using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using System.IO;
namespace SqlSync.Controls
{
    public partial class ViewLogFileMenuItem : ToolStripMenuItem
    {
        log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        BackgroundWorker bg;
        public ViewLogFileMenuItem()
        {
            InitializeComponent();
            CustomInitializeComponent();
        }
        private void CustomInitializeComponent()
        {
            this.Text = "View Application Log File";
            this.Click += new System.EventHandler(this.ViewLogFileMenuItem_Click);
            this.Image = global::SqlSync.Properties.Resources.New;
            this.Size = new System.Drawing.Size(251, 22);

            this.bg = new BackgroundWorker();
            this.bg.WorkerReportsProgress = true;
            this.bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            this.bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            this.bg.DoWork += new DoWorkEventHandler(bg_DoWork);
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            bg.ReportProgress(0);
            try
            {
                log4net.Appender.IAppender[] appenders = log.Logger.Repository.GetAppenders();
                foreach (log4net.Appender.IAppender appender in appenders)
                {
                    if (appender is log4net.Appender.RollingFileAppender)
                    {
                        string file = ((log4net.Appender.RollingFileAppender)appender).File;
                        if (File.Exists(file))
                        {

                            string contents;
                            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    contents = sr.ReadToEnd();
                                }
                            }

                            e.Result = contents;

                            return;
                        }
                    }
                }
            }
            catch (Exception exe)
            {
                log.Error("Error loading application log file.", exe);
                e.Result = exe;
            }
            bg.ReportProgress(100);
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is string)
            {
                SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(e.Result.ToString(), "", "Sql Build Manager Application Log", SqlSync.Highlighting.SyntaxHightlightType.LogFile);
                frmScript.WordWrap = false;
                frmScript.ScrollToEndOnLoad = true;
                frmScript.Show();
            }
            else if(e.Result is Exception)
            {
                MessageBox.Show("Sorry, there was an error trying to load the application log file","Something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.Owner != null && this.Owner.FindForm() != null)
            {
                if (e.ProgressPercentage == 0)
                    this.Owner.FindForm().Cursor = Cursors.WaitCursor;
                else
                    this.Owner.FindForm().Cursor = Cursors.Default;
            }
        }

        private void ViewLogFileMenuItem_Click(object sender, EventArgs e)
        {
            bg.RunWorkerAsync();
        }
    }
}
