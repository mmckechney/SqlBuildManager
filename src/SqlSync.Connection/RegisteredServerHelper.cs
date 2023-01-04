using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
namespace SqlSync.Connection
{
    public class RegisteredServerHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string resisteredServerFileName = string.Empty;
        public static string RegisteredServerFileName
        {
            get
            {
                if (RegisteredServerHelper.resisteredServerFileName == string.Empty)
                {
                    return Path.Combine(SqlBuildManager.Logging.Configure.AppDataPath, "RegisteredServers.xml");
                }
                else
                {
                    return RegisteredServerHelper.resisteredServerFileName;
                }
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
                log.LogWarning("Unable to save registered servers. \"RegisteredServers\" object is null");
                return false;
            }

            RegisteredServerHelper.registeredServerData = regServers;
            return SerializeRegisteredServers(regServers, RegisteredServerHelper.RegisteredServerFileName);
        }
        internal static RegisteredServers GetRegisteredServers()
        {
            string localRegisteredServerPath = Path.Combine(SqlBuildManager.Logging.Configure.AppDataPath, "RegisteredServers.xml");
            string serverFileContents = string.Empty;
            try
            {
                if (RegisteredServerHelper.RegisteredServerFileName.ToLower().StartsWith("http"))
                {
                    var httpClient = new HttpClient();
                    serverFileContents = httpClient.GetStringAsync(RegisteredServerHelper.RegisteredServerFileName).GetAwaiter().GetResult();
                }
                else
                {
                    serverFileContents = File.ReadAllText(RegisteredServerHelper.RegisteredServerFileName);

                }


                //Write this file to the local path, in case it is unavailable next time.
                if (!File.Exists(localRegisteredServerPath))
                {
                    log.LogDebug($"Copying Registered Server file to local path {localRegisteredServerPath}");
                    File.WriteAllText(localRegisteredServerPath, serverFileContents);
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Unable to load Registered Servers file at {RegisteredServerHelper.RegisteredServerFileName}");
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

        internal static RegisteredServers DeserializeRegisteredServers(string serverFileContents)
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
            catch (Exception exe)
            {
                log.LogError("Error deserializing registered server contents", exe);
            }

            return null;
        }
        internal static bool SerializeRegisteredServers(RegisteredServers regServers, string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                log.LogWarning("Unable to serialize registered servers. The destination fileName is null or empty");
                return false;
            }

            if (regServers == null)
            {
                log.LogWarning($"Unable to serialize registered servers to {fileName}. \"RegisteredServers\" object is null");
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
                log.LogError(exe, $"Unable to serialze file to {fileName}");
                return false;
            }
        }
    }
}
