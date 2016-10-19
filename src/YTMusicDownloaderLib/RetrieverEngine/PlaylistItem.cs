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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using NLog;
using VideoLibrary;

namespace YTMusicDownloaderLib.RetrieverEngine
{
    public class PlaylistItem
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Properties
        public string VideoId { get; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; }

        [JsonIgnore]
        public string DownloadUrl { get; private set; }

        public bool AutoDownload { get; set; }
        #endregion

        public PlaylistItem(string videoId, string title, string thumbnailUrl, bool autoDownload, string downloadUrl = "")
        {
            VideoId = videoId;
            Title = title;
            ThumbnailUrl = thumbnailUrl;
            DownloadUrl = downloadUrl;
            AutoDownload = autoDownload;
        }

        public bool RetreiveDownloadUrl()
        {
            var videos = YouTube.Default.GetAllVideos($"https://www.youtube.com/watch?v={VideoId}");
            try
            {
                var audios =
                videos.Where(v => v.AudioFormat == AudioFormat.Aac && v.AdaptiveKind == AdaptiveKind.Audio)
                    .OrderByDescending(v => v.AudioBitrate).ToList();
                if (audios.Count == 0)
                    throw new InvalidOperationException("No audio avalaible");

                DownloadUrl = audios[0].GetUriAsync().GetAwaiter().GetResult();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error downloading track {0} with id {1}: No audio available", Title, VideoId);
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            return VideoId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as PlaylistItem;
            return item != null && item.VideoId == VideoId;
        }
    }
}
