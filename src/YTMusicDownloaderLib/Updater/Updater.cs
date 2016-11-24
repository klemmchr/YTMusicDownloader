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
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using YTMusicDownloaderLib.Properties;

namespace YTMusicDownloaderLib.Updater
{
    public class Updater
    {
        #region Fields        
        #endregion

        #region Properties
        #endregion

        #region Construction
        public Updater()
        {

        }
        #endregion

        #region Methods
        public static async Task<Update> IsUpdateAvailable(Version assemblyVersion)
        {
            return await Task.Run(async () =>
            {
                var client = new GitHubClient(new ProductHeaderValue(Settings.Default.GitHubRepositoryName));
                var release = (await client.Repository.Release.GetAll(Settings.Default.GitHubRepositoryOwner, Settings.Default.GitHubRepositoryName))[0];
                var match = Regex.Match(release.TagName, @"(\d+\.\d+\.\d+(\.\d+)?)");
                if (!match.Success)
                    return null;

                var version = match.Groups[0].ToString();
                var updateVersion = new Version(version);

                var update = new Update(updateVersion, release.AssetsUrl, GetAssets(release.AssetsUrl));
                return updateVersion.CompareTo(assemblyVersion) > 0 ? update : null;
            });
        }

        private static List<Asset> GetAssets(string dataUrl)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    return JsonConvert.DeserializeObject<List<Asset>>(client.DownloadString(dataUrl));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
    }
}