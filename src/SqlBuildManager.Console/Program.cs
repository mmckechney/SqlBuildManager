using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Kubernetes;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SqlSync.SqlBuild.SqlSyncBuildData;
using sb = SqlSync.SqlBuild;
using SqlBuildManager.Console.CloudStorage;
using System.CommandLine.IO;
using SqlBuildManager.Console.KeyVault;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

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



