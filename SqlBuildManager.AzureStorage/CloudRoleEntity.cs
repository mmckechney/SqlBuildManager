using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SqlBuildManager.AzureStorage
{
    public class CloudRoleEntity :TableEntity
    {
        public static string PartitionKeyName = "CloudRole";
        public CloudRoleEntity(string vmName)
        {
            this.PartitionKey = CloudRoleEntity.PartitionKeyName;
            this.RowKey = vmName;
        }

        public CloudRoleEntity() { }

        public string VmName
        {
            get
            {
                return this.RowKey;
            }
            set
            {
                this.RowKey = value;
            }
        }

        public string IpAddress
        {
            get;
            set;
        }

        public bool IsOnline { get; set; }
        public bool IsProcessing { get; set; }

        public override string ToString()
        {
            return string.Format("VmName={0}; IpAddress={1}; IsOnline={2}; IsProcessing={3}", this.VmName, this.IpAddress, this.IsOnline, this.IsProcessing);
        }

    }
}
