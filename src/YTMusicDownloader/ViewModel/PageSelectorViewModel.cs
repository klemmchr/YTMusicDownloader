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
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace YTMusicDownloader.ViewModel
{
    public class PageSelectorViewModel : ViewModelBase
    {
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

        public void UpdatePageview()
        {
            PageNumberMax = Math.Max(1,
                (int) Math.Ceiling(_workspaceViewModel.DisplayedTracksSource.Count*1.0/(ItemsPerPage*1.0)));
            PageNumber = Math.Min(PageNumberMax, PageNumber);
        }

        #endregion

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

                    UpdatePageview();
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

        public RelayCommand FirstPageCommand => new RelayCommand(() =>
        {
            PageNumber = 1;
            _workspaceViewModel.OnPageNumberChanged();
        });

        public RelayCommand LastPageCommand => new RelayCommand(() =>
        {
            PageNumber = PageNumberMax;
            _workspaceViewModel.OnPageNumberChanged();
        });

        public RelayCommand PageForwardCommand => new RelayCommand(() =>
        {
            PageNumber = Math.Min(++PageNumber, PageNumberMax);
            _workspaceViewModel.OnPageNumberChanged();
        });

        public RelayCommand PageBackwardCommand => new RelayCommand(() =>
        {
            PageNumber = Math.Max(--PageNumber, 1);
            _workspaceViewModel.OnPageNumberChanged();
        });

        #endregion

        #endregion
    }
}