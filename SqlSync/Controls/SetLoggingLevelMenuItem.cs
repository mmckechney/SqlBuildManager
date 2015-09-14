using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using System.IO;
namespace SqlSync.Controls
{
    public partial class SetLoggingLevelMenuItem : ToolStripMenuItem
    {
        log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            this.dEBUGToolStripMenuItem.Text = "DEBUG";
            this.dEBUGToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // iNFOToolStripMenuItem
            // 
            this.iNFOToolStripMenuItem.Name = "iNFOToolStripMenuItem";
            this.iNFOToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.iNFOToolStripMenuItem.Text = "INFO";
            this.iNFOToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // wARNToolStripMenuItem
            // 
            this.wARNToolStripMenuItem.Name = "wARNToolStripMenuItem";
            this.wARNToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.wARNToolStripMenuItem.Text = "WARN";
            this.wARNToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);
            // 
            // eRRORToolStripMenuItem
            // 
            this.eRRORToolStripMenuItem.Name = "eRRORToolStripMenuItem";
            this.eRRORToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.eRRORToolStripMenuItem.Text = "ERROR";
            this.eRRORToolStripMenuItem.Click += new System.EventHandler(this.mnuLoggingLevel_Click);


        }

        void SetLoggingLevelMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            dEBUGToolStripMenuItem.Checked = false;
            wARNToolStripMenuItem.Checked = false;
            eRRORToolStripMenuItem.Checked = false;
            iNFOToolStripMenuItem.Checked = false;

            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            string level = rootLogger.Level.DisplayName.ToUpper();
            switch (level)
            {
                case "DEBUG":
                    dEBUGToolStripMenuItem.Checked = true;
                    break;
                case "WARN":
                    wARNToolStripMenuItem.Checked = true;
                    break;
                case "ERROR":
                    eRRORToolStripMenuItem.Checked = true;
                    break;
                case "INFO":
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
            //There is only the root logger at the moment, but this is good code so I'll keep it around...
            //Found at: http://geekswithblogs.net/rakker/archive/2007/08/22/114900.aspx

            //log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();

            //foreach (log4net.Repository.ILoggerRepository repository in repositories)
            //{
            //    repository.Threshold = repository.LevelMap[level];
            //    log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
            //    log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
            //    foreach (log4net.Core.ILogger logger in loggers)
            //    {
            //        ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];
            //    }
            //}

            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];

            log.InfoFormat("Logging level set to {0}", level);

        }
    }
}
