using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;

namespace SqlSync.Controls
{
    public partial class SetLoggingLevelMenuItem : ToolStripMenuItem
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ToolStripMenuItem dEBUGToolStripMenuItem;
        private ToolStripMenuItem iNFOToolStripMenuItem;
        private ToolStripMenuItem wARNToolStripMenuItem;
        private ToolStripMenuItem eRRORToolStripMenuItem;

        public SetLoggingLevelMenuItem()
        {
            InitializeComponent();
            CustomInitializeComponent();
        }
        private void CustomInitializeComponent()
        {
            Text = "Set Logging Level";
            Size = new System.Drawing.Size(251, 22);
            DropDownOpening += new EventHandler(SetLoggingLevelMenuItem_DropDownOpening);

            dEBUGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            iNFOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            wARNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            eRRORToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            DropDownItems.AddRange(new ToolStripItem[]{dEBUGToolStripMenuItem,
                iNFOToolStripMenuItem,
                wARNToolStripMenuItem,
                eRRORToolStripMenuItem});
            // 
            // dEBUGToolStripMenuItem
            // 
            dEBUGToolStripMenuItem.Name = "dEBUGToolStripMenuItem";
            dEBUGToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            dEBUGToolStripMenuItem.Text = "Debug";
            dEBUGToolStripMenuItem.Click += new System.EventHandler(mnuLoggingLevel_Click);
            // 
            // iNFOToolStripMenuItem
            // 
            iNFOToolStripMenuItem.Name = "iNFOToolStripMenuItem";
            iNFOToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            iNFOToolStripMenuItem.Text = "Informnation";
            iNFOToolStripMenuItem.Click += new System.EventHandler(mnuLoggingLevel_Click);
            // 
            // wARNToolStripMenuItem
            // 
            wARNToolStripMenuItem.Name = "wARNToolStripMenuItem";
            wARNToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            wARNToolStripMenuItem.Text = "Warning";
            wARNToolStripMenuItem.Click += new System.EventHandler(mnuLoggingLevel_Click);
            // 
            // eRRORToolStripMenuItem
            // 
            eRRORToolStripMenuItem.Name = "eRRORToolStripMenuItem";
            eRRORToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            eRRORToolStripMenuItem.Text = "Error";
            eRRORToolStripMenuItem.Click += new System.EventHandler(mnuLoggingLevel_Click);


        }

        void SetLoggingLevelMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            dEBUGToolStripMenuItem.Checked = false;
            wARNToolStripMenuItem.Checked = false;
            eRRORToolStripMenuItem.Checked = false;
            iNFOToolStripMenuItem.Checked = false;


            LogLevel level = SqlBuildManager.Logging.ApplicationLogging.GetLogLevel();
            switch (level)
            {
                case LogLevel.Debug:
                    dEBUGToolStripMenuItem.Checked = true;
                    break;
                case LogLevel.Warning:
                    wARNToolStripMenuItem.Checked = true;
                    break;
                case LogLevel.Error:
                    eRRORToolStripMenuItem.Checked = true;
                    break;
                case LogLevel.Information:
                    iNFOToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void mnuLoggingLevel_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripItem)
            {
                string value = ((ToolStripItem)sender).Text;
                SetDynamicLoggingLevel(value);
            }
        }
        private void SetDynamicLoggingLevel(string level)
        {
            if (!Enum.TryParse<LogLevel>(level, out LogLevel tmp))
            {
                tmp = LogLevel.Information;
            }
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(tmp);


            log.LogInformation($"Logging level set to {tmp.ToString()}");

        }
    }
}
