using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using SqlBuildManager.Services;
using log4net;
using System.Reflection;
namespace SqlBuildManager.Services.Host
{
    public partial class SbmService : ServiceBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static List<ServiceHost> serviceHosts = new List<ServiceHost>();
        public SbmService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceHost sbmBuildService = new ServiceHost(typeof(BuildService));

                sbmBuildService.Open();
                serviceHosts.Add(sbmBuildService);
                Log.Info("Successfully started BuildService on: " + sbmBuildService.BaseAddresses[0].AbsoluteUri);

            }
            catch (Exception exe)
            {
                Log.Error("Error starting BuildService " + exe.ToString());
                System.Diagnostics.EventLog.WriteEntry("SqlBuildManager.Service", exe.ToString(), EventLogEntryType.Error, 991);
            }
        }

        protected override void OnStop()
        {
            foreach (ServiceHost svc in SbmService.serviceHosts)
            {
                Log.Info("Stopping service on: " + svc.BaseAddresses[0].AbsoluteUri);
                svc.Close();
            }
            log4net.LogManager.Shutdown();

        }
    }
}
