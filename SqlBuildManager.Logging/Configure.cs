using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
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

                log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;

                foreach(var appender in hierarchy.Root.Appenders)
                {
                    if(appender is log4net.Appender.RollingFileAppender)
                    {
                        var rollAppender = appender as RollingFileAppender;
                        string baseFile = Path.GetFileName(rollAppender.File);
                        //Set to the appdata folder
                        var newName = Path.Combine(Configure.AppDataPath, baseFile);
                        rollAppender.File = newName;
                        rollAppender.ActivateOptions();
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
