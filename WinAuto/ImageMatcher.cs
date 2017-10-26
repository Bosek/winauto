using System;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WinAuto
{
    /// <summary>
    /// Per pixel image matching.
    /// </summary>
    public class ImageMatcher
    {
        public TimeSpan LastOperationTime { private set; get; }
        public int ThreadCount { set; get; } = Environment.ProcessorCount;

        /// <summary>
        /// Possible color threshold. Lower value means ImageMatcher can find even not 100% matching images, but it can be slower.
        /// Default is 1(so ImageMatcher looks for exact match).
        /// </summary>
        public float Threshold { set; get; } = 1;

        static Bitmap applyFilters(Bitmap bitmap)
        {
            var filter = new GrayScale();

            return filter.Apply(bitmap);
        }
        static bool compareColorChannel(byte channel1, byte channel2, float threshold)
        {
            threshold = Math.Min(threshold, 1f);
            threshold = Math.Max(threshold, 0.1f);
            float max = Math.Max(channel1, channel2);
            float min = Math.Min(channel1, channel2);

            return (1 - ((max - min) / 255) >= threshold);
        }

        static Color roundAlphaChannel(Color color)
        {
            // Color is immutable
            return Color.FromArgb(color.A == 255 ? 255 : 0, color.R, color.G, color.B);
        }
        static bool compareColors(Color templateColor, Color sourceColor, float threshold)
        {
            templateColor = roundAlphaChannel(templateColor);

            if (templateColor.A == 0)
                return true;

            return (
                compareColorChannel(templateColor.R, sourceColor.R, threshold) &&
                compareColorChannel(templateColor.G, sourceColor.G, threshold) &&
                compareColorChannel(templateColor.B, sourceColor.B, threshold)
                );
        }

        static Queue<Rectangle> prepareWorkQueue(Size haystackSize, Size needleSize, Rectangle searchZone)
        {
            // Make sure we are not searching in zone out of haystack boundaries
            if (searchZone.X + searchZone.Width > haystackSize.Width)
                searchZone.Width = Math.Abs(searchZone.X - haystackSize.Width);
            if (searchZone.Y + searchZone.Height > haystackSize.Height)
                searchZone.Height = Math.Abs(searchZone.Y - haystackSize.Height);

            var workQueue = new Queue<Rectangle>();

            for (int y = searchZone.Y; y < Math.Floor((decimal)(searchZone.Height / needleSize.Height)); y++)
            {
                for (int x = searchZone.X; x < Math.Floor((decimal)(searchZone.Width / needleSize.Width)); x++)
                {
                    if (x * needleSize.Width + 2 * needleSize.Width > haystackSize.Width)
                        continue;
                    if (y * needleSize.Height + 2 * needleSize.Height > haystackSize.Height)
                        continue;
                    workQueue.Enqueue(new Rectangle(x * needleSize.Width, y * needleSize.Height, needleSize.Width, needleSize.Height));
                }
            }
            return workQueue;
        }

        Rectangle? searchNeedle(ImageData haystackImageData, ImageData needleImageData, Rectangle searchZone, float threshold)
        {
            Rectangle? found = null;

            var maybeFound = false;

            for (int sY = searchZone.Y; sY < searchZone.Y + searchZone.Height; sY++)
            {
                for (int sX = searchZone.X; sX < searchZone.X + searchZone.Width; sX++)
                {
                    for (int tY = 0; tY < searchZone.Height; tY++)
                    {
                        for (int tX = 0; tX < searchZone.Width; tX++)
                        {
                            var tPixel = needleImageData.GetPixel(tX, tY);
                            var sPixel = haystackImageData.GetPixel(sX + tX, sY + tY);

                            maybeFound = compareColors(tPixel, sPixel, threshold);

                            if (!maybeFound)
                                break;
                        }
                        if (!maybeFound)
                            break;
                    }
                    if (maybeFound)
                    {
                        found = new Rectangle(sX, sY, searchZone.Width, searchZone.Height);
                        break;
                    }
                }
                if (maybeFound)
                    break;
            }
            return found;
        }

        Rectangle? workThread(Queue<Rectangle> workQueue, ImageData haystackImageData, ImageData needleImageData, float threshold)
        {
            while (workQueue.Count > 0)
            {
                var taskZone = workQueue.Dequeue();

                var result = searchNeedle(haystackImageData, needleImageData, taskZone, threshold);
                if (result.HasValue)
                {
                    workQueue.Clear();
                    return result.Value;
                }
            }
            return null;
        }

        Rectangle? matchBitmaps(Bitmap haystack, Bitmap needle, Rectangle searchZone, float threshold)
        {
            Rectangle? found = null;

            var haystackImageData = new ImageData(haystack);
            var needleImageData = new ImageData(needle);

            var workQueue = prepareWorkQueue(haystack.Size, needle.Size, searchZone);
            var taskList = new List<Task<Rectangle?>>();
            for (int t = 0; t < ThreadCount; t++)
            {
                taskList.Add(new Task<Rectangle?>(delegate() { return workThread(workQueue, haystackImageData, needleImageData, threshold); }));
            }

            haystackImageData.Lock();
            needleImageData.Lock();

            taskList.ForEach(task => task.Start());
            Task.WaitAll(taskList.ToArray());

            foreach(var task in taskList)
            {
                var result = task.Result;
                if (result.HasValue)
                    found = result.Value;
            }

            haystackImageData.Unlock();
            needleImageData.Unlock();

            return found;
        }

        [Obsolete]
        public Rectangle? FindTemplate(Image haystack, Image needle)
        {
            return FindNeedle(haystack, needle);
        }

        [Obsolete]
        public Rectangle? FindTemplate(Image haystack, Image needle, Rectangle sourceRect)
        {
            return FindNeedle(haystack, needle, sourceRect);
        }

        /// <summary>
        /// Tries to match needle image with source image. Per pixel searching, no magic.
        /// Regardless of this approach, algorithm seems to be pretty fast for simple automatization purposes.
        /// If you want to speed matching as much as possible, try to look inside smaller areas
        /// and try to experiment with your needle images. More unique
        /// it is, more speed you can get.
        /// </summary>
        /// <param name="haystack">Source image(eg. screenshot)</param>
        /// <param name="needle">Image to look for</param>
        /// <returns>
        /// Rectangle holding position and size of found template
        /// Null if nothing was found
        /// </returns>
        public Rectangle? FindNeedle(Image haystack, Image needle)
        {
            return FindNeedle(haystack, needle, new Rectangle(new Point(0, 0), haystack.Size));
        }
        /// <summary>
        /// Tries to match needle image with source image. Per pixel searching, no magic.
        /// Regardless of this approach, algorithm seems to be pretty fast for simple automatization purposes.
        /// If you want to speed matching as much as possible, try to look inside smaller areas
        /// and try to experiment with your needle images. More unique
        /// it is, more speed you can get.
        /// </summary>
        /// <param name="haystack">Source image(eg. screenshot)</param>
        /// <param name="needle">Image to look for</param>
        /// <param name="sourceRect">
        /// Rectangle within source image determining searching area.
        /// This can speed up whole matching process in case you provide screenshot but want to scan just small portion of its actual size.
        /// </param>
        /// <returns>
        /// Rectangle holding position and size of found needle
        /// Null if nothing was found
        /// </returns>
        public Rectangle? FindNeedle(Image haystack, Image needle, Rectangle sourceRect)
        {
            Rectangle? rectangle;
            using (var screenBitmap = new Bitmap(haystack))
            using (var needleBitmap = new Bitmap(needle))
            {
                var startTime = DateTime.Now;
                rectangle = matchBitmaps(screenBitmap, needleBitmap, sourceRect, Threshold);
                var endTime = DateTime.Now;
                LastOperationTime = (endTime - startTime);
            }

            return rectangle;
        }

        /// <summary>
        /// Crops image.
        /// </summary>
        /// <param name="image">Image to be cropped.</param>
        /// <param name="rectangle">Rectangle of the new image</param>
        /// <returns>Cropped image</returns>
        public static Image CropImage(Image image, Rectangle rectangle)
        {
            image = ((Bitmap)image).Clone(rectangle, image.PixelFormat);
            return image;
        }
    }
}
