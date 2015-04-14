using log4net;
using log4net.Appender;
using Microsoft.WindowsAzure.ServiceRuntime;
using SqlBuildManager.AzureStorage;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.Services.History;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
namespace SqlBuildManager.Services
{
    // NOTE: If you change the class name "BuildService" here, you must also update the reference to "BuildService" in Web.config.
    public class CloudBuildService : BuildService
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string buildHistoryFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";

        public CloudBuildService() :base()
        {
        }
       


       
    }
}
