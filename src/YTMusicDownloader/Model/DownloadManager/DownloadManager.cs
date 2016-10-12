using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace YTMusicDownloader.Model.DownloadManager
{
    public class DownloadManager
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Queue<DownloadItem> _queue;
        private readonly List<DownloadItem> _activeDownloads;
        private bool _active;
        private Thread _thread;
        #endregion

        #region Construction
        public DownloadManager()
        {
            _queue = new Queue<DownloadItem>();
            _activeDownloads = new List<DownloadItem>();
        }
        #endregion

        #region Methods
        public void AddToQueue(DownloadItem item)
        {
            _queue.Enqueue(item);

            StartManager();
        }

        public void Abort()
        {
            _thread?.Abort();
            
            _queue.Clear();
            _activeDownloads.Clear();
        }

        private void StartManager()
        {
            if(_active) return;

            _active = true;

            _thread = new Thread(() =>
            {
                try
                {
                    while (_queue.Count > 0 && _queue.Peek() != null)
                    {
                        DownloadItem();

                        while (_activeDownloads.Count >= Properties.Settings.Default.ParallelDownloads)
                        {
                            Thread.Sleep(10);
                        }
                    }

                    _active = false;
                }
                catch (ThreadInterruptedException)
                {
                    // ignored
                }
            });
            _thread.Start();
        }

        private void DownloadItem()
        {
            if (_activeDownloads.Count >= Properties.Settings.Default.ParallelDownloads) return;
            
            DownloadItem item;
            try
            {
                item = _queue.Dequeue();
            }
            catch
            {
                return;
            }
            
            if (item != null)
            {
                item.DownloadItemDownloadCompleted += (sender, args) =>
                {
                    if(args.Error != null)
                        Logger.Error(args.Error, "Error downloading track {0}", ((DownloadItem)sender).Item.VideoId);

                    var dItem = (DownloadItem) sender;
                    _activeDownloads.Remove(dItem);

                    dItem.Dispose();
                };

                _activeDownloads.Add(item);
                Task.Run(() => item.StartDownload());
            }
        }
        #endregion
    }
}