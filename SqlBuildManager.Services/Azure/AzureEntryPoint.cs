using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using log4net.Appender;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading;
using SqlBuildManager.AzureStorage;
using System.Net.NetworkInformation;
namespace SqlBuildManager.Services.Azure
{
    class AzureEntryPoint : RoleEntryPoint 
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static RoleManager storageRoleManager = null;
        public static RoleManager StorageRoleManager
        {
            get
            {
                if(storageRoleManager == null)
                {
                    storageRoleManager = new SqlBuildManager.AzureStorage.RoleManager();
                }
                return storageRoleManager;
            }
        }
        public override void Run()  /* Worker Role entry point */
        {
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public override bool OnStart()
        {
            bool success = false;
            InitializeLogging();
            try
            {
                success = AzureEntryPoint.StorageRoleManager.InsertCloudRoleEntity(Environment.MachineName, GetIpAddress());
            }
            catch(Exception exe)
            {
                log.Error("Error registering service role with Table Storage", exe);
            }
            
            if(!success)
            {
                log.Warn("Unable to register service.");
            }
            return base.OnStart();
        }

        public override void OnStop()
        {
            bool success = false;
            try
            {
                if (AzureEntryPoint.storageRoleManager == null)
                {
                    storageRoleManager = new RoleManager();
                }
                success = AzureEntryPoint.StorageRoleManager.DeleteCloudRoleEntity(Environment.MachineName);
            }
            catch (Exception exe)
            {
                log.Error("Error unregistering service role with Table Storage", exe);
            }

            if (!success)
            {
                log.Warn("Unable to unregister service.");
            }

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

                //var tableAppender = log.Logger.Repository.GetAppenders().Where(a => a.Name == "AzureTableAppender").First();
                //if(tableAppender != null && tableAppender is log4net.Appender.AzureTableAppender)
                //{
                //    string connection = CloudConfigurationManager.GetSetting("StorageConnectionString");
                //    ((AzureTableAppender)tableAppender).ConnectionString = connection;
                //}
                log.Info("Initialized Logging!");
            }
            catch (Exception exe)
            {
                System.IO.File.WriteAllText("C:\\Errorlog.txt", string.Format("Unable to configure Log4Net: {0}", exe.ToString()));
            }

        }

        
    }
}

