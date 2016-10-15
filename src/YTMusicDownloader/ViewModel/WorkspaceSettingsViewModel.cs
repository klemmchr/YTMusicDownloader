using System;
using System.Collections.Generic;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using iTunesLib;
using YTMusicDownloader.Model.DownloadManager;
using YTMusicDownloader.Model.ITunes;
using YTMusicDownloader.ViewModel.Helpers;
using YTMusicDownloader.ViewModel.Messages;

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
        public ObservableImmutableList<string> Playlists { get; }
        public Dictionary<DownloadFormat, string> DownloadFormatOptions { get; }

        // ReSharper disable once InconsistentNaming
        public bool ITunesSyncEnabled
        {
            get { return _workspaceViewModel.Workspace.Settings.ITunesSyncEnabled; }
            set
            {
                _workspaceViewModel.Workspace.Settings.ITunesSyncEnabled = value;
                RaisePropertyChanged(nameof(ITunesSyncEnabled));
                if (value) RefreshITunesPlaylists(); else ResetITunesPlaylists();
            }
        }

        public int SelectedPlaylistIndex {
            get { return _selectedPlaylistIndex; }
            set
            {
                _selectedPlaylistIndex = value;
                PlaylistSelectionChanged();
                RaisePropertyChanged(nameof(SelectedPlaylistIndex));
            }
        }

        public bool DeleteNotSyncedItems
        {
            get { return _workspaceViewModel.Workspace.Settings.DeleteNotSyncedItems; }
            set { _workspaceViewModel.Workspace.Settings.DeleteNotSyncedItems = value; }
        }

        public DownloadFormat SelectedDownloadFormatOption
        {
            get { return _workspaceViewModel.Workspace.Settings.DownloadFormat; }
            set
            {
                _workspaceViewModel.Workspace.Settings.DownloadFormat = value;
                RaisePropertyChanged();

                Messenger.Default.Send (
                    new ShowMessageDialogMessage (
                        "Download format changed", 
                        $"You have changed the download format to {value.ToString().ToLower()}.\n\nTo convert your current tracks to the new format simply just sync your workspace or download them manually again."
                    )
                );
            }
        }
        #endregion

        #region Construction
        public WorkspaceSettingsViewModel(WorkspaceViewModel workspaceViewModel)
        {
            _workspaceViewModel = workspaceViewModel;

            Playlists = new ObservableImmutableList<string>();
            _playlists = new List<IITPlaylist>();
            DownloadFormatOptions = new Dictionary<DownloadFormat, string>();

            ResetITunesPlaylists();

            foreach (var format in (DownloadFormat[]) Enum.GetValues(typeof(DownloadFormat)))
            {
                DownloadFormatOptions.Add(format, format.ToString());
            }
        }
        #endregion

        #region Methods
        public void SettingsPageSelected()
        {
            if(ITunesSyncEnabled)
                RefreshITunesPlaylists();
        }

        private void ResetITunesPlaylists()
        {
            Playlists.Clear();
            Playlists.Add("Disabled");
            _playlists.Clear();
            _playlists.Add(null);

            SelectedPlaylistIndex = 0;
        }

        private void RefreshITunesPlaylists()
        {
            new Thread(() =>
            {
                ResetITunesPlaylists();

                _playlists.AddRange(ITunesSync.GetAllPlaylists());
                
                foreach (var playlist in _playlists)
                {
                    if (playlist != null)
                        Playlists.Add(playlist.Name);
                }
            }).Start();
        }

        private void PlaylistSelectionChanged()
        {
            
        }
        #endregion
    }
}