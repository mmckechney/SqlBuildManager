using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace SqlBuildManager.Logging
{
    public class ApplicationLogging
	{
		private static ILoggerFactory Factory = null;
		private static string _rootLoggingFile = string.Empty;
		private static bool resetPath = false;
		private static LogLevel logLevel = LogLevel.Information;
		private static LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch();
		public static void ConfigureStandardLogger(ILoggerFactory factory)
		{

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

			//Get log level 
			 Enum.TryParse<LogLevel>(configuration["Serilog:MinimumLevel"], out logLevel);


			Serilog.Core.Logger logger;

			if (string.IsNullOrWhiteSpace(_rootLoggingFile))
			{
				logger = new LoggerConfiguration()
					.ReadFrom.Configuration(configuration)
					.MinimumLevel.ControlledBy(levelSwitch)
					.CreateLogger();
			}
			else
            {
				logFileName = Path.Combine(_rootLoggingFile, Path.GetFileName(logFileName));
				logger = new LoggerConfiguration()
					.ReadFrom.Configuration(configuration)
					.MinimumLevel.ControlledBy(levelSwitch)
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
		public static LogLevel GetLogLevel()
        {
			return logLevel;

		}
		public static void SetLogLevel(LogLevel level)
		{
			switch(level)
            {
				case LogLevel.Trace:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
					break;

				case LogLevel.Debug:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
					break;


				case LogLevel.Warning:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Warning;
					break;

				case LogLevel.Error:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Error;
					break;

				case LogLevel.Critical:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Fatal;
					break;

				case LogLevel.Information:
				default:
					levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
					break;
			}
			

		}

		private static string logFileName = string.Empty;
		public static string LogFileName
		{
			get
			{
				return logFileName;
			}
            set
            {
				if (!Path.IsPathFullyQualified(value))
                {
					logFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", value);
				}
				else
                {
					logFileName = value;

				}

			}
		}

		public static void FlushLogs()
        {
			Log.CloseAndFlush();
        }


	}
}
