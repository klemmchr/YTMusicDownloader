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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using YTMusicDownloader.Properties;
using YTMusicDownloader.ViewModel.Helpers;
using YTMusicDownloaderLib.DownloadManager;
using YTMusicDownloaderLib.RetrieverEngine;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    public enum FilterMode
    {
        [Description("Nothing")] None,
        [Description("Downloaded")] Downloaded,
        [Description("Not downloaded")] NotDownloaded
    }

    public class WorkspaceViewModel : ViewModelBase
    {
        #region Construction        

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorkspaceViewModel" /> class.
        /// </summary>
        /// <param name="workspace">The workspace for the view model.</param>
        public WorkspaceViewModel(Workspace workspace)
        {
            Initializing = true;
            Workspace = workspace;
            Name = Workspace.Name;
            PlaylistUrl = workspace.Settings.PlaylistUrl;
            DownloadingAllSongsText = "Download all";
            SearchTextboxWatermark = "Search ...";

            Tracks = new ObservableImmutableList<PlaylistItemViewModel>();
            Tracks.CollectionChanged += TracksOnCollectionChanged;
            DisplayedTracks = new ObservableImmutableList<PlaylistItemViewModel>();
            PageSelectorViewModel = new PageSelectorViewModel(this);
            DownloadManager = new DownloadManager(Settings.Default.ParallelDownloads);
            WorkspaceSettingsViewModel = new WorkspaceSettingsViewModel(this);
            DisplayedTracksSource = new List<PlaylistItemViewModel>();
            Workspace.Settings.PropertyChanged += SettingsOnPropertyChanged;
            FilterModes = new Dictionary<FilterMode, string>();
            Settings.Default.PropertyChanged += ApplicationSettingsOnPropertyChanged;

            _watcher = new FileSystemWatcher
            {
                Path = Workspace.Path,
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.*"
            };

            _watcher.Created += WatcherOnCreated;
            _watcher.Deleted += WatcherOnDeleted;
            _watcher.Renamed += WatcherOnRenamed;

            SetupFilter();
        }

        #endregion

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly FileSystemWatcher _watcher;

        private double _playlistFetchProgress;
        private bool _fetchingPlaylist;
        private bool _downloadingAllSongs;
        private int _downloadItemsRemaining;
        private bool _initializing;
        private string _downloadingAllSongsText;
        private int _downloadedTracks;
        private string _searchText;
        private string _searchTextboxWatermark;
        private bool _playlistUrlChanged;
        private FilterMode _selectedFilterMode;
        private bool _searchTextChangeInProgress;

        #endregion

        #region Properties

        public Workspace Workspace { get; }
        public DownloadManager DownloadManager { get; }
        public PageSelectorViewModel PageSelectorViewModel { get; }
        public WorkspaceSettingsViewModel WorkspaceSettingsViewModel { get; }


        public ObservableImmutableList<PlaylistItemViewModel> Tracks { get; }
        public ObservableImmutableList<PlaylistItemViewModel> DisplayedTracks { get; }
        public List<PlaylistItemViewModel> DisplayedTracksSource { get; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name
        {
            get { return Workspace.Name; }
            set
            {
                Workspace.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        ///     Gets or sets the playlist URL.
        /// </summary>
        /// <value>
        ///     The playlist URL.
        /// </value>
        public string PlaylistUrl
        {
            get { return Workspace.Settings.PlaylistUrl; }
            set
            {
                if (value != null)
                {
                    Workspace.SetPlaylistUrl(value);
                    RaisePropertyChanged(nameof(PlaylistUrl));
                    _playlistUrlChanged = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the playlist fetch progress.
        /// </summary>
        /// <value>
        ///     The playlist fetch progress.
        /// </value>
        public double PlaylistFetchProgress
        {
            get { return _playlistFetchProgress; }
            set
            {
                _playlistFetchProgress = value;
                RaisePropertyChanged(nameof(PlaylistFetchProgress));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the playlist items are fetched.
        /// </summary>
        /// <value>
        ///     <c>true</c> if is fetching the playlist items; otherwise, <c>false</c>.
        /// </value>
        public bool FetchingPlaylist
        {
            get { return _fetchingPlaylist; }
            set
            {
                _fetchingPlaylist = value;
                RaisePropertyChanged(nameof(FetchingPlaylist));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the sync progress is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if progress is active; otherwise, <c>false</c>.
        /// </value>
        public bool DownloadingAllSongs
        {
            get { return _downloadingAllSongs; }
            set
            {
                _downloadingAllSongs = value;
                RaisePropertyChanged(nameof(DownloadingAllSongs));
                DownloadingAllSongsText = value ? "Cancel download" : "Download all";
            }
        }

        /// <summary>
        ///     Gets or sets the amount of items which are remaining to download.
        /// </summary>
        /// <value>
        ///     The download items remaining.
        /// </value>
        public int DownloadItemsRemaining
        {
            get { return _downloadItemsRemaining; }
            set
            {
                _downloadItemsRemaining = value;
                RaisePropertyChanged(nameof(DownloadItemsRemaining));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="WorkspaceViewModel" /> is initializing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if initializing; otherwise, <c>false</c>.
        /// </value>
        public bool Initializing
        {
            get { return _initializing; }
            set
            {
                _initializing = value;
                RaisePropertyChanged(nameof(Initializing));
            }
        }

        public int DownloadedTracks
        {
            get { return _downloadedTracks; }
            set
            {
                _downloadedTracks = value;
                RaisePropertyChanged(nameof(DownloadedTracks));
            }
        }

        public int TrackCount => Tracks.Count;

        /// <summary>
        ///     Gets or sets the text displayed in the "Download all" button.
        /// </summary>
        public string DownloadingAllSongsText
        {
            get { return _downloadingAllSongsText; }
            set
            {
                _downloadingAllSongsText = value;
                RaisePropertyChanged(nameof(DownloadingAllSongsText));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                SearchTextChanged();
            }
        }

        public Dictionary<FilterMode, string> FilterModes { get; }

        public string SearchTextboxWatermark
        {
            get { return _searchTextboxWatermark; }
            set
            {
                _searchTextboxWatermark = value;
                RaisePropertyChanged(nameof(SearchTextboxWatermark));
            }
        }

        public FilterMode SelectedFilterMode
        {
            get { return _selectedFilterMode; }
            set
            {
                _selectedFilterMode = value;
                RaisePropertyChanged(nameof(SelectedFilterMode));
                SearchTextChanged();
                OnPageNumberChanged();
            }
        }

        /// <summary>
        ///     Gets the update playlist Url command.
        /// </summary>
        public RelayCommand UpdatePlaylistUrlCommand => new RelayCommand(UpdatePlaylistUrl);

        /// <summary>
        ///     Gets the download all songs command for the "Download all" button.
        /// </summary>
        /// <value>
        ///     The download all songs command.
        /// </value>
        public RelayCommand DownloadAllSongsCommand => new RelayCommand(() =>
        {
            if (DownloadingAllSongs)
                CancelDownload();
            else
                DownloadAllSongs();
        });
        #endregion

        #region Methods        

        /// <summary>
        ///     Inits the workspace loading.
        ///     Called when the workspace was selected for the first time.
        /// </summary>
        public void Init()
        {
            new Thread(() =>
            {
                // TODO: Handle when folder was deleted
#if DEBUG
                var watch = Stopwatch.StartNew();
#endif
                UpdateTracks();
                CleanupWorkspaceFolder();
#if DEBUG
                Logger.Trace("Initialized workspace view model for workspace {0}. Duration: {1} ms", Workspace,
                    watch.ElapsedMilliseconds);
#else
                Logger.Trace("Initialized workspace view model for workspace {0}.", Workspace);
#endif

                Initializing = false;
                _watcher.EnableRaisingEvents = true;
            }).Start();
        }

        private void SetupFilter()
        {
            foreach (var mode in Enum.GetNames(typeof(FilterMode)))
            {
                var attributes = typeof(FilterMode).GetMember(mode)[0].GetCustomAttributes(
                    typeof(DescriptionAttribute), false);
                FilterModes.Add((FilterMode) Enum.Parse(typeof(FilterMode), mode),
                    ((DescriptionAttribute) attributes[0]).Description);
            }

            SelectedFilterMode = FilterMode.None;
        }

        private void ApplicationSettingsOnPropertyChanged(object sender,
            PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Settings.Default.ParallelDownloads))
                DownloadManager.ParallelDownloads = Settings.Default.ParallelDownloads;
        }

        private void CleanupWorkspaceFolder()
        {
            if (!Workspace.Settings.DeleteNotSyncedItems || (Tracks.Count == 0))
                return;

            try
            {
                foreach (var file in Directory.GetFiles(Workspace.Path))
                {
                    var name = Path.GetFileNameWithoutExtension(file);

                    if ((!file.EndsWith(".m4a") && !file.EndsWith(".mp3")) ||
                        Tracks.All(item => item.Item.Title != name))
                        try
                        {
                            File.Delete(file);
                            Logger.Trace("Workspace cleanup for workspace {0}: Deleted file {1}", Workspace.Path, file);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "Workspace cleanup for workspace {0}: Error deleting file {1}",
                                Workspace.Path, file);
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Workspace cleanup for workspace {0}: Error obtaining files", Workspace.Path);
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(WorkspaceSettings.DownloadFormat))
            {
                foreach (var track in Tracks)
                    track.CheckForTrack();

                SearchTextChanged();
            }
        }

        /// <summary>
        ///     Called when a file was renamed in the workspace.
        ///     Searches for an existing song in the list and renames its title.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="renamedEventArgs">The <see cref="RenamedEventArgs" /> instance containing the event data.</param>
        private void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            var newTitle = Path.GetFileNameWithoutExtension(renamedEventArgs.Name);

            foreach (var track in Tracks)
                if (track.Title == newTitle)
                {
                    var previousState = track.DownloadState;

                    if (Path.GetExtension(renamedEventArgs.Name)?.Replace(".", "") !=
                        Workspace.Settings.DownloadFormat.ToString().ToLower())
                    {
                        track.DownloadState = DownloadState.NeedsConvertion;

                        if (previousState == DownloadState.Downloaded)
                            DownloadedTracks--;

                        if (SelectedFilterMode == FilterMode.NotDownloaded)
                        {
                            DisplayedTracksSource.Add(track);
                            DisplayedTracksSource.Sort();
                            OnPageNumberChanged();
                        }
                    }
                    else
                    {
                        track.DownloadState = DownloadState.Downloaded;

                        if (previousState == DownloadState.NotDownloaded)
                            DownloadedTracks++;

                        if (SelectedFilterMode == FilterMode.Downloaded)
                        {
                            DisplayedTracksSource.Add(track);
                            DisplayedTracksSource.Sort();
                            OnPageNumberChanged();
                        }
                    }

                    return;
                }
        }

        /// <summary>
        ///     Called when a file was deleted in the workspace.
        ///     Searches for an existing song in the list and sets its download status to false.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs" /> instance containing the event data.</param>
        private void WatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (Path.GetExtension(fileSystemEventArgs.Name)?.Replace(".", "") !=
                Workspace.Settings.DownloadFormat.ToString().ToLower())
                return;

            var title = Path.GetFileNameWithoutExtension(fileSystemEventArgs.Name);

            foreach (var track in Tracks)
            {
                if (track.Title != title) continue;

                DownloadedTracks--;
                track.DownloadState = DownloadState.NotDownloaded;

                if (SelectedFilterMode == FilterMode.NotDownloaded)
                {
                    DisplayedTracksSource.Add(track);
                    DisplayedTracksSource.Sort();
                    OnPageNumberChanged();
                }

                return;
            }
        }

        /// <summary>
        ///     Called when a file was created in the workspace folder.
        ///     Searches for an existing song and sets its download status to true.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs" /> instance containing the event data.</param>
        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var title = Path.GetFileNameWithoutExtension(fileSystemEventArgs.Name);
            var extension = Path.GetExtension(fileSystemEventArgs.Name)?.Replace(".", "");

            foreach (var track in Tracks)
            {
                if (track.Title != title) continue;
                if (track.Downloading) return;

                if (extension != Workspace.Settings.DownloadFormat.ToString().ToLower())
                {
                    track.DownloadState = DownloadState.NeedsConvertion;
                }
                else
                {
                    track.DownloadState = DownloadState.Downloaded;
                    DownloadedTracks++;
                }

                if (SelectedFilterMode == FilterMode.NotDownloaded)
                {
                    DisplayedTracksSource.Add(track);
                    DisplayedTracksSource.Sort();
                    OnPageNumberChanged();
                }

                return;
            }
        }

        /// <summary>
        ///     Called when the tracks collection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="notifyCollectionChangedEventArgs">
        ///     The <see cref="NotifyCollectionChangedEventArgs" /> instance containing
        ///     the event data.
        /// </param>
        private void TracksOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged(nameof(TrackCount));
        }

        /// <summary>
        ///     Updates the playlist Url.
        /// </summary>
        private async void UpdatePlaylistUrl()
        {
            if (_playlistUrlChanged)
            {
                Workspace.SetPlaylistUrl(PlaylistUrl);
                Tracks.Clear();
            }

            await Task.Run(() =>
            {
                foreach (var track in Tracks)
                    track.CheckForTrack();
            });

            PlaylistFetchProgress = 0;
            FetchingPlaylist = true;

            var retreiver = new PlaylistItemsRetriever(Settings.Default.PlaylistReceiveMaximum);
            retreiver.PlaylistItemsRetrieverProgressChanged +=
                delegate(object sender, PlaylistItemRetreiverProgressChangedEventArgs args)
                {
                    PlaylistFetchProgress = args.Progress;
                };

            retreiver.PlaylistItemsRetrieverCompleted +=
                (sender, args) =>
                {
                    new Thread(() =>
                    {
                        Workspace.Settings.Items.Clear();

                        foreach (var result in args.Result)
                            Workspace.Settings.Items.Add(result);

                        UpdateTracks();

                        FetchingPlaylist = false;

                        if (_playlistUrlChanged)
                        {
                            _playlistUrlChanged = false;
                            CleanupWorkspaceFolder();
                        }
                    }).Start();
                };

            try
            {
                retreiver.GetPlaylistItems(Workspace.PlaylistId);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to retreive playlist items for workspace {0}", Workspace);
                FetchingPlaylist = false;
            }
        }

        /// <summary>
        ///     Updates the tracklist.
        ///     Compares old and new songs. Removes all songs which are not in the fetched playlist and adds all songs which are
        ///     new in the playlist.
        /// </summary>
        private void UpdateTracks()
        {
            DisplayedTracksSource.Clear();
            // Get playlist items from the viewmodel collection
            var tracks = Tracks.Select(t => t.Item);

            // Determinite items that should be added and removed
            var playlistItems = tracks as PlaylistItem[] ?? tracks.ToArray();
            var addItems = Workspace.Settings.Items.Except(playlistItems).ToList();
            var removeItems = playlistItems.Except(Workspace.Settings.Items).ToList();

            // Remove the specified items
            Tracks.RemoveAll(item => removeItems.Contains(item.Item));


            // Add the new items
            foreach (var item in addItems)
                Tracks.Add(new PlaylistItemViewModel(item, this));

            var i = 0;
            DownloadedTracks = 0;
            Workspace.Settings.Items.Clear();

            foreach (var track in Tracks)
            {
                if (track.DownloadState == DownloadState.Downloaded)
                    DownloadedTracks++;

                track.Index = i++;
                Workspace.Settings.Items.Add(track.Item);
            }
            Workspace.SaveWorkspaceConfig();

            DisplayedTracksSource.AddRange(Tracks);
            OnPageNumberChanged();
            PageSelectorViewModel.UpdatePageview();
        }

        /// <summary>
        ///     Called when the page number was changed.
        /// </summary>
        internal void OnPageNumberChanged()
        {
            if (Tracks.Count == 0) return;

            DisplayedTracks.Clear();
            var pageItems = new List<PlaylistItemViewModel>();

            var startingIndex = (PageSelectorViewModel.PageNumber - 1)*PageSelectorViewModel.ItemsPerPage;
            var endingIndex = startingIndex +
                              Math.Min(DisplayedTracksSource.Count - startingIndex, PageSelectorViewModel.ItemsPerPage);

            for (var i = startingIndex; i < endingIndex; i++)
            {
                var current = DisplayedTracksSource[i];

                if (!DisplayedTracks.Contains(current))
                {
                    if (current.Thumbnail == null)
                        current.UpdateThumbnail();

                    pageItems.Add(current);
                }
            }

            DisplayedTracks.RemoveAll(x => !pageItems.Contains(x));
            DisplayedTracks.AddRange(pageItems);
            DisplayedTracks.Sort();
        }

        /// <summary>
        ///     Downloads all songs.
        /// </summary>
        private void DownloadAllSongs()
        {
            DownloadingAllSongs = true;
            DownloadItemsRemaining = 0;

            foreach (var track in Tracks)
            {
                track.CheckForTrack();

                if ((track.DownloadState != DownloadState.Downloaded) && track.AutoDownload)
                {
                    DownloadItemsRemaining++;

                    var handler = track.DownloadSong(false);
                    if (handler == null) continue;

                    DownloadManager.AddToQueue(handler);
                }
            }

            if (DownloadItemsRemaining <= 0)
                DownloadingAllSongs = false;
        }

        internal void HandlerOnDownloadItemDownloadCompleted(object sender, DownloadCompletedEventArgs args)
        {
            var downloadManagerItem = (DownloadManagerItem) sender;
            downloadManagerItem.DownloadItemDownloadCompleted -= HandlerOnDownloadItemDownloadCompleted;

            if (DownloadingAllSongs && (--DownloadItemsRemaining <= 0))
                DownloadingAllSongs = false;

            if (args.Cancelled)
                return;

            DownloadedTracks++;

            SearchTextChanged();
        }

        private void SearchTextChanged()
        {
            if (_searchTextChangeInProgress)
                return;

            _searchTextChangeInProgress = true;
            var refreshPageView = false;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchTextboxWatermark = "Search ...";
                if (DisplayedTracksSource.Count != Tracks.Count)
                {
                    DisplayedTracksSource.Clear();
                    DisplayedTracksSource.AddRange(Tracks);
                    refreshPageView = true;
                }
            }
            else
            {
                var results = Tracks.Where(x => x.Item.Title.ToLower().Contains(SearchText.ToLower())).ToList();
                DisplayedTracksSource.Clear();
                DisplayedTracksSource.AddRange(results);

                SearchTextboxWatermark = $"{results.Count} results";

                refreshPageView = true;
            }

            ApplyFilter();

            if (refreshPageView)
                OnPageNumberChanged();

            PageSelectorViewModel.UpdatePageview();
            _searchTextChangeInProgress = false;
        }

        private void ApplyFilter()
        {
            switch (SelectedFilterMode)
            {
                case FilterMode.Downloaded:
                {
                    DisplayedTracksSource.RemoveAll(x => x.DownloadState != DownloadState.Downloaded);
                } break;

                case FilterMode.NotDownloaded:
                {
                    DisplayedTracksSource.RemoveAll(x => x.DownloadState == DownloadState.Downloaded);
                } break;
            }
        }

        /// <summary>
        ///     Cancels the download of all songs.
        /// </summary>
        private void CancelDownload()
        {
            DownloadManager.Abort();

            foreach (var track in Tracks)
                track.StopDownload();

            DownloadingAllSongs = false;
        }

        #endregion
    }
}