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
using System.IO;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using YTMusicDownloader.Model.Helpers;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.Properties;
using YTMusicDownloaderLib.Updater;

namespace YTMusicDownloader.ViewModel
{
    internal class UpdateViewModel
    {
        #region Fields

        private Updater _updater;
        private string _savePath;
        private ProgressDialogController _progressDialogController;
        #endregion

        #region Properties
        public Update AvailableUpdate { get; private set; }
        #endregion

        #region Construction
        public UpdateViewModel()
        {
            Init();
        }
        #endregion

        #region Methods

        private async void Init()
        {
            AvailableUpdate = await Updater.IsUpdateAvailable(new Version(Assembly.GetAssemblyVersion()), Assembly.GetAssemblyLocation());
            
            if (AvailableUpdate == null)
                return;

            Messenger.Default.Send(new ShowMessageDialogMessage(Resources.MainWindow_UpdateAvailable_Title, string.Format(Resources.MainWindow_UpdateAvailable_Description, AvailableUpdate), MessageDialogStyle.AffirmativeAndNegative,
                result =>
                {
                    if (result == MessageDialogResult.Affirmative)
                    {
                        StartDownload();
                    }
                }));
        }

        private void StartDownload()
        {
            Messenger.Default.Send(new ShowProgressDialogMessage(Resources.MainWindow_Update_UpdateProgress_Title, Resources.MainWindow_Update_UpdateProgress_Description, StartDownloadInternal, true));
        }

        private void StartDownloadInternal(ProgressDialogController progressDialogController)
        {
            _progressDialogController = progressDialogController;
            _progressDialogController.Minimum = 0;
            _progressDialogController.Maximum = 100;
            _progressDialogController.Canceled += ProgressDialogControllerOnCanceled;
            _progressDialogController.Closed += ProgressDialogControllerOnCanceled;

            _savePath = Path.GetTempFileName();
            _updater = new Updater(AvailableUpdate.GetMatchingAsset().DownloadUrl, _savePath);

            _updater.UpdateProgressChanged += (sender, args) =>
            {
                _progressDialogController.SetProgress(args.ProgressPercentage);
            };

            _updater.UpdaterDownloadCompleted += UpdaterOnUpdaterDownloadCompleted;

            _updater.StartDownload();
        }

        private void UpdaterOnUpdaterDownloadCompleted(object sender, UpdateCompletedEventArgs arsg)
        {
            throw new NotImplementedException();
        }

        private void ProgressDialogControllerOnCanceled(object sender, EventArgs eventArgs)
        {
            _updater?.CancelDownload();
        }

        #endregion
    }
}