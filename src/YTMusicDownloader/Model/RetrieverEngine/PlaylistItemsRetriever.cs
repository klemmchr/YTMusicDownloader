using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;

namespace YTMusicDownloader.Model.RetrieverEngine
{
    internal class PlaylistItemsRetriever
    {
        #region Events
        public delegate void PlaylistItemsRetrieverProgressChangedEventHandler(object sender, PlaylistItemRetreiverProgressChangedEventArgs e);
        public delegate void PlaylistItemRetreiverCompletedEventHandler(object sender, PlaylistItemRetreiverCompletedEventArgs e);

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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void GetPlaylistItems(string playlistId)
        {
            if(string.IsNullOrEmpty(playlistId))
                throw new ArgumentException(nameof(playlistId));

            new Thread(() =>
            {
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
                            OnPlaylistItemsRetrieverCompleted(new PlaylistItemRetreiverCompletedEventArgs(playlistItems));
                            return;
                        }

                        var parsedRequest = JObject.Parse(response.Content);
                        pageToken = parsedRequest["nextPageToken"]?.ToString();
                        if (totalResults == -1)
                            totalResults = int.Parse(parsedRequest["pageInfo"]["totalResults"].ToString());

                        foreach (var current in parsedRequest["items"].Children().ToList())
                        {
                            try
                            {
                                var title = Regex.Replace(current["snippet"]["title"].ToString(), @"[\\/<>\|:""*?]", "");
                                var thumbnailUrl = current["snippet"]["thumbnails"]["medium"]["url"].ToString();
                                var videoId = current["snippet"]["resourceId"]["videoId"].ToString();

                                playlistItems.Add(new PlaylistItem(videoId, title, thumbnailUrl, true));

                                OnPlaylistItemsRetrieverProgressChanged(new PlaylistItemRetreiverProgressChangedEventArgs(playlistItems.Count, totalResults));
                            }
                            catch (Exception)
                            {
                                // ignore
                            }
                        }

                        if (playlistItems.Count >= Properties.Settings.Default.PlaylistReceiveMaximum) break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error retrieving playlist content for id {0}", playlistId);
                }

                OnPlaylistItemsRetrieverCompleted(new PlaylistItemRetreiverCompletedEventArgs(playlistItems));
            }).Start();
        }
    }
}
