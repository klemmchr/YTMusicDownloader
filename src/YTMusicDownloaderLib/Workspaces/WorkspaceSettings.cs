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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using YTMusicDownloaderLib.DownloadManager;
using YTMusicDownloaderLib.RetrieverEngine;

namespace YTMusicDownloaderLib.Workspaces
{
    [JsonObject(MemberSerialization.OptOut)]
    public class WorkspaceSettings: INotifyPropertyChanged
    {
        #region NotifyPropertyChangedEvent
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields
        private string _playlistUrl;
        private HashSet<PlaylistItem> _items = new HashSet<PlaylistItem>();
        private int _itemsPerPage = 10;
        private bool _deleteNotSyncedItems;
        private DownloadFormat _downloadFormat = DownloadFormat.MP3;
        private bool _iTunesSyncEnabled;
        private string _iTunesSyncPlaylist;

        #endregion

        #region Properties

        public string PlaylistUrl
        {
            get { return _playlistUrl; }
            set
            {
                _playlistUrl = value;
                RaisePropertyChanged();
            }
        }

        public HashSet<PlaylistItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged();
            }
        }

        public int ItemsPerPage
        {
            get { return _itemsPerPage; }
            set
            {
                _itemsPerPage = value;
                RaisePropertyChanged();
            }
        }

        public bool DeleteNotSyncedItems
        {
            get { return _deleteNotSyncedItems; }
            set
            {
                _deleteNotSyncedItems = value;
                RaisePropertyChanged();
            }
        }

        public DownloadFormat DownloadFormat
        {
            get { return _downloadFormat; }
            set
            {
                _downloadFormat = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once InconsistentNaming
        public bool ITunesSyncEnabled
        {
            get { return _iTunesSyncEnabled; }
            set
            {
                _iTunesSyncEnabled = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once InconsistentNaming
        public string ITunesSyncPlaylist
        {
            get { return _iTunesSyncPlaylist; }
            set
            {
                _iTunesSyncPlaylist = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}
