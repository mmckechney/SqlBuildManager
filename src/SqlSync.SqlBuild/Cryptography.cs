using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using SqlSync.Connection;
namespace SqlSync.SqlBuild
{
    /// <summary>
    /// http://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
    /// </summary>
    public static class Cryptography
    {
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

        public static string DecryptText(string input, string password)
        {
            try
            {
                // Get the bytes of the string
                byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

                string result = Encoding.UTF8.GetString(bytesDecrypted);

                return result;
            }
            catch
            {
                return input;
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
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                cmdLine.AuthenticationArgs.UserName = Cryptography.EncryptText(cmdLine.AuthenticationArgs.UserName, ConnectionHelper.ConnectCryptoKey);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                cmdLine.AuthenticationArgs.Password = Cryptography.EncryptText(cmdLine.AuthenticationArgs.Password, ConnectionHelper.ConnectCryptoKey);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchAccountKey))
            {
                cmdLine.BatchArgs.BatchAccountKey = Cryptography.EncryptText(cmdLine.BatchArgs.BatchAccountKey, ConnectionHelper.ConnectCryptoKey);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.StorageAccountKey))
            {
                cmdLine.BatchArgs.StorageAccountKey = Cryptography.EncryptText(cmdLine.BatchArgs.StorageAccountKey, ConnectionHelper.ConnectCryptoKey);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.EventHubConnectionString))
            {
                cmdLine.BatchArgs.EventHubConnectionString = Cryptography.EncryptText(cmdLine.BatchArgs.EventHubConnectionString, ConnectionHelper.ConnectCryptoKey);
            }

            return cmdLine;
        }

        public static CommandLineArgs DecryptSensitiveFields(CommandLineArgs cmdLine)
        {
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                cmdLine.AuthenticationArgs.UserName = Cryptography.DecryptText(cmdLine.AuthenticationArgs.UserName, ConnectionHelper.ConnectCryptoKey);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                cmdLine.AuthenticationArgs.Password = Cryptography.DecryptText(cmdLine.AuthenticationArgs.Password, ConnectionHelper.ConnectCryptoKey);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchAccountKey))
            {
                cmdLine.BatchArgs.BatchAccountKey = Cryptography.DecryptText(cmdLine.BatchArgs.BatchAccountKey, ConnectionHelper.ConnectCryptoKey);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.StorageAccountKey))
            {
                cmdLine.BatchArgs.StorageAccountKey = Cryptography.DecryptText(cmdLine.BatchArgs.StorageAccountKey, ConnectionHelper.ConnectCryptoKey);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.EventHubConnectionString))
            {
                cmdLine.BatchArgs.EventHubConnectionString = Cryptography.DecryptText(cmdLine.BatchArgs.EventHubConnectionString, ConnectionHelper.ConnectCryptoKey);
            }

            return cmdLine;
        }
    }
}
