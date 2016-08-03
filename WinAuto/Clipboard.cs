using System;

namespace WinAuto
{
    delegate string GetClipboardDelegate();
    delegate void SetClipboardDelegate(string text);
    /// <summary>
    /// Basic clipboard handling.
    /// </summary>
    public class Clipboard
    {
        static event GetClipboardDelegate GetClipboard;
        static event SetClipboardDelegate SetClipboard;

        static Clipboard instance;
        Clipboard()
        {
            GetClipboard += new GetClipboardDelegate(() =>
            {
                var text = String.Empty;
                var staThread = new System.Threading.Thread(() =>
                {
                    text = System.Windows.Forms.Clipboard.GetText();
                });
                staThread.SetApartmentState(System.Threading.ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                return text;
            });
            SetClipboard += new SetClipboardDelegate((string text) =>
            {
                var staThread = new System.Threading.Thread(() =>
                {
                    System.Windows.Forms.Clipboard.SetText(text);
                });
                staThread.SetApartmentState(System.Threading.ApartmentState.STA);
                staThread.Start();
                staThread.Join();
            });
        }

        /// <summary>
        /// Gets clipboard text content.
        /// </summary>
        /// <returns>Clipboard text content.</returns>
        public static string GetText(){
            if (instance == null)
                instance = new Clipboard();
            return GetClipboard?.Invoke();
        }

        /// <summary>
        /// Sets clipboard text content.
        /// </summary>
        /// <param name="text">Text to insert into the clipboard.</param>
        public static void SetText(string text)
        {
            if (instance == null)
                instance = new Clipboard();
            SetClipboard?.Invoke(text);
        }
    }
}
