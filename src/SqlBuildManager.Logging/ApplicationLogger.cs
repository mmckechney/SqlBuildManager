using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace SqlBuildManager.Logging
{
    public class ApplicationLogging
	{
		private static ILoggerFactory Factory = null;

		private static string _rootLoggingFile = string.Empty;
		private static bool resetPath = false;
		public static void ConfigureStandardLogger(ILoggerFactory factory)
		{
			// var serilogLogger = new LoggerConfiguration()
			// 	.MinimumLevel.Debug()
			// 	.Enrich.WithThreadId()
			// 	.Enrich.WithThreadName()
			// 	.WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} - {Message}{NewLine}{Exception}")
			// 	.WriteTo.RollingFile("logFileFromHelper.log", outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} - {Message}{NewLine}{Exception}")
			// 	.CreateLogger();

			// factory.AddSerilog(serilogLogger);

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			string fileOutputTemplate;
			try
			{
				fileOutputTemplate = configuration["Serilog:WriteTo:1:Args:outputTemplate"];
			}
			catch
            {
				fileOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} - {Message}{NewLine}{Exception}";

			}

			Serilog.Core.Logger logger;

			if (string.IsNullOrWhiteSpace(_rootLoggingFile))
			{
				logFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", "SqlBuildManager.Console.log");

				logger = new LoggerConfiguration()
					.ReadFrom.Configuration(configuration)
					.CreateLogger();
			}
			else
            {
				logFileName = Path.Combine(_rootLoggingFile, "SqlBuildManager.Console.log");
				logger = new LoggerConfiguration()
					.ReadFrom.Configuration(configuration)
					.WriteTo.File(logFileName, outputTemplate : fileOutputTemplate)
					.CreateLogger();
			}
			factory.AddSerilog(logger);
	
		}

		public static ILoggerFactory LoggerFactory
		{
			get
			{
				if (Factory == null || resetPath)
				{
					Factory = new LoggerFactory();
					ConfigureStandardLogger(Factory);
					resetPath = false;
				}
				return Factory;
			}
			set { Factory = value; }
		}
		public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>() => LoggerFactory.CreateLogger(typeof(T));
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type) => LoggerFactory.CreateLogger(type);
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type, string rootLoggingPath)
		{
			resetPath = true;
			_rootLoggingFile = rootLoggingPath;
			return LoggerFactory.CreateLogger(type);
		}
 

		public static bool IsDebug()
		{
			return Log.IsEnabled(Serilog.Events.LogEventLevel.Debug);
		}

		private static string logFileName = string.Empty;
		public static string LogFileName
		{
			get
			{
				return logFileName;
			}
		}

		public static void FlushLogs()
        {
			Log.CloseAndFlush();
        }


	}
}
