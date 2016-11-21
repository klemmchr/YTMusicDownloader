using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;
using YTMusicDownloaderLib.RetrieverEngine;
using YTMusicDownloaderLibShared.Tracks;

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
                GetArtworkAndAlbum(information);
            }
            catch
            {
                // ignored
            }

            return information;
        }

        private static void GetArtworkAndAlbum(TrackInformation information)
        {
            if (string.IsNullOrEmpty(information.Artist) || string.IsNullOrEmpty(information.Name))
                return;

            try
            {
                var client = new RestClient("https://itunes.apple.com");
                var request = new RestRequest("/search");

                request.AddParameter("term", information.ToString());
                request.AddParameter("country", "US");
                request.AddParameter("media", "music");
                request.AddParameter("limit", 1);

                var result = client.Execute(request);
                var parsed = JObject.Parse(result.Content);
                var tracks = parsed["results"].Children().ToList();
                if (tracks.Count == 0)
                    return;

                var artwork = tracks[0]["artworkUrl100"].ToString();
                information.CoverUrl = artwork.Replace("100x100", "600x600");
                information.Album = tracks[0]["trackName"].ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error retrieving artwork for track {0}", information);
            }
        }
    }
}