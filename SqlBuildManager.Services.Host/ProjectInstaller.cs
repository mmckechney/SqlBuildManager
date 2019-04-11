using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using log4net;
namespace SqlBuildManager.Services.Host
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        static ILog log = log4net.LogManager.GetLogger(typeof(ProjectInstaller));
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        internal static string Password { set; private get; }
        internal static string Username { set; private get; }

        private const string password = "password";
        private const string username = "username";

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            this.serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Username = null;
            this.serviceProcessInstaller1.Password = null;
            if (stateSaver.Count == 0)
            {
                stateSaver.Add(username, ProjectInstaller.Username);
                stateSaver.Add(password, ProjectInstaller.Password);
            }

            if (stateSaver.Contains(password))
            {
                this.serviceProcessInstaller1.Password = stateSaver[password].ToString();
                System.Console.WriteLine("found custom password");
            }
            else
            {
                System.Console.WriteLine("no custom password");
            }

            if (stateSaver.Contains(username))
            {
                this.serviceProcessInstaller1.Username = stateSaver[username].ToString();
                System.Console.WriteLine("found custom username");
            }
            else
            {
                System.Console.WriteLine("no custom username");
            }

            //base.Install(null);
            base.Install(stateSaver);
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

        private void SbmService_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
