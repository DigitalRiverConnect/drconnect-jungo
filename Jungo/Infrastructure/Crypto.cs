using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jungo.Infrastructure.Config;

namespace Jungo.Infrastructure
{
    public class Crypto : ICrypto
    {
        private readonly AesSettings _aesSettings;
        private readonly bool _enabled;

        public Crypto()
        {
            var thumbprint = ConfigLoader.Get<CryptographicServiceThumbprintConfig>().ThumbPrint;
            _enabled = !string.IsNullOrWhiteSpace(thumbprint);
            if (!_enabled)
                return;

            var cert = GetCertificate(thumbprint);

            var config = ConfigLoader.Get<CryptographicServiceConfig>();

            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"key", config.AesKey},
                {"initVector", config.AesInitVector},
                {"salt", config.AesSalt}
            };

            dict.Keys.ToList().ForEach(key => { dict[key] = Decrypt(cert, dict[key]); });

            _aesSettings = new AesSettings(dict["key"], dict["initVector"], dict["salt"]);

        }

        #region ICrypto Members

        public bool TryEncrypt(string input, out string output)
        {
            output = null;
            try
            {
                output = Encrypt(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryDecrypt(string input, out string output)
        {
            output = null;
            try
            {
                output = Decrypt(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Encrypt(string input)
        {
            if (!_enabled || string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(input)));
        }

        public byte[] Encrypt(byte[] input)
        {
            return _enabled ? Encrypt(_aesSettings, input) : input;
        }

        public string Decrypt(string input)
        {
            if (!_enabled || string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(input)));
        }

        public byte[] Decrypt(byte[] input)
        {
            return _enabled ? Decrypt(_aesSettings, input) : input;
        }

        #endregion

        public class CryptographicServiceThumbprintConfig
        {
            public string ThumbPrint { get; set; }
        }

        public class CryptographicServiceConfig
        {
            public string AesKey { get; set; }
            public string AesInitVector { get; set; }
            public string AesSalt { get; set; }
        }

        public class AesSettings
        {
            public AesSettings(string key, string initVector, string salt)
            {
                var saltBytes = Encoding.Default.GetBytes(salt);
                InitVectorBytes = Encoding.Default.GetBytes(initVector);

                // First, we need to create a password, from which the key will be derived.
                // This password will be generated from the specified key and salt value. 
                var password = new Rfc2898DeriveBytes(key, saltBytes, 2);

                // Use the password to generate pseudo-random bytes for the encryption
                // key. We're using 256-bit AES so we need to specify the size of the 
                // key in bytes (instead of bits) so divide the key size by 8.
                KeyBytes = password.GetBytes(256/8);
            }

            public byte[] InitVectorBytes { get; private set; }
            public byte[] KeyBytes { get; private set; }
        }

        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            var cleanedUpThumbprint = thumbprint.Replace(" ", "").ToUpper();
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var cert = store.Certificates.Cast<X509Certificate2>()
                .FirstOrDefault(
                    c => c.Thumbprint.Equals(cleanedUpThumbprint, StringComparison.InvariantCultureIgnoreCase));

            if (cert == null)
                throw new Exception(string.Format("Cannot retrieve certificate based on thumbprint {0}", thumbprint));

            return cert;
        }

        private static byte[] Encrypt(AesSettings aesSettings, byte[] input)
        {
            // Generate a decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of key bytes.
            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};
            using (var encryptor = symmetricKey.CreateEncryptor(aesSettings.KeyBytes, aesSettings.InitVectorBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(input, 0, input.Length);
                        cryptoStream.FlushFinalBlock();
                        var encryptedBytes = memoryStream.ToArray();
                        return encryptedBytes;
                    }
                }
            }
        }

        private static byte[] Decrypt(AesSettings aesSettings, byte[] input)
        {
            // Generate a decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of key bytes.
            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};
            using (var decryptor = symmetricKey.CreateDecryptor(aesSettings.KeyBytes, aesSettings.InitVectorBytes))
            {
                using (var memoryStream = new MemoryStream(input, 0, input.Length))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        // Since at this point we don't know what the size of decrypted data
                        // will be, allocate the buffer long enough to hold the source bytes.
                        // The source bytes will never be longer than the decrypted bytes.
                        var decryptedBytes = new byte[input.Length];
                        var decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                        var result = new byte[decryptedByteCount];
                        Array.Copy(decryptedBytes, result, decryptedByteCount);
                        return result;
                    }
                }
            }
        }

        private static string Decrypt(X509Certificate2 cert, string input)
        {
            // Decrypts the Encrypted message using the private key.
            var providerReceiver = (RSACryptoServiceProvider)cert.PrivateKey;
            var plainReceiver = providerReceiver.Decrypt(Convert.FromBase64String(input), false);

            return Encoding.UTF8.GetString(plainReceiver);
        }
    }
}
