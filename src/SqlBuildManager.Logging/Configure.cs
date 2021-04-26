
using System;
using System.IO;
using System.Reflection;
using SqlBuildManager.Logging.Threaded;
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
            Threaded.Configure.CloseAndFlushAllLoggers();
            ApplicationLogging.CloseAndFlush();

            //Needed to make sure all files are saved and flushed
            Thread.Sleep(3000);
        }
        
    }
}
