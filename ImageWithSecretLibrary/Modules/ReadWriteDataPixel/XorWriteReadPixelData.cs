using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using ImageWithSecretLibrary.Interfaces;

namespace ImageWithSecretLibrary.Modules
{
    public class XorWriteReadPixelData : IWriteReadPixelData
    {
        private bool isData = true;

        public byte[] ReadData(Color pixel, Color pixel2)
        {
            if (isData)
            {
                return new byte[] {
                (byte) (pixel.A ^ pixel2.A),
                (byte) (pixel.R ^ pixel2.R),
                (byte) (pixel.G ^ pixel2.G),
                (byte) (pixel.B ^ pixel2.B)
            };
            }
            else
            {
                return new byte[] {
                (byte) (pixel.A ^ pixel2.A),
                (byte) (pixel.R ^ pixel2.R),
                (byte) (pixel.G ^ pixel2.G),
                (byte) (pixel.B ^ pixel2.B)
            };
            }
        }

        public Color WriteData(Color pixel, byte? A, byte? R, byte? G, byte? B)
        {
            if (isData)
            {
                R = R.HasValue ? (byte)(R.Value ^ pixel.R) : pixel.R;
                G = G.HasValue ? (byte)(G.Value ^ pixel.G) : pixel.G;
                B = B.HasValue ? (byte)(B.Value ^ pixel.B) : pixel.B;
                return Color.FromArgb(pixel.A, R.Value, G.Value, B.Value);
            }
            else
            {
                A = A.HasValue ? (byte)(A.Value ^ pixel.A) : pixel.A;
                return Color.FromArgb(A.Value, pixel.R, pixel.G, pixel.B);
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
        
        public byte GetID()
        {
            return 0b00000001;
        }
    }
}