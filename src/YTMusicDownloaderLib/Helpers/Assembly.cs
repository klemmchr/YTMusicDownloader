using System.Diagnostics;

namespace YTMusicDownloaderLib.Helpers
{
    public class Assembly
    {
        public static string GetAssemblyVersion()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return fvi.FileVersion;
        }
    }
}