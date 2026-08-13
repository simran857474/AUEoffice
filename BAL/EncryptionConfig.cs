using System;
using System.Configuration;

namespace Eoffice.BAL
{
    /// <summary>
    /// Configuration class for encryption settings.
    /// In production, keys should be stored securely (Azure Key Vault, AWS Secrets Manager, etc.)
    /// </summary>
    public static class EncryptionConfig
    {
        /// <summary>
        /// Indicates whether encryption is enabled for the application.
        /// Set to false during development/testing if needed.
        /// </summary>
        public static bool IsEncryptionEnabled
        {
            get
            {
                string value = ConfigurationManager.AppSettings["EnableEncryption"];
                return string.IsNullOrEmpty(value) || bool.Parse(value);
            }
        }

        /// <summary>
        /// Indicates whether we're in migration mode.
        /// During migration, the application should handle both encrypted and plaintext values.
        /// </summary>
        public static bool IsMigrationMode
        {
            get
            {
                string value = ConfigurationManager.AppSettings["EncryptionMigrationMode"];
                return !string.IsNullOrEmpty(value) && bool.Parse(value);
            }
        }

        /// <summary>
        /// Base64 encoded AES key (256-bit / 32 bytes)
        /// WARNING: Store this securely in production!
        /// </summary>
        public static string AesKeyBase64
        {
            get
            {
                return ConfigurationManager.AppSettings["AesEncryptionKey"];
            }
        }

        /// <summary>
        /// Base64 encoded IV (128-bit / 16 bytes)
        /// WARNING: Store this securely in production!
        /// </summary>
        public static string AesIvBase64
        {
            get
            {
                return ConfigurationManager.AppSettings["AesEncryptionIV"];
            }
        }
    }
}
