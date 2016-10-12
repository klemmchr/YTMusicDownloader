using YTMusicDownloader.Model.DownloadManager;

namespace YTMusicDownloader.ViewModel.Messages
{
    class DownloadFormatChangedMessage
    {
        public DownloadFormat NewFormat { get; }

        public DownloadFormatChangedMessage(DownloadFormat newFormat)
        {
            NewFormat = newFormat;
        }
    }
}
