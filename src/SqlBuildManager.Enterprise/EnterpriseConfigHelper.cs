using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using SqlBuildManager.Enterprise.ActiveDirectory;
using System.Linq;
namespace SqlBuildManager.Enterprise
{

    public class EnterpriseConfigHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static EnterpriseConfiguration enterpriseConfig = null;
        private static List<string> adGroupMemberships = null;

        public static List<string> AdGroupMemberships
        {
            get
            {
                if (EnterpriseConfigHelper.adGroupMemberships == null)
                {
                    EnterpriseConfigHelper.adGroupMemberships = AdHelper.GetGroupMemberships(System.Environment.UserName).ToList();
                }
                return EnterpriseConfigHelper.adGroupMemberships;
            }
            set
            {
                EnterpriseConfigHelper.adGroupMemberships = value;
            }
        }

        public static EnterpriseConfiguration EnterpriseConfig
        {
            get
            {
                if (EnterpriseConfigHelper.enterpriseConfig == null)
                {
                    EnterpriseConfigHelper.enterpriseConfig = LoadEnterpriseConfiguration();
                }
                if(EnterpriseConfigHelper.enterpriseConfig == null)
                {
                    log.Warn("Unable to load the EnterpriseConfiguration. Creating a new shell EnterpriseConfiguration object");
                    EnterpriseConfigHelper.enterpriseConfig = new EnterpriseConfiguration();

                }
                return EnterpriseConfigHelper.enterpriseConfig;
            }
            set { EnterpriseConfigHelper.enterpriseConfig = value; }
        }

        internal static EnterpriseConfiguration LoadEnterpriseConfiguration()
        {
            if(System.Configuration.ConfigurationManager.AppSettings["Enterprise.ConfigFileLocation"] != null &&
                System.Configuration.ConfigurationManager.AppSettings["Enterprise.ConfigFileLocation"].Length > 0)
            {
                return LoadEnterpriseConfiguration(System.Configuration.ConfigurationManager.AppSettings["Enterprise.ConfigFileLocation"]);
            }
            else if (File.Exists(@"I:\mmckechney\Sql Build Manager\Enterprise\EnterpriseConfiguration.xml"))
            {
                return
                    LoadEnterpriseConfiguration(
                        @"I:\mmckechney\Sql Build Manager\Enterprise\EnterpriseConfiguration.xml");
            }
            else
            {
                return LoadEnterpriseConfiguration(@"C:\force_a_local_file_check");
            }

        }
        public static EnterpriseConfiguration LoadEnterpriseConfiguration(string configPath)
        {
            string localConfigPath = SqlBuildManager.Logging.Configure.AppDataPath + @"\EnterpriseConfiguration.xml";
            string configuration = string.Empty;
            try
            {
                configPath = Path.GetFullPath(configPath);
                System.Net.WebRequest req = System.Net.WebRequest.Create(configPath);
                req.Proxy = System.Net.WebRequest.DefaultWebProxy;
                req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                req.Timeout = 2000;
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

                configuration = sr.ReadToEnd();
                sr.Close();


                //Write this file to the local path, in case it is unavailable next time.
                if (File.Exists(localConfigPath))
                    File.SetAttributes(localConfigPath, FileAttributes.Normal);

                File.WriteAllText(localConfigPath, configuration);
                log.DebugFormat("Loaded Enterprise Configuration from {0} and saved to {1}", configPath, localConfigPath);
            }
            catch(Exception exe)
            {
                log.Error(String.Format("Unable to load Enterprise configuration from {0}",configPath), exe);
            }

            if (configuration.Length > 0)
                return DeserializeConfiguration(configuration);


            //If we get here, then we need to try the local copy.
            if(File.Exists(localConfigPath))
            {
                log.DebugFormat("Reading EnterpriseConfig from local path:{0}", localConfigPath);
                configuration = File.ReadAllText(localConfigPath);
                return DeserializeConfiguration(configuration);
            }

            log.ErrorFormat("Unable to load EnterpriseConfig from configuation file!");
            return null;

           
        }
        internal static EnterpriseConfiguration DeserializeConfiguration(string configuration)
        {
            try
            {
                using (StringReader sr = new StringReader(configuration))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(EnterpriseConfiguration));
                    object tmp = xmlS.Deserialize(sr);
                    if (tmp != null)
                    {
                        log.Debug("Successfully deserialized Enterprise Configuration");
                        return (EnterpriseConfiguration)tmp;
                    }
                }

            }
            catch(Exception exe)
            {
                log.Error("Error deserializing enterprise configuration file from: " + configuration, exe);
            }

            return null;
        }

        public static int GetMinumumScriptTimeout(string scriptName, int generalMinumum)
        {
            TimeoutSetting ts = GetApplicableTimeoutSetting(scriptName);
            if (ts == null)
            {
                if (generalMinumum == 0)
                    return 500;
                else
                    return generalMinumum;
            }
            else
                return ts.MinumumTimeout;
        }
        public static bool UserHasCustomTimeoutSetting()
        {
            if (EnterpriseConfigHelper.EnterpriseConfig.CustomScriptTimeouts == null)
                return false;

            List<TimeoutSetting[]> ts = GetCurrentUsersTimeoutSettings();
            if (ts == null || ts.Count() == 0)
                return false;
            else
                return true;
        }


        private static List<TimeoutSetting[]> GetCurrentUsersTimeoutSettings()
        {
            if (EnterpriseConfigHelper.EnterpriseConfig.CustomScriptTimeouts == null)
                return null;

            //Find any timeout sets that apply to the group
            List<CustomScriptTimeouts> to = EnterpriseConfigHelper.EnterpriseConfig.CustomScriptTimeouts.ToList();
            var ts = (from t in to
                      from a in t.ApplyToGroup
                      join g in EnterpriseConfigHelper.AdGroupMemberships on a.GroupName.ToLowerInvariant() equals g.ToLowerInvariant()
                      select t.TimeoutSetting);

            if (ts.Count() > 0)
                return ts.ToList();
            else
                return null;

        }
        private static TimeoutSetting GetApplicableTimeoutSetting(string scriptName)
        {

            try
            {
                string extension = Path.GetExtension(scriptName).ToLowerInvariant();

                List<TimeoutSetting[]> ts = GetCurrentUsersTimeoutSettings();
                if(ts == null)
                    return null;
              
                //If we find some that apply, find a matching extension value
                if (ts.Count() > 0)
                {
                    var tsaList = from s in ts.ToList()
                                  from tsa in s
                                  where tsa.FileExtension.ToLowerInvariant() == extension
                                  select tsa;

                    if (tsaList.Count() > 0)
                        return tsaList.First();
                }
            }
            catch(Exception exe)
            {
                log.Warn(string.Format("Unable to get Enterprise TimeoutSetting object for file '{0}'", scriptName), exe);
            }
            return null;
        }

    }
}
