using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using SqlSync.Connection;
using SqlSync.SqlBuild.Syncronizer;
namespace SqlBuildManager.Console
{
    public class Synchronize
    {
        public static string GetDatabaseRunHistoryDifference(string[] args)
        {
            CommandLineArgs cmdLine = ParseAndValidateFlags(args);
            if (cmdLine == null)
            {
                return null;
            }

            var history = GetDatabaseRunHistoryDifference(cmdLine);
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


        public static bool SyncDatabases(string[] args)
        {
            CommandLineArgs cmdLine = ParseAndValidateFlags(args);
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
            System.Console.WriteLine(message);
        }

        private static CommandLineArgs ParseAndValidateFlags(string[] args)
        {
            CommandLineArgs cmdLine = CommandLine.ParseCommandLineArg(args);

            if (string.IsNullOrEmpty(cmdLine.GoldDatabase))
            {
                System.Console.WriteLine("Missing /GoldDatabase=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.GoldServer))
            {
                System.Console.WriteLine("Missing /GoldServer=\"<server>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Database))
            {
                System.Console.WriteLine("Missing /Database=\"<database>\" flag");
                return null;
            }

            if (string.IsNullOrEmpty(cmdLine.Server))
            {
                System.Console.WriteLine("Missing /Server=\"<server>\" flag");
                return null;
            }
            return cmdLine;
        }

        
    }
}
