using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using SqlBuildManager.ServiceClient.CustomExtensionMethods;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System.ServiceModel.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Polly;
namespace SqlBuildManager.ServiceClient
{
    public class BuildServiceManager
    {

        private Policy azureServiceCallPolicy = null;
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(BuildServiceManager));
        public BuildServiceManager(List<string> serverNames, ServiceClient.Protocol protocol)
            : this()
        {
            this.protocol = protocol;
            SetServerNames(serverNames);
        }
        public BuildServiceManager(ServiceClient.Protocol protocol) :this()
        {
            this.protocol = protocol;
        }
        public BuildServiceManager()
        {
            if (!logger.Logger.Repository.Configured)
                log4net.Config.BasicConfigurator.Configure();

            ConfigurePollyRetryPolicies();
        }

        private void ConfigurePollyRetryPolicies()
        {
            azureServiceCallPolicy = Policy.Handle<Exception>().WaitAndRetry(
                                                        5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                                                        
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

        private Protocol protocol = Protocol.Tcp;
        public void SetProtocol(Protocol protocol)
        {
            this.protocol = protocol;
            foreach (var e in endPoints)
            {
                e.Protocol = protocol;
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

            string azureTemplate = ConfigurationManager.AppSettings["DynamicAzureHttpEndpointTemplate"];
            if (String.IsNullOrEmpty(azureTemplate))
            {
                string msg = "Unable to load the Azure Dynamic Endpoint. Please make sure you have an <appSettings> value for DynamicAzureHttpEndpointTemplate";
                logger.Fatal(msg);
                throw new ApplicationException(msg);
            }
            foreach (string server in serverNames)
            {
                IEnumerable<ServerConfigData> endPoint = (from e in endPoints where e.ServerName == server select e);

                var enumer = endPoint.GetEnumerator();
                if (!enumer.MoveNext())
                {
                    switch(this.protocol)
                    {
                        case Protocol.AzureHttp:
                            endPoints.Add(new ServerConfigData(azureTemplate.Replace(serverReplaceKey, server)));
                            break;
                        default:
                            endPoints.Add(new ServerConfigData(server, httpTemplate.Replace(serverReplaceKey, server), tcpTemplate.Replace(serverReplaceKey, server), this.protocol));
                            break;
                    }
                    
                }
            }

            List<ServerConfigData> toRemove = (from e in endPoints where ! (from s in serverNames select s.ToString()).Contains(e.ServerName) select e).ToList();
            for (int i = 0; i < toRemove.Count; i++)
                this.endPoints.Remove(toRemove[i]);
        }

        public IList<ServerConfigData> GetServiceStatus()
        {

            Parallel.ForEach(this.endPoints, data =>
                {
                    ServiceStatus stat = GetServiceStatus(data.ActiveServiceEndpoint);
                    data.ExecutionReturn = stat.ExecutionStatus;
                    data.ServiceReadiness = stat.Readiness;
                    data.ServiceVersion = stat.CurrentVersion;
                    data.LastStatusCheck = DateTime.Now;
                });
            
            //avoid returning endpoints directly as it will cause a thread issue
            return new BindingList<ServerConfigData>(this.endPoints);

        }

        public IList<ServiceStatus> GetServiceStatus(IList<string> taskedEndpoints)
        {
            List<ServiceStatus> lstStat = new List<ServiceStatus>();
            Parallel.ForEach(taskedEndpoints, endPoint =>
            {
                ServiceStatus stat = GetServiceStatus(endPoint);
                stat.ServerName = endPoint;
                stat.Endpoint = endPoint;
                lock(lstStat)
                {
                    lstStat.Add(stat);
                }
            });

            //avoid returning endpoints directly as it will cause a thread issue
            return lstStat;

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
                    }, GetEndpointConfigName(endpointAddress));

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

        public List<string> SubmitBuildRequest(BuildSettings settings, DistributionType loadDistributionType)
        {
            List<string> taskedEndpoints = new List<string>();
            logger.Info("Accepted BuildRequest with distribution type of '" + Enum.GetName(typeof(DistributionType), loadDistributionType)+ "'");
            
            if( (this.endPoints == null || this.endPoints.Count == 0) && 
                (settings.RemoteExecutionServers != null && settings.RemoteExecutionServers.Count > 0))
            {
                foreach(ServerConfigData cfg in settings.RemoteExecutionServers)
                    this.endPoints.Add(cfg);
            }
            IDictionary<ServerConfigData, BuildSettings> distibutedLoad = this.DistributeBuildLoad(settings, loadDistributionType,this.endPoints.ToList());
            var exeServerCount = distibutedLoad.Count(x => x.Value.MultiDbTextConfig.Length > 0);
            logger.Info("Load distributed to " + exeServerCount.ToString() + " execution servers");

            try
            {
                List<string> untaskedExeServers;
                List<string> unassignedDbServers;
                if (loadDistributionType == DistributionType.OwnMachineName)
                {
                    ValidateLoadDistribution(loadDistributionType, settings.RemoteExecutionServers, settings.MultiDbTextConfig, out untaskedExeServers, out unassignedDbServers);
                    if (untaskedExeServers.Count > 0)
                        logger.Warn("The following Execution Servers are not tasked: " + String.Join(", ", untaskedExeServers.ToArray()));

                    if (unassignedDbServers.Count > 0)
                        logger.Warn("The following Database Servers will not have their databases updated: " + String.Join(", ", unassignedDbServers.ToArray()));
                }
                else
                {
                    var untasked = distibutedLoad.Where(x => x.Value.MultiDbTextConfig.Length == 0);
                    if(untasked.Any())
                    {
                        var lst = untasked.Select(x => x.Key.ActiveServiceEndpoint);
                        logger.Warn("The following Execution Endpoints are not tasked:\r\n" + String.Join("\r\n", lst.ToArray()));
                    }
                }
            }
            catch (Exception exe)
            {
                logger.Warn("Unable to validate load distribution.", exe);
            }

        
            foreach(KeyValuePair<ServerConfigData, BuildSettings> set in distibutedLoad)
            {
                set.Value.BuildRequestFrom = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
                if (set.Value.MultiDbTextConfig.Length > 0)
                {
                    SubmitRequestToServer(set.Key, set.Value);
                    taskedEndpoints.Add(set.Key.ActiveServiceEndpoint);
                }
            }

            return taskedEndpoints;
        }

        private void SubmitRequestToServer(ServerConfigData remoteServer, BuildSettings setting)
        {
            try
            {
                BuildServiceManager.Using<BuildServiceClient>(client =>
                    {
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(remoteServer.ActiveServiceEndpoint);
                        if (client.SubmitBuildPackage(setting))
                        {
                            logger.Info("Submitted Package to " + client.Endpoint.Address + " with " + setting.MultiDbTextConfig.Length.ToString() + " target databases");
                            remoteServer.ServiceReadiness = ServiceReadiness.PackageAccepted;
                        }
                        else
                        {
                            logger.Error("Package submission failed to " + " with " + setting.MultiDbTextConfig.Length.ToString() + " target databases");
                        }
                    }, GetEndpointConfigName(remoteServer.ActiveServiceEndpoint));
                    
                
            }
            catch (Exception exe)
            {
                logger.Error("Unable to submit build package.", exe);
                remoteServer.ServiceReadiness = ServiceReadiness.Error;
            }
        }
        public bool SubmitServiceResetRequest(ServerConfigData remoteServer)
        {
            try
            {
                bool resetSuccessful = false;
                BuildServiceManager.Using<BuildServiceClient>(client =>
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(remoteServer.ActiveServiceEndpoint);
                    resetSuccessful = client.ResetServerStatus();
                }, GetEndpointConfigName(remoteServer.ActiveServiceEndpoint));
                return resetSuccessful;

            }
            catch (Exception exe)
            {
                logger.Error("Unable to submit reset request.", exe);
                return false;
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
                           client.Endpoint.Address = new System.ServiceModel.EndpointAddress(remoteServer.ActiveServiceEndpoint);

                           connTestSet = GetConnectionTestSettings(loadSet.Value.MultiDbTextConfig);

                           ConnectionTestResult[] result = client.TestDatabaseConnectivity(connTestSet);
                           if (result == null || result.Length == 0)
                           {
                               logger.Error("TestConnection to " + remoteServer.ActiveServiceEndpoint + " failed to return data");
                           }
                           else
                           {
                               loadSet.Key.ConnectionTestResults = result.ToList();
                               tmpConfigData.Add(loadSet.Key);
                           }
                       }, GetEndpointConfigName(remoteServer.ActiveServiceEndpoint));

                }
                catch (ActionNotSupportedException actExe)
                {
                    logger.Error("Unable to execute connection test for " + loadSet.Key.ActiveServiceEndpoint + ". This endpoint does not have this method available. Do you need to update your version?", actExe);
                    remoteServer.ServiceReadiness = ServiceReadiness.Error;
                    var failList = (from ts in connTestSet.TargetServers
                            from db in ts.Value
                            select new ConnectionTestResult() { ServerName = ts.Key, DatabaseName = db, Successful = false }).ToList();
                    remoteServer.ConnectionTestResults = failList;
                    tmpConfigData.Add(remoteServer);
                }
                catch (Exception exe)
                {
                    logger.Error("Unable to execute connection test for " + loadSet.Key.ActiveServiceEndpoint, exe);
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

        internal ConnectionTestSettings GetConnectionTestSettings(string[] multiDbTextConfig)
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
            }, GetEndpointConfigName(endpointAddress));
            return retValue;
        }
        public string GetErrorsLog(string endpointAddress)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetLastExecutionErrorsLog();
            }, GetEndpointConfigName(endpointAddress));
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
                }, GetEndpointConfigName(endpointAddress));
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
            },GetEndpointConfigName(endpointAddress));

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
                }, GetEndpointConfigName(endpointAddress));
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

                }, GetEndpointConfigName(endpointAddress));

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
                }, GetEndpointConfigName(endpointAddress));

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
            }, GetEndpointConfigName(endpointAddress));

            return returnValue;
        }
        public string GetFailureDatabasesConfig(string endpointAddress)
        {
            string retValue = string.Empty;
            BuildServiceManager.Using<BuildServiceClient>(client =>
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(endpointAddress);
                retValue = client.GetLastFailuresDatabaseConfig();
            }, GetEndpointConfigName(endpointAddress));
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
            IDictionary<ServerConfigData, BuildSettings> distributed;
            if (loadDistributionType == DistributionType.OwnMachineName)
                distributed =  SplitLoadToOwningServers(unifiedSettings, executionServers);
            else
                distributed =  SplitLoadEvenly(unifiedSettings, executionServers);

            foreach(var d in distributed)
            {
                var setting = d.Value;
                string pw = (setting.SqlBuildManagerProjectFileName + String.Join("|",setting.MultiDbTextConfig) + setting.BuildRunGuid).Sha256Hash();
            
                d.Value.DbUserName = Cryptography.EncryptText(d.Value.DbUserName,pw);
                d.Value.DbPassword = Cryptography.EncryptText(d.Value.DbPassword,pw);
            }

            return distributed;
        }

        internal IDictionary<ServerConfigData, BuildSettings> SplitLoadToOwningServers(BuildSettings unifiedSettings, IList<ServerConfigData> executionServers)
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
        internal IDictionary<ServerConfigData, BuildSettings> SplitLoadEvenly(BuildSettings unifiedSettings, IList<ServerConfigData> executionServers)
        {
            Dictionary<ServerConfigData, BuildSettings> distributedConfig = new Dictionary<ServerConfigData, BuildSettings>();
            IEnumerable<string> allDbTargets = unifiedSettings.MultiDbTextConfig.AsEnumerable();
            IEnumerable<IEnumerable<string>> dividedDbTargets = allDbTargets.SplitIntoChunks(executionServers.Count);

            if (dividedDbTargets.Count() <= executionServers.Count)
            {
                int i = 0;
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
        public static void Using<TService>(Action<TService> action, string configurationName)
            where TService : BuildServiceClient, IDisposable, new()
            
        {

            TService service = (TService)new BuildServiceClient(configurationName);
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


        public List<ServerConfigData> GetListOfAzureInstancePublicUrls()
        {
            Polly.Policy.Handle<CommunicationException>();

            string dynamicAzureTemplate = ConfigurationManager.AppSettings["DynamicAzureHttpEndpointTemplate"];
            List<ServerConfigData> srvData = new List<ServerConfigData>();
            string dns = ConfigurationManager.AppSettings["AzureDnsName"];
            if(string.IsNullOrEmpty(dns))
            {
                logger.Error("Unable to find \"AzureDnsName\" app setting. Can not connect to Azure");
            }

            List<string> instanceUrls = new List<string>();
            try
            {
                string address = string.Format("http://{0}/BuildService.svc", dns);
                azureServiceCallPolicy.Execute(() =>
                        {

                            BuildServiceManager.Using<BuildServiceClient>(client =>
                            {
                                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(new Uri(address));
                                instanceUrls = client.GetListOfAzureInstancePublicUrls().ToList();

                            }, "http_BuildServiceEndpoint");

                            foreach (var url in instanceUrls)
                            {
                                srvData.Add(new ServerConfigData(dynamicAzureTemplate.Replace(serverReplaceKey, url)));
                            }

                        });
                        
            }
            catch (Exception exe)
            {
                logger.Error("Unable to check for Azure instances.", exe);
                
            }
            return srvData;

        }

        private static ChannelEndpointElementCollection configEndpointCollection = null;
        private static string GetEndpointConfigName(string endpointUrl)
        {
            if (configEndpointCollection == null)
            {
                ClientSection clientSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
                configEndpointCollection = clientSection.ElementInformation.Properties[string.Empty].Value as ChannelEndpointElementCollection;
            }

            foreach (ChannelEndpointElement endpointElement in configEndpointCollection)
            {
               if(endpointElement.Address.ToString().StartsWith("http") && endpointUrl.StartsWith("http"))
               {
                   return endpointElement.Name;
               }
               if (endpointElement.Address.ToString().StartsWith("net.tcp") && endpointUrl.StartsWith("net.tcp"))
               {
                   return endpointElement.Name;
               }

            }
            return configEndpointCollection[0].Name;
        }
    }
    public enum SummaryLogType
    {
        Commits, 
        Errors
    }
}
