using System;
using System.IO;
using System.Security.Cryptography;

namespace Study
{
    public static class AES1
    {
        // Make AES-key based on password given by user
        public static byte[] CreateKey(string key)
        {
            byte[] bytesOfOwnKey = System.Text.Encoding.UTF8.GetBytes(key);

            // Hash the password with SHA256
            byte[] AESKey = SHA256Managed.Create().ComputeHash(bytesOfOwnKey);

            return AESKey;
        }

        public static byte[] Encrypt(byte[] plainText, string key, string iv)
        {
            return Encrypt(plainText, Base64Decode(key), Base64Decode(iv));
        }

        public static byte[] Encrypt(byte[] plainData, byte[] Key, byte[] IV)
        {
            var data = Base64Encode(plainData);
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static byte[] Decrypt(byte[] plainText, string key, string iv)
        {
            return Decrypt(plainText, Base64Decode(key), Base64Decode(iv));
        }

        public static byte[] Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string base64PlainText = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            base64PlainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return Base64Decode(base64PlainText);
        }

        public static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Base64Decode(string data)
        {
            return Convert.FromBase64String(data);
        }
    }
}