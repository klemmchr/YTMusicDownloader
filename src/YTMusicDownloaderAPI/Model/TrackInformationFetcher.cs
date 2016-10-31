using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;
using RestSharp;
using YTMusicDownloaderAPI.Properties;
using YTMusicDownloaderLibShared.Tracks;

namespace YTMusicDownloaderAPI.Model
{
    public static class TrackInformationFetcher
    {
        /// <summary>
        /// Gets the track information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static TrackInformation GetTrackInformation(string name)
        {
            var information = new TrackInformation();
            // Normalize track title
            name = Regex.Replace(name, @"[\[【].+?[\]】]", "");
            name = Regex.Replace(name, @"[^\w\s\d-]", "");

            GetArtistAndTrack(name, information);
            if (string.IsNullOrEmpty(information.Name) || string.IsNullOrEmpty(information.Artist))
                return information;

            return information;
        }

        private static void GetArtistAndTrack(string name, TrackInformation information)
        {
            var client = new RestClient("https://ws.audioscrobbler.com");
            var request = new RestRequest("/2.0", Method.GET);
            request.AddParameter("method", "track.search");
            request.AddParameter("track", name);
            request.AddParameter("api_key", Properties.Settings.LastFmApiKey);
            request.AddParameter("format", "json");
            request.AddParameter("limit", 1);

            var result = client.Execute(request);
            if(result.StatusCode != HttpStatusCode.OK)
                throw new WebException();

            var json = JObject.Parse(result.Content);

            var results = json["results"]["trackmatches"]["track"].Children().ToList();
            if (results.Count == 0)
                return;

            information.Artist = results[0]["artist"].ToString();
            information.Name = results[0]["name"].ToString();
        }
    }
}