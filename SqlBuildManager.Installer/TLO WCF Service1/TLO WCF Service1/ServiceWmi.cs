using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Management.Instrumentation;
using Paychex.Tlo.Wcf.Services.Behaviors.WMI;
namespace TLO_WCF_Service1
{
    [InstrumentationClass(InstrumentationType.Instance)]
    public class ServiceWmi : WmiServiceMonitorBase
    {
        public ServiceWmi()
        {
        }
    }

    // Let the system know that the InstallUtil.exe tool will be run
    // against this assembly in order to register the .dll's schema to WMI.
    [System.ComponentModel.RunInstaller(true)]
    public class TloWmiInstaller : DefaultManagementProjectInstaller
    {
        public TloWmiInstaller()
        {
            ManagementInstaller mgmtInstaller = new ManagementInstaller();
            Installers.Add(mgmtInstaller);
        }
    }


}