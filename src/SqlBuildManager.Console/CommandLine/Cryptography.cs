using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using sb = SqlSync.SqlBuild;
namespace SqlBuildManager.Console.CommandLine
{

    /// <summary>
    /// http://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
    /// </summary>
    public static class Cryptography
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CommandLineArgs EncryptSensitiveFields(CommandLineArgs cmdLine)
        {
            //Don't double in encrypt..
            bool tmp;
            (tmp,cmdLine) = DecryptSensitiveFields(cmdLine, true); 
            string key = GetSettingsFileEncryptionKey(cmdLine);

            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                cmdLine.AuthenticationArgs.UserName = sb.Cryptography.EncryptText(cmdLine.AuthenticationArgs.UserName, key);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                cmdLine.AuthenticationArgs.Password = sb.Cryptography.EncryptText(cmdLine.AuthenticationArgs.Password, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                cmdLine.ConnectionArgs.BatchAccountKey = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.BatchAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                cmdLine.ConnectionArgs.StorageAccountKey = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.StorageAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                cmdLine.ConnectionArgs.EventHubConnectionString = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                cmdLine.ConnectionArgs.StorageAccountKey = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.StorageAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                cmdLine.ConnectionArgs.EventHubConnectionString = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = sb.Cryptography.EncryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key);
            }

            return cmdLine;
        }

        public static (bool, CommandLineArgs) DecryptSensitiveFields(CommandLineArgs cmdLine, bool suppressLog  = false)
        {
            //Nothing to do if none of the settings came from a settings file!
            if(string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                return (true,cmdLine);
            }
            bool consolidated = true;
            bool success;
            string key = GetSettingsFileEncryptionKey(cmdLine);

            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                (success,  cmdLine.AuthenticationArgs.UserName) = sb.Cryptography.DecryptText(cmdLine.AuthenticationArgs.UserName, key, "--username", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                (success, cmdLine.AuthenticationArgs.Password) = sb.Cryptography.DecryptText(cmdLine.AuthenticationArgs.Password, key, "--password", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                (success, cmdLine.ConnectionArgs.BatchAccountKey) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.BatchAccountKey, key, "--batchaccountkey", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                (success, cmdLine.ConnectionArgs.StorageAccountKey) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.StorageAccountKey, key, "--storageaccountkey", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                (success, cmdLine.ConnectionArgs.EventHubConnectionString) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key, "--eventhubconnection", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                (success, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key, "--servicebustopicconnection", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                (success, cmdLine.ConnectionArgs.StorageAccountKey) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.StorageAccountKey, key, "--storageaccountkey", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                (success, cmdLine.ConnectionArgs.EventHubConnectionString) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key, "--eventhubconnection", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                (success, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) = sb.Cryptography.DecryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key, "--servicebustopicconnection", suppressLog);
                consolidated = consolidated & success;
            }

            return (consolidated, cmdLine);
        }


        private static string GetSettingsFileEncryptionKey(CommandLineArgs cmdLine)
        {
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey))
            {
               if(File.Exists(Path.GetFullPath(cmdLine.SettingsFileKey)))
                {
                    return File.ReadAllText(cmdLine.SettingsFileKey).Trim();
                }
               else
                {
                    return cmdLine.SettingsFileKey;
                }
            }
           
            var ev = Environment.GetEnvironmentVariable(keyEnvronmentVariableName);
            if (!string.IsNullOrWhiteSpace(ev))
            {
                return ev;
            }

            return GetDerivedKey();
        }
        private static readonly string keyEnvronmentVariableName = "sbm-settingsfilekey";
        private static readonly string store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", "sbm-store.txt");
        private static readonly string kek = "x/A?D(G+KbPeShVmYq3t6w9y$B&E)H@McQfTjWnZr4u7x!A%C*F-JaNdRgUkXp2s5v8y/B?E(G+KbPeShVmYq3t6w9z$C&F)J@McQfTjWnZr4u7x!A%D*G-KaPdRgUkXp2s5v8y/B?E(H+MbQeThVmYq3t6w9z$C&F)J@NcRfUjXnZr4u7x!A%D*G-KaPdSgVkYp3s5v8y/B?E(H+MbQeThWmZq4t7w9z$C&F)J@NcRfUjXn2r5u8x/A%D*G-KaP";
        private static string GetDerivedKey()
        {


            if(!File.Exists(store))
            {
                SetDerivedKey();
            }
            string e = File.ReadAllText(store);
            (bool success,string pt) = sb.Cryptography.DecryptText(e, kek,"");
            return pt;
        }
        private static bool SetDerivedKey()
        {
            string key = GenerateEncryptionKey();
            string wrapped = sb.Cryptography.EncryptText(key, kek);
            File.WriteAllText(store,wrapped);
            return true;
        }
        private static string GenerateEncryptionKey()
        {
            var a = Aes.Create();
            a.GenerateKey();
           var encoded =  Convert.ToBase64String(a.Key);
            return encoded;
        }
    }
}
