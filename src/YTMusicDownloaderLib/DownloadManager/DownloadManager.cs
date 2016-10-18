using System.Collections.Generic;
using System.Threading;
using NLog;

namespace YTMusicDownloaderLib.DownloadManager
{
    public class DownloadManager
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Queue<DownloadManagerItem> _queue;
        private readonly List<DownloadManagerItem> _activeDownloads;
        private Thread _thread;
        #endregion

        #region Properties
        public int ParallelDownloads { get; set; }
        #endregion

        #region Construction
        public DownloadManager(int parallelDownloads)
        {
            ParallelDownloads = parallelDownloads;

            _queue = new Queue<DownloadManagerItem>();
            _activeDownloads = new List<DownloadManagerItem>();
        }
        #endregion

        #region Methods
        public void AddToQueue(DownloadManagerItem item)
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
            if(_thread != null && _thread.IsAlive) return;

            _thread = new Thread(() =>
            {
                try
                {
                    while (_queue.Count > 0 && _queue.Peek() != null)
                    {
                        DownloadItem();
                        
                        do
                        {
                            Thread.Sleep(10);
                        } while (_activeDownloads.Count >= ParallelDownloads);
                    }
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
            if (_activeDownloads.Count >= ParallelDownloads) return;

            DownloadManagerItem item = null;
            try
            {
                item = _queue.Dequeue();
            }
            catch
            {
                // ignored
            }
            
            if (item != null)
            {
                item.DownloadItemDownloadCompleted += (sender, args) =>
                {
                    if(args.Error != null)
                        Logger.Error(args.Error, "Error downloading track {0}", ((DownloadManagerItem)sender).Item.VideoId);
                    
                    _activeDownloads.Remove((DownloadManagerItem)sender);
                };

                _activeDownloads.Add(item);
                item.StartDownload();
            }
        }
        #endregion
    }
}