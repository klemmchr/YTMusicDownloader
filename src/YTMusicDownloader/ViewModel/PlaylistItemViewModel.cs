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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using YTMusicDownloaderLib.DownloadManager;
using YTMusicDownloaderLib.RetrieverEngine;
using DownloadProgressChangedEventArgs = YTMusicDownloaderLib.DownloadManager.DownloadProgressChangedEventArgs;

namespace YTMusicDownloader.ViewModel
{
    public class PlaylistItemViewModel : ViewModelBase, IComparable<PlaylistItemViewModel>
    {
        public PlaylistItemViewModel(PlaylistItem item, WorkspaceViewModel workspaceWorkspaceViewModel)
        {
            if (IsInDesignMode)
                Downloading = true;

            Item = item;
            Title = item.Title;
            _workspaceViewModel = workspaceWorkspaceViewModel;

            CheckForTrack();
        }

        public int CompareTo(PlaylistItemViewModel other)
        {
            return other == null ? 1 : Index.CompareTo(other.Index);
        }

        public void CheckForTrack()
        {
            try
            {
                if (!File.Exists(GetFilePath()))
                {
                    foreach (var format in Enum.GetNames(typeof(DownloadFormat)))
                    {
                        var path = Path.Combine(_workspaceViewModel.Workspace.Path, $"{Item.Title}.{format.ToLower()}");

                        if (File.Exists(path))
                        {
                            _uncovertedPath = path;
                            DownloadState = DownloadState.NeedsConvertion;

                            return;
                        }
                    }

                    DownloadState = DownloadState.NotDownloaded;
                }
                else
                {
                    DownloadState = DownloadState.Downloaded;
                }
            }
            catch (Exception)
            {
                DownloadState = DownloadState.NotDownloaded;
            }
        }

        public string GetFilePath()
        {
            return Path.Combine(_workspaceViewModel.Workspace.Path,
                $"{Item.Title}.{_workspaceViewModel.Workspace.Settings.DownloadFormat.ToString().ToLower()}");
        }

        public async void UpdateThumbnail()
        {
            Thumbnail = await Task.Run(() =>
            {
                var image = new BitmapImage();

                try
                {
                    using (var client = new WebClient())
                    {
                        var result = client.DownloadData(Item.ThumbnailUrl);

                        using (var ms = new MemoryStream(result))
                        {
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();
                            image.Freeze();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Error downloading thumbnail for video {0}", Item.VideoId);
                }

                return image;
            });
        }

        internal DownloadManagerItem DownloadSong(bool overwrite = true)
        {
            Downloading = true;
            DownloadProgress = 1;
            DownloadPending = true;
            if (DownloadState == DownloadState.NeedsConvertion)
                _downloadItem = new ConvertionItem(Item, _uncovertedPath, GetFilePath());
            else
                _downloadItem = new DownloadItem(Item, GetFilePath(),
                    _workspaceViewModel.Workspace.Settings.DownloadFormat, overwrite);

            _downloadItem.DownloadItemDownloadProgressChanged += DownloadItemOnDownloadItemDownloadProgressChanged;
            _downloadItem.DownloadItemDownloadCompleted += DownloadItemOnDownloadItemDownloadCompleted;

            return _downloadItem;
        }

        private void DownloadItemOnDownloadItemDownloadProgressChanged(object sender,
            DownloadProgressChangedEventArgs args)
        {
            DownloadPending = false;
            DownloadProgress = args.ProgressPercentage;
        }

        private void DownloadItemOnDownloadItemDownloadCompleted(object sender, DownloadCompletedEventArgs args)
        {
            DownloadPending = false;
            Downloading = false;

            CheckForTrack();

            var managerItem = (DownloadManagerItem) sender;
            managerItem.DownloadItemDownloadProgressChanged -= DownloadItemOnDownloadItemDownloadProgressChanged;
            managerItem.DownloadItemDownloadCompleted -= DownloadItemOnDownloadItemDownloadCompleted;
            managerItem.Dispose();
            _downloadItem = null;

            _workspaceViewModel.HandlerOnDownloadItemDownloadCompleted(sender, args);
        }

        public void Rename(string newTitle)
        {
            Item.Title = newTitle;
            RaisePropertyChanged(nameof(Title));
        }

        public void StopDownload()
        {
            _downloadItem?.StopDownload();
            _downloadItem = null;
            Downloading = false;
            DownloadProgress = 0;
        }

        public override void Cleanup()
        {
            base.Cleanup();

            Thumbnail.StreamSource.Dispose();
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var vm = obj as PlaylistItemViewModel;

            return (vm != null) && (vm.Item == Item);
        }

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly WorkspaceViewModel _workspaceViewModel;
        private DownloadManagerItem _downloadItem;

        private BitmapImage _thumbnail;
        private bool _downloading;
        private DownloadState _downloadState;
        private double _downloadProgress;
        private bool _downloadPending;
        private string _downloadText;
        private string _uncovertedPath;
        private int _index;

        #endregion

        #region Properties

        public PlaylistItem Item { get; }

        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                RaisePropertyChanged(nameof(Thumbnail));
            }
        }

        public string Title
        {
            get { return Item.Title; }
            set
            {
                if (!Downloading && (Title != value))
                {
                    if (DownloadState == DownloadState.NotDownloaded)
                    {
                        Item.Title = value;
                        RaisePropertyChanged(nameof(Title));
                        return;
                    }

                    try
                    {
                        var path = DownloadState == DownloadState.NeedsConvertion ? _uncovertedPath : GetFilePath();

                        File.Move(path,
                            Path.Combine(_workspaceViewModel.Workspace.Path,
                                value + "." + _workspaceViewModel.Workspace.Settings.DownloadFormat.ToString().ToLower()));
                        Item.Title = value;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public bool AutoDownload
        {
            get { return Item.AutoDownload; }
            set
            {
                Item.AutoDownload = value;
                RaisePropertyChanged(nameof(AutoDownload));
            }
        }

        public bool Downloading
        {
            get { return _downloading; }
            set
            {
                _downloading = value;
                RaisePropertyChanged(nameof(Downloading));
            }
        }

        public DownloadState DownloadState
        {
            get { return _downloadState; }
            set
            {
                _downloadState = value;
                RaisePropertyChanged(nameof(DownloadText));

                switch (value)
                {
                    case DownloadState.Downloaded:
                        DownloadText = "Downloaded";
                        break;
                    case DownloadState.NeedsConvertion:
                        DownloadText = "Needs convertion";
                        break;
                    case DownloadState.NotDownloaded:
                        DownloadText = "Not Downloaded";
                        break;
                }
            }
        }

        public double DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                _downloadProgress = value;
                RaisePropertyChanged(nameof(DownloadProgress));
            }
        }

        public bool DownloadPending
        {
            get { return _downloadPending; }
            set
            {
                if (_downloadPending != value)
                {
                    _downloadPending = value;
                    RaisePropertyChanged(nameof(DownloadPending));
                }
            }
        }

        public string DownloadText
        {
            get { return _downloadText; }
            set
            {
                _downloadText = value;
                RaisePropertyChanged(nameof(DownloadText));
            }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                RaisePropertyChanged(nameof(Index));
            }
        }

        public RelayCommand DownloadCommand => new RelayCommand(() =>
        {
            if (Downloading)
            {
                StopDownload();
            }
            else
            {
                var handler = DownloadSong();
                _workspaceViewModel.DownloadManager.AddToQueue(handler);
            }
        });

        #endregion
    }
}