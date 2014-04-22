using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace SqlBuildManager.Services.Host
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }


        private void serviceProcessInstaller1_BeforeUninstall(object sender, InstallEventArgs e)
        {

            try
            {
                using (ServiceController controller = new ServiceController(this.SbmService.ServiceName, Environment.MachineName))
                    controller.Stop();
            }
            catch
            { }
        }

        private void serviceProcessInstaller1_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                using (ServiceController controller = new ServiceController(this.SbmService.ServiceName))
                    controller.Start();
            }
            catch
            { }
        }

        private void serviceProcessInstaller1_BeforeInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
