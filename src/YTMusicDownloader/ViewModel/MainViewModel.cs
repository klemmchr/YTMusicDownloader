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

using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using YTMusicDownloader.Properties;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.Workspaces;
using System.Diagnostics;

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
        private bool _isLoaded;
        private string _selectedWorkspaceName;
        private bool _selectedWorkspaceVisible;
        private int _selectedWorkspaceIndex = -1;

        #endregion

        #region Properties

        public static bool IsReleaseVersion { get; }

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

                if (value == 2)
                    SelectedWorkspace?.WorkspaceSettingsViewModel.SettingsPageSelected();
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
            private set
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
        public RelayCommand RemoveWorkspaceCommand => new RelayCommand(RemoveWorkspace);

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

            Messenger.Default.Register<WorkspaceErrorMessage>(this, message =>
            {
                if (SelectedWorkspace.Workspace.Equals(message.Workspace))
                    SelectedTabIndex = 0;

                SelectedWorkspace = null;
            });

            Messenger.Default.Register<SelectWorkspaceMessage>(this, message =>
            {
                if (message.WorkspaceViewModel != null)
                {
                    SelectWorkspace(message.WorkspaceViewModel);
                }
            });

            Startup();

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

        public void Startup()
        {
            Task.Run(async () =>
            {
                while (_dialogCoordinator == null)
                    await Task.Delay(50);

                IsLoaded = true;

                /*
                if (!Settings.Default.FirstStartup) return;

                var result = await _dialogCoordinator.ShowMessageAsync(this, "YouTube Music Downloader",
                    "Thank you for using the YouTube Music Downloader!\n\nThis tool will help you keeping your playlists in sync and makes it easy to download your favourite YouTube Playlist all at once.\nDo you want to start a small tour to get started?",
                    MessageDialogStyle.AffirmativeAndNegative);

                Settings.Default.FirstStartup = false;
                if (result == MessageDialogResult.Negative) return;

                SelectedTabIndex = 2;
                await _dialogCoordinator.ShowMessageAsync(this, "Getting started tour",
                    "This is the settings page. Here you can configure the YT Music Downloader to fit your needs.\n\nOn the left side you can see the general settings. If you load your first workspace you can see the workspace settings on the right side.");

                SelectedTabIndex = 0;
                await _dialogCoordinator.ShowMessageAsync(this, "Getting started tour",
                    "This is the Workspace page.\nHere you can create a workspace for every music library you want to create. The workpace is the centralized place where all your downloaded music will be stored.");

                await _dialogCoordinator.ShowMessageAsync(this, "Getting started tour",
                    "This was the small tour to get you started.\n\nHave fun using the YouTube Music Downloader!");
                    */
            });
        }

        private async void SelectWorkspace(WorkspaceViewModel workspaceViewModel)
        {
            await Task.Run(() =>
            {
                SelectedWorkspace = workspaceViewModel;
                if (!Directory.Exists(SelectedWorkspace.Workspace.Path))
                {
                    Messenger.Default.Send(new ShowMessageDialogMessage(Resources.MainWindow_Workspaces_WorkspaceNotAvailable_Title,
                        Resources.MainWindow_Workspaces_WorkspaceNotAvailable_Content));
                    return;
                }

                SelectedTabIndex = 1;
                SelectedWorkspace.Load();

                Logger.Debug("Selected workspace {0}", SelectedWorkspace.Workspace.Name);
            });
        }

        private void RemoveWorkspace()
        {
            if ((SelectedWorkspaceIndex > Workspaces.Count - 1) || (SelectedWorkspaceIndex < 0))
                return;

            if (Workspaces[SelectedWorkspaceIndex] == null)
                return;

            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = Resources.Yes,
                NegativeButtonText = Resources.No,
                FirstAuxiliaryButtonText = Resources.Cancel
            };

            Messenger.Default.Send(
                new ShowMessageDialogMessage(string.Format(Resources.MainViewModel_RemoveWorkspace_Title, Workspaces[SelectedWorkspaceIndex].Name),
                    Resources.MainViewModel_RemoveWorkspace_Description,
                    MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                    result =>
                    {
                        if (result == MessageDialogResult.FirstAuxiliary)
                            return;

                        var deleteMode = result == MessageDialogResult.Affirmative
                            ? DeleteMode.DeleteWorkspace
                            : DeleteMode.KeepWorkspace;

                        if ((SelectedWorkspace != null) &&
                            SelectedWorkspace.Workspace.Equals(Workspaces[SelectedWorkspaceIndex].Workspace))
                            SelectedWorkspace = null;

                        var workspace = Workspaces[SelectedWorkspaceIndex].Workspace;

                        WorkspaceManagement.RemoveWorkspace(workspace, deleteMode);
                        Workspaces.RemoveAt(SelectedWorkspaceIndex);

                        Logger.Debug("Removed workspace {0} - deleteMode: {1}", workspace.Name, deleteMode);
                    }, dialogSettings));
        }

        #endregion
    }
}