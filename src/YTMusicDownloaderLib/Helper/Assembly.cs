using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMusicDownloaderLib.Helper
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
