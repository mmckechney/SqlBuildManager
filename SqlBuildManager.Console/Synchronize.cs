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
            return history.ToString();

        }
        public static DatabaseRunHistory GetDatabaseRunHistoryDifference(CommandLineArgs cmdLine)
        {
            DatabaseDiffer differ = new DatabaseDiffer();
            return differ.GetDatabaseHistoryDifference(cmdLine.GoldServer, cmdLine.GoldDatabase, cmdLine.Server,
                                                cmdLine.Database);
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
