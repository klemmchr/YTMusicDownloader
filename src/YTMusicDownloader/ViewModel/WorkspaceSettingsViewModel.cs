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
using MahApps.Metro.IconPacks;
using YTMusicDownloader.Properties;
using YTMusicDownloaderLib.DownloadManager;

namespace YTMusicDownloader.ViewModel
{
    internal class WorkspaceSettingsViewModel
    {
        #region Fields 

        private readonly WorkspaceViewModel _workspaceViewModel;

        #endregion

        #region Construction

        public WorkspaceSettingsViewModel(WorkspaceViewModel workspaceViewModel)
        {
            _workspaceViewModel = workspaceViewModel;

            Settings = new ObservableCollection<SettingViewModel>();
            SetupSettings();
        }

        #endregion

        #region Properties

        public ObservableCollection<SettingViewModel> Settings { get; }

        #endregion

        #region Methods

        private void SetupSettings()
        {
            var workspaceSettings = _workspaceViewModel.Workspace.Settings;

            Settings.Add(new SettingViewModel(workspaceSettings, nameof(workspaceSettings.AutoSync),
                Resources.MainWindow_Settings_Workspace_AutoSyncOnStartup_Title,
                Resources.MainWindow_Settings_Workspace_AutoSyncOnStartup_Description, PackIconMaterialKind.Cached, true));
            Settings.Add(new SettingViewModel(workspaceSettings, nameof(workspaceSettings.DeleteNotSyncedItems),
                Resources.MainWindow_Settings_Workspace_Cleanup_Title,
                Resources.MainWindow_Settings_Workspace_Cleanup_Description, PackIconMaterialKind.Broom, false));
            Settings.Add(new SettingViewModel(workspaceSettings, nameof(workspaceSettings.DownloadFormat),
                Resources.MainWindow_Settings_Workspace_DownloadFormat_Title,
                Resources.MainWindow_Settings_Workspace_DownloadFormat_Description, PackIconMaterialKind.FileMultiple,
                DownloadFormat.MP3));
        }

        #endregion
    }
}