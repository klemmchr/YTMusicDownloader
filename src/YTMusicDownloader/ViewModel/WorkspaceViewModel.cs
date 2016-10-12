using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using YTMusicDownloader.Model.DownloadManager;
using YTMusicDownloader.Model.RetrieverEngine;
using YTMusicDownloader.Model.Workspaces;
using YTMusicDownloader.ViewModel.Helpers;
using YTMusicDownloader.ViewModel.Messages;

namespace YTMusicDownloader.ViewModel
{
    public class WorkspaceViewModel: ViewModelBase
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly FileSystemWatcher _watcher;

        private double _playlistFetchProgress;
        private bool _fetchingPlaylist;
        private bool _downloadingAllSongs;
        private int _downloadItemsRemaining;
        private bool _initializing;
        private string _downloadingAllSongsText;
        #endregion

        #region Properties
        public Workspace Workspace { get; }
        public DownloadManager DownloadManager { get; }
        public PageSelectorViewModel PageSelectorViewModel { get; }
        public WorkspaceSettingsViewModel WorkspaceSettingsViewModel { get; }

        public ObservableImmutableList<PlaylistItemViewModel> Tracks { get; }
        public ObservableImmutableList<PlaylistItemViewModel> DisplayedTracks { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
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
        /// Gets or sets the playlist URL.
        /// </summary>
        /// <value>
        /// The playlist URL.
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
                }
            }
        }

        /// <summary>
        /// Gets or sets the playlist fetch progress.
        /// </summary>
        /// <value>
        /// The playlist fetch progress.
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
        /// Gets or sets a value indicating whether the playlist items are fetched.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is fetching the playlist items; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether the sync progress is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if progress is active; otherwise, <c>false</c>.
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
        /// Gets or sets the amount of items which are remaining to download.
        /// </summary>
        /// <value>
        /// The download items remaining.
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
        /// Gets or sets a value indicating whether this <see cref="WorkspaceViewModel"/> is initializing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initializing; otherwise, <c>false</c>.
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

        public int TrackCount => Tracks.Count;

        public string WorkspacePath => Workspace.Path;

        /// <summary>
        /// Gets or sets the text displayed in the "Download all" button.
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

        /// <summary>
        /// Gets the update playlist Url command.
        /// </summary>
        public RelayCommand UpdatePlaylistUrlCommand => new RelayCommand(UpdatePlaylistUrl);

        /// <summary>
        /// Gets the download all songs command for the "Download all" button.
        /// </summary>
        /// <value>
        /// The download all songs command.
        /// </value>
        public RelayCommand DownloadAllSongsCommand => new RelayCommand(() =>
        {
            if (DownloadingAllSongs)
            {
                CancelDownload();
            }
            else
            {
                DownloadAllSongs();
            }
        });
        #endregion

        #region Construction        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceViewModel"/> class.
        /// </summary>
        /// <param name="workspace">The workspace for the view model.</param>
        public WorkspaceViewModel(Workspace workspace)
        {
            Initializing = true;
            Workspace = workspace;
            Name = Workspace.Name;
            PlaylistUrl = workspace.Settings.PlaylistUrl;
            DownloadingAllSongsText = "Download all";
            
            Tracks = new ObservableImmutableList<PlaylistItemViewModel>();
            Tracks.CollectionChanged += TracksOnCollectionChanged;
            DisplayedTracks = new ObservableImmutableList<PlaylistItemViewModel>();
            PageSelectorViewModel = new PageSelectorViewModel(this);
            DownloadManager = new DownloadManager();
            WorkspaceSettingsViewModel = new WorkspaceSettingsViewModel(this);
            Workspace.Settings.PropertyChanged += SettingsOnPropertyChanged;

            _watcher = new FileSystemWatcher
            {
                Path = WorkspacePath,
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.m4a",
            };

            _watcher.Created += WatcherOnCreated;
            _watcher.Deleted += WatcherOnDeleted;
            _watcher.Renamed += WatcherOnRenamed;
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Inits the workspace loading.
        /// Called when the workspace was selected for the first time.
        /// </summary>
        public void Init()
        {
            new Thread(() =>
            {
#if DEBUG
                var watch = Stopwatch.StartNew();
#endif
                UpdateTracks();
                CleanupWorkspaceFolder();
#if DEBUG
                Logger.Trace("Initialized workspace view model for workspace {0}. Duration: {1} ms", Workspace, watch.ElapsedMilliseconds);
#else
                Logger.Trace("Initialized workspace view model for workspace {0}.", Workspace);
#endif

                Initializing = false;
                _watcher.EnableRaisingEvents = true;
            }).Start();
        }

        private void CleanupWorkspaceFolder()
        {
            if(!Workspace.Settings.DeleteNotSyncedItems || Tracks.Count == 0)
                return;

            try
            {
                foreach (var file in Directory.GetFiles(Workspace.Path))
                {
                    var name = Path.GetFileNameWithoutExtension(file);

                    if ((!file.EndsWith(".m4a") && !file.EndsWith(".mp3")) || !(Tracks.Any(item => item.Item.Title == name)))
                    {
                        try
                        {
                            File.Delete(file);
                            Logger.Trace("Workspace cleanup for workspace {0}: Deleted file {1}", Workspace.Path, file);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "Workspace cleanup for workspace {0}: Error deleting file {1}", Workspace.Path, file);
                        }
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
                var type = typeof(DownloadFormat);
                var memInfo = type.GetMember(Workspace.Settings.DownloadFormat.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                var fileEnding = ((DescriptionAttribute) attributes[0]).Description;

                _watcher.Filter = $"*{fileEnding}";
            }
                Messenger.Default.Send(new DownloadFormatChangedMessage(Workspace.Settings.DownloadFormat));
        }

        /// <summary>
        /// Called when a file was renamed in the workspace.
        /// Searches for an existing song in the list and renames its title.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="renamedEventArgs">The <see cref="RenamedEventArgs"/> instance containing the event data.</param>
        private void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            foreach (var track in Tracks)
            {
                if (track.Title.Replace(".mp3", "").Replace(".m4a", "") != renamedEventArgs.OldName) continue;

                track.Title = renamedEventArgs.Name.Replace(".m4a", "");
                break;
            }
        }

        /// <summary>
        /// Called when a file was deleted in the workspace.
        /// Searches for an existing song in the list and sets its download status to false.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void WatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            foreach (var track in Tracks)
            {
                if (track.Title.Replace(".mp3", "").Replace(".m4a", "") != fileSystemEventArgs.Name) continue;

                track.Downloaded = false;
                break;
            }
        }

        /// <summary>
        /// Called when a file was created in the workspace folder.
        /// Searches for an existing song and sets its download status to true.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fileSystemEventArgs">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            foreach (var track in Tracks)
            {
                if (track.Title.Replace(".mp3", "").Replace(".m4a", "") != fileSystemEventArgs.Name) continue;

                track.Downloaded = true;
                break;
            }
        }

        /// <summary>
        /// Called when the tracks collection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="notifyCollectionChangedEventArgs">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void TracksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            PageSelectorViewModel.UpdatePageview();

            RaisePropertyChanged(nameof(TrackCount));
        }

        /// <summary>
        /// Updates the playlist Url.
        /// </summary>
        private async void UpdatePlaylistUrl()
        {
            if (Workspace.Settings.PlaylistUrl != PlaylistUrl)
            {
                Workspace.SetPlaylistUrl(PlaylistUrl);
                Tracks.Clear();
            }

            await Task.Run(() =>
            {
                foreach (var track in Tracks)
                {
                    track.CheckForTrack();
                }
            });
            
            PlaylistFetchProgress = 0;
            FetchingPlaylist = true;

            var retreiver = new PlaylistItemsRetriever();
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
                    Workspace.Settings.Items.AddRange(args.Result);

                    UpdateTracks();

                    FetchingPlaylist = false;
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
        /// Updates the tracklist.
        /// Compares old and new songs. Removes all songs which are not in the fetched playlist and adds all songs which are new in the playlist.
        /// </summary>
        private void UpdateTracks()
        {
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
            {
                Tracks.Add(new PlaylistItemViewModel(item, this));
            }

            Workspace.Settings.Items.Clear();
            foreach (var track in Tracks)
            {
                Workspace.Settings.Items.Add(track.Item);
            }
            Workspace.SaveWorkspaceConfig();

            OnPageNumberChanged();
        }

        /// <summary>
        /// Called when the page number was changed.
        /// </summary>
        internal void OnPageNumberChanged()
        {
            if (Tracks.Count == 0) return;

            DisplayedTracks.Clear();

            var startingIndex = (PageSelectorViewModel.PageNumber - 1) * PageSelectorViewModel.ItemsPerPage;
            var endingIndex = startingIndex + Math.Min(Tracks.Count - startingIndex, PageSelectorViewModel.ItemsPerPage);
            for (var i = startingIndex; i < endingIndex; i++)
            {
                var current = Tracks[i];
                if (current.Thumbnail == null)
                    current.UpdateThumbnail();

                DisplayedTracks.Add(current);
            }
        }

        /// <summary>
        /// Downloads all songs.
        /// </summary>
        private void DownloadAllSongs()
        {
            DownloadingAllSongs = true;
            DownloadItemsRemaining = 0;

            foreach (var track in Tracks)
            {
                track.CheckForTrack();
                if (!track.Downloaded && track.AutoDownload)
                {
                    DownloadItemsRemaining ++;

                    var handler = track.DownloadSong(false);
                    if(handler == null) continue;

                    handler.DownloadItemDownloadCompleted += (sender, args) =>
                    {
                        DownloadItemsRemaining --;
                        if (DownloadItemsRemaining <= 0)
                        {
                            DownloadingAllSongs = false;
                        }
                    };

                    DownloadManager.AddToQueue(handler);
                }
            }

            if (DownloadItemsRemaining == 0)
                DownloadingAllSongs = false;
        }

        /// <summary>
        /// Cancels the download of all songs.
        /// </summary>
        private void CancelDownload()
        {
            DownloadManager.Abort();

            foreach (var track in Tracks)
            {
                track.StopDownload();    
            }

            DownloadingAllSongs = false;
        }
#endregion
    }
}
