using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using iTunesLib;
using YTMusicDownloader.Model.ITunes;

namespace YTMusicDownloader.ViewModel
{
    public class WorkspaceSettingsViewModel: ViewModelBase
    {
        #region Fields
        private readonly WorkspaceViewModel _workspaceViewModel;
        private readonly List<IITPlaylist> _playlists;
        private int _selectedPlaylistIndex;
        #endregion

        #region Properties
        public ObservableCollection<string> Playlists { get; }
        public int SelectedPlaylistIndex {
            get { return _selectedPlaylistIndex; }
            set
            {
                _selectedPlaylistIndex = value;
                PlaylistSelectionChanged();
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool DeleteNotSyncedItems
        {
            get { return _workspaceViewModel.Workspace.Settings.DeleteNotSyncedItems; }
            set { _workspaceViewModel.Workspace.Settings.DeleteNotSyncedItems = value; }
        }

        public WorkspaceSettingsViewModel(WorkspaceViewModel workspaceViewModel)
        {
            _workspaceViewModel = workspaceViewModel;

            Playlists = new ObservableCollection<string> {"Disabled"};
            _playlists = new List<IITPlaylist> {null};
        }

        public async void SettingsPageSelected()
        {
            _playlists.Clear();
            _playlists.Add(null);
            _playlists.AddRange(await ITunesSync.GetAllPlaylists());

            Playlists.Clear();
            Playlists.Add("Disabled");
            
            foreach (var playlist in _playlists)
            {
                if(playlist != null)
                    Playlists.Add(playlist.Name);
            }

            if (_selectedPlaylistIndex == -1)
                _selectedPlaylistIndex = 0;
        }

        private void PlaylistSelectionChanged()
        {
            
        }
    }
}
