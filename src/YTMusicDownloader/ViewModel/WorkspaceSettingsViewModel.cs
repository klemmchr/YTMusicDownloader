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
using System.Collections.Generic;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using iTunesLib;
using YTMusicDownloader.ViewModel.Helpers;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.DownloadManager;
using YTMusicDownloaderLib.ITunes;

namespace YTMusicDownloader.ViewModel
{
    public class WorkspaceSettingsViewModel : ViewModelBase
    {
        #region Construction

        public WorkspaceSettingsViewModel(WorkspaceViewModel workspaceViewModel)
        {
            _workspaceViewModel = workspaceViewModel;

            Playlists = new ObservableImmutableList<string>();
            _playlists = new List<IITPlaylist>();
            DownloadFormatOptions = new Dictionary<DownloadFormat, string>();

            ResetITunesPlaylists();

            foreach (var format in (DownloadFormat[]) Enum.GetValues(typeof(DownloadFormat)))
                DownloadFormatOptions.Add(format, format.ToString());
        }

        #endregion

        #region Fields

        private readonly WorkspaceViewModel _workspaceViewModel;

        private int _selectedPlaylistIndex;
        private readonly List<IITPlaylist> _playlists;

        #endregion

        #region Properties

        public ObservableImmutableList<string> Playlists { get; }
        public Dictionary<DownloadFormat, string> DownloadFormatOptions { get; }
        public IITPlaylist SelectedPlaylist { get; private set; }

        // ReSharper disable once InconsistentNaming
        public bool ITunesSyncEnabled
        {
            get { return _workspaceViewModel.Workspace.Settings.ITunesSyncEnabled; }
            set
            {
                _workspaceViewModel.Workspace.Settings.ITunesSyncEnabled = value;
                RaisePropertyChanged(nameof(ITunesSyncEnabled));
                if (value) RefreshITunesPlaylists();
                else ResetITunesPlaylists();
            }
        }

        public int SelectedPlaylistIndex
        {
            get { return _selectedPlaylistIndex; }
            set
            {
                _selectedPlaylistIndex = value;
                RaisePropertyChanged(nameof(SelectedPlaylistIndex));
                new Thread(() => PlaylistSelectionChanged(value)).Start();
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

                Messenger.Default.Send(
                    new ShowMessageDialogMessage(
                        "Download format changed",
                        $"You have changed the download format to {value.ToString().ToLower()}.\n\nTo convert your current tracks to the new format simply just sync your workspace or download them manually again."
                    )
                );
            }
        }

        #endregion

        #region Methods

        public void SettingsPageSelected()
        {
            if (ITunesSyncEnabled)
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
                Playlists.Clear();
                Playlists.Add("Disabled");
                _playlists.Clear();
                _playlists.Add(null);

                var i = 1;
                foreach (var playlist in ITunesSync.GetAllPlaylists())
                {
                    Playlists.Add(playlist.Name);
                    _playlists.Add(playlist);

                    if (playlist.Name == _workspaceViewModel.Workspace.Settings.ITunesSyncPlaylist)
                        SelectedPlaylistIndex = i;

                    i++;
                }

                if (SelectedPlaylistIndex == -1)
                    SelectedPlaylistIndex = 0;
            }).Start();
        }

        private void PlaylistSelectionChanged(int index)
        {
            IITPlaylist newPlaylist = null;
            if ((index > 0) && (index < _playlists.Count))
            {
                newPlaylist = _playlists[index];
                _workspaceViewModel.Workspace.Settings.ITunesSyncPlaylist = newPlaylist.Name;
                ITunesSync.RemoveOldTracks(newPlaylist);
            }

            foreach (var track in _workspaceViewModel.Tracks)
            {
                if (SelectedPlaylist != null)
                    ITunesSync.RemoveTrack(SelectedPlaylist, track.Item);

                if (newPlaylist != null)
                    ITunesSync.AddTrack(newPlaylist, track.Item, track.GetFilePath());
            }

            if (newPlaylist != null)
                SelectedPlaylist = newPlaylist;
        }

        #endregion
    }
}