using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using System;

namespace SqlBuildManager.Logging.Threaded
{
    public class EventHubLogging
    {



		private static ILoggerFactory _LoggerFactory = null;
		private static string _EventHubConnectionString = string.Empty;

		public static void ConfigureEventHubLogger(ILoggerFactory factory)
		{

			EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(_EventHubConnectionString);


			var serilogLogger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.AzureEventHub(new JsonFormatter(), eventHubClient, writeInBatches: true, batchPostingLimit: 50)
				.CreateLogger();

			_LoggerFactory.AddSerilog(serilogLogger);
		}

		public static ILoggerFactory EventHubLoggerFactory
		{
			get
			{
				if (_LoggerFactory == null)
				{
					_LoggerFactory = new LoggerFactory();
					ConfigureEventHubLogger(_LoggerFactory);
				}
				return _LoggerFactory;
			}
			set { _LoggerFactory = value; }
		}
		public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type, string connectionString)
		{
			_EventHubConnectionString = connectionString;
			try
			{
				return EventHubLoggerFactory.CreateLogger(type);
			}
			catch
			{
				return null;
			}
		}
	}
}
