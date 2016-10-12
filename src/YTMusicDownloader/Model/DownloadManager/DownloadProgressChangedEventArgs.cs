using System;

namespace YTMusicDownloader.Model.DownloadManager
{
    public class DownloadProgressChangedEventArgs: EventArgs
    {
        public int ProgressPercentage { get; }
        public DownloadProgressChangedEventArgs(int progressPercentange)
        {
            ProgressPercentage = progressPercentange;
        }
    }
}
