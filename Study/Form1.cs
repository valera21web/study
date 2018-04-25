using LibConvert;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study
{
    public partial class Form1 : Form
    {
        private Bitmap imageOriginal;
        private Bitmap imageEncrypted;
        private Aes AesKeys = Aes.Create();

        private int countBitsToWrite = 4;
        private bool R_Available = true;
        private bool G_Available = true;
        private bool B_Available = true;


        public Form1()
        {
            InitializeComponent();
            AesKeys.Padding = PaddingMode.PKCS7;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open Image";
            openFileDialog1.Filter = "Image (*.png, *.jpg)|*.png; *.jpg";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var upload = new Bitmap(openFileDialog1.FileName);
                imageOriginal = new Bitmap(upload.Width, upload.Height, PixelFormat.Format32bppPArgb);
                using (Graphics gr = Graphics.FromImage(imageOriginal)) {
                    gr.DrawImage(upload, new Rectangle(0, 0, imageOriginal.Width, imageOriginal.Height));
                }
                pictureBox1.Image = imageOriginal;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBoxKey.Text = Convert.ToBase64String(AesKeys.Key);
            //textBoxKey2.Text = Convert.ToBase64String(AesKeys.Key);
            textBoxIV.Text = Convert.ToBase64String(AesKeys.IV);
            //textBoxIV2.Text = Convert.ToBase64String(AesKeys.IV);

            var obj = new LibConverter(imageOriginal)
                .SetR(R_Available)
                .SetG(G_Available)
                .SetB(B_Available);
            obj = obj.SetDataToEncrypt(textBox1.Text);
            //obj = obj.AESEncryptData(AesKeys.Key, AesKeys.IV);
            
            imageEncrypted = new Bitmap(obj.Do());
            pictureBox2.Image = imageEncrypted;
        }

        private void buttonSaveEnc_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Png Image (.png)|*.png";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                imageEncrypted.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var obj = new LibConverter(imageOriginal).SetEncodedImage(imageEncrypted);
            var a = obj.Decrypt(AesKeys.Key, AesKeys.IV);
            textBoxEnc2.Text = a;
        }
    }
}
