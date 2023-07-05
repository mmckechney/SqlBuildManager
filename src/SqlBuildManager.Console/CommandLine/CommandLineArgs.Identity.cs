using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {

        public string IdentityName
        {
            set
            {
                IdentityArgs.IdentityName = value;
                this.DirectPropertyChangeTracker.Add("Identity.IdentityName");
            }
        }
        public string ClientId
        {
            set
            {
                IdentityArgs.ClientId = value;
                this.DirectPropertyChangeTracker.Add("Identity.ClientId");
            }
        }
            
        public string PrincipalId
        {
            set
            {
                IdentityArgs.PrincipalId = value;
                this.DirectPropertyChangeTracker.Add("Identity.PrincipalId");
            }
        }
        public string ResourceId
        {
            set
            {
                IdentityArgs.ResourceId = value;
                this.DirectPropertyChangeTracker.Add("Identity.ResourceId");
            }
        }
        public string IdentityResourceGroup
        {
            set
            {
                IdentityArgs.ResourceGroup = value;
                this.DirectPropertyChangeTracker.Add("Identity.ResourceGroup");
            }
        }
        public string SubscriptionId
        {
            set
            {
                IdentityArgs.SubscriptionId = value; 
                ContainerAppArgs.SubscriptionId = value;
                AciArgs.SubscriptionId = value; 
                EventHubArgs.SubscriptionId = value;
                this.DirectPropertyChangeTracker.Add("Identity.SubscriptionId");
            }
        }
        public string TenantId
        {
            set
            {
                IdentityArgs.TenantId = value;
                this.DirectPropertyChangeTracker.Add("Identity.TenantId");
            }
        }
        [JsonIgnore]
        public virtual string ServiceAccountName
        {
            set
            {
                if (IdentityArgs == null) IdentityArgs = new Identity();
                IdentityArgs.ServiceAccountName = value;
                this.DirectPropertyChangeTracker.Add("Identity.ServiceAccountName");
            }
        }

        public class Identity : ArgsBase
        {
            private string _resourceid = string.Empty;
            public string IdentityName { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string PrincipalId { get; set; } = string.Empty;
            [JsonIgnore]
            public string ResourceId
            {
                get
                {
                    if (_resourceid == null) _resourceid = String.Empty;
                    if (!_resourceid.StartsWith("/subscriptions") && !string.IsNullOrEmpty(IdentityName) && !string.IsNullOrEmpty(ResourceGroup) && !string.IsNullOrEmpty(SubscriptionId))
                    {
                        _resourceid = $"/subscriptions/{SubscriptionId}/resourcegroups/{ResourceGroup}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{IdentityName}";
                    }
                    return _resourceid;
                }
                set
                {
                    (string sub, string rg, string name) = Arm.ArmHelper.GetSubRgAndIdentityName(value);
                    if (sub != null)
                    {
                        IdentityName = name;
                        SubscriptionId = sub;
                        ResourceGroup = rg;

                    }
                    _resourceid = value;
                }
            }
            public string ResourceGroup { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;

            public string ServiceAccountName { get; set; } = string.Empty;
        }
    }
}
