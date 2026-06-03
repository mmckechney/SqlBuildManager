using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace SqlSync.SqlBuild.Utilities
{

    /// <summary>
    /// AES-256 encryption helper for settings-file secrets.
    ///
    /// Format v1 (current): authenticated encryption. The base64 payload is laid out as
    ///   [version(1)][salt(16)][iv(16)][ciphertext(n*16)][hmac(32)]
    /// A random salt and IV are generated per call (output is non-deterministic). A 64-byte key
    /// is derived with PBKDF2-SHA256 (100,000 iterations); the first 32 bytes are the AES key and
    /// the next 32 bytes are the HMAC-SHA256 key. The HMAC covers version||salt||iv||ciphertext and
    /// is verified (constant-time) before decryption, so a wrong password fails deterministically.
    ///
    /// Legacy format (read-only, for backward compatibility): raw AES-256-CBC ciphertext whose key
    /// and IV were both derived with PBKDF2-SHA1 (1,000 iterations) from a static salt. Existing
    /// encrypted settings files remain readable; they are upgraded to v1 the next time the value is
    /// saved (callers re-encrypt via <see cref="EncryptText"/>).
    /// </summary>
    public static class Cryptography
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        private const byte FormatVersion1 = 0x01;
        private const int SaltSize = 16;
        private const int IvSize = 16;
        private const int HmacSize = 32;
        private const int Pbkdf2Iterations = 100_000;
        // Header = version + salt + iv; trailer = hmac. Ciphertext is a non-zero multiple of the AES block size.
        private const int HeaderSize = 1 + SaltSize + IvSize;
        private const int MinV1Length = HeaderSize + 16 + HmacSize; // 1 + 16 + 16 + 16 + 32 = 81

        public static string EncryptText(string input, string password)
        {
            try
            {
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);

                // Preserve legacy password normalization (SHA256 of the password) so the same
                // password works across legacy and v1 payloads.
                byte[] passwordBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

                byte[] bytesEncrypted = AES_Encrypt_V1(bytesToBeEncrypted, passwordBytes);

                return Convert.ToBase64String(bytesEncrypted);
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
                if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(password))
                {
                    return (true, input);
                }

                byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                byte[] passwordBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

                byte[] bytesDecrypted;
                if (IsV1Format(bytesToBeDecrypted))
                {
                    // Structurally a v1 payload: only the v1 path can succeed. Never fall back to
                    // legacy (a v1 length can never be mistaken for legacy), so a failed HMAC/decrypt
                    // is a genuine wrong-password/corruption and must report failure.
                    bytesDecrypted = AES_Decrypt_V1(bytesToBeDecrypted, passwordBytes);
                }
                else
                {
                    bytesDecrypted = AES_Decrypt_Legacy(bytesToBeDecrypted, passwordBytes);
                }

                string result = Encoding.UTF8.GetString(bytesDecrypted);
                return (true, result);
            }
            catch (Exception)
            {
                if (!suppressLog)
                {
                    log.LogWarning($"Failed to decrypt {description} value. Returning unmodified string");
                }
                return (false, input);
            }
        }

        /// <summary>
        /// A payload is v1 only if it carries the version marker and its length matches the
        /// fixed-overhead layout. Legacy ciphertext length is always a multiple of 16, while a v1
        /// length is always (16k + 1), so the two are mutually exclusive — no ambiguity.
        /// </summary>
        private static bool IsV1Format(byte[] data)
        {
            return data.Length >= MinV1Length
                && data[0] == FormatVersion1
                && (data.Length - HeaderSize - HmacSize) % 16 == 0;
        }

        // Legacy static salt — required to read settings files encrypted by older versions. Do NOT use for new encryption.
        private static byte[] saltBytes = new byte[] { 84, 104, 105, 115, 73, 115, 83, 113, 108, 66, 117, 105, 108, 100, 77, 97, 110, 97, 103, 101, 114, 66, 121, 116, 101, 73, 110, 105, 116, 105, 97, 108, 105, 122, 97, 116, 105, 111, 110 };

        private static byte[] AES_Encrypt_V1(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

            // Derive 64 bytes: [0..32) AES key, [32..64) HMAC key.
            byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, 64);
            byte[] encKey = new byte[32];
            byte[] macKey = new byte[32];
            Buffer.BlockCopy(keyMaterial, 0, encKey, 0, 32);
            Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

            byte[] cipherText;
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encKey;
                aes.IV = iv;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.FlushFinalBlock();
                }
                cipherText = ms.ToArray();
            }

            // Encrypt-then-MAC over version || salt || iv || ciphertext.
            byte[] macInput = new byte[HeaderSize + cipherText.Length];
            macInput[0] = FormatVersion1;
            Buffer.BlockCopy(salt, 0, macInput, 1, SaltSize);
            Buffer.BlockCopy(iv, 0, macInput, 1 + SaltSize, IvSize);
            Buffer.BlockCopy(cipherText, 0, macInput, HeaderSize, cipherText.Length);

            byte[] mac = HMACSHA256.HashData(macKey, macInput);

            byte[] output = new byte[macInput.Length + HmacSize];
            Buffer.BlockCopy(macInput, 0, output, 0, macInput.Length);
            Buffer.BlockCopy(mac, 0, output, macInput.Length, HmacSize);
            return output;
        }

        private static byte[] AES_Decrypt_V1(byte[] data, byte[] passwordBytes)
        {
            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(data, 1, salt, 0, SaltSize);
            Buffer.BlockCopy(data, 1 + SaltSize, iv, 0, IvSize);

            int cipherLen = data.Length - HeaderSize - HmacSize;
            byte[] cipherText = new byte[cipherLen];
            Buffer.BlockCopy(data, HeaderSize, cipherText, 0, cipherLen);

            byte[] storedMac = new byte[HmacSize];
            Buffer.BlockCopy(data, HeaderSize + cipherLen, storedMac, 0, HmacSize);

            byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, 64);
            byte[] encKey = new byte[32];
            byte[] macKey = new byte[32];
            Buffer.BlockCopy(keyMaterial, 0, encKey, 0, 32);
            Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

            // Verify the MAC (constant-time) before decrypting. A wrong password fails here deterministically.
            byte[] macInput = new byte[HeaderSize + cipherLen];
            Buffer.BlockCopy(data, 0, macInput, 0, HeaderSize + cipherLen);
            byte[] computedMac = HMACSHA256.HashData(macKey, macInput);
            if (!CryptographicOperations.FixedTimeEquals(computedMac, storedMac))
            {
                throw new CryptographicException("Authentication tag mismatch.");
            }

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = encKey;
            aes.IV = iv;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherText, 0, cipherText.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        // Legacy read path: AES-256-CBC, key+IV both derived via PBKDF2-SHA1/1000 from the static salt.
        private static byte[] AES_Decrypt_Legacy(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes;
            using (var AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                AES.Key = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, 1000, HashAlgorithmName.SHA1, AES.KeySize / 8);
                AES.IV = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, 1000, HashAlgorithmName.SHA1, AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }

            return decryptedBytes;
        }

        public static String Sha256Hash(this String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }


    }
}
