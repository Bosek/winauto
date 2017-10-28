using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Drawing;
using WinAuto;

namespace Tests
{
    [TestClass]
    public class Test
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ImageMatcherSimpleTest()
        {
            var imageMatcher = new ImageMatcher();
            var gameScreen = Image.FromFile("gameScreen.png");
            var spellNeedle = Image.FromFile("spellNeedle.png");

            var result = imageMatcher.FindNeedle(gameScreen, spellNeedle);

            var seconds = imageMatcher.LastOperationTime.ToString("ss");
            var miliseconds = imageMatcher.LastOperationTime.ToString("fff");
            TestContext.WriteLine($"Operation time: {seconds}s {miliseconds}ms");
            Assert.AreNotEqual(result, null);

            gameScreen.Dispose();
            spellNeedle.Dispose();
        }

        [TestMethod]
        public void ImageMatcherThresholdTest()
        {
            var imageMatcher = new ImageMatcher();
            var starScreen = Image.FromFile("starScreen.png");
            var starNeedle100 = Image.FromFile("starNeedle100.png");
            var starNeedle75 = Image.FromFile("starNeedle75.png");

            Assert.AreNotEqual(imageMatcher.FindNeedle(starScreen, starNeedle100), null);

            imageMatcher.Threshold = 0.8f;

            Assert.AreNotEqual(imageMatcher.FindNeedle(starScreen, starNeedle100), null);
            Assert.AreEqual(imageMatcher.FindNeedle(starScreen, starNeedle75), null);

            imageMatcher.Threshold = 0.75f;

            Assert.AreNotEqual(imageMatcher.FindNeedle(starScreen, starNeedle100), null);
            Assert.AreNotEqual(imageMatcher.FindNeedle(starScreen, starNeedle75), null);

            starScreen.Dispose();
            starNeedle100.Dispose();
            starNeedle75.Dispose();
        }

        [TestMethod]
        public void ImageMatcherComplexTest()
        {
            //Start TestApp
            var appProcess = new Process
            {
                StartInfo = new ProcessStartInfo("TestGUIApp.exe")
            };
            appProcess.Start();

            //Wait for TestApp to start
            System.Threading.Thread.Sleep(1000);

            try
            {
                var imageMatcher = new ImageMatcher();

                var windowPosition = WinAPI.GetWindowRectangle(appProcess);

                //Capture screenshot and load frame(with alpha channel) needle image
                var screenshot = WinAPI.CaptureWindow(appProcess);
                var frameNeedle = Image.FromFile("frameNeedle.png");

                //Try to find frame template on the screen
                var result = imageMatcher.FindNeedle(screenshot, frameNeedle, new Rectangle(0, 0, screenshot.Width, screenshot.Height / 3));
                Assert.AreNotEqual(result.HasValue, false);

                //Image in the frame becomes our new template image
                var needleImage = ImageMatcher.CropImage(screenshot, result.Value);
                //Recalculate searching zone
                var sceneRectangle = new Rectangle(0, result.Value.Y + result.Value.Height, screenshot.Width, screenshot.Height - (result.Value.Y + result.Value.Height));

                //Try to find an image to click on
                result = imageMatcher.FindNeedle(screenshot, needleImage, sceneRectangle);
                Assert.AreNotEqual(result.HasValue, false);

                //Image found, click on it
                Input.LeftMouseClick(windowPosition.Value.X + result.Value.X + result.Value.Width / 2,
                    windowPosition.Value.Y + result.Value.Y + result.Value.Height / 2);

                //Wait some time to redraw scene
                Input.MakeDelay(1000);
                //Take new screenshot
                screenshot = WinAPI.CaptureWindow(appProcess);
                //If clicked on right image, scene should redraw and template won't be found
                result = imageMatcher.FindNeedle(screenshot, needleImage);
                Assert.AreEqual(result.HasValue, false);

                frameNeedle.Dispose();
                needleImage.Dispose();
                screenshot.Dispose();
            }
            finally
            {
                if (!appProcess.HasExited)
                    appProcess.Kill();
            }
        }
    }
}
