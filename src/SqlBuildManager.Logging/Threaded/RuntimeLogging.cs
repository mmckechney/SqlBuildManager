﻿using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
namespace SqlBuildManager.Logging.Threaded
{
    public class RuntimeLogging
    {
        private static ILoggerFactory _LoggerFactory = null;
        private static string _rootLoggingPath = string.Empty;
        private static Serilog.Core.Logger serilogLogger = null;
        public static void ConfigureLogger(ILoggerFactory factory)
        {

            serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                 .WriteTo.Async(a => a.File(Path.Combine(_rootLoggingPath, "SqlBuildManager.ThreadedExecution.log"), outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff }] {Message}{NewLine}"))
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
