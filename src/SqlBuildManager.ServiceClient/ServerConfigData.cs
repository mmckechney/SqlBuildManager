using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using System.Xml.Serialization;
using SqlSync.Connection;
namespace SqlBuildManager.ServiceClient
{
    [Serializable()]
    public class ServerConfigData
    {
        public ServerConfigData()
        {
            this.ConnectionTestResults = new List<ConnectionTestResult>();
        }
        public ServerConfigData(string serverName, string httpServiceEndpoint, string tcpServiceEndpoint,ServiceClient.Protocol protocol) : this()
        {
            this.serverName = serverName;
            this.httpServiceEndpoint = httpServiceEndpoint;
            this.tcpServiceEndpoint = tcpServiceEndpoint;
            this.protocol = protocol;
        }
        public ServerConfigData(string azureHttpServiceEndpoint)
            : this()
        {
            var tmp = new Uri(azureHttpServiceEndpoint);
            this.serverName = tmp.Host + ":" + tmp.Port;
            this.azureHttpServiceEndpoint = azureHttpServiceEndpoint;
            this.protocol = Protocol.AzureHttp;
        }
        private string serverName = string.Empty;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }

        private Protocol protocol = Protocol.Tcp;
        public Protocol Protocol
        {
            get { return this.protocol; }
            set { this.protocol = value; }
        }
        private string tcpServiceEndpoint = string.Empty;

        public string TcpServiceEndpoint
        {
            //get { return tcpServiceEndpoint; }
            set { tcpServiceEndpoint = value; }
        }
        private string httpServiceEndpoint = string.Empty;

        public string HttpServiceEndpoint
        {
            //get { return httpServiceEndpoint; }
            set { httpServiceEndpoint = value; }
        }
        private string azureHttpServiceEndpoint = string.Empty;
        public string AzureHttpServiceEndpoint
        {
            //get { return httpServiceEndpoint; }
            set { azureHttpServiceEndpoint = value; }
        }
        public string ActiveServiceEndpoint
        {
            get
            {
                switch (this.protocol)
                {
                    case ServiceClient.Protocol.Http:
                        return httpServiceEndpoint;
                    case ServiceClient.Protocol.AzureHttp:
                        return azureHttpServiceEndpoint;
                    case ServiceClient.Protocol.Tcp:
                    default:
                        return tcpServiceEndpoint;

                }
            }
            
        }

        private ServiceReadiness serviceReadiness = ServiceReadiness.Unknown;
        public ServiceReadiness ServiceReadiness
        {
            get { return serviceReadiness; }
            set { serviceReadiness = value; }
        }

        private ExecutionReturn exeReturn = ExecutionReturn.Waiting;
        public ExecutionReturn ExecutionReturn
        {
            get { return exeReturn; }
            set { exeReturn = value; }
        }

        private DateTime lastStatusCheck;
        public DateTime LastStatusCheck
        {
            get { return lastStatusCheck; }
            set { lastStatusCheck = value; }
        }

        private string serviceVersion;
        public string ServiceVersion
        {
            get { return serviceVersion; }
            set { serviceVersion = value; }
        }


        [XmlIgnore()]
        public IList<ConnectionTestResult> ConnectionTestResults
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.serverName;
        }
    }
}
