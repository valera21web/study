using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Study
{
    interface IEncrypt
    {
        byte[] Encrypt(byte[] plainText, byte[] key, byte[] key2);
        byte[] Decrypt(byte[] plainText, byte[] key, byte[] key2);
    }
}
