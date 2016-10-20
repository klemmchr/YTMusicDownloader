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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using YTMusicDownloader.Properties;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            Workspaces = new ObservableCollection<WorkspaceViewModel>();

            if (IsInDesignMode)
            {
                IsWorkspaceSelected = true;
                return;
            }

            LoadWorkspaces();

            Messenger.Default.Register<ShowMessageDialogMessage>(this,
                async message =>
                {
                    var result = await _dialogCoordinator.ShowMessageAsync(this, message.Title, message.Content, message.Style, message.Settings);
                    message.Callback?.Invoke(result);
                });
            FirstStartup();

            Logger.Trace("Initialized Main View Model");
        }

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDialogCoordinator _dialogCoordinator;

        private WorkspaceViewModel _selectedWorkspace;
        private int _selectedTabIndex;
        private bool _isAddingWorkspace;
        #endregion

        #region Properties

        public WorkspaceViewModel SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                _selectedWorkspace = value;
                IsWorkspaceSelected = true;

                RaisePropertyChanged(nameof(SelectedWorkspace));
                RaisePropertyChanged(nameof(SelectedWorkspaceName));
                RaisePropertyChanged(nameof(SelectedWorkspaceVisible));
                RaisePropertyChanged(nameof(IsWorkspaceSelected));
            }
        }

        public string SelectedWorkspaceName => _selectedWorkspace?.Workspace.ToString();
        public bool SelectedWorkspaceVisible => _selectedWorkspace != null;

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

        public int SelectedWorkspaceIndex { get; set; }
        public bool IsWorkspaceSelected { get; private set; }

        public ObservableCollection<WorkspaceViewModel> Workspaces { get; }

        public RelayCommand AddWorkspaceCommand => new RelayCommand(() => IsAddingWorkspace = true);
        public RelayCommand SelectWorkspaceCommand => new RelayCommand(SelectWorkspace, () => SelectedWorkspaceIndex != -1);
        public RelayCommand RemoveWorkspaceCommand => new RelayCommand(RemoveWorkspace, () => SelectedWorkspaceIndex != -1);
        #endregion

        #region Methods

        public void LoadWorkspaces()
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            foreach (var workspace in WorkspaceManagement.Workspaces)
                Workspaces.Add(new WorkspaceViewModel(workspace));

            Messenger.Default.Register<CloseAddWorkspaceFlyoutMessage>(this, (message) => IsAddingWorkspace = false);
            Messenger.Default.Register<AddWorkspaceMessage>(this, (message) =>
            {
                if (message.Workspace != null)
                    Workspaces.Add(new WorkspaceViewModel(message.Workspace));
            });
#if DEBUG
            Logger.Trace("Loaded all workspaces: {0} ms", watch.ElapsedMilliseconds);
#else
            Logger.Debug("Loaded all workspaces");
#endif
        }

        public void FirstStartup()
        {
            if (!Settings.Default.FirstStartup) return;

            Task.Run(async () =>
            {
                while (_dialogCoordinator == null)
                    Thread.Sleep(50);

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
            });
        }

        private void SelectWorkspace()
        {
            if (SelectedWorkspaceIndex > Workspaces.Count - 1)
                return;

            SelectedWorkspace = Workspaces[SelectedWorkspaceIndex];
            SelectedWorkspace.Init();

            SelectedTabIndex = 1;

            Logger.Debug("Selected workspace {0}", SelectedWorkspace.Workspace.Name);
        }

        private void RemoveWorkspace()
        {
            if ((SelectedWorkspaceIndex > Workspaces.Count - 1) || (SelectedWorkspaceIndex < 0))
                return;

            if (Workspaces[SelectedWorkspaceIndex] == null)
                return;

            var dialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                FirstAuxiliaryButtonText = "Cancel"
            };

            Messenger.Default.Send(new ShowMessageDialogMessage($"Delete workspace {Workspaces[SelectedWorkspaceIndex].Name}", Resources.MainViewModel_RemoveWorkspace_Description, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                result =>
                {
                    if (result == MessageDialogResult.FirstAuxiliary)
                        return;

                    var deleteMode = result == MessageDialogResult.Affirmative
                        ? DeleteMode.DeleteWorkspace
                        : DeleteMode.KeepWorkspace;

                    if ((SelectedWorkspace != null) && SelectedWorkspace.Workspace.Equals(Workspaces[SelectedWorkspaceIndex].Workspace))
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