using ImageWithSecretLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageWithSecretLibrary
{
    public class INV : IEncrypt
    {
        private string Key;
        public INV(string key)
        {
            this.Key = key;
        }

        public byte GetID()
        {
            return 0b00000100;
        }

        public byte[] Encrypt(byte[] plainText)
        {
            byte[] encrypted = new byte[plainText.Length];

            // Apply NOR operation for every bit of plainText
            ((new BitArray(plainText)).Not()).CopyTo(encrypted, 0);

            return encrypted;
        }

        public byte[] Decrypt(byte[] cipherText)
        {
            return Encrypt(cipherText);
        }
    }
}
