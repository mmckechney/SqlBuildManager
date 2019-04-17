using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SqlSync.SqlBuild;
using log4net;
using SqlSync.ObjectScript;
using SqlSync.Connection;
namespace SqlBuildManager.Console
{
    class BackoutCommandLine
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static string CreateBackoutPackage(CommandLineArgs cmdLine)
        {
            
            if(string.IsNullOrWhiteSpace(cmdLine.PackageName))
            {
                log.Error("/PackageName argument is required when creating a backout package");
            }
            string sourcePackageName = cmdLine.PackageName;
            if (cmdLine.Server.Length == 0 || cmdLine.Database.Length == 0)
            {
                log.Error("/server and /database arguments are required when creating a backout package");
                return string.Empty;
            }

            ConnectionData connectionData = new ConnectionData(cmdLine.Server,cmdLine.Database);
            if (cmdLine.AuthenticationArgs.Password.Length > 0)
                connectionData.Password = cmdLine.AuthenticationArgs.Password;
            if (cmdLine.AuthenticationArgs.UserName.Length > 0)
                connectionData.UserId = cmdLine.AuthenticationArgs.UserName;

            if (connectionData.UserId.Length > 0 && connectionData.Password.Length > 0)
            {
                connectionData.AuthenticationType = AuthenticationType.Password;
            }
            else
            {
                connectionData.AuthenticationType = AuthenticationType.Windows;
            }

            return SqlSync.ObjectScript.BackoutPackage.CreateDefaultBackoutPackage(connectionData, sourcePackageName, cmdLine.Server, cmdLine.Database);
        }
    }
}
