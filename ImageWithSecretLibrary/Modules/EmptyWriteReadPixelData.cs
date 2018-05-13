using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using ImageWithSecretLibrary.Interfaces;

namespace ImageWithSecretLibrary.Modules
{
    public class EmptyWriteReadPixelData : IWriteReadPixelData
    {
        private bool isData = true;

        public byte[] ReadData(Color pixel, Color pixel2)
        {
            if (isData)
            {
                byte[] result = new byte[3];
                result[0] = (byte)(0b00001111 & pixel.R);
                result[1] = (byte)(0b00001111 & pixel.G);
                result[2] = (byte)(0b00001111 & pixel.B);
                return result;
            }
            else
            {
                byte[] result = new byte[1];
                result[0] = (byte)(0b00001111 & pixel.A);
                return result;
            }
        }

        public Color WriteData(Color pixel, byte? A, byte? R, byte? G, byte? B)
        {
            if (isData)
            {
                return Color.FromArgb(pixel.A,
                R.HasValue ? (pixel.R & 0b11110000) | R.Value : pixel.R,
                G.HasValue ? (pixel.G & 0b11110000) | G.Value : pixel.G,
                B.HasValue ? (pixel.B & 0b11110000) | B.Value : pixel.B);
            }
            else
            {
                return Color.FromArgb(A.HasValue ? (pixel.A & 0b11110000) | A.Value : pixel.A, pixel.R, pixel.G, pixel.B);
            }
        }

        public void SetSettingsMode()
        {
            isData = false;
        }

        public void SetDataMode()
        {
            isData = true;
        }
    }
}