using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
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
        private static RoleManager roleManager = null;
        public static RoleManager RoleManager
        {
            get
            {
                if(roleManager == null)
                {
                    roleManager = new RoleManager();
                }
                return roleManager;
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
            try
            {
               
                success = AzureEntryPoint.RoleManager.InsertCloudRoleEntity(Environment.MachineName, GetIpAddress());
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
                if (AzureEntryPoint.roleManager == null)
                {
                    roleManager = new RoleManager();
                }
                success = AzureEntryPoint.RoleManager.DeleteCloudRoleEntity(Environment.MachineName);
            }
            catch (Exception exe)
            {
                log.Error("Error unregistering service role with Table Storage", exe);
            }

            if (!success)
            {
                log.Warn("Unable to unregister service.");
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

        
    }
}

