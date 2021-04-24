using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using SqlSync.Connection;
using Microsoft.Extensions.Logging;
namespace SqlSync.SqlBuild
{
   
    /// <summary>
    /// http://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
    /// </summary>
    public static class Cryptography
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string EncryptText(string input, string password)
        {
            try
            {
                // Get the bytes of the string
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

                string result = Convert.ToBase64String(bytesEncrypted);

                return result;
            }
            catch
            {
                return input;
            }
        }

        public static (bool, string) DecryptText(string input, string password, string description, bool suppressLog = false)
        {
            try
            {
                // Get the bytes of the string
                byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

                string result = Encoding.UTF8.GetString(bytesDecrypted);

                return (true, result);
            }
            catch (Exception exe)
            {
                if (!suppressLog)
                {
                    log.LogWarning($"Failed to decrypt {description} value. Returning unmodified string");
                }
                return (false, input);
            }
        }

        private static byte[] saltBytes = new byte[] { 84, 104, 105, 115, 73, 115, 83, 113, 108, 66, 117, 105, 108, 100, 77, 97, 110, 97, 103, 101, 114, 66, 121, 116, 101, 73, 110, 105, 116, 105, 97, 108, 105, 122, 97, 116, 105, 111, 110 };

        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static String Sha256Hash(this String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static CommandLineArgs EncryptSensitiveFields(CommandLineArgs cmdLine)
        {
            //Don't double in encrypt..
            bool tmp;
            (tmp,cmdLine) = DecryptSensitiveFields(cmdLine, true); 
            string key = GetSettingsFileEncryptionKey(cmdLine);

            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                cmdLine.AuthenticationArgs.UserName = Cryptography.EncryptText(cmdLine.AuthenticationArgs.UserName, key);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                cmdLine.AuthenticationArgs.Password = Cryptography.EncryptText(cmdLine.AuthenticationArgs.Password, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchAccountKey))
            {
                cmdLine.BatchArgs.BatchAccountKey = Cryptography.EncryptText(cmdLine.BatchArgs.BatchAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.StorageAccountKey))
            {
                cmdLine.BatchArgs.StorageAccountKey = Cryptography.EncryptText(cmdLine.BatchArgs.StorageAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.EventHubConnectionString))
            {
                cmdLine.BatchArgs.EventHubConnectionString = Cryptography.EncryptText(cmdLine.BatchArgs.EventHubConnectionString, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ServiceBusTopicConnectionString))
            {
                cmdLine.BatchArgs.ServiceBusTopicConnectionString = Cryptography.EncryptText(cmdLine.BatchArgs.ServiceBusTopicConnectionString, key);
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
                (success,  cmdLine.AuthenticationArgs.UserName) = Cryptography.DecryptText(cmdLine.AuthenticationArgs.UserName, key, "--username", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                (success, cmdLine.AuthenticationArgs.Password) = Cryptography.DecryptText(cmdLine.AuthenticationArgs.Password, key, "--password", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchAccountKey))
            {
                (success, cmdLine.BatchArgs.BatchAccountKey) = Cryptography.DecryptText(cmdLine.BatchArgs.BatchAccountKey, key, "--batchaccountkey", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.StorageAccountKey))
            {
                (success, cmdLine.BatchArgs.StorageAccountKey) = Cryptography.DecryptText(cmdLine.BatchArgs.StorageAccountKey, key, "--storageaccountkey", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.EventHubConnectionString))
            {
                (success, cmdLine.BatchArgs.EventHubConnectionString) = Cryptography.DecryptText(cmdLine.BatchArgs.EventHubConnectionString, key, "--eventhubconnection", suppressLog);
                consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ServiceBusTopicConnectionString))
            {
                (success, cmdLine.BatchArgs.ServiceBusTopicConnectionString) = Cryptography.DecryptText(cmdLine.BatchArgs.ServiceBusTopicConnectionString, key, "--servicebustopicconnection", suppressLog);
                consolidated = consolidated & success;
            }

            return (consolidated, cmdLine);
        }


        private static string GetSettingsFileEncryptionKey(CommandLineArgs cmdLine)
        {
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey))
            {
               if(File.Exists(cmdLine.SettingsFileKey))
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
            (bool success,string pt) = DecryptText(e, kek,"");
            return pt;
        }
        private static bool SetDerivedKey()
        {
            string key = GenerateEncryptionKey();
            string wrapped = EncryptText(key, kek);
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
