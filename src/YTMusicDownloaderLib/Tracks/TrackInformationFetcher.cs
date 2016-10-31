using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YTMusicDownloaderLib.RetrieverEngine;
using YTMusicDownloaderLibShared.Tracks;
using NLog;
using RestSharp.Extensions.MonoHttp;

namespace YTMusicDownloaderLib.Tracks
{
    public static class TrackInformationFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static TrackInformation GetTrackInformation(PlaylistItem item)
        {
            var information = new TrackInformation();

            try
            {
                var client = new RestClient("http://ytdownloaderapi.azurewebsites.net");
                var request = new RestRequest("/api/trackInfo");
                request.AddParameter("name", item.Title);

                var result = client.Execute(request);
                if (result.StatusCode != HttpStatusCode.OK)
                    return information;

                information = JsonConvert.DeserializeObject<TrackInformation>(result.Content);
                GetArtwork(information);
            }
            catch
            {
                // ignored
            }

            return information;
        }

        private static void GetArtwork(TrackInformation information)
        {
            if(string.IsNullOrEmpty(information.Artist) || string.IsNullOrEmpty(information.Name))
                return;

            try
            {
                var client = new RestClient("https://api.spotify.com");
                var request = new RestRequest("/v1/search");

                request.AddParameter("q", information.ToString());
                request.AddParameter("type", "track");
                request.AddParameter("limit", 1);

                var result = client.Execute(request);
                var parsed = JObject.Parse(result.Content);
                var tracks = parsed["tracks"]["items"].Children().ToList();
                if (tracks.Count == 0)
                    return;
                
                var artworks = JsonConvert.DeserializeObject<List<Artwork>>(tracks[0]["album"]["images"].ToString());
                information.Artwork = artworks.OrderByDescending(x => x.Height).ThenByDescending(x => x.Width).ToList()[0];
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error retrieving artwork for track {0}", information);
            }
        }
    }
}
