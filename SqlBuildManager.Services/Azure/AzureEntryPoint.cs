using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using log4net.Appender;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading;
using System.Net.NetworkInformation;
namespace SqlBuildManager.Services.Azure
{
    class AzureEntryPoint : RoleEntryPoint 
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public override void Run()  /* Worker Role entry point */
        {
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public override bool OnStart()
        {
            InitializeLogging();
           
            return base.OnStart();
        }

        public override void OnStop()
        {
            var appenders =log.Logger.Repository.GetAppenders();
            foreach(var appender in appenders)
            {
                if(appender is BufferingAppenderSkeleton)
                {
                    ((BufferingAppenderSkeleton)appender).Flush();
                }
            }

        }

        private string GetIpAddress()
        {
            var ethernet = NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
            if (ethernet.Any())
            {
                var ip = ethernet.First().GetIPProperties().UnicastAddresses.Where(u => u.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ip.Any())
                {
                    return ip.First().Address.ToString();
                }

            }

            return "0.0.0.0";
        }

        private void InitializeLogging()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                if (!log.Logger.Repository.Configured)
                {
                    log4net.Config.BasicConfigurator.Configure(new log4net.Appender.EventLogAppender());
                }

                var appender = log.Logger.Repository.GetAppenders().Select(a => a.Name).Aggregate((i,j) => i +"; "+ j);
                log.InfoFormat("Configured Appenders: {0}", appender);
                log.Info("Initialized Logging!");
            }
            catch (Exception exe)
            {
                System.IO.File.WriteAllText("C:\\Errorlog.txt", string.Format("Unable to configure Log4Net: {0}", exe.ToString()));
            }

            BuildService.SetAppLogFileLocation();

        }

        
    }
}

