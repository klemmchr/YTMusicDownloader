using System.Diagnostics;
using System.IO;

namespace YTMusicDownloader.Model.Helpers
{
    public class Assembly
    {
        private static System.Reflection.Assembly GetExecutingAssembly()
        {
            return System.Reflection.Assembly.GetExecutingAssembly();
        }

        public static string GetAssemblyLocation()
        {
            return GetExecutingAssembly().Location;
        }

        public static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(GetAssemblyLocation());
        }

        public static string GetAssemblyVersion()
        {
            var fvi = FileVersionInfo.GetVersionInfo(GetAssemblyLocation());
            return fvi.FileVersion;
        }
    }
}