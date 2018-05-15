using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageWithSecretLibrary.Interfaces
{
    public interface IWriteReadPixelData
    {
        Color WriteData(Color pixel, byte? A, byte? R, byte? B, byte? G);
        byte[] ReadData(Color pixel, Color pixel2);

        void SetSettingsMode();
        void SetDataMode();
        
        byte GetID();
    }
}