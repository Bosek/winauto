using WinAuto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Drawing;

namespace Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void ImageMatcherThresholdTest()
        {
            var imageMatcher = new ImageMatcher();
            var starScreen = Image.FromFile("starScreen.png");
            var starTemplate100 = Image.FromFile("starTemplate100.png");
            var starTemplate75 = Image.FromFile("starTemplate75.png");

            Assert.AreNotEqual(imageMatcher.FindTemplate(starScreen, starTemplate100), null);

            imageMatcher.Threshold = 0.8f;

            Assert.AreNotEqual(imageMatcher.FindTemplate(starScreen, starTemplate100), null);
            Assert.AreEqual(imageMatcher.FindTemplate(starScreen, starTemplate75), null);

            imageMatcher.Threshold = 0.75f;

            Assert.AreNotEqual(imageMatcher.FindTemplate(starScreen, starTemplate100), null);
            Assert.AreNotEqual(imageMatcher.FindTemplate(starScreen, starTemplate75), null);

            starScreen.Dispose();
            starTemplate100.Dispose();
            starTemplate75.Dispose();
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
                //Initialize DDIY modules
                var imageMatcher = new ImageMatcher();

                //Try to find main window rectangle
                var appRectangle = WinAPI.GetWindowRectangle(appProcess);
                Assert.AreNotEqual(appRectangle, null);
                if (appRectangle == null)
                    return;

                //Screenshot and load frame(with alpha channel) template
                var screenshot = WinAPI.CaptureScreen();
                var frameTemplate = Image.FromFile("frameTemplate.png");

                //Try to find frame template on the screen
                var found = imageMatcher.FindTemplate(screenshot, frameTemplate, appRectangle.Value);
                Assert.AreNotEqual(found, null);
                if (found == null)
                    return;

                //Image in the frame becomes our new template image
                var templateImage = ImageMatcher.CropImage(screenshot, found.Value);
                //Recalculate searching zone
                var sceneRectangle = appRectangle.Value;
                sceneRectangle.Y = found.Value.Y + found.Value.Height;
                sceneRectangle.Height = appRectangle.Value.Height - (sceneRectangle.Y - appRectangle.Value.Y);
                //Try to find image to click on
                found = imageMatcher.FindTemplate(screenshot, templateImage, sceneRectangle);
                Assert.AreNotEqual(found, null);

                //Image found, click on it
                Input.LeftMouseClick(found.Value.X + found.Value.Width / 2,
                    found.Value.Y + found.Value.Height / 2);

                //Wait some time to redraw scene
                Input.MakeDelay(1000);
                //Take new screenshot
                screenshot = WinAPI.CaptureScreen();
                //If clicked on right image, scene should redraw and template won't be found
                found = imageMatcher.FindTemplate(screenshot, templateImage, appRectangle.Value);
                Assert.AreEqual(found, null);

                frameTemplate.Dispose();
                templateImage.Dispose();
                screenshot.Dispose();
            }
            finally
            {
                appProcess.Kill();
            }
        }
    }
}
