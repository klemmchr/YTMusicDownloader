using System;
using System.IO;
using YTMusicDownloader.Model.RetrieverEngine;

namespace YTMusicDownloader.Model.DownloadManager
{
    internal class ConvertionItem : DownloadManagerItem
    {
        public string CurrentPath { get; }
        public string NewPath { get; }

        public ConvertionItem(PlaylistItem item, string currentPath, string newPath) : base(item)
        {
            CurrentPath = currentPath;
            NewPath = newPath;
        }

        public override void StartDownload()
        {
            var oldExtension = Path.GetExtension(CurrentPath)?.ToLower();
            var newExtension = Path.GetExtension(NewPath)?.ToLower();

            if (oldExtension == ".m4a" && newExtension == ".mp3")
            {
                try
                {
                    MusicFormatConverter.M4AToMp3(CurrentPath);
                    OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(false));
                }
                catch (Exception ex)
                {
                    OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, ex));
                }
            }
            else
            {
                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, new InvalidOperationException("No supported extension")));
            }
        }
    }
}
