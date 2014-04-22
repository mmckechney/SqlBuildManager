namespace SqlBuildManager.Services.Host
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.SbmService = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            this.serviceProcessInstaller1.Committed += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_Committed);
            this.serviceProcessInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            this.serviceProcessInstaller1.BeforeInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_BeforeInstall);
            this.serviceProcessInstaller1.BeforeUninstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_BeforeUninstall);
            // 
            // SbmService
            // 
            this.SbmService.Description = "Agent service for receiving remote execution commands from Sql Build Manager";
            this.SbmService.ServiceName = "SqlBuildManager.Service";
            this.SbmService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.SbmService});

        }

        #endregion

        private const string password = "password";
        private const string username = "username";
        public override void Install(System.Collections.IDictionary stateSaver)
        {

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
                System.Console.WriteLine("found custom username");
                this.serviceProcessInstaller1.Username = stateSaver[username].ToString();
            }
            else
            {
                System.Console.WriteLine("no custom username");
            }

            base.Install(stateSaver);
        }

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller SbmService;
    }
}