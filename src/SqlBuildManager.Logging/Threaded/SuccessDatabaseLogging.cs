using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
namespace SqlBuildManager.Logging.Threaded
{
    public class SuccessDatabaseLogging
    {
		private static ILoggerFactory _LoggerFactory = null;
		private static string _rootLoggingPath = string.Empty;

		public static void ConfigureLogger(ILoggerFactory factory)
		{

			var serilogLogger = new LoggerConfiguration()
			   .MinimumLevel.Debug()
			   .WriteTo.File(Path.Combine(_rootLoggingPath, "SuccessDatabases.cfg"), outputTemplate: "{Message}{NewLine}")
			   .CreateLogger();

			_LoggerFactory.AddSerilog(serilogLogger);

		}

		public static ILoggerFactory LoggerFactory
		{
			get
			{
				if (_LoggerFactory == null)
				{
					_LoggerFactory = new LoggerFactory();
					ConfigureLogger(_LoggerFactory);
				}
				return _LoggerFactory;
			}
			set { _LoggerFactory = value; }
		}
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type, string rootLoggingPath)
		{
			_rootLoggingPath = rootLoggingPath;
			try
			{
				return LoggerFactory.CreateLogger(type);
			}
			catch
			{
				return null;
			}
		}
	}
}
