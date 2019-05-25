using log4net;
using SqlBuildManager.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace SqlBuildManager.Console
{
    public class RemoteAzureHealth
    {
        static BuildServiceManager manager;
        static RemoteAzureHealth()
        {
            manager = new BuildServiceManager();
        }
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<ServerConfigData> GetAzureInstances()
        {
            List<ServerConfigData> serverData = manager.GetListOfAzureInstancePublicUrls();
            return serverData;
        }
        public static List<string> GetDatabaseErrorList(string instance)
        {
            List<string> targetEndpoints = new List<string>();
            var servers = GetAzureInstances();
            if (instance.ToLower() != "all")
            {
                var targetEndpoint = servers.Where(s => s.ServerName.ToLower() == instance.ToLower()).Select(x => x.ActiveServiceEndpoint).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(targetEndpoint))
                {
                    log.ErrorFormat("Cannot find provided instance target '{0}' for error log", instance);
                    return null;
                }
                targetEndpoints.Add(targetEndpoint);
            }
            else
            {
                targetEndpoints.AddRange(servers.Select(x => x.ActiveServiceEndpoint));
            }

           StringBuilder sb = new StringBuilder();
            Parallel.ForEach(targetEndpoints, target =>
                {
                    var text = manager.GetFailureDatabasesConfig(target);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        log.WarnFormat("No error log retrieved from '{0}'", target);
                    }
                    else
                    {
                        string parsed = text.Replace("client,", "").Replace(",","");
                        sb.AppendLine(parsed);
                    }
                });

            log.Debug(sb.ToString());

            List<string> dbsInError = new List<string>();
            dbsInError.AddRange(sb.ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            
            return dbsInError;
            
            
        }

        /// <summary>
        /// setting can be:
        ///    1) server/database: myserver:database (to get details for just that database)
        ///    2) A remote server address:  remoteserver:port (to get all db details from that server)
        ///    3) All - meaning all remote servers and all databases  (to get all db details from all remote agents and databses)
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        internal static string GetErrorDetail(string setting)
        {
            log.InfoFormat("Starting error log detail retrieval for {0}", setting);
            List<string> targetEndpoints = new List<string>();
            var remotes = GetAzureInstances();

            //What kind of setting to we have?
            
           
            IEnumerable<string> endPoints;
            if(setting.ToLower().Trim() == "all")
            {
                endPoints = remotes.Select(x => x.ActiveServiceEndpoint);  //"all"
            }
            else
            {
                endPoints = remotes.Where(r => r.ActiveServiceEndpoint.ToLower() == setting.ToLower()).Select(x => x.ActiveServiceEndpoint); //Specific endpoint
            }

            if (endPoints.Any())
            {
                //StringBuilder sb = new StringBuilder();
                Dictionary<string, string> dbLog = new Dictionary<string, string>();
                foreach (var singleEndPoint in endPoints)
                {
                    if (!string.IsNullOrWhiteSpace(singleEndPoint))
                    {
                        log.InfoFormat("Found Active remote service at {0}. Querying for log details...", singleEndPoint);
                      
                        var text = manager.GetFailureDatabasesConfig(singleEndPoint);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            var errorDbs = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            Parallel.ForEach(errorDbs, db =>
                            {
                                string svcDb = db.Replace(":client,", ".").Replace(":,","."); //service is expecting server.db not server:client,db or server:,client
                                string logDb = db.Replace(":client,", ":").Replace(":,", ":"); //want to consistently log server:db or server:,client
                                var errLog = manager.GetDetailedDatabaseLog(singleEndPoint, svcDb);
                                if (!string.IsNullOrWhiteSpace(errLog))
                                {
                                    string errorMessage = ParseDetailedLog(errLog, logDb);
                                    //lock (sb)
                                    //{
                                    //    sb.AppendFormat("{0}{1}\r\n", logDb.PadRight(55,' '), errorMessage);
                                    //}
                                    lock(dbLog)
                                    {
                                        dbLog.Add(singleEndPoint + "  "+ logDb, errorMessage);
                                    }
                                }

                            });
                        }
                        
                    }
                }

                var sorted = dbLog.OrderBy(i => i.Value);
                StringBuilder sortSb = new StringBuilder();
                foreach(var item in sorted)
                {
                    sortSb.AppendFormat("{0}{1}\r\n", item.Key.PadRight(115, ' '), item.Value);
                }

                return sortSb.ToString();
                //return sb.ToString();
            }

            //single database
            //loop through the remotes to find the database target
            string[] serverAndDb = setting.Split(':');
            foreach(var remote in remotes)
            {
                var text = manager.GetFailureDatabasesConfig(remote.ActiveServiceEndpoint);
                if(!string.IsNullOrWhiteSpace(text))
                {
                    //Found our server...
                    if(text.ToLower().IndexOf(serverAndDb[0].ToLower()) > -1 && text.ToLower().IndexOf(serverAndDb[1].ToLower()) > -1)
                    {
                        var dbLog = manager.GetDetailedDatabaseLog(remote.ActiveServiceEndpoint, string.Join(".", serverAndDb));
                        if (!string.IsNullOrWhiteSpace(dbLog))
                        {
                            return dbLog;
                        }
                    }
                    break;
                }
            }

            return "No error detail found";
        }


        private static string ParseDetailedLog(string log, string serverAndDb)
        {
            // Error Message:
            //Login failed

            var lines = log.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.ToLower().StartsWith("error message:") || x.ToLower().StartsWith("login failed"))
                .Select(x => x);

            if(lines.Any())
            {
                return string.Join("\r\n", lines);
            }else
            {
                var formatted = serverAndDb.LastIndexOf('.');
                serverAndDb = new Regex(@"\.").Replace(serverAndDb,":",1,serverAndDb.LastIndexOf('.') -1);
                return string.Format("No error message parsed. Try command /RemoteErrorDetail=\"{0}\" to get full log detail", serverAndDb); 
            }
        }
    }
}
