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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;

namespace YTMusicDownloaderLib.RetrieverEngine
{
    public class PlaylistItemsRetriever
    {
        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        public PlaylistItemsRetriever(int playlistReceiveMaximum)
        {
            PlaylistReceiveMaximum = playlistReceiveMaximum;
        }

        #region Properties

        public int PlaylistReceiveMaximum { get; set; }

        #endregion

        public void GetPlaylistItems(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                throw new ArgumentException(nameof(playlistId));

            var playlistItems = new List<PlaylistItem>();
            var client = new RestClient("http://ytdownloaderapi.azurewebsites.net");
            var request = new RestRequest("api/PlaylistData", Method.GET);

            var pageToken = "";
            var totalResults = -1;

            try
            {
                while (pageToken != null)
                {
                    request.Parameters.Clear();
                    request.AddParameter("playlistId", playlistId);
                    request.AddParameter("pageToken", pageToken);

                    var response = client.Execute(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        OnPlaylistItemsRetrieverCompleted(new PlaylistItemRetreiverCompletedEventArgs(true,
                            playlistItems));
                        return;
                    }

                    var parsedRequest = JObject.Parse(response.Content);
                    pageToken = parsedRequest["nextPageToken"]?.ToString();
                    if (totalResults == -1)
                        totalResults = int.Parse(parsedRequest["pageInfo"]["totalResults"].ToString());

                    foreach (var current in parsedRequest["items"].Children().ToList())
                        try
                        {
                            var title = Regex.Replace(current["snippet"]["title"].ToString(), @"[\\/<>\|:""*?]", "");
                            var thumbnailUrl = current["snippet"]["thumbnails"]["medium"]["url"].ToString();
                            var videoId = current["snippet"]["resourceId"]["videoId"].ToString();

                            playlistItems.Add(new PlaylistItem(videoId, title, thumbnailUrl, true));

                            OnPlaylistItemsRetrieverProgressChanged(
                                new PlaylistItemRetreiverProgressChangedEventArgs(playlistItems.Count, totalResults));
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                    if (playlistItems.Count >= PlaylistReceiveMaximum) break;
                }

                OnPlaylistItemsRetrieverCompleted(new PlaylistItemRetreiverCompletedEventArgs(false, playlistItems));
                return;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error retrieving playlist content for id {0}", playlistId);
            }

            OnPlaylistItemsRetrieverCompleted(new PlaylistItemRetreiverCompletedEventArgs(true, playlistItems));
        }

        #region Events

        public delegate void PlaylistItemsRetrieverProgressChangedEventHandler(
            object sender, PlaylistItemRetreiverProgressChangedEventArgs e);

        public delegate void PlaylistItemRetreiverCompletedEventHandler(
            object sender, PlaylistItemRetreiverCompletedEventArgs e);

        public event PlaylistItemsRetrieverProgressChangedEventHandler PlaylistItemsRetrieverProgressChanged;
        public event PlaylistItemRetreiverCompletedEventHandler PlaylistItemsRetrieverCompleted;

        protected virtual void OnPlaylistItemsRetrieverProgressChanged(PlaylistItemRetreiverProgressChangedEventArgs e)
        {
            PlaylistItemsRetrieverProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnPlaylistItemsRetrieverCompleted(PlaylistItemRetreiverCompletedEventArgs e)
        {
            PlaylistItemsRetrieverCompleted?.Invoke(this, e);
        }

        #endregion
    }
}