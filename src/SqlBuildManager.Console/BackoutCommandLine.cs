using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
namespace SqlBuildManager.Console
{
    class BackoutCommandLine
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static string CreateBackoutPackage(CommandLineArgs cmdLine)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.LogError("--PackageName argument is required when creating a backout package");
            }
            string sourcePackageName = cmdLine.BuildFileName;
            if (cmdLine.Server.Length == 0 || cmdLine.Database.Length == 0)
            {
                log.LogError("--server and --database arguments are required when creating a backout package");
                return string.Empty;
            }

            ConnectionData connectionData = new ConnectionData(cmdLine.Server, cmdLine.Database);
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
