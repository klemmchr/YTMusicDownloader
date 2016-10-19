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
using System.ComponentModel;
using System.Windows;
using YTMusicDownloader.Properties;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.FirstStartup)
                CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;
            Left = screenWidth/2 - windowWidth/2;
            Top = screenHeight/2 - windowHeight/2;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WorkspaceManagement.SaveWorkspaces();
            base.OnClosing(e);
            Environment.Exit(0);
        }
    }
}