using SqlBuildManager.Enterprise;
using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.Notification
{
    public partial class TableWatchControl : UserControl
    {
        public TableWatchControl()
        {
            InitializeComponent();
        }

        TableWatch watch = null;
        string emailTo = string.Empty;
        public TableWatchControl(TableWatch watch) : this()
        {
            this.watch = watch;
        }

        private void TableWatchControl_Load(object sender, EventArgs e)
        {
            if (watch == null)
                return;

            lblDescription.Text = watch.Description;
            for (int i = 0; i < watch.Notify.Length; i++)
            {
                lstNotifyUser.Items.Add(watch.Notify[i].Name);
                emailTo += watch.Notify[i].EMail + ";";
            }

            for (int i = 0; i < watch.FoundTables.Count; i++)
            {
                lstTables.Items.Add(watch.FoundTables[i]);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string subject = watch.EmailSubject;
            string body = watch.EmailBody + "\r\n----------------------------------\r\nReferenced Script:\r\n\r\n" + watch.Script;
            body = body.Replace("\r", "%0D").Replace("\n", "%0A");
            string to = emailTo;

            string mailto = "mailto:" + to + "?subject=" + subject + "&body=" + body;
            Process prc = new Process();
            prc.StartInfo.FileName = mailto;
            prc.Start();
        }


    }
}
