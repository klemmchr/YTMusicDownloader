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
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using YTMusicDownloader.Model.Helpers;
using YTMusicDownloader.Properties;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.Helpers;
using YTMusicDownloaderLib.Properties;
using YTMusicDownloaderLib.Updater;
using YTMusicDownloaderLib.Workspaces;
#if DEBUG
using System.Diagnostics;

#endif

namespace YTMusicDownloader.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDialogCoordinator _dialogCoordinator;

        private WorkspaceViewModel _selectedWorkspace;
        private int _selectedTabIndex;
        private bool _isAddingWorkspace;
        private string _selectedWorkspaceName;
        private bool _selectedWorkspaceVisible;
        private int _selectedWorkspaceIndex = -1;
        private bool _isLoaded;

        #endregion

        #region Properties

        public static bool IsReleaseVersion { get; }
        public UpdateViewModel UpdateViewModel { get; private set; }

        public WorkspaceViewModel SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                _selectedWorkspace = value;
                SelectedWorkspaceName = _selectedWorkspace?.Workspace.ToString();
                SelectedWorkspaceVisible = _selectedWorkspace != null;

                RaisePropertyChanged(nameof(SelectedWorkspace));
            }
        }

        public string SelectedWorkspaceName
        {
            get { return _selectedWorkspaceName; }
            set
            {
                _selectedWorkspaceName = value;
                RaisePropertyChanged(nameof(SelectedWorkspaceName));
            }
        }

        public bool SelectedWorkspaceVisible
        {
            get { return _selectedWorkspaceVisible; }
            set
            {
                _selectedWorkspaceVisible = value;
                RaisePropertyChanged(nameof(SelectedWorkspaceVisible));
            }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                RaisePropertyChanged(nameof(SelectedTabIndex));
            }
        }

        public bool IsAddingWorkspace
        {
            get { return _isAddingWorkspace; }
            set
            {
                _isAddingWorkspace = value;
                RaisePropertyChanged(nameof(IsAddingWorkspace));
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                RaisePropertyChanged(nameof(IsLoaded));
            }
        }

        public int SelectedWorkspaceIndex
        {
            get { return _selectedWorkspaceIndex; }
            set
            {
                _selectedWorkspaceIndex = value;
                RaisePropertyChanged(nameof(SelectedWorkspaceIndex));
            }
        }

        public ObservableCollection<WorkspaceViewModel> Workspaces { get; }

        public RelayCommand AddWorkspaceCommand => new RelayCommand(() => IsAddingWorkspace = true);

        #endregion

        #region Construction

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            Workspaces = new ObservableCollection<WorkspaceViewModel>();

            if (IsInDesignMode)
            {
                SelectedWorkspaceVisible = true;
                SelectedWorkspaceName = "Design Mode";
                return;
            }

            LoadWorkspaces();

            Messenger.Default.Register<ShowMessageDialogMessage>(this,
                async message =>
                {
                    var result =
                        await
                            _dialogCoordinator.ShowMessageAsync(this, message.Title, message.Content, message.Style,
                                message.Settings);
                    message.Callback?.Invoke(result);
                });

            Messenger.Default.Register<ShowProgressDialogMessage>(this, async message =>
            {
                var controller =
                    await
                        _dialogCoordinator.ShowProgressAsync(this, message.Title, message.Description,
                            message.IsCancelable, message.MetroDialogSettings);

                message.Callback?.Invoke(controller);
            });

            Messenger.Default.Register<WorkspaceErrorMessage>(this, message =>
            {
                if (SelectedWorkspace.Workspace.Equals(message.Workspace))
                    SelectedTabIndex = 0;

                SelectedWorkspace = null;
            });

            Messenger.Default.Register<SelectWorkspaceMessage>(this, message =>
            {
                if (message.WorkspaceViewModel != null)
                    SelectWorkspace(message.WorkspaceViewModel);
            });

            Messenger.Default.Register<RemoveWorkspaceMessage>(this, message =>
            {
                if (message.WorkspaceViewModel != null)
                    RemoveWorkspace(message.WorkspaceViewModel);
            });

            Logger.Trace("Initialized Main View Model");
        }

        static MainViewModel()
        {
#if DEBUG
            IsReleaseVersion = true;
#endif
        }

        #endregion

        #region Methods

        public async void LoadWorkspaces()
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            foreach (var workspace in WorkspaceManagement.Workspaces)
                Workspaces.Add(new WorkspaceViewModel(workspace));

            Messenger.Default.Register<CloseAddWorkspaceFlyoutMessage>(this, message => IsAddingWorkspace = false);
            Messenger.Default.Register<AddWorkspaceMessage>(this, message =>
            {
                if (message.Workspace != null)
                    Workspaces.Add(new WorkspaceViewModel(message.Workspace));
            });

            foreach (var workspace in Workspaces)
                await workspace.Init();

            
#if DEBUG
            Logger.Trace("Loaded all workspaces: {0} ms", watch.ElapsedMilliseconds);
#else
            Logger.Debug("Loaded all workspaces");
#endif
        }

        public async void Startup()
        {
            IsLoaded = true;

            await Task.Delay(2000);

            UpdateViewModel = new UpdateViewModel();
        }

        private async void SelectWorkspace(WorkspaceViewModel workspaceViewModel)
        {
            await Task.Run(() =>
            {
                SelectedWorkspace = workspaceViewModel;
                if (!Directory.Exists(SelectedWorkspace.Workspace.Path))
                {
                    Messenger.Default.Send(
                        new ShowMessageDialogMessage(Resources.MainWindow_Workspaces_WorkspaceNotAvailable_Title,
                            Resources.MainWindow_Workspaces_WorkspaceNotAvailable_Content));
                    return;
                }

                SelectedTabIndex = 1;
                SelectedWorkspace.Load();

                Logger.Debug("Selected workspace {0}", SelectedWorkspace.Workspace.Name);
            });
        }

        private void RemoveWorkspace(WorkspaceViewModel workspaceViewModel)
        {
            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = Resources.Yes,
                NegativeButtonText = Resources.No,
                FirstAuxiliaryButtonText = Resources.Cancel
            };

            Messenger.Default.Send(
                new ShowMessageDialogMessage(
                    string.Format(Resources.MainViewModel_RemoveWorkspace_Title, workspaceViewModel.Name),
                    Resources.MainViewModel_RemoveWorkspace_Description,
                    MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                    result =>
                    {
                        if (result == MessageDialogResult.FirstAuxiliary)
                            return;

                        var deleteMode = result == MessageDialogResult.Affirmative
                            ? DeleteMode.DeleteWorkspace
                            : DeleteMode.KeepWorkspace;

                        var workspace = workspaceViewModel.Workspace;

                        if ((SelectedWorkspace != null) &&
                            SelectedWorkspace.Workspace.Equals(workspace))
                            SelectedWorkspace = null;

                        WorkspaceManagement.RemoveWorkspace(workspace, deleteMode);
                        Workspaces.Remove(workspaceViewModel);

                        Logger.Debug("Removed workspace {0} - deleteMode: {1}", workspace.Name, deleteMode);
                    }, dialogSettings));
        }

        #endregion
    }
}