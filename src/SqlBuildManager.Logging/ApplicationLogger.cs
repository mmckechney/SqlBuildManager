using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SqlBuildManager.Logging
{
	public class ApplicationLogging
	{
		private static ILoggerFactory _Factory = null;

		public static void ConfigureLogger(ILoggerFactory factory)
		{
			var serilogLogger = new LoggerConfiguration()
		    .MinimumLevel.Debug()
			.Enrich.WithThreadId()
			.Enrich.WithThreadName()
		   .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} - {Message}{NewLine}{Exception}")
		   .WriteTo.RollingFile("logFileFromHelper.log", outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff } {Level:u3} TH:{ThreadId,3}] {SourceContext} -{Message}{NewLine}{Exception}")
		   .CreateLogger();

			factory.AddSerilog(serilogLogger);
			
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
	}
}
