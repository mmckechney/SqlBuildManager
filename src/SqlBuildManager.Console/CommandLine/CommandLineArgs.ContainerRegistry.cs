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
        public ContainerRegistry ContainerRegistryArgs { get; set; } = new ContainerRegistry();
        public string RegistryServer
        {
            set
            {
                ContainerRegistryArgs.RegistryServer = value;
                this.DirectPropertyChangeTracker.Add("ContainerRegistry.RegistryServer");
            }
        }
        public string ImageName
        {
            set
            {
                ContainerRegistryArgs.ImageName = value;
                this.DirectPropertyChangeTracker.Add("ContainerRegistry.ImageName");
            }
        }
        public string ImageTag
        {
            set
            {
                ContainerRegistryArgs.ImageTag = value;
                this.DirectPropertyChangeTracker.Add("ContainerRegistry.ImageTag");
            }
        }
        public string RegistryUserName
        {
            set
            {
                ContainerRegistryArgs.RegistryUserName = value;
                this.DirectPropertyChangeTracker.Add("ContainerRegistry.RegistryUserName");
            }
        }
        public string RegistryPassword
        {
            set
            {
                ContainerRegistryArgs.RegistryPassword = value;
                this.DirectPropertyChangeTracker.Add("ContainerRegistry.RegistryPassword");
            }
        }

        public class ContainerRegistry : ArgsBase
        {
            public string RegistryServer { get; set; } = string.Empty;

            private string imageName = string.Empty;
            public string ImageName
            {
                set { imageName = value; }
                get
                {
                    if (string.IsNullOrEmpty(imageName))
                        return "sqlbuildmanager";
                    else
                        return imageName;
                }
            }
            public string ImageTag { get; set; } = string.Empty;
            public string RegistryUserName { get; set; } = string.Empty;
            public string RegistryPassword { get; set; } = string.Empty;

        }
    }
}
