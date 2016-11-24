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
namespace YTMusicDownloaderLib.Updater
{
    public class Asset
    {
        #region Properties
        public string Name { get; }
        public string DownloadUrl { get; }
        public Architecture Architecture { get; }
        #endregion

        #region Construction
        // ReSharper disable once InconsistentNaming
        public Asset(string name, string browser_download_url)
        {
            Name = name;
            DownloadUrl = browser_download_url;
            Architecture = name.Contains("x64") ? Architecture.x64 : Architecture.x86;
        }
        #endregion
    }
}