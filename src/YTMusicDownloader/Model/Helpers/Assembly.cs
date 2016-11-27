using System.Diagnostics;

namespace YTMusicDownloader.Model.Helpers
{
    public class Assembly
    {
        public static string GetAssemblyLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public static string GetAssemblyVersion()
        {
            var fvi = FileVersionInfo.GetVersionInfo(GetAssemblyLocation());
            return fvi.FileVersion;
        }
    }
}