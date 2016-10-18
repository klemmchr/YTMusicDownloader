using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using YTMusicDownloaderLib.RetrieverEngine;

namespace YTMusicDownloaderLib.DownloadManager
{
    public class ConvertionItem : DownloadManagerItem
    {
        public string CurrentPath { get; }
        public string NewPath { get; }

        public ConvertionItem(PlaylistItem item, string currentPath, string newPath) : base(item)
        {
            CurrentPath = currentPath;
            NewPath = newPath;
        }

        public override async void StartDownload()
        {
            await Task.Run(() =>
            {
                var oldExtension = Path.GetExtension(CurrentPath)?.ToLower();
                var newExtension = Path.GetExtension(NewPath)?.ToLower();

                if (oldExtension == ".m4a" && newExtension == ".mp3")
                {
                    try
                    {
                        using (var reader = new MediaFoundationReader(CurrentPath))
                        {
                            MediaFoundationEncoder.EncodeToMp3(reader, NewPath);
                        }

                        File.Delete(CurrentPath);

                        OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(false));
                    }
                    catch (Exception ex)
                    {
                        OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, ex));
                    }
                }
                else if (oldExtension == ".mp3" && newExtension == ".m4a")
                {
                    try
                    {
                        using (var reader = new MediaFoundationReader(CurrentPath))
                        {
                            MediaFoundationEncoder.EncodeToAac(reader, NewPath);
                        }

                        File.Delete(CurrentPath);

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
            });
        }
    }
}
