using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Collections.Generic;
namespace SqlBuildManager.Logging
{
    public class ApplicationLogging
	{
		private static ILoggerFactory Factory = null;
		private static string _logFileName = string.Empty;
		private static bool addPath = false;
		private static Microsoft.Extensions.Logging.LogLevel logLevel = Microsoft.Extensions.Logging.LogLevel.Information;
		private static LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch();
		private static List<string> loggingPaths = new List<string>();
		private static Serilog.Core.Logger serilogLogger = null;
		public static void ConfigureStandardLogger(ILoggerFactory factory)
		{
    		var logOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} - {Message}{NewLine}{Exception}";
			var consoleOutput = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,2}] {Message}{NewLine}{Exception}";

			var cfg = new LoggerConfiguration()
					.MinimumLevel.ControlledBy(levelSwitch)
					.Enrich.WithThreadId()
					.Enrich.WithThreadName()
					.WriteTo.Console(outputTemplate: consoleOutput);
			foreach(var file in loggingPaths)
            {
				cfg.WriteTo.Async(a => a.File(file, outputTemplate: logOutputTemplate, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: false, shared: true));
			}
			serilogLogger = cfg.CreateLogger();

			factory.AddSerilog(serilogLogger);
	
		}

		public static ILoggerFactory LoggerFactory
		{
			get
			{
				if (Factory == null || addPath)
				{
					Factory = new LoggerFactory();
					ConfigureStandardLogger(Factory);
					addPath = false;
				}
				return Factory;
			}
			set { Factory = value; }
		}
		public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>() => LoggerFactory.CreateLogger(typeof(T));
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type) => LoggerFactory.CreateLogger(type);
		public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>(string logFileName) => CreateLogger<T>(logFileName, string.Empty);
		public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>(string logFileName, string rootLoggingPath)
		{
			if(string.IsNullOrWhiteSpace(rootLoggingPath))
            {
				LogFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", logFileName);
			}
			else
            {
				LogFileName = Path.Combine(rootLoggingPath, logFileName);
            }
			addPath = true;
			return LoggerFactory.CreateLogger(typeof(T));
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
			logLevel = level;
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


		public static string LogFileName
		{
			get
			{
				return _logFileName;
			}
            private set
            {
				if (!Path.IsPathFullyQualified(value))
                {
					_logFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", value);
				}
				else
                {
					_logFileName = value;

				}

				_logFileName = Path.GetFullPath(_logFileName); //Fix to make sure we have a proper format
				if (!loggingPaths.Contains(_logFileName))
				{
					loggingPaths.Add(_logFileName);
				}

			}
		}

		public static void CloseAndFlush()
		{
			if (serilogLogger != null)
			{
				serilogLogger.Dispose();
			}
			Factory = null;

		}


	}
}
