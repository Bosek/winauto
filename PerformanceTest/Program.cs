using System;
using System.Drawing;
using WinAuto;

namespace PerformanceTest
{
    class Program
    {
        // Before LockBits it took about 3s 500ms
        // After LockBits it took about 1s 600ms
        // Multithreading FTW. It takes about 130ms now!
        static void Main(string[] args)
        {
            while (true)
            {
                var gameScreen = Image.FromFile("gameScreen.png");
                var spellNeedle = Image.FromFile("spellNeedle.png");

                var imageMatcher = new ImageMatcher();
                var found = imageMatcher.FindNeedle(gameScreen, spellNeedle);

                var seconds = imageMatcher.LastOperationTime.ToString("ss");
                var miliseconds = imageMatcher.LastOperationTime.ToString("fff");
                Console.WriteLine($"{(found.HasValue ? "Found" : "Not found")} in {seconds}s {miliseconds}ms {(found.HasValue ? $"on position {found.Value.ToString()}" : "")}");

                gameScreen.Dispose();
                spellNeedle.Dispose();

                Console.WriteLine("Hit enter to repeat.");
                Console.ReadLine();
            }
        }
    }
}
