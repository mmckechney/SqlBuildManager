using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using Microsoft.Extensions.Configuration;


namespace SqlBuildManager.Logging
{
	public class ApplicationLogging
	{
		private static ILoggerFactory _Factory = null;
		private static string _LogFileName = string.Empty;


		public static void ConfigureLogger(ILoggerFactory factory)
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

			var logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();

			factory.AddSerilog(logger);
	
		}

		public static ILoggerFactory LoggerFactory
		{
			get
			{
				if (_Factory == null)
				{
					_Factory = new LoggerFactory();
					ConfigureLogger(_Factory);
				}
				return _Factory;
			}
			set { _Factory = value; }
		}
		public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>() => LoggerFactory.CreateLogger(typeof(T));
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type) => LoggerFactory.CreateLogger(type);

		public static bool IsDebug()
		{
			return Log.IsEnabled(Serilog.Events.LogEventLevel.Debug);
		}

		public static string LogFileName
		{
			get
			{
				//TODO: Actually set the file name!
				return _LogFileName;
			}
		}
	}
}
