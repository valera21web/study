using System;

namespace ImageWithSecretLibrary.Interfaces
{
    public interface IData<T>
    {
        byte[] ToBytes(T data);
        T ToObject(byte[] plainText);
        byte GetID();
    }
}
