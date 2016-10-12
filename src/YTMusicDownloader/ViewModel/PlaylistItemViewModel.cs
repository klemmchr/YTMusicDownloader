using System;
using System.IO;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using YTMusicDownloader.Model.DownloadManager;
using YTMusicDownloader.Model.RetrieverEngine;
using YTMusicDownloader.Model.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    public class PlaylistItemViewModel : ViewModelBase
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly WorkspaceViewModel _viewModel;
        private Workspace _workspace;
        private DownloadItem _downloadItem;

        private BitmapImage _thumbnail;
        private bool _downloading;
        private bool _downloaded;
        private readonly string _filePath;
        private int _downloadProgress;
        private bool _downloadPending;
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

        public Workspace Workspace
        {
            get { return _workspace; }
            set
            {
                _workspace = value;
                RaisePropertyChanged(nameof(Workspace));
            }
        }

        public string Title
        {
            get { return Item.Title; }
            set
            {
                if (!Downloading && Title != value)
                {
                    if(Rename(value))
                        Item.Title = value;

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

        public bool Downloaded
        {
            get { return _downloaded; }
            set
            {
                _downloaded = value;
                RaisePropertyChanged(nameof(Downloaded));
                RaisePropertyChanged(nameof(DownloadText));
                RaisePropertyChanged(nameof(DownloadTextColor));
            }
        }

        public int DownloadProgress
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

        public string DownloadText => Downloaded ? "Downloaded" : "Not Downloaded";
        public string DownloadTextColor => Downloaded ? "#008A00" : "#FD0018";

        public RelayCommand DownloadCommand => new RelayCommand(() =>
        {
            if (Downloading)
            {
                StopDownload();
            }
            else
            {
                var handler = DownloadSong();
                _viewModel.DownloadManager.AddToQueue(handler);
            }
        });
        #endregion
        
        public PlaylistItemViewModel(PlaylistItem item, WorkspaceViewModel workspaceViewModel)
        {
            if (IsInDesignMode)
            {
                Downloading = true;
            }

            Item = item;
            Title = item.Title;
            Workspace = workspaceViewModel.Workspace;
            _viewModel = workspaceViewModel;

            _filePath = Path.Combine(Workspace.Path, $"{Item.Title}.m4a");
            CheckForTrack();
        }

        public void CheckForTrack()
        {
            try
            {
                Downloaded = File.Exists(_filePath);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public async void UpdateThumbnail()
        {
           Thumbnail = await Item.UpdateThumbnail();
        }

        internal DownloadItem DownloadSong(bool overwrite = true)
        {
            Downloading = true;
            DownloadProgress = 1;
            DownloadPending = true;
            _downloadItem = new DownloadItem(Item, _filePath, overwrite);
            
            if (_downloadItem == null) return null;

            _downloadItem.DownloadItemDownloadCompleted += (sender, args) =>
            {
                DownloadPending = false;
                Downloading = false;
                Downloaded = !args.Cancelled || Downloaded;
                _downloadItem = null;
            };

            _downloadItem.DownloadItemDownloadProgressChanged += (sender, args) =>
            {
                DownloadPending = false;
                DownloadProgress = args.ProgressPercentage;
            };

            return _downloadItem;
        }

        private bool Rename(string newTitle)
        {
            try
            {
                File.Move(Path.Combine(Workspace.Path, Item.Title + ".m4a"), Path.Combine(Workspace.Path, newTitle + ".m4a"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
    }
}
