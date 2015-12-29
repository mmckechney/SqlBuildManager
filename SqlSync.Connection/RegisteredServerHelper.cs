using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
namespace SqlSync.Connection
{
    public class RegisteredServerHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string resisteredServerFileName = string.Empty;
        public static string RegisteredServerFileName
        {
            get
            {
                if (RegisteredServerHelper.resisteredServerFileName == string.Empty)
                {
                    string homePath = SqlBuildManager.Logging.Configure.AppDataPath + @"\";
                    return homePath + "RegisteredServers.xml";
                }
                else
                    return RegisteredServerHelper.resisteredServerFileName;
            }
            set
            {
                RegisteredServerHelper.resisteredServerFileName = value;
            }
        }

        private static RegisteredServers registeredServerData = null;
        public static RegisteredServers RegisteredServerData
        {
            get
            {
                if (RegisteredServerHelper.registeredServerData == null)
                {
                    RegisteredServerHelper.registeredServerData = GetRegisteredServers();
                }
                return RegisteredServerHelper.registeredServerData;
            }
        }
        public static bool ReloadRegisteredServerData(string fileName)
        {
            registeredServerData = null;
            RegisteredServerHelper.RegisteredServerFileName = fileName;
            RegisteredServers tmp = RegisteredServerHelper.RegisteredServerData;
            RegisteredServerHelper.RegisteredServerFileName = string.Empty;
            if (tmp != null)
            {
                RegisteredServerHelper.SaveRegisteredServers(tmp);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool SaveRegisteredServers(RegisteredServers regServers)
        {
            if (regServers == null)
            {
                log.Warn("Unable to save registered servers. \"RegisteredServers\" object is null");
                return false;
            }

            RegisteredServerHelper.registeredServerData = regServers;
            return SerializeRegisteredServers(regServers, RegisteredServerHelper.RegisteredServerFileName);
        }
        private static RegisteredServers GetRegisteredServers()
        {
            string localRegisteredServerPath = SqlBuildManager.Logging.Configure.AppDataPath + @"\RegisteredServers.xml";
            string serverFileContents = string.Empty;
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(RegisteredServerHelper.RegisteredServerFileName);
                req.Proxy = System.Net.WebRequest.DefaultWebProxy;
                req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                req.Timeout = 2000;
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

                serverFileContents = sr.ReadToEnd();
                sr.Close();


                //Write this file to the local path, in case it is unavailable next time.
                if (!File.Exists(localRegisteredServerPath))
                {
                    log.DebugFormat("Copying Registered Server file to local path {0}", localRegisteredServerPath);
                    File.WriteAllText(localRegisteredServerPath, serverFileContents);
                }
            }
            catch(Exception exe)
            {
                log.Error(String.Format("Unable to load Registered Servers file at {0}", RegisteredServerHelper.RegisteredServerFileName), exe);
            }

            if (serverFileContents.Length > 0)
            {
                return DeserializeRegisteredServers(serverFileContents);
            }
            else
            {
                return null;
            }
        }

        private static RegisteredServers DeserializeRegisteredServers(string serverFileContents)
        {
            try
            {
                using (StringReader sr = new StringReader(serverFileContents))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(RegisteredServers));
                    object tmp = xmlS.Deserialize(sr);
                    if (tmp != null)
                        return (RegisteredServers)tmp;
                }
            }
            catch(Exception exe)
            {
                log.Error("Error deserializing registered server contents", exe);
            }

            return null;
        }
        private static bool SerializeRegisteredServers(RegisteredServers regServers, string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                log.Warn("Unable to serialize registered servers. The destination fileName is null or empty");
                return false;
            }

            if (regServers == null)
            {
                log.WarnFormat("Unable to serialize registered servers to {0}. \"RegisteredServers\" object is null", fileName);
                return false;
            }
            try
            {
                System.Xml.XmlTextWriter tw = null;
                try
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(RegisteredServers));
                    tw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8);
                    tw.Formatting = System.Xml.Formatting.Indented;
                    xmlS.Serialize(tw, regServers);
                }
                finally
                {
                    if (tw != null)
                        tw.Close();
                }
                return true;    
            }
            catch (System.UnauthorizedAccessException exe)
            {
                log.Error(String.Format("Unable to serialze file to {0}", fileName), exe);
                return false;
            }
        }
    }
}
