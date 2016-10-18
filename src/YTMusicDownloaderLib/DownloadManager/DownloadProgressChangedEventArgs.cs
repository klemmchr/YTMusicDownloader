using System;

namespace YTMusicDownloaderLib.DownloadManager
{
    public class DownloadProgressChangedEventArgs: EventArgs
    {
        public double ProgressPercentage { get; }
        public DownloadProgressChangedEventArgs(long processedBytes, long totalBytes)
        {
            if (totalBytes == 0)
                ProgressPercentage = 0;
            else
                ProgressPercentage = ((processedBytes * 1.0) / (totalBytes * 1.0)) * 100;
        }

        public DownloadProgressChangedEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
    }
}
