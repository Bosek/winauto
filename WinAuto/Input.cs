using System;
using System.Collections.Generic;
using System.Drawing;
using WindowsInput;
using WindowsInput.Native;

namespace WinAuto
{
    /// <summary>
    /// Abstract class with input-type-independent methods - everything except keyboard input.
    /// Basically wrapper for linked WindowsInput library.
    ///
    /// TODO: Implement own API calls and exclude InputSimulator library.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Dictionary of key codes
        /// </summary>
        public static Dictionary<string, int> KeyCodes
        {
            get
            {
                var dict = new Dictionary<string, int>();
                var codeNames = Enum.GetNames(typeof(VirtualKeyCode));
                var codeValues = Enum.GetValues(typeof(VirtualKeyCode));

                for (int i = 0; i < codeNames.Length; i++)
                {
                    dict.Add(codeNames[i], (int)codeValues.GetValue(i));
                }
                return dict;
            }
        }

        /// <summary>
        /// Default waiting time delay.
        /// </summary>
        public static readonly int DefaultDelay = 200;
        /// <summary>
        /// Waits specified time.
        /// Simple Thread.Sleep(delay)
        /// </summary>
        /// <param name="delay">Delays in ms to wait</param>
        public static void MakeDelay(int? delay = null)
        {
            System.Threading.Thread.Sleep(delay.GetValueOrDefault(DefaultDelay));
        }

        /// <summary>
        /// Calculates virtual screen X position based on absolute X position
        /// </summary>
        /// <param name="x">Absolute X position</param>
        /// <returns>Virtual screen X position</returns>
        protected static double calcVirtualScreenX(int x)
        {
            var screenWidth = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
            return x * 65535 / screenWidth;
        }

        /// <summary>
        /// Calculates virtual screen Y position based on absolute Y position
        /// </summary>
        /// <param name="y">Absolute Y position</param>
        /// <returns>Virtual screen Y position</returns>
        protected static double calcVirtualScreenY(int y)
        {
            var screenHeight = System.Windows.Forms.SystemInformation.VirtualScreen.Height;
            return y * 65535 / screenHeight;
        }

        /// <summary>
        /// Move mouse to X,Y location.
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public static void MouseMove(int x, int y)
        {
            var simulator = new MouseSimulator(new InputSimulator());

            simulator.MoveMouseToPositionOnVirtualDesktop(calcVirtualScreenX(x), calcVirtualScreenY(y));
        }
        /// <summary>
        /// Move mouse to point location.
        /// </summary>
        /// <param name="point">Point position</param>
        public static void MouseMove(Point point)
        {
            MouseMove(point.X, point.Y);
        }

        /// <summary>
        /// Left mouse button click on location
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="delay">Waiting time delay</param>
        public static void LeftMouseClick(int x, int y, int? delay = null)
        {
            var simulator = new MouseSimulator(new InputSimulator());

            simulator.MoveMouseToPositionOnVirtualDesktop(calcVirtualScreenX(x), calcVirtualScreenY(y));
            MakeDelay(delay);
            simulator.LeftButtonClick();
        }
        /// <summary>
        /// Move mouse to point location.
        /// </summary>
        /// <param name="point">Point position</param>
        public static void LeftMouseClick(Point point)
        {
            LeftMouseClick(point.X, point.Y);
        }

        /// <summary>
        /// Flow:
        /// -> move mouse to sX, sY
        /// -> delay
        /// -> left mouse down
        /// -> delay
        /// -> move mouse to dX, dY
        /// -> delay
        /// -> left mouse up
        /// </summary>
        /// <param name="sX">Source X</param>
        /// <param name="sY">Source Y</param>
        /// <param name="dX">Destination X</param>
        /// <param name="dY">Destination Y</param>
        /// <param name="delay">Waiting time delay</param>
        public static void MouseDrag(int sX, int sY, int dX, int dY, int? delay = null)
        {
            var simulator = new MouseSimulator(new InputSimulator());

            simulator.MoveMouseToPositionOnVirtualDesktop(calcVirtualScreenX(sX), calcVirtualScreenY(sY));
            MakeDelay(delay);
            simulator.LeftButtonDown();
            MakeDelay(delay);
            simulator.MoveMouseToPositionOnVirtualDesktop(calcVirtualScreenX(dX), calcVirtualScreenY(dY));
            MakeDelay(delay);
            simulator.LeftButtonUp();
        }
        /// <summary>
        /// Flow:
        /// -> move mouse to startPoint
        /// -> delay
        /// -> left mouse down
        /// -> delay
        /// -> move mouse to destinationPoint
        /// -> delay
        /// -> left mouse up
        /// </summary>
        /// <param name="startPoint">Start drag point</param>
        /// <param name="destinationPoint">Destination drop point</param>
        /// <param name="delay">Waiting time delay</param>
        public static void MouseDrag(Point startPoint, Point destinationPoint, int? delay = null)
        {
            MouseDrag(startPoint.X, startPoint.Y, destinationPoint.X, destinationPoint.Y, delay);
        }

        /// <summary>
        /// Right mouse button click on specific location
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="delay">Waiting time delay</param>
        public static void RightMouseClick(int x, int y, int? delay = null)
        {
            var simulator = new MouseSimulator(new InputSimulator());

            simulator.MoveMouseToPositionOnVirtualDesktop(calcVirtualScreenX(x), calcVirtualScreenY(y));
            MakeDelay(delay);
            simulator.RightButtonClick();
        }
        /// <summary>
        /// Right mouse button click on specific location
        /// </summary>
        /// <param name="point">Point position</param>
        /// <param name="delay">Waiting time delay</param>
        public static void RightMouseClick(Point point, int? delay = null)
        {
            RightMouseClick(point.X, point.Y, delay);
        }

        /// <summary>
        /// Pressing up to four different keys(3 modifiers and 1 normal key).
        /// </summary>
        /// <param name="keyCodeType">DirectInputKeyCode or VirtualKeyCode</param>
        /// <param name="keyModifier1">Key modifier code</param>
        /// <param name="keyModifier2">Key modifier code</param>
        /// <param name="keyModifier3">Key modifier code</param>
        /// <param name="key">Key code</param>
        public static void Key(
            int keyModifier1,
            int keyModifier2,
            int keyModifier3,
            int key)
        {

            var simulator = new KeyboardSimulator(new InputSimulator());

            if (keyModifier1 == 0 &&
                keyModifier2 == 0 &&
                keyModifier3 == 0)
                simulator.KeyPress((VirtualKeyCode)key);
            else
                simulator.ModifiedKeyStroke(
                    (VirtualKeyCode)keyModifier1 |
                    (VirtualKeyCode)keyModifier2 |
                    (VirtualKeyCode)keyModifier3,
                    (VirtualKeyCode)key);
        }
        public static void Key(int key)
        {
            Key(0, 0, 0, key);
        }

        public static void Key(int keyModifier1, int key)
        {
            Key(keyModifier1, 0, 0, key);
        }

        public static void Key(int keyModifier1, int keyModifier2, int key)
        {
            Key(keyModifier1, keyModifier2, 0, key);
        }

        /// <summary>
        /// Text input
        /// </summary>
        /// <param name="text">Text to input</param>
        public static void Text(string text)
        {
            var simulator = new KeyboardSimulator(new InputSimulator());

            simulator.TextEntry(text);
        }
    }
}
