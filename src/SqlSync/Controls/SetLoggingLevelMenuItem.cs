using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
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
            this.Text = "Set Logging Level";
            this.Size = new System.Drawing.Size(251, 22);
            this.DropDownOpening += new EventHandler(SetLoggingLevelMenuItem_DropDownOpening);

            this.dEBUGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iNFOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wARNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eRRORToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.DropDownItems.AddRange(new ToolStripItem[]{this.dEBUGToolStripMenuItem,
                this.iNFOToolStripMenuItem,
                this.wARNToolStripMenuItem,
                this.eRRORToolStripMenuItem});
            // 
            // dEBUGToolStripMenuItem
            // 
            this.dEBUGToolStripMenuItem.Name = "dEBUGToolStripMenuItem";
            this.dEBUGToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.dEBUGToolStripMenuItem.Text = "Debug";
            this.dEBUGToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // iNFOToolStripMenuItem
            // 
            this.iNFOToolStripMenuItem.Name = "iNFOToolStripMenuItem";
            this.iNFOToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.iNFOToolStripMenuItem.Text = "Informnation";
            this.iNFOToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // wARNToolStripMenuItem
            // 
            this.wARNToolStripMenuItem.Name = "wARNToolStripMenuItem";
            this.wARNToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.wARNToolStripMenuItem.Text = "Warning";
            this.wARNToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // eRRORToolStripMenuItem
            // 
            this.eRRORToolStripMenuItem.Name = "eRRORToolStripMenuItem";
            this.eRRORToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.eRRORToolStripMenuItem.Text = "Error";
            this.eRRORToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);


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
            if(!Enum.TryParse<LogLevel>(level, out LogLevel tmp))
            {
                tmp = LogLevel.Information;
            }
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(tmp);
          
           
            log.LogInformation($"Logging level set to {tmp.ToString()}");

        }
    }
}
