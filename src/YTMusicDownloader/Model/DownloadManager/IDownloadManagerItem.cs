namespace YTMusicDownloader.Model.DownloadManager
{
    public delegate void DownloadItemDownloadCompletedEventHandler(object sender, DownloadCompletedEventArgs args);
    public delegate void DownloadItemDownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs args);

    public interface IDownloadManagerItem
    {
        event DownloadItemDownloadCompletedEventHandler DownloadItemDownloadCompleted;
        event DownloadItemDownloadProgressChangedEventHandler DownloadItemDownloadProgressChanged;

        void StartDownload();
        void StopDownload();
    }
}