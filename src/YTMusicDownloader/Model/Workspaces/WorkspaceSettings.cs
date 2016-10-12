using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using YTMusicDownloader.Model.DownloadManager;
using YTMusicDownloader.Model.RetrieverEngine;

namespace YTMusicDownloader.Model.Workspaces
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
        private List<PlaylistItem> _items = new List<PlaylistItem>();
        private int _itemsPerPage = 10;
        private bool _deleteNotSyncedItems;
        private DownloadFormat _downloadFormat = DownloadFormat.M4A;

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

        public List<PlaylistItem> Items
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
        #endregion
    }
}
