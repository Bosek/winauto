using System.Drawing;
using System.Drawing.Imaging;

namespace WinAuto
{
    /// <summary>
    /// Image base filter class.
    /// </summary>
    public abstract class Filter
    {
        public abstract Bitmap Apply(Bitmap original);
    }
    /// <summary>
    /// Simple gray scale filter.
    /// </summary>
    public class GrayScaleFilter : Filter
    {
        public override Bitmap Apply(Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            var g = Graphics.FromImage(newBitmap);
            var colorMatrix = new ColorMatrix(
               new float[][]
                {
                    new float[] { .3f,  .3f,  .3f, 0f, 0f},
                    new float[] {.59f, .59f, .59f, 0f, 0f},
                    new float[] {.11f, .11f, .11f, 0f, 0f},
                    new float[] {  0f,   0f,   0f, 1f, 0f},
                    new float[] {  0f,   0f,   0f, 0f, 1f}
            });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            g.Dispose();
            return newBitmap;
        }
    }

    /// <summary>
    /// Image inversion filter.
    /// </summary>
    public class InvertedColorsFilter : Filter
    {
        public override Bitmap Apply(Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            var g = Graphics.FromImage(newBitmap);
            var colorMatrix = new ColorMatrix(
               new float[][]
                {
                    new float[] { -1f,   0f,   0f, 0f, 0f},
                    new float[] {  0f,  -1f,   0f, 0f, 0f},
                    new float[] {  0f,   0f,  -1f, 0f, 0f},
                    new float[] {  0f,   0f,   0f, 1f, 0f},
                    new float[] {  1f,   1f,   1f, 0f, 1f}
            });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            g.Dispose();
            return newBitmap;
        }
    }
}
