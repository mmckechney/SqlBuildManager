using log4net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net;
using System.Reflection;

namespace SqlBuildManager.AzureStorage
{
    /// <summary>
    /// http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-tables/
    /// </summary>
    public class RoleManager
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        CloudStorageAccount storageAccount;
        CloudTableClient roleTableClient;
        CloudTable roleTable;
        public RoleManager()
        {
            if(RoleEnvironment.IsAvailable)
            {
                string connection = CloudConfigurationManager.GetSetting("StorageConnectionString");
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Storage connection string: {0}", connection);
                }

                this.storageAccount = CloudStorageAccount.Parse(connection);
                InitializeRoleTable();
            }
        }
        public RoleManager(string storageConnectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            InitializeRoleTable();
        }

        private bool InitializeRoleTable()
        {
            this.roleTableClient = storageAccount.CreateCloudTableClient();

            this.roleTable = this.roleTableClient.GetTableReference("CloudRoleEntity");
            return this.roleTable.CreateIfNotExists();
        }


        public bool InsertCloudRoleEntity(string vmName, string ipAddress)
        {
            CloudRoleEntity cre = new CloudRoleEntity(vmName) {IpAddress=ipAddress, IsProcessing = false };

            TableOperation insertOperation = TableOperation.InsertOrReplace(cre);
            TableResult result;
            try
            {
                // Execute the insert operation.
                result = roleTable.Execute(insertOperation);

                if (result.HttpStatusCode == (int)HttpStatusCode.NoContent)
                    return true;
                else
                    return false;
            }
            catch (Exception exe)
            {
                log.Error(string.Format("Unable to insert new CloudRole entity: {0}", cre.ToString()), exe);
                return false;
            }

        }

        public bool DeleteCloudRoleEntity(string vmName)
        {

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<CloudRoleEntity>(CloudRoleEntity.PartitionKeyName, vmName);
                TableResult retrievedResult = roleTable.Execute(retrieveOperation);
                CloudRoleEntity deleteEntity = (CloudRoleEntity)retrievedResult.Result;

                // Create the Delete TableOperation.
                if (deleteEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                    roleTable.Execute(deleteOperation);
                    
                }
                return true;
            }
            catch (Exception exe)
            {
                log.Error(string.Format("Unable to delete CloudRole entity with vmName: {0}", vmName), exe);
                return false;
            }

        }




    }
}
