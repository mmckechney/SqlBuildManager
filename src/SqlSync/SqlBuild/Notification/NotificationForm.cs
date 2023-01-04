using SqlBuildManager.Enterprise;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.Notification
{
    public partial class NotificationForm : Form
    {
        public NotificationForm()
        {
            InitializeComponent();
        }

        List<TableWatch> watches = null;
        public NotificationForm(List<TableWatch> watches)
            : this()
        {
            this.watches = watches;
        }

        private void NotificationForm_Load(object sender, EventArgs e)
        {
            if (watches == null)
                return;

            int width = 0;
            int height = 0;
            int start = 0;
            for (int i = 0; i < watches.Count; i++)
            {
                TableWatchControl ctrl = new TableWatchControl(watches[i]);
                ctrl.Location = new Point(0, start);

                flowMain.Controls.Add(ctrl);
                //ctrl.Anchor = AnchorStyles.Right;
                start += ctrl.Height;
                if (ctrl.Width > width)
                    width = ctrl.Width;

                height += ctrl.Height;
            }

            if (height > flowMain.Height)
                Height = 450;
            //this.pnlHelp.Width = width;
            Width = width + 40;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ScriptChangeSettings");
        }

    }
}

