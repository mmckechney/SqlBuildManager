using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SqlSync.SqlBuild.MultiDb;
namespace SqlSync.SqlBuild.Remote
{
    public class RemoteHelper
    {
        public static string BuildRemoteExecutionCommandline(string sbmFileName, string overrideSettingFile, string remoteExeServersFile, string rootLoggingPath, 
            string distributionType, bool isTrial, bool isTransactional, string buildDescription, int allowedRetryCount, string username, string password, string platinumDacpacFile)
        {

            //if (sbmFileName == null || sbmFileName.Length == 0)
            //{
            //    throw new ArgumentException("The Sql Build Manager file must be specified", "sbmFileName");
            //}
            if (string.IsNullOrWhiteSpace(rootLoggingPath))
            {
                throw new ArgumentException("The Root logging path must be specified", "rootLoggingPath");
            }
            if (string.IsNullOrWhiteSpace(overrideSettingFile))
            {
                throw new ArgumentException("The Override Target Settings file must be specified", "overrideSettingFile");
            }
            if (string.IsNullOrWhiteSpace(distributionType))
            {
                throw new ArgumentException("The Distribution type setting must be specified", "distributionType");
            }
            if (distributionType != "equal"  &&  distributionType != "local")
            {
                throw new ArgumentException("The Distribution type setting allowed values are \"equal\" or \"local\"", "distributionType");
            }
            if (string.IsNullOrWhiteSpace(remoteExeServersFile))
            {
                throw new ArgumentException("The Remote Servers file must be specified or be set to \"derive\" to get from the override target server list", "remoteExeServersFile");
            }
            if (string.IsNullOrWhiteSpace(buildDescription))
            {
                throw new ArgumentException("A Build Description is required.", "buildDescription");
            }
            if (isTrial && !isTransactional)
            {
                throw new ArgumentException("Invalid combination of Trial and Transactional settings. Can not run a Trial when transactions are disabled", "isTransactional");
            }

            if (string.IsNullOrWhiteSpace(sbmFileName) && string.IsNullOrWhiteSpace(platinumDacpacFile))
            {
                throw new ArgumentException("A value is needed for either the build package file or the platinum dacpac", "sbmFileName");
            }

            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.Console.exe";
            StringBuilder sb = new StringBuilder();

            sb.Append("\"" + exePath + "\" ");
            sb.Append("/remote=true ");
            if (!string.IsNullOrWhiteSpace(sbmFileName))
                sb.Append("/build=\"" + sbmFileName + "\" ");

            if (!string.IsNullOrWhiteSpace(platinumDacpacFile))
                sb.Append("/PlatinumDacpac=\"" + platinumDacpacFile + "\" ");

            sb.Append("/RemoteServers=\"" + remoteExeServersFile + "\" ");
            sb.Append("/override=\"" + overrideSettingFile +"\" ");
            sb.Append("/RootLoggingPath=\""+ rootLoggingPath +"\" ");
            sb.Append("/DistributionType=\""+ distributionType +"\" ");
            sb.Append("/Description=\"" + buildDescription + "\" ");
            sb.Append("/transactional=" + isTransactional +  " ");
            sb.Append("/trial=" + isTrial + " ");
            sb.Append("/TimeoutRetryCount=" + allowedRetryCount.ToString() + " ");
            if(!string.IsNullOrWhiteSpace(username))
            {
                sb.Append("/username=\""+username +"\" ");

            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                sb.Append("/password=\"" + password + "\"");

            }



           return sb.ToString();
        }

        /// <summary>
        /// Utility method to take a one of the multi-database configuration types and getting 
        /// an text based array of values back.
        /// Used for the Remote execution service
        /// </summary>
        /// <param name="fileName">Name of configuration file (.multiDb, .multiDbQ or .cfg)</param>
        /// <returns>String array of multi-database override settings</returns>
        public static string[] GetMultiDbConfigLinesArray(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new MultiDbConfigurationException(String.Format("The specified file name '{0}' does not exist", fileName));
            }
            try
            {
                if (fileName.ToLower().EndsWith(".multidb"))
                {
                    MultiDbData multiDb = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);
                    string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb);
                    return cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (fileName.ToLower().EndsWith(".cfg"))
                {
                    return File.ReadAllText(fileName).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); //would use ReadAllLines but want to make sure we remove empties.
                }
                else if (fileName.ToLower().EndsWith(".multidbq"))
                {
                    string message;
                    MultiDbData dbData = MultiDbHelper.CreateMultiDbConfigFromQuery(fileName, out message);
                    if (dbData == null)
                    {
                        throw new MultiDbConfigurationException("Error creating configuration.\r\n" + message);
                    }
                    string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(dbData);
                    return cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    throw new MultiDbConfigurationException("Unrecognized file type. Please designate a file of type .multiDbQ, .multiDb or .cfg");
                }
            }
            catch (Exception exe)
            {
                throw new MultiDbConfigurationException("Error with configuration.\r\n" + exe.ToString());
            }

        }

        public static string[] GetUniqueServerNamesFromMultiDb(string[] configArray)
        {
            List<string> uniqueServers = new List<string>();
            for (int i = 0; i < configArray.Length; i++)
            {
                string server = configArray[i].Split(':')[0].Split('\\')[0].ToUpper().Trim();
                if (!uniqueServers.Contains(server))
                    uniqueServers.Add(server);
            }

            return uniqueServers.ToArray();
        }

        public static List<string> GetRemoteExecutionServers(string remoteServerValue, MultiDbData multiDb)
        {
            List<string> remote = new List<string>();
            if (remoteServerValue.ToLower().Trim() == "derive")
            {
                string[] tmpCfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
                remote = RemoteHelper.GetUniqueServerNamesFromMultiDb(tmpCfg).ToList();
            }
            else
            {
                remote = File.ReadAllLines(remoteServerValue).ToList();
                remote = (from r in remote where r.Trim().Length > 0 select r.Trim()).ToList();

            }

            return remote;
        }
    }
}
