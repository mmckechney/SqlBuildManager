using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SqlSync.Connection;
using SqlBuildManager.Services.History;
using SqlBuildManager.ServiceClient;

namespace SqlBuildManager.Services
{
    [ServiceContract(Namespace = "http://schemas.mckechney.com/SqlBuildManager.Services/CloudBuildService")]
    public interface ICloudBuildService
    {
        [OperationContract]
        IList<ServerConfigData> GetInstanceServiceStatus();
    }
}
