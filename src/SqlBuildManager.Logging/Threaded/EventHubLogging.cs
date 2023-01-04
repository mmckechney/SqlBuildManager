using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
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
        private static Serilog.Core.Logger serilogLogger = null;

        private static string _EventHubName = string.Empty;
        private static string _EventHubNamespace = string.Empty;
        private static string _ManagedIdentityIdClient = string.Empty;
        public static void ConfigureEventHubLogger(ILoggerFactory factory)
        {

            EventHubProducerClient eventHubClient;
            if (!string.IsNullOrWhiteSpace(_EventHubConnectionString))
            {
                eventHubClient = new EventHubProducerClient(_EventHubConnectionString);
            }
            else
            {
                eventHubClient = new EventHubProducerClient(_EventHubNamespace, _EventHubName, GetAadTokenCredential());
            }

            serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.AzureEventHub(new JsonFormatter(), eventHubClient, writeInBatches: true, batchPostingLimit: 50)
                .CreateLogger();

            _LoggerFactory.AddSerilog(serilogLogger);
        }
        private static TokenCredential GetAadTokenCredential()
        {
            TokenCredential _tokenCred;
            if (string.IsNullOrWhiteSpace(_ManagedIdentityIdClient))
            {
                _tokenCred = new DefaultAzureCredential();
            }
            else
            {
                _tokenCred = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions()
                    {
                        ManagedIdentityClientId = _ManagedIdentityIdClient,
                        ExcludeAzureCliCredential = false
                    });
            }
            return _tokenCred;
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
        public static Microsoft.Extensions.Logging.ILogger CreateLogger(Type type, string eventHubNamespace, string eventHubName, string managedIdentityClientId)
        {
            _EventHubNamespace = eventHubNamespace;
            _EventHubName = eventHubName;
            _ManagedIdentityIdClient = managedIdentityClientId;
            try
            {
                return EventHubLoggerFactory.CreateLogger(type);
            }
            catch
            {
                return null;
            }
        }

        public static void CloseAndFlush()
        {
            if (serilogLogger != null)
            {
                serilogLogger.Dispose();
            }
            _LoggerFactory = null;
        }

    }
}
