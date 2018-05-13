using System;

namespace ImageWithSecretLibrary.Interfaces
{
    public interface IEncrypt
    {
        byte[] Encrypt(byte[] plainText);
        byte[] Decrypt(byte[] cipherText);
        byte GetID();
    }
}
