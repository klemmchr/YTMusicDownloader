using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

            var searchTerm = NormalizeSongTitle(item.Title);
            information.Name = searchTerm;

            try
            {
                var client = new RestClient("https://itunes.apple.com");
                var request = new RestRequest("/search");

                request.AddParameter("term", searchTerm);
                request.AddParameter("country", "US");
                request.AddParameter("media", "music");
                request.AddParameter("limit", 1);

                var result = client.Execute(request);
                var parsed = JObject.Parse(result.Content);
                var tracks = parsed["results"].Children().ToList();
                if (tracks.Count == 0)
                    return information;

                var artwork = tracks[0]["artworkUrl100"].ToString();
                information.CoverUrl = artwork.Replace("100x100", "600x600");
                information.Name = tracks[0]["trackName"].ToString();
                information.Album = tracks[0]["collectionName"].ToString();
                information.Artist = tracks[0]["artistName"].ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error retrieving artwork for track {0}", information);
            }

            return information;
        }

        private static string NormalizeSongTitle(string title)
        {
            var parsed = Regex.Replace(title, @"[\[【].+?[\]】]", "");
            parsed = Regex.Replace(parsed, @"\(.+?\)", "");
            return Regex.Replace(parsed, @"[^\w\s\d-]", "");
        }
    }
}