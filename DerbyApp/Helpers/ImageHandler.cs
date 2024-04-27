using System;
using System.Drawing;
using System.IO;

namespace DerbyApp.Helpers
{
    internal class ImageHandler
    {
        public static byte[] ImageToByteArray(Image img)
        {
            using var stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static string LoadImageFromBytes(byte[] cData)
        {
            return $"base64:{Convert.ToBase64String(cData)}";
        }
    }
}
