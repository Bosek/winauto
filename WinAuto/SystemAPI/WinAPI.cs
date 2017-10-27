using WinAuto.APIHelpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;

namespace WinAuto
{
    /// <summary>
    /// Implementation of Windows API native calls.
    /// </summary>
    public class WinAPI
    {
        static Image captureWindow(IntPtr handle)
        {
            var hdcSrc = User32.GetWindowDC(handle);

            var width = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
            var height = System.Windows.Forms.SystemInformation.VirtualScreen.Height;

            var hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            var hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            var hOld = GDI32.SelectObject(hdcDest, hBitmap);

            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            Image img = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        static bool isAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Finds MAIN window of the process and return its rectangle(position and size).
        /// </summary>    
        /// <param name="process">Process</param>
        /// <returns>MAIN window rectangle or null</returns>
        public static Rectangle? GetWindowRectangle(Process process)
        {
            User32.RECT processRECT;
            if (User32.GetWindowRect(process.MainWindowHandle, out processRECT))
            {
                var x = Math.Max(processRECT.Left, 0);
                var y = Math.Max(processRECT.Top, 0);
                var width = processRECT.Right - x;
                var height = processRECT.Bottom - y;

                if (width > 0 && height > 0)
                    return new Rectangle(x, y, width, height);
            }
            return null;
        }
        /// <summary>
        /// Finds MAIN window of the process and return its rectangle(position and size).
        /// </summary>      
        /// <param name="pid">Process ID</param>
        /// <returns>MAIN window rectangle or null</returns>
        public static Rectangle? GetWindowRectangle(int pid)
        {
            return GetWindowRectangle(Process.GetProcessById(pid));
        }
        /// <summary>
        /// Finds MAIN window of the process and return its rectangle(position and size).
        /// </summary>
        /// <param name="name">Process name. Usually filename without .exe extension. First one found will be used.</param>
        /// <returns>MAIN window rectangle or null</returns>
        public static Rectangle? GetWindowRectangle(string name)
        {
            var processList = Process.GetProcessesByName(name);

            if (processList.Length > 0)
                return GetWindowRectangle(processList[0]);
            return null;
        }

        /// <summary>
        /// Captures MAIN window of the process regardless of it's position on the screen or transparency
        /// </summary>
        /// <param name="process">Process</param>
        /// <returns>MAIN window bitmap or null</returns>
        public static Bitmap CaptureWindow(Process process)
        {
            var windowRect = GetWindowRectangle(process);
            if (!windowRect.HasValue)
                return null;

            var screenBitmap = new Bitmap(windowRect.Value.Width, windowRect.Value.Height);
            var success = false;
            using (var graphics = Graphics.FromImage(screenBitmap))
            {
                var devicehandle = graphics.GetHdc();

                success = User32.PrintWindow(process.MainWindowHandle, devicehandle, 0);

                graphics.ReleaseHdc(devicehandle);
            }

            if (success)
            {
                return screenBitmap;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Captures MAIN window of the process regardless of it's position on the screen or transparency
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <returns>MAIN window bitmap or null</returns>
        public static Bitmap CaptureWindow(int pid)
        {
            return CaptureWindow(Process.GetProcessById(pid));
        }
        /// <summary>
        /// Captures MAIN window of the process regardless of it's position on the screen or transparency
        /// </summary>
        /// <param name="name">Process name. Usually filename without .exe extension. First one found will be used.</param>
        /// <returns>MAIN window bitmap or null</returns>
        public static Bitmap GetWindowScreen(string name)
        {
            var processList = Process.GetProcessesByName(name);

            if (processList.Length > 0)
                return CaptureWindow(processList[0]);
            return null;
        }

        /// <summary>
        /// Finds MAIN window of the process and focuses it. 
        /// </summary>
        /// <param name="process">Process</param>
        public static void FocusWindow(Process process)
        {
            User32.SetForegroundWindow(process.MainWindowHandle);
        }
        /// <summary>
        /// Finds MAIN window and focus it.
        /// </summary>
        /// <param name="pid">Process ID</param>
        public static void FocusWindow(int pid)
        {
            FocusWindow(Process.GetProcessById(pid));
        }
        /// <summary>
        /// Finds MAIN window and focus it.
        /// </summary>
        /// <param name="name">Process name. Usually filename without .exe extension.</param>
        public static void FocusWindow(string name)
        {
            var processList = Process.GetProcessesByName(name);
            if (processList.Length > 0)
                FocusWindow(processList[0]);
        }

        /// <summary>
        /// Captures whole screen with all monitors.
        /// </summary>
        /// <returns>Screenshot image</returns>
        public static Image CaptureScreen()
        {
            return captureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Blocks user input. Handy if you don't want user to interfere with the application.
        /// Users can use Ctrl+Alt+Delete to unblock app.
        /// TO WORK, APP HAS TO RUN WITH ADMINISTRATION PRIVILEGES!
        /// </summary>
        /// <param name="block">
        /// TRUE to block
        /// FALSE to unblock
        /// </param>
        /// <returns>
        /// TRUE if blocked
        /// FALSE if not blocked
        /// </returns>
        public static bool BlockInput(bool block)
        {
            User32.BlockInput(block);
            return isAdministrator();
        }
    }
}
