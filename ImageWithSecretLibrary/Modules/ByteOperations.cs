using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageWithSecretLibrary.Modules
{
    public class ByteOperations
    {
        private readonly byte[] bits = { 0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80 };

        public List<byte[]> SplitDataBytes(byte[] inputData)
        {
            List<byte[]> data = new List<byte[]>();

            int len = inputData.Length * 2;
            for (int i = 0; i < len;)
            {
                byte[] tmp = new byte[3];
                for (int p = 0; p < 3 && i < len; ++p, ++i)
                {
                    var j = (int)(i / 2);
                    if (i % 2 == 1)
                    {
                        tmp[p] = (byte)(inputData[j] & 0b00001111);
                    }
                    else
                    {
                        tmp[p] = (byte)(inputData[j] >> 4);
                    }
                }
                data.Add(tmp);
            }
            return data;
        }

        public List<byte> SplitSettingsBytes(BitArray inputData)
        {
            List<byte> data = new List<byte>();

            int bitPossition = inputData.Length - 1, row = 0;
            while (bitPossition >= 0)
            {
                byte tmp = 0x0;
                for (int offsetRight = 4; offsetRight > 0 && bitPossition >= 0; --bitPossition, --offsetRight)
                {
                    if (inputData[bitPossition] == true)
                        tmp = (byte)(tmp | bits[offsetRight - 1]);
                }
                data.Add(tmp);
                ++row;
            }
            return data;
        }

        public byte[] JoinBytes(byte[] inputData)
        {
            byte[] data = new byte[inputData.Length / 2];
            for (int i = 0; i < data.Length; ++i)
            {
                var r = i * 2;
                data[i] = (byte)((inputData[r] << 4) | inputData[r + 1]);
            }
            return data;
        }
    }
}
