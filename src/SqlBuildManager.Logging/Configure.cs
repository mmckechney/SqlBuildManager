
using System;
using System.IO;
using System.Threading;
namespace SqlBuildManager.Logging
{
    public class Configure
    {
        public static string AppDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager");
            }
        }

        public static void CloseAndFlushAllLoggers()
        {
            Threaded.Configure.CloseAndFlushAllLoggers(false);
            ApplicationLogging.CloseAndFlush();

            //Needed to make sure all files are saved and flushed
            Thread.Sleep(1000);
        }

    }
}
