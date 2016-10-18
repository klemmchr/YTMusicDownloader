using System;

namespace YTMusicDownloaderLib.DownloadManager
{
    public class DownloadCompletedEventArgs: EventArgs
    {
        public bool Cancelled { get; }
        public Exception Error { get; }

        public DownloadCompletedEventArgs(bool cancelled, Exception error = null)
        {
            Cancelled = cancelled;
            Error = error;
        }
    }
}
