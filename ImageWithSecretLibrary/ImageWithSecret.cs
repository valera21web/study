using ImageWithSecretLibrary.Interfaces;
using ImageWithSecretLibrary.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageWithSecretLibrary
{
    public class ImageWithSecret<T_Data>
    {
        public Bitmap Image { set; get; }
        public IData<T_Data> DataReader { set; get; }
        public IWriteReadPixelData WriteReadData { set; get; }
        public List<IEncrypt> Encrypts = new List<IEncrypt>();
        public ICompression DataCompression { set; get; }
        private ByteOperations _byteOperations = new ByteOperations();
        private LockBitmap _lockBitmap;

        public Bitmap Encrypt(T_Data data)
        {
            if (Image == null)
                throw new ArgumentException("Parameter cannot be null", "Image");
            if (DataReader == null)
                throw new ArgumentException("Parameter cannot be null", "DataReader");
            if (WriteReadData == null)
                throw new ArgumentException("Parameter cannot be null", "WriteReadData");
            _lockBitmap = new LockBitmap(Image);
            _lockBitmap.LockBits();

            var dataToEncrypt = DataReader.ToBytes(data);
            WriteReadData.SetSettingsMode();
            if(Encrypts != null && Encrypts.Count > 0)
            {
                foreach(var enc in Encrypts)
                {
                    dataToEncrypt = enc.Encrypt(dataToEncrypt);
                }
            }
            if(DataCompression != null)
            {
                dataToEncrypt = DataCompression.Compression(dataToEncrypt);
            }

            #region Write configs into image on Alpha path of color

            var dataLengthInBits = dataToEncrypt.Length * 8;
            var toWriteLength = _byteOperations.SplitSettingsBytes(new BitArray(BitConverter.GetBytes(dataLengthInBits)));
            int col = 0, rowIndex = 1;
            foreach (var row in toWriteLength)
            {
                _lockBitmap.SetPixel(col, rowIndex, WriteReadData.WriteData(_lockBitmap.GetPixel(col, rowIndex), row, null, null, null));
                ++col;
            }
            #endregion
            
            WriteReadData.SetDataMode();
            var toWrite = _byteOperations.SplitDataBytes(dataToEncrypt);
            int x = 0, y = 1;
            foreach (var row in toWrite)
            {
                if (x >= Image.Width)
                {
                    y++; x = 0;
                }
                var p = _lockBitmap.GetPixel(x, y);
                var np = WriteReadData.WriteData(p, null, row[0], row[1], row[2]);
                _lockBitmap.SetPixel(x, y, np);
                var p4 = _lockBitmap.GetPixel(x, y);
                ++x;
            }
            // Unlock the bits.
            _lockBitmap.UnlockBits();
            var p1 = _lockBitmap.GetPixel(0, 1);
            var p2 = _lockBitmap.GetPixel(1, 1);
            var p3 = _lockBitmap.GetPixel(2, 1);
            Image = _lockBitmap.GetImage();
            var p11 = Image.GetPixel(0, 1);
            var p21 = Image.GetPixel(1, 1);
            var p31 = Image.GetPixel(2, 1);
            return Image;
        }

        // original: maybe is null
        public T_Data Decrypt(Bitmap original)
        {
            WriteReadData.SetSettingsMode();
            //var pixel = original.GetPixel(0, 0);
            //var pixelEnc = Image.GetPixel(0, 0);
            

            byte[] countDataList = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                var r = i * 2;
                var bigPath = WriteReadData.ReadData(Image.GetPixel(r, 1), original == null ? new Color() : original.GetPixel(r, 1))[0];
                var littlePath = WriteReadData.ReadData(Image.GetPixel(r + 1, 1), original == null ? new Color() : original.GetPixel(r + 1, 1))[0];

                bigPath = (byte) (bigPath << 4);
                countDataList[3 - i] = (byte)(bigPath | littlePath); // index from end becouse big endian format
            }
            var dataLengthBits = BitConverter.ToInt32(countDataList, 0);
            var dataLengthBytes = dataLengthBits / 8;

            countDataList = new byte[dataLengthBytes * 2];

            WriteReadData.SetDataMode();
            int x = 0, y = 1;
            for (int i = 0; i < countDataList.Length;)
            {
                if (x >= Image.Width)
                {
                    y++; x = 0;
                }
                var pixelEnc = Image.GetPixel(x, y);
                var pixelOrg = original == null ? new Color() : original.GetPixel(x, y);
                var d = WriteReadData.ReadData(pixelEnc, pixelOrg);


                if (i < countDataList.Length)
                    countDataList[i++] = d[0];
                if (i < countDataList.Length)
                    countDataList[i++] = d[1];
                if (i < countDataList.Length)
                    countDataList[i++] = d[2];
                ++x;
            }

            var data = _byteOperations.JoinBytes(countDataList);

            // Encrypt aes ....
            // decompression

            return DataReader.ToObject(data);
        }
    }
}
