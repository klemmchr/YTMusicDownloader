using System.ComponentModel;

namespace YTMusicDownloaderLib.DownloadManager
{
    public enum DownloadState
    {
        [Description("The item is downloaded and has the right format")]
        Downloaded,

        [Description("The item is not downloaded")]
        NotDownloaded,

        [Description("The item is downloaded but has a wrong format")]
        NeedsConvertion
    }
}