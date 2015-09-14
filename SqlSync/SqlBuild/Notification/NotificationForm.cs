using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.Enterprise;
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
            if (this.watches == null)
                return;

            int width = 0;
            int height = 0;
            int start = 0;
            for (int i = 0; i < this.watches.Count; i++)
            {
                TableWatchControl ctrl = new TableWatchControl(this.watches[i]);
                ctrl.Location = new Point(0, start);
                
                flowMain.Controls.Add(ctrl);
                //ctrl.Anchor = AnchorStyles.Right;
                start += ctrl.Height;
                if (ctrl.Width > width)
                    width = ctrl.Width;

                height += ctrl.Height;
            }

            if (height > this.flowMain.Height)
                this.Height = 450;
            //this.pnlHelp.Width = width;
            this.Width = width + 40;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("ScriptChangeSettings");
        }

    }
}

