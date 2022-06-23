using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{

    public class Program
    {
        static Program()
        {
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Program>(applicationLogFileName);
        }
        public const string applicationLogFileName = "SqlBuildManager.Console.log";

        private static Microsoft.Extensions.Logging.ILogger log;
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };
        private static CancellationToken cancellationToken = new CancellationToken();
        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args);//.Build().Run();
            await host.RunConsoleAsync(Program.cancellationToken);
            return Environment.ExitCode;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<Worker.StartArgs>(new Worker.StartArgs(args));
                    services.AddSingleton<CommandLineArgs>();
                    services.AddHostedService<Worker>();
                })
                .ConfigureAppConfiguration((hostContext, appConfiguration) =>
                {
                    appConfiguration.SetBasePath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    appConfiguration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
                    appConfiguration.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                    logging.AddFilter("System", LogLevel.Warning);
                });
            return builder;
        }

    }
}



