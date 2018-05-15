using System;

namespace ImageWithSecretLibrary.Interfaces
{
    public interface ICompression
    {
        byte[] Compression(byte[] plainText);
        byte[] Recovery(byte[] plainText);
        byte GetID();
    }
}
