using ClippySharp.Core;

namespace DerbyApp.ClippySharp
{
    public class BitmapWrapper : IBitmapWrapper
    {
        public object NativeObject => bitmap;

        readonly System.Drawing.Bitmap bitmap;
        public BitmapWrapper(System.Drawing.Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }
    }
}
