using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using log4net;
using System.Configuration.Install;
using System.Reflection;
using SqlSync.SqlBuild;
namespace SqlBuildManager.Services.Host
{
    static class Program
    {
    
        static ILog log = log4net.LogManager.GetLogger(typeof(Program));
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            if (System.Environment.UserInteractive)
            {
                log4net.Config.XmlConfigurator.Configure();
                try
                {
                    var arguments = CommandLine.Arguments.ParseArguments(args);

                    if (args.Length == 0 || arguments.ContainsKey("?") || arguments.ContainsKey("help"))
                    {
                        System.Console.WriteLine(Properties.Resources.ConsoleHelp);
                        Environment.Exit(0);
                    }

                    if (arguments.ContainsKey("install") || arguments.ContainsKey("reinstall"))
                    {
                        if (!arguments.ContainsKey("username") || !arguments.ContainsKey("password"))
                        {
                            Console.Error.WriteLine("Installation requires both a /username and a /password commandline argument");
                            Environment.Exit(98);
                        }

                        if (string.IsNullOrEmpty(arguments["username"]) || string.IsNullOrEmpty(arguments["password"]))
                        {
                            Console.Error.WriteLine("Installation requires values for both the /username and /password commandline arguments");
                            Environment.Exit(99);
                        }


                        if (arguments.ContainsKey("install") && IsServiceInstalled("SqlBuildManager.Service"))
                        {
                            Console.Error.WriteLine("Cannot run installation, service is already installed. Use the /reinstall flag instead");
                            Environment.Exit(96);
                        }

                        ProjectInstaller.Username = arguments["username"];
                        ProjectInstaller.Password = arguments["password"];



                        if(arguments.ContainsKey("reinstall") && IsServiceInstalled("SqlBuildManager.Service"))
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        }

                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

                    }
                    else if (arguments.ContainsKey("uninstall"))
                    {
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                    }
                }
                catch(Exception exe)
                {
                    log.Error("Install failed", exe);
                    if (exe.StackTrace.IndexOf("username", StringComparison.InvariantCultureIgnoreCase) > -1 || exe.StackTrace.IndexOf("password", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        Console.WriteLine("Error with your credentials. Please check your password. You may also need to add '.\' as a prefix to your username");
                    }
                    Environment.Exit(97);
                }

                
            }
            else
            {
                    ServiceBase.Run(new SbmService());
            }
           
        }


        private static bool IsServiceInstalled(string serviceName)
        {
            // Get a list of current services
            ServiceController[] services = ServiceController.GetServices();

            return services.Where(s => s.ServiceName == serviceName).Any();
        }
    }
}
