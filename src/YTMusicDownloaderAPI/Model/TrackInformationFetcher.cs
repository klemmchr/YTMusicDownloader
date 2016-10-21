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
using YTMusicDownloaderLibShared.TrackInformation;

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

            GetThumbnail(information);

            return information;
        }

        private static void GetArtistAndTrack(string name, TrackInformation information)
        {
            var client = new RestClient("https://ws.audioscrobbler.com");
            var request = new RestRequest("/2.0", Method.GET);
            request.AddParameter("method", "track.search");
            request.AddParameter("track", name);
            request.AddParameter("api_key", Settings.LastFmApiKey);
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

        private static void GetThumbnail(TrackInformation information)
        {
            // Request search for the track
            var client = new RestClient("https://api.discogs.com");
            var request = new RestRequest("/database/search", Method.GET);

            request.AddParameter("q", $"{information.Artist} {information.Name}");
            request.AddParameter("type", "release");
            request.AddParameter("per_page", 1);
            request.AddParameter("key", Settings.DiscogsApiKey);
            request.AddParameter("secret", Settings.DiscogsApiSecret);
            request.AddHeader("Accept", "application/vnd.discogs.v2.html+json");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new WebException();

            var searchResponse = JObject.Parse(response.Content);
            var searchResults = searchResponse["results"].Children().ToList();
            if (searchResults.Count == 0)
                return;

            information.CoverUrl = searchResults[0]["thumb"].ToString();
            information.Album = searchResults[0]["title"].ToString();
        }
    }
}