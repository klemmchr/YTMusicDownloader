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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Octokit;
using YTMusicDownloaderLib.Properties;

namespace YTMusicDownloaderLib.Updater
{
    public class Updater
    {
        #region Construction

        public Updater(string downloadUrl, string targetFilePath)
        {
            if (string.IsNullOrEmpty(downloadUrl))
                throw new ArgumentException(nameof(downloadUrl));

            DownloadUrl = downloadUrl;

            SavePath = targetFilePath;
            _targetFileStream = File.Create(targetFilePath);
        }

        #endregion

        #region Events

        public delegate void UpdateCompletedEventHandler(object sender, UpdateCompletedEventArgs arsg);

        public event UpdateCompletedEventHandler UpdaterDownloadCompleted;

        public void OnUpdateCompleted(UpdateCompletedEventArgs args)
        {
            UpdaterDownloadCompleted?.Invoke(this, args);

            if ((_targetFileStream != null) && _targetFileStream.CanRead)
            {
                _targetFileStream.Close();
                _targetFileStream.Dispose();
            }
        }

        public delegate void UpdateProgressChangedEventHandler(object sender, UpdateProgressChangedEventArgs args);

        public event UpdateProgressChangedEventHandler UpdateProgressChanged;

        public void OnUpdateProgressChanged(UpdateProgressChangedEventArgs args)
        {
            UpdateProgressChanged?.Invoke(this, args);
        }

        #endregion

        #region Fields        

        private readonly FileStream _targetFileStream;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        public string DownloadUrl { get; }
        public string SavePath { get; }

        #endregion

        #region Methods

        public void StartDownload()
        {
            using (var client = new WebClient())
            {
                client.OpenReadCompleted += ClientOnOpenReadCompleted;
                client.OpenReadAsync(new Uri(DownloadUrl));
            }
        }

        public void CancelDownload()
        {
            OnUpdateCompleted(new UpdateCompletedEventArgs(true));
        }

        private void ClientOnOpenReadCompleted(object sender, OpenReadCompletedEventArgs openReadCompletedEventArgs)
        {
            if (openReadCompletedEventArgs.Cancelled || (openReadCompletedEventArgs.Error != null))
                OnUpdateCompleted(new UpdateCompletedEventArgs(true, openReadCompletedEventArgs.Error));

            try
            {
                var totalLength = 0;
                var processed = 0;
                try
                {
                    totalLength = int.Parse(((WebClient) sender).ResponseHeaders["Content-Length"]);
                }
                catch (Exception)
                {
                    // ignored
                }

                var buffer = new byte[16384];
                int read;
                while ((read = openReadCompletedEventArgs.Result.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _targetFileStream.Write(buffer, 0, read);
                    processed += read;
                    OnUpdateProgressChanged(new UpdateProgressChangedEventArgs(processed, totalLength));
                }

                OnUpdateCompleted(new UpdateCompletedEventArgs(false));
            }
            catch (Exception ex)
            {
                OnUpdateCompleted(new UpdateCompletedEventArgs(true, ex));
            }
            finally
            {
                openReadCompletedEventArgs.Result.Close();
                openReadCompletedEventArgs.Result.Dispose();
            }
        }

        public void StartUpdater(string updaterPath, string targetDirectory, string zipPath, string appPath)
        {
            try
            {
                var tempUpdaterPath = Path.GetTempFileName().Replace("tmp", "exe");
                File.Copy(updaterPath, tempUpdaterPath, true);
                Process.Start(tempUpdaterPath, $"\"{targetDirectory}\" \"{zipPath}\" \"{appPath}\"");

                Thread.Sleep(10);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to start updater {0}", updaterPath);
            }
        }

        #endregion

        #region StaticMethods

        public static async Task<Update> IsUpdateAvailable(Version assemblyVersion, string assemblyPath)
        {
            return await Task.Run(async () =>
            {
                var client = new GitHubClient(new ProductHeaderValue(Settings.Default.GitHubRepositoryName));
                var release =
                (await
                    client.Repository.Release.GetAll(Settings.Default.GitHubRepositoryOwner,
                        Settings.Default.GitHubRepositoryName))[0];
                var match = Regex.Match(release.TagName, @"(\d+\.\d+\.\d+(\.\d+)?)");
                if (!match.Success)
                    return null;

                var version = match.Groups[0].ToString();
                var updateVersion = new Version(version);

                var update = new Update(updateVersion, GetAssets(release.AssetsUrl), assemblyPath);
                return updateVersion.CompareTo(assemblyVersion) > 0 ? update : null;
            });
        }

        public static void StartUpdate(Update update, string updaterAssemblyPath)
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            try
            {
                var path = Path.GetTempFileName();
                File.Create(path).Close();
                File.AppendAllText(path, JsonConvert.SerializeObject(update));

                Process.Start(updaterAssemblyPath, path);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error starting updater");
            }
        }

        private static List<Asset> GetAssets(string dataUrl)
        {
            var result = new List<Asset>();

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent,
                        "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    var data = client.DownloadString(dataUrl);
                    var json = JArray.Parse(data);

                    result.AddRange(
                        json.Select(
                            current => new Asset(current["name"].ToString(), current["browser_download_url"].ToString())));

                    return result;
                }
            }
            catch (Exception)
            {
                return result;
            }
        }

        #endregion
    }
}