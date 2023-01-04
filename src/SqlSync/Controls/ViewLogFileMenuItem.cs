using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
namespace SqlSync.Controls
{
    public partial class ViewLogFileMenuItem : ToolStripMenuItem
    {
        private static string logFileName = string.Empty;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        BackgroundWorker bg;
        public ViewLogFileMenuItem()
        {
            InitializeComponent();
            CustomInitializeComponent();
        }
        private void CustomInitializeComponent()
        {
            Text = "View Application Log File";
            Click += new System.EventHandler(ViewLogFileMenuItem_Click);
            Image = global::SqlSync.Properties.Resources.New;
            Size = new System.Drawing.Size(251, 22);

            bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            bg.ReportProgress(0);
            try
            {
                string file = SqlBuildManager.Logging.ApplicationLogging.LogFileName;
                if (!File.Exists(file))
                {
                    file = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + DateTime.Now.ToString("yyyyMMdd") + Path.GetExtension(file));
                }

                if (File.Exists(file))
                {
                    logFileName = file;
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
            catch (Exception exe)
            {
                log.LogError(exe, "Error loading application log file.");
                e.Result = exe;
            }
            bg.ReportProgress(100);
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is string)
            {
                SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(e.Result.ToString(), "", logFileName, SqlSync.Highlighting.SyntaxHightlightType.LogFile);
                frmScript.WordWrap = false;
                frmScript.ScrollToEndOnLoad = true;
                frmScript.Show();
            }
            else if (e.Result is Exception || e.Result == null)
            {
                MessageBox.Show("Sorry, there was an error trying to load the application log file", "Something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Owner != null && Owner.FindForm() != null)
            {
                if (e.ProgressPercentage == 0)
                    Owner.FindForm().Cursor = Cursors.WaitCursor;
                else
                    Owner.FindForm().Cursor = Cursors.Default;
            }
        }

        private void ViewLogFileMenuItem_Click(object sender, EventArgs e)
        {
            bg.RunWorkerAsync();
        }
    }
}
