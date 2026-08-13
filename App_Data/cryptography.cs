using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

/// <summary>
/// Summary description for cryptography
/// </summary>
/// 


public partial class cryptography
{

    #region Fields
    private static byte[] key = { };
    private static byte[] IV = { 38, 55, 206, 48, 28, 64, 20, 16 };
    private static string stringKey = "!5663a#KN";
    private const int keysize = 256;
    #endregion
    private static byte[] key_check = { };
    private static byte[] IV_check = { 38, 55, 206, 48, 28, 64, 20, 16 };
    private static string stringKey_check = "@BSA&12@!@#$3456legal!5663a#KN";
    
    static public string base64Encode(string sData)
    {
        try
        {
            byte[] encData_byte = new byte[sData.Length];

            encData_byte = System.Text.Encoding.UTF8.GetBytes(sData);
            //encData_byte = System.Text.ASCIIEncoding.Default.GetBytes(sData);
            string encodedData = Convert.ToBase64String(encData_byte);
            //string encodedData = Convert.ToString(encData_byte);
            return encodedData;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in base64Encode" + ex.Message);
        }

    }
    static public string base64Decode(string sData)
    {

        //System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();

        //System.Text.Decoder utf8Decode = encoder.GetDecoder();

        ////byte[] todecode_byte = Convert.FromBase64String(sData);

        //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        //byte[] todecode_byte = encoding.GetBytes(sData);

        //int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);

        //char[] decoded_char = new char[charCount];

        //utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);

        //string result = new String(decoded_char);

        if (sData.Length > 0)
        {
            byte[] todecode_byte = Convert.FromBase64String(sData);

            string encodedData = System.Text.Encoding.UTF8.GetString(todecode_byte);
            return encodedData;
        }
        else
        {
            return "";
        }
    }

    public static string Encrypt(string text)
    {
        try
        {
            byte[] encData_byte = new byte[text.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(text);
            string encodedData = Convert.ToBase64String(encData_byte);
            return encodedData;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in base64Encode" + ex.Message);
        }
    }

    public static string Decrypt(string text)
    {
        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        System.Text.Decoder utf8Decode = encoder.GetDecoder();
        byte[] todecode_byte = Convert.FromBase64String(text);
        int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
        char[] decoded_char = new char[charCount];
        utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
        string result = new String(decoded_char);
        return result;
    } 


    public static string EncryptText(string plainText)
    {
        string password = stringKey_check;
        if (plainText == null)
        {
            return null;
        }
        if (password == null)
        {
            password = String.Empty;
        }
        // Get the bytes of the string
        var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        // Hash the password with SHA256 
        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
        var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);
        return Convert.ToBase64String(bytesEncrypted);
    }

    /// <summary>
    /// Decrypt a string.
    /// </summary>
    /// <param name="encryptedText">String to be decrypted</param>
    /// <param name="password">Password used during encryption</param>
    /// <exception cref="FormatException"></exception>
    public static string DecryptText(string encryptedText)
    {
        string password = stringKey_check;
        if (encryptedText == null)
        {
            return null;
        }
        if (password == null)
        {
            password = String.Empty;
        }
        // Get the bytes of the string
        var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
        var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);
        return Encoding.UTF8.GetString(bytesDecrypted);
    }

    private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[] encryptedBytes = null;
        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.KeySize = 256;
                AES.BlockSize = 128;
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

    private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[] decryptedBytes = null;
        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.KeySize = 256;
                AES.BlockSize = 128;
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


    public static string CreateHash256(string PlainText)
    {
        var crypt = new SHA256Managed();
        string hash1 = String.Empty;
        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(PlainText));
        foreach (byte theByte in crypto)
        {
            hash1 += theByte.ToString("x2");
        }
        return hash1;
    }
}
