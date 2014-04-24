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
        internal static string CreateBackoutPackage(string[] args)
        {
            string sourcePackageName = args[1];
            var cmdLine = CommandLine.ParseCommandLineArg(args);

            if (cmdLine.Server.Length == 0 || cmdLine.Database.Length == 0)
            {
                log.Error("/server and /database arguments are required when creating a backout package");
                return string.Empty;
            }

            ConnectionData connectionData = new ConnectionData(cmdLine.Server,cmdLine.Database);
            if (cmdLine.Password.Length > 0)
                connectionData.Password = cmdLine.Password;
            if (cmdLine.UserName.Length > 0)
                connectionData.UserId = cmdLine.UserName;

            if (connectionData.UserId.Length > 0 && connectionData.Password.Length > 0)
            {
                connectionData.UseWindowAuthentication = false;
            }
            else
            {
                connectionData.UseWindowAuthentication = true;
            }

            return SqlSync.ObjectScript.BackoutPackage.CreateDefaultBackoutPackage(connectionData, sourcePackageName, cmdLine.Server, cmdLine.Database);
        }
    }
}
