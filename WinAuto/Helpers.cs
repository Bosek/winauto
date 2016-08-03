using System.Diagnostics;
using System.Reflection;

namespace WinAuto
{
    /// <summary>
    /// A few helping methods for devs.
    /// </summary>
    public class Info
    {
        /// <returns>Current product version of system.</returns>
        public static string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        }
    }
}
