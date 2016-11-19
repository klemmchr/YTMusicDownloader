/*
    Copyright 2016 Christian Klemm

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using YTMusicDownloaderLib.RetrieverEngine;

namespace YTMusicDownloaderLib.DownloadManager
{
    public abstract class DownloadManagerItem : IDownloadManagerItem, IDisposable
    {
        protected DownloadManagerItem(PlaylistItem item)
        {
            Item = item;
        }

        #region Properties

        public PlaylistItem Item { get; }

        #endregion

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

        public event DownloadItemDownloadStartedEventHandler DownloadItemDownloadStarted;

        public void OnDownloadItemDownloadStarted(EventArgs e)
        {
            DownloadItemDownloadStarted?.Invoke(this, e);
        }

        public event DownloadItemConvertionStartedEventHandler DownloadItemConvertionStarted;

        public void OnDownloadItemConvertionStarted(EventArgs e)
        {
            DownloadItemConvertionStarted?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public virtual void StartDownload()
        {
        }

        public virtual void StopDownload()
        {
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public virtual void Dispose()
        {
        }

        public override bool Equals(object obj)
        {
            var item = obj as DownloadManagerItem;

            return Item.Equals(item?.Item);
        }

        #endregion
    }
}