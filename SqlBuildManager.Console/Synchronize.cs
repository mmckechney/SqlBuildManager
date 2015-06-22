using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using SqlSync.Connection;
using SqlSync.SqlBuild.Syncronizer;
using log4net;
namespace SqlBuildManager.Console
{
    public class Synchronize
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetDatabaseRunHistoryTextDifference(CommandLineArgs cmdLine)
        {
            cmdLine = ValidateFlags(cmdLine);
            if (cmdLine == null)
            {
                return null;
            }

            DatabaseDiffer differ = new DatabaseDiffer();
            var history = differ.GetDatabaseHistoryDifference(cmdLine.GoldServer, cmdLine.GoldDatabase, cmdLine.Server, cmdLine.Database);
             //= GetDatabaseRunHistoryDifference(cmdLine);
            string header = String.Format("{0} Package differences found between Gold:[{1}].[{2}] and Target:[{3}].[{4}]\r\n", history.BuildFileHistory.Count(),cmdLine.GoldServer,cmdLine.GoldDatabase,cmdLine.Server,cmdLine.Database);
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
            return differ.GetDatabaseHistoryDifference(cmdLine.GoldServer, cmdLine.GoldDatabase, cmdLine.Server,
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
            
            return dbSync.SyncronizeDatabases(cmdLine.GoldServer, cmdLine.GoldDatabase, cmdLine.Server,
                                       cmdLine.Database,cmdLine.ContinueOnFailure);

        }

        static void dbSync_SyncronizationInfoEvent(string message)
        {
            log.Info(message);
        }

        private static CommandLineArgs ValidateFlags(CommandLineArgs cmdLine)
        {

            if (string.IsNullOrEmpty(cmdLine.GoldDatabase))
            {
                log.Error("Missing /GoldDatabase=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.GoldServer))
            {
                log.Error("Missing /GoldServer=\"<server>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Database))
            {
                log.Error("Missing /Database=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Server))
            {
                log.Error("Missing /Server=\"<server>\" flag");
                return null;
            }
            return cmdLine;
        }

        
    }
}
