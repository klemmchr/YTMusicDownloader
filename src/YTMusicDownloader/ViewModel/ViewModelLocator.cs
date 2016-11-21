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

/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:YTMusicDownloader"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Workspace=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Microsoft.Practices.ServiceLocation;
using NLog;
using YTMusicDownloaderLib.RetrieverEngine;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator : ViewModelBase
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<GeneralSettingsViewModel>();
            SimpleIoc.Default.Register<AddWorkspaceViewModel>();
            SimpleIoc.Default.Register<AboutTabViewModel>();
            

            LogManager.GetCurrentClassLogger().Trace("Registered all view models in view model locator");
#if DEBUG
            if (IsInDesignModeStatic)
            {
                DesignWorkspace = new WorkspaceViewModel(new Workspace(@"D:\Downloads"));
                Workspaces = new List<WorkspaceViewModel> {DesignWorkspace};

                DesignPlaylistItem = new PlaylistItemViewModel(
                        new PlaylistItem("6SDloNzDrFg", "Avae - Daydream (feat. Paniz)",
                            "https://i.ytimg.com/vi/6SDloNzDrFg/mqdefault.jpg", true), DesignWorkspace);

                DesignSetting = new SettingViewModel(Properties.Settings.Default, "ParallelDownloads", "Test setting", 
                    "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et", 
                    PackIconMaterialKind.Account, 10, 1, 10);
                DesignSettings = new List<SettingViewModel> {DesignSetting};
            }
#endif
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public GeneralSettingsViewModel GeneralSettings => ServiceLocator.Current.GetInstance<GeneralSettingsViewModel>();
        public AddWorkspaceViewModel AddWorkspace => ServiceLocator.Current.GetInstance<AddWorkspaceViewModel>();
        public AboutTabViewModel AboutTab => ServiceLocator.Current.GetInstance<AboutTabViewModel>();
        public PlaylistItemViewModel DesignPlaylistItem { get; }
        public WorkspaceViewModel DesignWorkspace { get; }
        /// <summary>
        /// Gets the workspaces for design mode.
        /// </summary>
        /// <value>
        /// The workspaces.
        /// </value>
        public List<WorkspaceViewModel> Workspaces { get; }
        public SettingViewModel DesignSetting { get; }
        public List<SettingViewModel> DesignSettings { get; }
    }
}