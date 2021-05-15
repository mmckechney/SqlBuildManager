
using System;
using System.IO;
using System.Reflection;
using SqlBuildManager.Logging.Threaded;
using System.Threading;
namespace SqlBuildManager.Logging.Threaded
{
    public class Configure
    {
        public static void CloseAndFlushAllLoggers(bool wait = true)
        {
            CommitLogging.CloseAndFlush();
            ErrorLogging.CloseAndFlush();
            EventHubLogging.CloseAndFlush();
            FailureDatabaseLogging.CloseAndFlush();
            RuntimeLogging.CloseAndFlush();
            SuccessDatabaseLogging.CloseAndFlush();

            //Needed to make sure all files are saved and flushed
            if (wait)
            {
                Thread.Sleep(3000);
            }
        }
        
    }
}
