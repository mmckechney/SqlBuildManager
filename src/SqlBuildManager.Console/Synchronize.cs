﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using SqlSync.Connection;
using SqlSync.SqlBuild.Syncronizer;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Console
{
    public class Synchronize
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetDatabaseRunHistoryTextDifference(CommandLineArgs cmdLine)
        {
            cmdLine = ValidateFlags(cmdLine);
            if (cmdLine == null)
            {
                return null;
            }

            DatabaseDiffer differ = new DatabaseDiffer();
            var history = differ.GetDatabaseHistoryDifference(cmdLine.SynchronizeArgs.GoldServer, cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.Server, cmdLine.Database);
             //= GetDatabaseRunHistoryDifference(cmdLine);
            string header = String.Format("{0} Package differences found between Gold:[{1}].[{2}] and Target:[{3}].[{4}]\r\n", history.BuildFileHistory.Count(),cmdLine.SynchronizeArgs.GoldServer, cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.Server,cmdLine.Database);
            if(history.BuildFileHistory.Any())
                return header + history.ToString();
            else
            {
                return "No differences found";
            }

        }
        public static DatabaseRunHistory GetDatabaseRunHistoryDifference(CommandLineArgs cmdLine)
        {
            DatabaseDiffer differ = new DatabaseDiffer();
            return differ.GetDatabaseHistoryDifference(cmdLine.SynchronizeArgs.GoldServer, cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.Server,
                                                cmdLine.Database);
        }


        public static bool SyncDatabases(CommandLineArgs cmdLine)
        {
            cmdLine = ValidateFlags(cmdLine);
            if (cmdLine == null)
            {
                return false;
            }

             DatabaseSyncer dbSync = new DatabaseSyncer();
             dbSync.SyncronizationInfoEvent += new DatabaseSyncer.SyncronizationInfoEventHandler(dbSync_SyncronizationInfoEvent);
            
            return dbSync.SyncronizeDatabases(cmdLine.SynchronizeArgs.GoldServer, cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.Server,
                                       cmdLine.Database,cmdLine.ContinueOnFailure);

        }

        static void dbSync_SyncronizationInfoEvent(string message)
        {
            log.LogInformation(message);
        }

        internal static CommandLineArgs ValidateFlags(CommandLineArgs cmdLine)
        {

            if (string.IsNullOrEmpty(cmdLine.SynchronizeArgs.GoldDatabase))
            {
                log.LogError("Missing /GoldDatabase=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.SynchronizeArgs.GoldServer))
            {
                log.LogError("Missing /GoldServer=\"<server>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Database))
            {
                log.LogError("Missing /Database=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Server))
            {
                log.LogError("Missing /Server=\"<server>\" flag");
                return null;
            }
            return cmdLine;
        }

        
    }
}
