using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace YTMusicDownloader.ViewModel
{
    public class PageSelectorViewModel: ViewModelBase
    {
        #region Fields
        private readonly WorkspaceViewModel _workspaceViewModel;

        private int _pageNumber;
        private int _pageNumberMax;
        #endregion

        #region Properties
        public ObservableCollection<int> ItemsPerPageOptions { get; }
        
        public int ItemsPerPage
        {
            get { return _workspaceViewModel.Workspace.Settings.ItemsPerPage; }
            set
            {
                _workspaceViewModel.Workspace.Settings.ItemsPerPage = value;
                RaisePropertyChanged(nameof(ItemsPerPage));

                if (!IsInDesignMode)
                {
                    _workspaceViewModel.OnPageNumberChanged();

                    UpdatePageview(_workspaceViewModel.SearchActive
                        ? _workspaceViewModel.SearchResult.Count
                        : _workspaceViewModel.Tracks.Count);
                }
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = value;
                RaisePropertyChanged(nameof(PageNumber));
                RaisePropertyChanged(nameof(PageBackwardEnabled));
                RaisePropertyChanged(nameof(PageForwardEnabled));
            }
        }

        public int PageNumberMax
        {
            get { return _pageNumberMax; }
            set
            {
                _pageNumberMax = value;
                RaisePropertyChanged(nameof(PageNumberMax));
                RaisePropertyChanged(nameof(PageBackwardEnabled));
                RaisePropertyChanged(nameof(PageForwardEnabled));
            }
        }

        public bool PageBackwardEnabled => PageNumber > 1;
        public bool PageForwardEnabled => PageNumber != PageNumberMax;

        #region Commands
        public RelayCommand FirstPageCommand => new RelayCommand(() => { PageNumber = 1; _workspaceViewModel.OnPageNumberChanged(); });
        public RelayCommand LastPageCommand => new RelayCommand(() => { PageNumber = PageNumberMax; _workspaceViewModel.OnPageNumberChanged(); });
        public RelayCommand PageForwardCommand => new RelayCommand(() => { PageNumber = Math.Min(++PageNumber, PageNumberMax); _workspaceViewModel.OnPageNumberChanged(); });
        public RelayCommand PageBackwardCommand => new RelayCommand(() => { PageNumber = Math.Max(--PageNumber, 1); _workspaceViewModel.OnPageNumberChanged(); });
        #endregion
        #endregion

        #region Construction

        public PageSelectorViewModel(WorkspaceViewModel workspaceViewModel)
        {
            _workspaceViewModel = workspaceViewModel;

            ItemsPerPageOptions = new ObservableCollection<int>
            {
                10,
                20,
                30,
                40,
                50
            };

            PageNumber = 1;
            PageNumberMax = 1;
        }
        #endregion

        #region Methods
        public void UpdatePageview(int trackCount)
        {
            PageNumberMax = Math.Max(1, (int)Math.Ceiling((trackCount * 1.0) / (ItemsPerPage * 1.0)));
            PageNumber = Math.Min(PageNumberMax, PageNumber);
        }
        #endregion
    }
}
