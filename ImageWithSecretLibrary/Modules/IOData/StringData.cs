using ImageWithSecretLibrary.Interfaces;
using System;
using System.Text;

namespace ImageWithSecretLibrary.Modules
{
    public class StringData : IData<String>
    {
        private Encoding _encoding;

        public StringData()
        {
            this._encoding = Encoding.UTF8;
        }
        public StringData(Encoding encoding)
        {
            if(encoding == null)
                this._encoding = Encoding.UTF8;
            else
                this._encoding = encoding;
        }

        public byte[] ToBytes(string data)
        {
            return _encoding.GetBytes(data);
        }

        public string ToObject(byte[] plainText)
        {
            return _encoding.GetString(plainText);
        }

        public byte GetID()
        {
            return 0b00000001;
        }
    }
}
