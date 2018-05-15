using ImageWithSecretLibrary.Interfaces;
using ImageWithSecretLibrary.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageWithSecretLibrary
{
    public class ImageWithSecret<T_Data>
    {
        public Bitmap Image { set; get; }
        public Bitmap ImageOrigimal { set; get; }
        public IData<T_Data> DataReader { set; get; }
        public IWriteReadPixelData WriteReadData { set; get; }
        public List<IEncrypt> Encrypts = new List<IEncrypt>();
        public ICompression DataCompression { set; get; }
        private ByteOperations _byteOperations = new ByteOperations();
        private LockBitmap _lockBitmap;
        private int WriteReadDataIdPosition = 8;
        private int DataCompressionIdPosition = 9;
        private int Encrypt1IdPosition = 10;
        private int Encrypt2IdPosition = 11;

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
            // write config ID of module name for Write/Read data in pixel
            //_lockBitmap.SetPixel(WriteReadDataIdPosition, 0, 
            //    WriteReadData.WriteData(_lockBitmap.GetPixel(WriteReadDataIdPosition, 0), WriteReadData.GetID(), null, null, null));
            setConfPixel(WriteReadDataIdPosition, WriteReadData.GetID());
            if (DataCompression != null)
            {
                dataToEncrypt = DataCompression.Compression(dataToEncrypt);
                // write config ID of module Compression
                //_lockBitmap.SetPixel(DataCompressionIdPosition, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(DataCompressionIdPosition, 0), DataCompression.GetID(), null, null, null));
                setConfPixel(DataCompressionIdPosition, DataCompression.GetID());
            }
            else
            {
                //_lockBitmap.SetPixel(DataCompressionIdPosition, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(DataCompressionIdPosition, 0), 0b00000000, null, null, null));
                setConfPixel(DataCompressionIdPosition, 0b00000000);
            }

            //_lockBitmap.SetPixel(Encrypt1IdPosition, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(Encrypt1IdPosition, 0), 0b00000000, null, null, null));
            //_lockBitmap.SetPixel(Encrypt2IdPosition, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(Encrypt2IdPosition, 0), 0b00000000, null, null, null));
            setConfPixel(Encrypt1IdPosition, 0b00000000);
            setConfPixel(Encrypt2IdPosition, 0b00000000);
            if (Encrypts != null && Encrypts.Count > 0)
            {
                var i = 0;
                foreach(var enc in Encrypts)
                {
                    dataToEncrypt = enc.Encrypt(dataToEncrypt);
                    setConfPixel(Encrypt1IdPosition + i, enc.GetID());
                    //_lockBitmap.SetPixel(Encrypt1IdPosition + i, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(Encrypt1IdPosition + i, 0), enc.GetID(), null, null, null));

                    if (++i > 1)
                        break;
                }
            }

            #region Write configs into image on Alpha path of color

            var dataLengthInBits = dataToEncrypt.Length * 8;
            var toWriteLength = _byteOperations.SplitSettingsBytes(new BitArray(BitConverter.GetBytes(dataLengthInBits)));
            int col = 0, rowIndex = 0;
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
            return _lockBitmap.GetImage();
        }

        private byte getConfPixel(int x)
        {
            return WriteReadData.ReadData(
                Image.GetPixel(x, 0), ImageOrigimal == null ? new Color() : ImageOrigimal.GetPixel(x, 0))[0];
        }
        private void setConfPixel(int x, byte val)
        {
            _lockBitmap.SetPixel(x, 0, WriteReadData.WriteData(_lockBitmap.GetPixel(x, 0), val, null, null, null));
        }

        // original: maybe is null
        public T_Data Decrypt(Bitmap original)
        {
            ImageOrigimal = original;
            WriteReadData.SetSettingsMode();
            var WriteReadDataId = getConfPixel(WriteReadDataIdPosition);
            if (WriteReadDataId != WriteReadData.GetID())
            {
                throw new ArgumentException("Parameter is not correct", "WriteReadData");
            }
            var CompressionId = getConfPixel(DataCompressionIdPosition);
            var EncryptId1 = getConfPixel(Encrypt1IdPosition);
            var EncryptId2 = getConfPixel(Encrypt2IdPosition);


            byte[] countDataList = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                var r = i * 2;
                var bigPath = getConfPixel(r);
                var littlePath = getConfPixel(r + 1);

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

            if (EncryptId2 > 0)
            {
                var enc = this.Encrypts.Where(r => r.GetID() == EncryptId2).FirstOrDefault();
                if (enc != null)
                {
                    data = enc.Decrypt(data);
                }
            }
            if (EncryptId1 > 0)
            {
                var enc = this.Encrypts.Where(r => r.GetID() == EncryptId1).FirstOrDefault();
                if(enc != null)
                {
                    data = enc.Decrypt(data);
                }
            }
            if (CompressionId > 0)
            {

            }

            return DataReader.ToObject(data);
        }
    }
}
