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

            var found = imageMatcher.FindNeedle(gameScreen, spellNeedle);

            var seconds = imageMatcher.LastOperationTime.ToString("ss");
            var miliseconds = imageMatcher.LastOperationTime.ToString("fff");
            TestContext.WriteLine($"Operation time: {seconds}s {miliseconds}ms");
            Assert.AreNotEqual(found, null);

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

                //Try to find main window rectangle
                var appRectangle = WinAPI.GetWindowRectangle(appProcess);
                Assert.AreNotEqual(appRectangle, null);
                if (appRectangle == null)
                    return;

                //Screenshot and load frame(with alpha channel) template
                var screenshot = WinAPI.CaptureScreen();
                var frameNeedle = Image.FromFile("frameNeedle.png");

                //Try to find frame template on the screen
                var found = imageMatcher.FindNeedle(screenshot, frameNeedle, appRectangle.Value);
                Assert.AreNotEqual(found, null);
                if (found == null)
                    return;

                //Image in the frame becomes our new template image
                var needleImage = ImageMatcher.CropImage(screenshot, found.Value);
                //Recalculate searching zone
                var sceneRectangle = appRectangle.Value;
                sceneRectangle.Y = found.Value.Y + found.Value.Height;
                sceneRectangle.Height = appRectangle.Value.Height - (sceneRectangle.Y - appRectangle.Value.Y);
                //Try to find image to click on
                found = imageMatcher.FindNeedle(screenshot, needleImage, sceneRectangle);
                Assert.AreNotEqual(found, null);
                if (found == null)
                    return;

                //Image found, click on it
                Input.LeftMouseClick(found.Value.X + found.Value.Width / 2,
                    found.Value.Y + found.Value.Height / 2);

                //Wait some time to redraw scene
                Input.MakeDelay(1000);
                //Take new screenshot
                screenshot = WinAPI.CaptureScreen();
                //If clicked on right image, scene should redraw and template won't be found
                found = imageMatcher.FindNeedle(screenshot, needleImage, appRectangle.Value);
                Assert.AreEqual(found, null);

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
