using System;
using System.IO;
using System.Security.Cryptography;

namespace Study
{
    public class Empty : IEncrypt
    {
        public byte[] Encrypt(byte[] plainData, byte[] Key, byte[] IV)
        {
            return null;
        }
        
        public byte[] Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            return null;
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