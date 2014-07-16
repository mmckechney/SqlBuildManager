using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using SqlBuildManager.ServiceClient.CustomExtensionMethods;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
namespace SqlBuildManager.ServiceClient
{
    public class BuildServiceManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(BuildServiceManager));
        public BuildServiceManager(List<string> serverNames) : this()
        {
            SetServerNames(serverNames);
        }
        public BuildServiceManager()
        {
            if (!logger.Logger.Repository.Configured)
                log4net.Config.BasicConfigurator.Configure();
        }
        public void SetServerNames(List<string> serverNames)
        {
            //this.endPoints.Clear();
            SetBuildServiceInstanceEndpoints(serverNames);
        }
        public IList<ServerConfigData> GetServerConfigData
        {
            get
            {
                return this.endPoints;
            }
        }
        private BindingList<ServerConfigData> endPoints = new BindingList<ServerConfigData>();
        public const string serverReplaceKey = "[[ServerName]]";
        private void SetBuildServiceInstanceEndpoints(List<string> serverNames)
        {
            string httpTemplate = ConfigurationManager.AppSettings["DynamicHttpEndpointTemplate"];
 
            if (String.IsNullOrEmpty(httpTemplate))
            {
                string msg = "Unable to load the Http Dynamic Endpoint. Please make sure you have an <appSettings> value for DynamicHttpEndpointTemplate";
                logger.Fatal(msg);
                throw new ApplicationException(msg);
            }
            string tcpTemplate = ConfigurationManager.AppSettings["DynamicTcpEndpointTemplate"];
            if(String.IsNullOrEmpty(tcpTemplate))
            {
                string msg = "Unable to load the tcp Dynamic Endpoint. Please make sure you have an <appSettings> value for DynamicTcpEndpointTemplate";
                logger.Fatal(msg);
                throw new ApplicationException(msg);
            }
            foreach (string server in serverNames)
            {
                IEnumerable<ServerConfigData> endPoint = (from e in endPoints where e.ServerName == server select e);
                var enumer = endPoint.GetEnumerator();
                if(!enumer.MoveNext())
                    endPoints.Add(new ServerConfigData(server,httpTemplate.Replace(serverReplaceKey,server), tcpTemplate.Replace(serverReplaceKey, server)));
            }

            List<ServerConfigData> toRemove = (from e in endPoints where ! (from s in serverNames select s.ToString()).Contains(e.ServerName) select e).ToList();
            for (int i = 0; i < toRemove.Count; i++)
                this.endPoints.Remove(toRemove[i]);
        }

        public IList<ServerConfigData> GetServiceStatus()
        {
            foreach (ServerConfigData data in this.endPoints)
            {
               ServiceStatus stat=  GetServiceStatus(data.TcpServiceEndpoint);
               data.ExecutionReturn = stat.ExecutionStatus;
               data.ServiceReadiness = stat.Readiness;
               data.ServiceVersion = stat.CurrentVersion;
               data.LastStatusCheck = DateTime.Now;

            }
            //avoid returning endpoints directly as it will cause a thread issue
            return new BindingList<ServerConfigData>(this.endPoints);
   
        }
        private ServiceStatus GetServiceStatus(string endpointAddress)
        {
            try
            {
                ServiceStatus status = new ServiceStatus();
                BuildServiceManager.Using<BuildServiceClient>(client =>
                    {
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(new Uri(endpointAddress));
                        status = client.GetServiceStatus();
                    });

                return status;
            }
            catch (Exception exe)
            {
                logger.Error("Unable to get endpoint status", exe);
                ServiceStatus stat = new ServiceStatus();
                stat.Readiness = ServiceReadiness.Unknown;
                stat.ExecutionStatus = ExecutionReturn.Waiting;
                return stat;
            }
        }

        public void SubmitBuildRequest(BuildSettings settings, DistributionType loadDistributionType)
        {
            logger.Info("Accepted BuildRequest with distribution type of '" + Enum.GetName(typeof(DistributionType), loadDistributionType)+ "'");
            
            if( (this.endPoints == null || this.endPoints.Count == 0) && 
                (settings.RemoteExecutionServers != null && settings.RemoteExecutionServers.Count > 0))
            {
                foreach(ServerConfigData cfg in settings.RemoteExecutionServers)
                    this.endPoints.Add(cfg);
            }
            IDictionary<ServerConfigData, BuildSettings> distibutedLoad = this.DistributeBuildLoad(settings, loadDistributionType,this.endPoints.ToList());
            logger.Info("Load distributed to " + distibutedLoad.Count().ToString() + " execution servers");

            try
            {
                List<string> untaskedExeServers;
                List<string> unassignedDbServers;
                ValidateLoadDistribution(loadDistributionType, settings.RemoteExecutionServers, settings.MultiDbTextConfig, out untaskedExeServers, out unassignedDbServers);
                if (untaskedExeServers.Count > 0)
                    logger.Warn("The following Execution Servers are not tasked: " + String.Join(", ", untaskedExeServers.ToArray()));

                if (unassignedDbServers.Count > 0)
                    logger.Warn("The following Database Servers will not have their databases updated: " + String.Join(", ", unassignedDbServers.ToArray()));
            }
            catch (Exception exe)
            {
                logger.Warn("Unable to validate load distribution.", exe);
            }

        
            foreach(KeyValuePair<ServerConfigData, BuildSettings> set in distibutedLoad)
            {
                set.Value.BuildRequestFrom = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
                SubmitRequestToServer(set.Key, set.Value);
            }
        }

        private void SubmitRequestToServer(ServerConfigData remoteServer, BuildSettings setting)
        {
            try
            {
                BuildServiceManager.Using<BuildServiceClient>(client =>
                    {
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(remoteServer.TcpServiceEndpoint);
                        if (client.SubmitBuildPackage(setting))
                        {
                            logger.Info("Submitted Package to " + client.Endpoint.Address + " with " + setting.MultiDbTextConfig.Length.ToString() + " target databases");
                            remoteServer.ServiceReadiness = ServiceReadiness.PackageAccepted;
                        }
                        else
                        {
                            logger.Error("Package submission failed to " + " with " + setting.MultiDbTextConfig.Length.ToString() + " target databases");
                        }
                    });
                    
                
            }
            catch (Exception exe)
            {
                logger.Error("Unable to submit build package.", exe);
                remoteServer.ServiceReadiness = ServiceReadiness.Error;
            }
        }

        public IList<ServerConfigData> TestDatabaseConnectivity(BuildSettings settings, DistributionType loadDistributionType)
        {
            if ((this.endPoints == null || this.endPoints.Count == 0) &&
                (settings.RemoteExecutionServers != null && settings.RemoteExecutionServers.Count > 0))
            {
                settings.RemoteExecutionServers.ForEach(r => this.endPoints.Add(r));
            }

            List<ServerConfigData> tmpConfigData = new List<ServerConfigData>();
            IDictionary<ServerConfigData, BuildSettings> distibutedLoad = this.DistributeBuildLoad(settings, loadDistributionType, this.endPoints.ToList());
            foreach (KeyValuePair<ServerConfigData, BuildSettings> loadSet in distibutedLoad)
            {
                ServerConfigData remoteServer = loadSet.Key;
                ConnectionTestSettings connTestSet = null;
                try
                {

                    BuildServiceManager.Using<BuildServiceClient>(client =>
                       {
                           client.Endpoint.Address = new System.ServiceModel.EndpointAddress(remoteServer.TcpServiceEndpoint);

                           connTestSet = GetConnectionTestSettings(loadSet.Value.MultiDbTextConfig);

                           ConnectionTestResult[] result = client.TestDatabaseConnectivity(connTestSet);
                           if (result == null || result.Length == 0)
                           {
                               logger.Error("TestConnection to " + remoteServer.TcpServiceEndpoint + " failed to return data");
                           }
                           else
                           {
                               loadSet.Key.ConnectionTestResults = result.ToList();
                               tmpConfigData.Add(loadSet.Key);
                           }
                       });

                }
                catch (ActionNotSupportedException actExe)
                {
                    logger.Error("Unable to execute connection test for " + loadSet.Key.TcpServiceEndpoint + ". This endpoint does not have this method available. Do you need to update your version?", actExe);
                    remoteServer.ServiceReadiness = ServiceReadiness.Error;
                    var failList = (from ts in connTestSet.TargetServers
                            from db in ts.Value
                            select new ConnectionTestResult() { ServerName = ts.Key, DatabaseName = db, Successful = false }).ToList();
                    remoteServer.ConnectionTestResults = failList;
                    tmpConfigData.Add(remoteServer);
                }
                catch (Exception exe)
                {
                    logger.Error("Unable to execute connection test for " + loadSet.Key.TcpServiceEndpoint, exe);
                    remoteServer.ServiceReadiness = ServiceReadiness.Error;
                    var failList = (from ts in connTestSet.TargetServers
                                    from db in ts.Value
                                    select new ConnectionTestResult() { ServerName = ts.Key, DatabaseName = db, Successful = false }).ToList();
                    remoteServer.ConnectionTestResults = failList;
                    tmpConfigData.Add(remoteServer);
                }
          
            }
            return tmpConfigData;
        }

        private ConnectionTestSettings GetConnectionTestSettings(string[] multiDbTextConfig)
        {
            ConnectionTestSettings connTestSet = new ConnectionTestSettings();
            connTestSet.TargetServers = new Dictionary<string, string[]>();

            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(multiDbTextConfig);

            foreach (ServerData serv in multiDb)
            {
                KeyValuePair<string, IList<string>> pair = new KeyValuePair<string, IList<string>>(serv.ServerName, new List<string>());
                foreach (var seq in serv.OverrideSequence)
                {
                    foreach (DatabaseOverride ovr in seq.Value)
                        pair.Value.Add(ovr.OverrideDbTarget);
                }
                connTestSet.TargetServers.Add(pair.Key, pair.Value.ToArray());
            }

            return connTestSet;
        }

       

        #region Retrieving Logs from Service
        public string GetCommitsLog(string endpointAddress)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetLastExecutionCommitsLog();
            });
            return retValue;
        }
        public string GetErrorsLog(string endpointAddress)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetLastExecutionErrorsLog();
            });
            return retValue;
        }
        public string GetSpecificSummaryLogFile(string endpointAddress, SummaryLogType type, DateTime submittedDate)
        {
            string retValue = string.Empty;
            try
            {
                BuildServiceManager.Using<BuildServiceClient>(client =>
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                    if (type == SummaryLogType.Commits)
                    {
                        retValue = client.GetSpecificCommitsLog(submittedDate);
                    }
                    else
                    {
                        retValue = client.GetSpecificErrorsLog(submittedDate);
                    }
                });
            }
            catch (ActionNotSupportedException actExe)
            {
                retValue = "Unable to retrieve specific summary log from  " + endpointAddress + " for " + submittedDate.ToString() + ".\r\nThis endpoint does not have this method available.\r\n\r\nDo you need to update your version?";
                logger.Error(retValue, actExe);
            }
            catch (Exception exe)
            {
                retValue = "Unable to retrieve specific summary log from  " + endpointAddress + " for " + submittedDate.ToString() + ".\r\n\r\nAn exception was thrown: \r\n" + exe.Message;
                logger.Error(retValue, exe);
            }
            return retValue;
        }
        public string GetDetailedDatabaseLog(string endpointAddress, string serverAndDatabase)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetDetailedDatabaseExecutionLog(serverAndDatabase);
            });

            return retValue;
        }
        public string GetSpecificDetailedDatabaseLog(string endpointAddress, string serverAndDatabase, DateTime submittedDate)
        {
            string retValue = string.Empty;
            try
            {

                BuildServiceManager.Using<BuildServiceClient>(client =>
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                    retValue = client.GetSpecificDatabaseExecutionLog(submittedDate, serverAndDatabase);
                });
            }
            catch (ActionNotSupportedException actExe)
            {
                retValue = "Unable to retrieve detailed database log from  " + endpointAddress + " for " + submittedDate.ToString() + ".\r\nThis endpoint does not have this method available.\r\n\r\nDo you need to update your version?";
                logger.Error(retValue, actExe);
            }
            catch (Exception exe)
            {
                retValue = "Unable to retrieve detailed database log from  " + endpointAddress + " for " + submittedDate.ToString() + ".\r\n\r\nAn exception was thrown: \r\n" + exe.Message;
                logger.Error(retValue, exe);
            }
            return retValue;
        }
        public string GetServiceLog(string endpointAddress)
        {
            string logContents = string.Empty;
            try
            {

                BuildServiceManager.Using<BuildServiceClient>(client =>
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);

                    logContents = client.GetServiceLogFile();

                });

            }
            catch (ActionNotSupportedException actExe)
            {
                logContents = "Unable to retrieve Service Log from  " + endpointAddress + ".\r\nThis endpoint does not have this method available.\r\n\r\nDo you need to update your version?";
                logger.Error(logContents, actExe);
            }
            catch (Exception exe)
            {
                logContents = "Unable to retrieve Service Log from  " + endpointAddress + ".\r\n\r\nAn exception was thrown: \r\n" + exe.Message;
                logger.Error(logContents, exe);
            }
            
            return logContents;
            
        }
        public IList<BuildRecord> GetBuildServiceHistory(string endpointAddress, out string message)
        {
            IList<BuildRecord> buildHistory = new List<BuildRecord>();
            message = string.Empty;
            try
            {

                BuildServiceManager.Using<BuildServiceClient>(client =>
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                    buildHistory = client.GetServiceBuildHistory().ToList();
                });

            }
            catch (ActionNotSupportedException actExe)
            {
                message = "Unable to retrieve Build Service History from  " + endpointAddress + ".\r\nThis endpoint does not have this method available.\r\n\r\nDo you need to update your version?";
                logger.Error(message, actExe);
            }
            catch (Exception exe)
            {
                message = "Unable to retrieve Build Service History Log from  " + endpointAddress + ".\r\n\r\nAn exception was thrown: \r\n" + exe.Message;
                logger.Error(message, exe);
            }

            if (buildHistory == null || buildHistory.Count == 0)
                message = "History is not available for this remote execution service.";

            return buildHistory;
        }
        public bool GetConsolidatedErrorLogs(string endpointAddress,  DateTime submittedDate, string localZipFileName)
        {
            bool returnValue = false;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                try
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                    byte[] zipContents = client.GetAllErrorLogsForExecution(submittedDate);
                    if (zipContents.Length > 0)
                    {
                        System.IO.File.WriteAllBytes(localZipFileName, zipContents);
                        returnValue = true;
                    }
                    else
                    {
                        logger.WarnFormat("Unable to save error zip file {0}. Byte length was 0", localZipFileName);
                        returnValue = false;
                    }
                }
                catch (ActionNotSupportedException actExe)
                {
                    string message = "Unable to retrieve consolidated summary log from  " + endpointAddress + " for " + submittedDate.ToString() + ".\r\nThis endpoint does not have this method available.\r\n\r\nDo you need to update your version?";
                    logger.Error(message, actExe);
                }
                catch (Exception exe)
                {
                    logger.ErrorFormat(String.Format("Unable to save zip file {0}",localZipFileName),exe);
                    returnValue = false;
                }
            });

            return returnValue;
        }
        public string GetFailureDatabasesConfig(string endpointAddress)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetLastFailuresDatabaseConfig();
            });
            return retValue;
        }

        #endregion

        #region Load / Distribution 
        public void ValidateLoadDistribution(DistributionType distType, List<ServerConfigData> serverConfigs, string[] multiDbTextConfigLines, out List<string> lstUntaskedExecutionServers, out List<string> lstUnassignedDatabaseServers)
        {

            if (distType == DistributionType.OwnMachineName)
            {
                //Get list if validated hosts from the data grid...
                IEnumerable<string> readyExecutionServers = (from x in serverConfigs where x.ServiceReadiness == ServiceReadiness.ReadyToAccept select x.ServerName);


                IEnumerable<string> distinctServers = (from x in multiDbTextConfigLines select x.Split(':')[0].Split('\\')[0]).Distinct();

                //Get list of execution servers that won't be used...
                lstUntaskedExecutionServers = (from x in readyExecutionServers where !distinctServers.Contains(x,StringComparer.InvariantCultureIgnoreCase) select x).ToList<string>();
                //Get list if database servers that won't have code updated...
                lstUnassignedDatabaseServers = (from x in distinctServers select x).Except((from y in readyExecutionServers select y), StringComparer.InvariantCultureIgnoreCase).ToList<string>();

            }
            else
            {
                lstUntaskedExecutionServers = new List<string>();
                lstUnassignedDatabaseServers = new List<string>();
            }
        }
        public IDictionary<ServerConfigData, BuildSettings> DistributeBuildLoad(BuildSettings unifiedSettings, DistributionType loadDistributionType, List<ServerConfigData> executionServers)
        {
            if (loadDistributionType == DistributionType.OwnMachineName)
                return SplitLoadToOwningServers(unifiedSettings, executionServers);
            else
                return SplitLoadEvenly(unifiedSettings, executionServers);
        }

        private IDictionary<ServerConfigData, BuildSettings> SplitLoadToOwningServers(BuildSettings unifiedSettings, IList<ServerConfigData> executionServers)
        {
            Dictionary<ServerConfigData, BuildSettings> distributedConfig = new Dictionary<ServerConfigData, BuildSettings>();
            foreach (ServerConfigData exeServer in executionServers)
            {
                BuildSettings tmpSetting = unifiedSettings.DeepClone<BuildSettings>();
                IEnumerable<string> myOwnServers = from x in unifiedSettings.MultiDbTextConfig
                                                   where x.ToUpper().StartsWith(exeServer.ServerName.ToUpper())
                                                   select x;
                tmpSetting.MultiDbTextConfig = myOwnServers.ToArray();
                distributedConfig.Add(exeServer, tmpSetting);
            }

            return distributedConfig;
        }
        private IDictionary<ServerConfigData, BuildSettings> SplitLoadEvenly(BuildSettings unifiedSettings, IList<ServerConfigData> executionServers)
        {
            Dictionary<ServerConfigData, BuildSettings> distributedConfig = new Dictionary<ServerConfigData, BuildSettings>();
            IEnumerable<string> allDbTargets = unifiedSettings.MultiDbTextConfig.AsEnumerable();
            IEnumerable<IEnumerable<string>> dividedDbTargets = allDbTargets.SplitIntoChunks(executionServers.Count);

            if (dividedDbTargets.Count() == executionServers.Count)
            { int i=0;
                foreach (IEnumerable<string> targetList in dividedDbTargets)
                {
                    BuildSettings tmpSetting = unifiedSettings.DeepClone<BuildSettings>();
                    tmpSetting.MultiDbTextConfig = targetList.ToArray();
                    distributedConfig.Add(executionServers[i], tmpSetting);
                    i++;
                }
            }
            else
            {
                logger.Error(String.Format("Divided targets and execution server count do not match: {0} and {1} respectively", dividedDbTargets.Count().ToString(), executionServers.Count.ToString()));
            }

            return distributedConfig;
        }
        #endregion

        /// <summary>
        /// WCF proxys do not clean up properly if they throw an exception. This method ensures that the service proxy is handeled correctly.
        /// Do not call TService.Close() or TService.Abort() within the action lambda.
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
        /// <param name="action">Lambda of the action to performwith the service</param>
        public static void Using<TService>(Action<TService> action)
            where TService : ICommunicationObject, IDisposable, new()
        {
            var service = new TService();
            bool success = false;
            try
            {
                action(service);
                if (service.State != CommunicationState.Faulted)
                {
                    
                    logger.DebugFormat("Service in non-faulted state. Calling Close()");
                    service.Close();
                    success = true;
                }
                else
                {
                    logger.Error("Service detected in a \"CommunicationState.Faulted\" state.");
                }
            }
            finally
            {
                if (!success)
                {
                    service.Abort();
                    logger.Error("Service detected in a \"CommunicationState.Faulted\" state: Abort() called.");
                }
            }
        }

    }
    public enum SummaryLogType
    {
        Commits, 
        Errors
    }
}
