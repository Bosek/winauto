using System;
using System.Threading;
using System.Drawing;

namespace TestGUIApp
{
    enum GeneratorColors
    {
        White=0,
        Black=1,
        Red=2,
        Green=3,
        Blue=4
    }
    class ImageGenerator
    {
        int width, height;
        int tileWidth, tileHeight;

        public ImageGenerator(int width, int height, int tileWidth, int tileHeight)
        {
            if (width < 100)
                width = 100;
            if (height < 100)
                height = 100;
            if (tileWidth < width / 10)
                tileWidth = width / 10;
            if (tileHeight < height / 10)
                tileHeight = height / 10;

            this.width = width;
            this.height = height;

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
        }

        public Image GenerateImage()
        {
            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            var random = new Random();
            for (int y = 0; y < height / tileHeight; y++)
            {
                for (int x = 0; x < width / tileWidth; x++)
                {
                    var pen = new Pen(Color.FromName(((GeneratorColors)random.Next(5)).ToString()));
                    var rectangle = new Rectangle( x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                    graphics.FillRectangle(pen.Brush, rectangle);
                    pen.Dispose();
                }
            }
            graphics.Dispose();

            //Give some time to Random to get a new number
            Thread.Sleep(15);
            return (Image)bitmap;
        }
    }
}
