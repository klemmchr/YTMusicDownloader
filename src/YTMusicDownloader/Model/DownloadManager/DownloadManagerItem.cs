using System;
using YTMusicDownloader.Model.RetrieverEngine;

namespace YTMusicDownloader.Model.DownloadManager
{
    public abstract class DownloadManagerItem: IDownloadManagerItem, IDisposable
    {
        #region Events
        public event DownloadItemDownloadCompletedEventHandler DownloadItemDownloadCompleted;
        public void OnDownloadItemDownloadCompleted(DownloadCompletedEventArgs e)
        {
            DownloadItemDownloadCompleted?.Invoke(this, e);
        }

        public event DownloadItemDownloadProgressChangedEventHandler DownloadItemDownloadProgressChanged;
        public void OnDownloadItemDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            DownloadItemDownloadProgressChanged?.Invoke(this, e);
        }
        #endregion

        #region Properties
        public PlaylistItem Item { get; }
        #endregion

        protected DownloadManagerItem(PlaylistItem item)
        {
            Item = item;
        }

        #region Methods
        public virtual void StartDownload() { }
        public virtual void StopDownload() { }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public virtual void Dispose()
        {
            
        }

        public override bool Equals(object obj)
        {
            var item = obj as DownloadItem;

            return Item.Equals(item?.Item);
        }
        #endregion
    }
}