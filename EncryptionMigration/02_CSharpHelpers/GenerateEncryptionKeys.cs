using System;
using System.Security.Cryptography;

namespace Eoffice.Security
{
    /// <summary>
    /// One-time utility to generate encryption keys
    /// Run this ONCE to generate keys, then copy to Web.config and App.config
    /// </summary>
    class GenerateEncryptionKeys
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("Encryption Key Generator");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("Generating AES-256 encryption keys...");
            Console.WriteLine();

            // Generate 256-bit (32 byte) key
            byte[] key = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            // Generate 128-bit (16 byte) IV
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }

            // Convert to Base64 for storage
            string keyBase64 = Convert.ToBase64String(key);
            string ivBase64 = Convert.ToBase64String(iv);

            Console.WriteLine("Generated Keys:");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("AES Key (Base64):");
            Console.WriteLine(keyBase64);
            Console.WriteLine();
            Console.WriteLine("AES IV (Base64):");
            Console.WriteLine(ivBase64);
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("Add these to your Web.config and App.config:");
            Console.WriteLine();
            Console.WriteLine("<appSettings>");
            Console.WriteLine($"  <add key=\"AesEncryptionKey\" value=\"{keyBase64}\" />");
            Console.WriteLine($"  <add key=\"AesEncryptionIV\" value=\"{ivBase64}\" />");
            Console.WriteLine("</appSettings>");
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("IMPORTANT:");
            Console.WriteLine("1. Copy these keys to BOTH Web.config AND App.config");
            Console.WriteLine("2. Keys must be IDENTICAL in both files");
            Console.WriteLine("3. Store keys securely (never commit to source control)");
            Console.WriteLine("4. Use Azure Key Vault or similar in production");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
