using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Eoffice.BAL
{
    /// <summary>
    /// Provides deterministic AES-256 encryption/decryption for sensitive data columns.
    /// IMPORTANT: Uses fixed IV for deterministic encryption - same plaintext always produces same ciphertext.
    /// This is required for JOIN operations and WHERE clause equality comparisons.
    /// 
    /// Keys are loaded from Web.config:
    /// - AesEncryptionKey: Base64 encoded 32-byte key
    /// - AesEncryptionIV: Base64 encoded 16-byte IV
    /// </summary>
    public static class DeterministicEncryptionHelper
    {
        private static byte[] _aesKey;
        private static byte[] _aesIV;
        private static readonly object _lock = new object();

        // Lazy load keys from Web.config
        private static byte[] AES_KEY
        {
            get
            {
                if (_aesKey == null)
                {
                    lock (_lock)
                    {
                        if (_aesKey == null)
                        {
                            LoadKeysFromConfig();
                        }
                    }
                }
                return _aesKey;
            }
        }

        private static byte[] AES_IV
        {
            get
            {
                if (_aesIV == null)
                {
                    lock (_lock)
                    {
                        if (_aesIV == null)
                        {
                            LoadKeysFromConfig();
                        }
                    }
                }
                return _aesIV;
            }
        }

        /// <summary>
        /// Loads encryption keys from Web.config
        /// </summary>
        private static void LoadKeysFromConfig()
        {
            try
            {
                string keyBase64 = ConfigurationManager.AppSettings["AesEncryptionKey"];
                string ivBase64 = ConfigurationManager.AppSettings["AesEncryptionIV"];

                if (string.IsNullOrEmpty(keyBase64) || string.IsNullOrEmpty(ivBase64))
                {
                    throw new ConfigurationErrorsException(
                        "Encryption keys not found in Web.config. " +
                        "Please add 'AesEncryptionKey' and 'AesEncryptionIV' to appSettings.");
                }

                _aesKey = Convert.FromBase64String(keyBase64);
                _aesIV = Convert.FromBase64String(ivBase64);

                // Validate key sizes
                if (_aesKey.Length != 32)
                {
                    throw new CryptographicException($"AES key must be 32 bytes (256-bit). Current size: {_aesKey.Length} bytes");
                }

                if (_aesIV.Length != 16)
                {
                    throw new CryptographicException($"AES IV must be 16 bytes (128-bit). Current size: {_aesIV.Length} bytes");
                }
            }
            catch (FormatException ex)
            {
                throw new ConfigurationErrorsException(
                    "Invalid Base64 format for encryption keys in Web.config.", ex);
            }
        }

        /// <summary>
        /// Encrypts a string value using AES-256 with fixed IV (deterministic).
        /// Same input will always produce the same encrypted output.
        /// </summary>
        /// <param name="plainText">The text to encrypt. Returns empty if null/empty.</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public static string Encrypt(string plainText)
        {
            // Handle null or empty
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = AES_KEY;
                    aes.IV = AES_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception in production
                throw new CryptographicException($"Encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts a Base64 encoded encrypted string using AES-256.
        /// </summary>
        /// <param name="cipherText">Base64 encoded encrypted text. Returns empty if null/empty.</param>
        /// <returns>Decrypted plaintext string</returns>
        public static string Decrypt(string cipherText)
        {
            // Handle null or empty
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = AES_KEY;
                    aes.IV = AES_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                // Not a valid Base64 string - might be plaintext that hasn't been encrypted yet
                return cipherText;
            }
            catch (Exception ex)
            {
                // Log the exception in production
                throw new CryptographicException($"Decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a string appears to be encrypted (valid Base64 format).
        /// Not 100% accurate but useful for migration scenarios.
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if the string appears to be Base64 encoded</returns>
        public static bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Base64 strings have specific characteristics
            // - Length is multiple of 4
            // - Only contains valid Base64 characters
            if (value.Length % 4 != 0)
                return false;

            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypts a value only if it's not already encrypted.
        /// Useful during migration phase when some data may already be encrypted.
        /// </summary>
        /// <param name="value">Value to encrypt</param>
        /// <returns>Encrypted value</returns>
        public static string EncryptIfNotEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (IsEncrypted(value))
                return value;

            return Encrypt(value);
        }

        /// <summary>
        /// Safely decrypts a value, returning the original if decryption fails.
        /// Useful during migration phase when some data may still be plaintext.
        /// </summary>
        /// <param name="value">Value to decrypt</param>
        /// <returns>Decrypted value or original if not encrypted</returns>
        public static string SafeDecrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (!IsEncrypted(value))
                return value;

            try
            {
                return Decrypt(value);
            }
            catch
            {
                // If decryption fails, return original value
                return value;
            }
        }
    }
}
