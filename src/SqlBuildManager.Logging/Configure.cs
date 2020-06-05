using log4net.Appender;
using System;
using System.IO;
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
        public static bool SetLoggingPath()
        {
            try {

                log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository("root") as log4net.Repository.Hierarchy.Hierarchy;

                foreach(var appender in hierarchy.Root.Appenders)
                {
                    if(appender is log4net.Appender.FileAppender)
                    {
                        var fileAppender = appender as FileAppender;
                        string baseFile = Path.GetFileName(fileAppender.File);
                        //Set to the appdata folder
                        var newName = Path.Combine(Configure.AppDataPath, baseFile);
                        fileAppender.File = newName;
                        fileAppender.ActivateOptions();
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
