using System;
using System.Net;
using System.Threading.Tasks;
using NLog;
using YTMusicDownloader.Model.RetrieverEngine;
using System.IO;

namespace YTMusicDownloader.Model.DownloadManager
{
    public class DownloadItem: IDisposable
    {
        #region Events
        public delegate void DownloadItemDownloadCompletedEventHandler(object sender, DownloadCompletedEventArgs args);

        public event DownloadItemDownloadCompletedEventHandler DownloadItemDownloadCompleted;

        protected virtual void OnDownloadItemDownloadCompleted(DownloadCompletedEventArgs e)
        {
            DownloadItemDownloadCompleted?.Invoke(this, e);
        }

        public delegate void DownloadItemDownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs args);

        public event DownloadItemDownloadProgressChangedEventHandler DownloadItemDownloadProgressChanged;

        protected virtual void OnDownloadItemDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            DownloadItemDownloadProgressChanged?.Invoke(this, e);
        }
        #endregion

        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private WebClient _client;
        #endregion
        #region Properties
        public PlaylistItem Item { get; }
        public string SavePath { get; }
        public bool Overwrite { get; }
        #endregion

        public DownloadItem(PlaylistItem item, string savePath, bool overwrite = false)
        {
            Item = item;
            SavePath = savePath;
            Overwrite = overwrite;
        }

        public void StartDownload()
        {
            if (File.Exists(SavePath) && !Overwrite)
            {
                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true));
                return;
            }

            OnDownloadItemDownloadProgressChanged(new DownloadProgressChangedEventArgs(1));
            Item.RetreiveDownloadUrl();

            if (string.IsNullOrEmpty(Item.DownloadUrl))
            {
                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, new InvalidOperationException("Could not retreive download url")));
                return;
            }

            // GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            using (_client = new WebClient())
            {
                _client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                try
                {
                    _client.DownloadDataCompleted +=
                        (sender, args) =>
                        {
                            Task.Run(() =>
                            {
                                DownloadCompleted(args);
                            });
                        };
                    _client.DownloadProgressChanged += (sender, args) => OnDownloadItemDownloadProgressChanged(new DownloadProgressChangedEventArgs(args.ProgressPercentage));
                    _client.DownloadDataAsync(new Uri(Item.DownloadUrl));
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Error downloading track {0}", Item.VideoId);
                    OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, ex));
                }
            }
        }

    private void DownloadCompleted(DownloadDataCompletedEventArgs args)
    {
        _client.Dispose();
        // _client = null;

        // GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        // GC.Collect(2, GCCollectionMode.Forced);

        if (args.Cancelled)
        {
            OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, args.Error));
            return;
        }

        try
        {
            // File.WriteAllBytes(SavePath, args.Result);

            /*
            using (var file = TagLib.File.Create(SavePath))
            {
                file.Save();
            }

            try
            {
                MusicFormatConverter.M4AToMp3(SavePath);
            }
            catch (Exception)
            {
                // ignore
            }
            */

            OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(false));
        }
        catch (Exception ex)
        {
            OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, ex));
            Logger.Error(ex, "Error writing track file for track {0}", Item.VideoId);
        }
    }

        public void StopDownload()
        {
            _client?.CancelAsync();
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public override bool Equals(object obj)
        {
            var item = obj as DownloadItem;

            return Item.Equals(item?.Item);
        }
    }
}
