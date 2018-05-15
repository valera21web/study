using ImageWithSecretLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageWithSecretLibrary
{
    public class ADD : IEncrypt
    {
        private string Key;

        public ADD(string key)
        {
            this.Key = key;
        }

        public byte GetID()
        {
            return 0b00001000;
        }
        // XOR'es bits with repeating Key pattern
        public byte[] Encrypt(byte[] plainText)
        {
            BitArray arr = new BitArray(plainText);
            BitArray KeyRepeat = new BitArray(plainText);
            BitArray KeyArr = new BitArray(Encoding.UTF8.GetBytes(Key));
            int KeyArrLen = KeyArr.Length;

            for (int i = 0; i < KeyRepeat.Length; i++)
            {
                KeyRepeat[i] = KeyArr[i % KeyArrLen];
            }

            byte[] result = new byte[plainText.Length];
            arr.Xor(KeyRepeat).CopyTo(result, 0);
            return result;
        }

        public byte[] Decrypt(byte[] cipherText)
        {
            return Encrypt(cipherText);
        }
    }
}
