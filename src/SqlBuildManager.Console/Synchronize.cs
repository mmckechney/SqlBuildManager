using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlSync.SqlBuild.Syncronizer;
using System;
using System.Linq;
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
            if(cmdLine == null)
            {
                return null;
            }
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
                log.LogError("Missing --golddatabase=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.SynchronizeArgs.GoldServer))
            {
                log.LogError("Missing --goldserver=\"<server>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Database))
            {
                log.LogError("Missing --database=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Server))
            {
                log.LogError("Missing --server=\"<server>\" flag");
                return null;
            }
            return cmdLine;
        }

        
    }
}
