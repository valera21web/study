using Study;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibConvert
{
    public class LibConverter
    {
        private Encoding encoding = Encoding.UTF8;

        private const byte _NOT = 0x0;
        private const byte _R = 0x4;
        private const byte _G = 0x2;
        private const byte _B = 0x1;

        private readonly byte[] bits = { 0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80 };

        private const byte _AES = 0x4;

        private Bitmap imageOriginal;
        private Bitmap imageModified;

        private const int countBitsToWrite = 4;
        private byte R_Available = _R;
        private byte G_Available = _G;
        private byte B_Available = _B;
        private byte AESEncrypt = _NOT;

        private byte[] dataToEncrypt;


        public LibConverter(Bitmap image)
        {
            this.imageOriginal = image;
        }

        public LibConverter(Bitmap image, bool isR, bool isG, bool isB, int countLastBitsToWrite)
        {
            this.imageOriginal = image;
            this.R_Available = (byte)(isR ? _R : _NOT);
            this.G_Available = (byte)(isG ? _G : _NOT);
            this.B_Available = (byte)(isB ? _B : _NOT);
        }

        #region Set/Get data
        public LibConverter SetR(bool isR)
        {
            this.R_Available = (byte)(isR ? _R : _NOT);
            return this;
        }
        public LibConverter SetEncodedImage(Bitmap image)
        {
            this.imageModified = image;
            return this;
        }
        public LibConverter SetG(bool isG)
        {
            this.G_Available = (byte)(isG ? _G : _NOT);
            return this;
        }
        public LibConverter SetB(bool isB)
        {
            this.B_Available = (byte)(isB ? _B : _NOT);
            return this;
        }
        public LibConverter SetDataToEncrypt(byte[] data)
        {
            this.dataToEncrypt = data;
            return this;
        }
        public LibConverter SetDataToEncrypt(string data)
        {
            this.dataToEncrypt = StringToBytes(data);
            return this;
        }
        public string GetEncryptedData()
        {
            return AES.Base64Encode(this.dataToEncrypt);
        }
        #endregion

        public LibConverter AESEncryptData(byte[] key, byte[] iv)
        {
            this.dataToEncrypt = AES.Encrypt(this.dataToEncrypt, key, iv);
            AESEncrypt = _AES;
            return this;
        }

        #region Do encrypt
        public Bitmap Do()
        {
            imageModified = new Bitmap(imageOriginal);

            #region Write configs into image on Alpha path of color
            byte confRGB = (byte)(R_Available | G_Available | B_Available);
            // write cofig: used R, G, B colors for writing data
            var pixel = imageModified.GetPixel(0, 0);
            imageModified.SetPixel(0, 0, Color.FromArgb(pixel.A ^ confRGB, pixel.R, pixel.G, pixel.B));

            // write cofig: count of bit can use for write data
            pixel = imageModified.GetPixel(1, 0);
            imageModified.SetPixel(1, 0, Color.FromArgb(pixel.A ^ countBitsToWrite, pixel.R, pixel.G, pixel.B));

            // write cofig: used or not AES encrypt
            pixel = imageModified.GetPixel(2, 0);
            imageModified.SetPixel(2, 0, Color.FromArgb(pixel.A ^ AESEncrypt, pixel.R, pixel.G, pixel.B));

            var dataLengthInBits = this.dataToEncrypt.Length * 8;
            var toWriteLength = ConvertDataToBytes(
                new BitArray(BitConverter.GetBytes(dataLengthInBits)),
                1 /* use only one alpha path */
                );
            int col = 0;
            foreach (var row in toWriteLength)
            {
                pixel = imageModified.GetPixel(col, 1);
                imageModified.SetPixel(col, 1, Color.FromArgb(pixel.A ^ row[0], pixel.R, pixel.G, pixel.B));
                ++col;
            }
            #endregion
            
            // write data into image
            var toWrite = ConvertDataToBytes(this.dataToEncrypt, 3/* to edit -> Available */);

            int x = 0, y = 1;
            foreach (var row in toWrite)
            {
                if (x >= imageModified.Width)
                {
                    y++; x = 0;
                }
                pixel = imageModified.GetPixel(x, y);
                var tmp = Color.FromArgb(pixel.A, pixel.R ^ row[0], pixel.G ^ row[1], pixel.B ^ row[2]);
                imageModified.SetPixel(x, y, tmp);
                ++x;
            }
            var pixel1 = imageModified.GetPixel(0, 1);
            var pixel2 = imageModified.GetPixel(1, 1);
            var pixel3 = imageModified.GetPixel(2, 1);
            return imageModified;
        }
        #endregion

        #region Decrypt
        public string Decrypt(byte[] key, byte[] iv)
        {
            var pixel = imageOriginal.GetPixel(0, 0);
            var pixelEnc = imageModified.GetPixel(0, 0);
            var RGB = pixel.A ^ pixelEnc.A;
            var isR = (RGB & _R) == _R;
            var isG = (RGB & _G) == _G;
            var isB = (RGB & _B) == _B;


            pixel = imageOriginal.GetPixel(1, 0);
            pixelEnc = imageModified.GetPixel(1, 0);
            var countBitsToWrite = pixel.A ^ pixelEnc.A;

            pixel = imageOriginal.GetPixel(2, 0);
            pixelEnc = imageModified.GetPixel(2, 0);
            var _AESEncrypt = pixel.A ^ pixelEnc.A;
            var isAESEncrypt = (_AESEncrypt & _AES) == _AES;

            byte[] countDataList = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                var r = i * 2;
                pixel = imageOriginal.GetPixel(r, 1);
                pixelEnc = imageModified.GetPixel(r, 1);
                var bigPath = pixel.A ^ pixelEnc.A;
                pixel = imageOriginal.GetPixel(r + 1, 1);
                pixelEnc = imageModified.GetPixel(r + 1, 1);
                var littlePath = pixel.A ^ pixelEnc.A;
                bigPath = bigPath << 4;

                countDataList[3 - i] = (byte)(bigPath | littlePath); // index from end becouse big endian format
            }


            #region get encrypted data

            var dataLengthBits = BitConverter.ToInt32(countDataList, 0);
            var dataLengthBytes = dataLengthBits / 8;
            countDataList = new byte[dataLengthBytes * 2];
            
            int x = 0, y = 1;
            for (int i = 0; i < countDataList.Length; )
            {
                if (x >= imageModified.Width)
                {
                    y++; x = 0;
                }
                pixel = imageOriginal.GetPixel(x, y);
                pixelEnc = imageModified.GetPixel(x, y);

                if (isR && i < countDataList.Length)
                {
                    countDataList[i] = (byte)(pixel.R ^ pixelEnc.R);
                    ++i;
                }
                if (isG && i < countDataList.Length)
                {
                    countDataList[i] = (byte)(pixel.G ^ pixelEnc.G);
                    ++i;
                }
                if (isB && i < countDataList.Length)
                {
                    countDataList[i] = (byte)(pixel.B ^ pixelEnc.B);
                    ++i;
                }
                ++x;
            }
            
            byte[] data = new byte[countDataList.Length / 2];
            for (int i = 0; i < data.Length; ++i)
            {
                var r = i * 2;
                data[i] = (byte)((countDataList[r] << 4) | countDataList[r + 1]);
            }
            #endregion

            if(isAESEncrypt)
            {
                data = AES.Decrypt(data, key, iv);
            }

            return BytesToString(data);
        }
        #endregion

        #region Private helpers
        private List<byte[]> ConvertDataToBytes(BitArray inputData, int countPaths)
        {
            List<byte[]> data = new List<byte[]>();

            int bitPossition = inputData.Length - 1, row = 0;
            while (bitPossition >= 0)
            {
                byte[] tmp = new byte[countPaths];
                for (int p = 0; p < countPaths; ++p)
                {
                    tmp[p] = 0x0;
                    for (int offsetRight = countBitsToWrite; offsetRight > 0 && bitPossition >= 0; --bitPossition, --offsetRight)
                    {
                        if (inputData[bitPossition] == true)
                            tmp[p] = (byte)(tmp[p] | bits[offsetRight - 1]);
                    }
                }
                data.Add(tmp);
                ++row;
            }
            return data;
        }
        private List<byte[]> ConvertDataToBytes(byte[] inputData, int countPaths)
        {
            List<byte[]> data = new List<byte[]>();

            int len = inputData.Length * 2;
            for (int i = 0; i < len;)
            {
                byte[] tmp = new byte[countPaths];
                for (int p = 0; p < countPaths && i < len; ++p, ++i)
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

        private byte[] StringToBytes(string input)
        {
            return encoding.GetBytes(input);
        }
        private string BytesToString(byte[] input)
        {
            return encoding.GetString(input);
        }
        #endregion
    }
}
