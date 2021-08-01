using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace SqlBuildManager.Console.Aci
{
    class AciHelper
    {
        internal static string CreateAciArmTemplate(CommandLineArgs cmdLine)
        {
            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string pathToTemplates = Path.Combine(exePath, "Aci");
            string template = File.ReadAllText(Path.Combine(pathToTemplates, "aci_arm_template.json"));
            template = template.Replace("{{identityName}}", cmdLine.AciArgs.IdentityName);
            template = template.Replace("{{identityResourceGroup}}", cmdLine.AciArgs.IdentityResourceGroup);
            template = template.Replace("{{aciName}}", cmdLine.AciArgs.AciName);

            string containerTemplate = File.ReadAllText(Path.Combine(pathToTemplates, "container_template.json"));
            List<string> containers = new List<string>();
            int padding = cmdLine.AciArgs.ContainerCount.ToString().Length;
            for(int i=0;i<cmdLine.AciArgs.ContainerCount;i++)
            {
                containers.Add(containerTemplate.Replace("{{counter}}", i.ToString().PadLeft(padding, '0')));
            }
            string allContainers = string.Join("," + Environment.NewLine, containers);

            template = template.Replace("{{Container_Placeholder}}", allContainers);
            return template;
        }
    }
}
