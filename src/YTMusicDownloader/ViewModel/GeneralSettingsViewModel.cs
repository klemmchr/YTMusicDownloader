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
using System.Linq;
using GalaSoft.MvvmLight;
using YTMusicDownloader.Properties;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    internal class GeneralSettingsViewModel : ViewModelBase
    {
        /*
        public string ParallelDownloads
        {
            get { return Settings.Default.ParallelDownloads.ToString(); }
            set
            {
                int parsed;
                if (int.TryParse(value, out parsed))
                    Settings.Default.ParallelDownloads = Math.Min(20, parsed);

                RaisePropertyChanged(nameof(ParallelDownloads));
            }
        }

        public string PlaylistReceiveMaximum
        {
            get { return Settings.Default.PlaylistReceiveMaximum.ToString(); }
            set
            {
                int parsed;
                if (int.TryParse(value, out parsed))
                    Settings.Default.PlaylistReceiveMaximum = Math.Min(15000, parsed);

                RaisePropertyChanged(nameof(PlaylistReceiveMaximum));
            }
        }

        public bool ShowAdvancedSettings
        {
            get { return Settings.Default.ShowAdvancedSettings; }
            set
            {
                Settings.Default.ShowAdvancedSettings = value;
                RaisePropertyChanged(nameof(ShowAdvancedSettings));
            }
        }
        */

        public ObservableCollection<SettingViewModel> Settings { get; }

        public GeneralSettingsViewModel()
        {
            Settings = new ObservableCollection<SettingViewModel>();
            SetupSettings();
        }

        private void SetupSettings()
        {
            var applicationSettings = Properties.Settings.Default;

            Settings.Add(new SettingViewModel(applicationSettings, nameof(applicationSettings.ParallelDownloads), Resources.MainWindow_Settings_General_ParallelDownloads_Title, Resources.MainWindow_Settings_General_ParallelDownloads_Description, 2, "Download", 0, 10));
            Settings.Add(new SettingViewModel(applicationSettings, nameof(applicationSettings.PlaylistReceiveMaximum), Resources.MainWindow_Settings_General_MaximumPlaylistItems_Title, Resources.MainWindow_Settings_General_MaximumPlaylistItems_Description, 5000, "PlaylistPlay", 0, 10000));
        }
    }
}