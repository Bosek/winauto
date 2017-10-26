using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinAuto
{
    class ImageData : IDisposable
    {
        public Bitmap Bitmap { get; }
        public BitmapData BitmapData { private set; get; }
        public bool Locked { private set; get; }
        public int PixelWidth { private set; get; }

        int getPixelWidth(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
                default:
                    throw new NotImplementedException($"{(PixelFormat)pixelFormat} is not implemented(yet).");
            }
        }

        public ImageData(Bitmap bitmap)
        {
            Bitmap = bitmap;
            PixelWidth = getPixelWidth(bitmap.PixelFormat);
        }

        public void Lock()
        {
            if (Locked)
                return;
            BitmapData = Bitmap.LockBits(new Rectangle(Point.Empty, Bitmap.Size), ImageLockMode.ReadOnly, Bitmap.PixelFormat);
            Locked = true;
        }
        public void Unlock()
        {
            if (!Locked)
                return;
            Bitmap.UnlockBits(BitmapData);
            Locked = false;
        }

        unsafe public Color GetPixel(int x, int y)
        {
            if (!Locked)
                throw new InvalidOperationException($"{nameof(ImageData)} object is not locked.");

            var scan0 = BitmapData.Scan0;
            var stride = BitmapData.Stride;
            var p = (byte*)scan0.ToPointer() + y * stride;
            var px = x * PixelWidth;
            var alpha = (byte)(PixelWidth == 4 ? p[px + 3] : 255);
            return Color.FromArgb(alpha, p[px + 2], p[px + 1], p[px + 0]);
        }

        public void Dispose()
        {
            Unlock();
            Bitmap.Dispose();
        }
    }
}
