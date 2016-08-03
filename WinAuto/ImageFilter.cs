using System.Drawing;
using System.Drawing.Imaging;

namespace WinAuto
{
    /// <summary>
    /// Image filter class.
    /// </summary>
    public abstract class Filter
    {
        public abstract Bitmap Apply(Bitmap original);
    }
    /// <summary>
    /// Simple gray scale filter.
    /// </summary>
    public class GrayScale : Filter
    {
        /// <summary>
        /// Applies filter to the Image
        /// </summary>
        /// <param name="original">Image to be processed</param>
        /// <returns>Processed image</returns>
        public override Bitmap Apply(Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            var g = Graphics.FromImage(newBitmap);
            var colorMatrix = new ColorMatrix(
               new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
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
