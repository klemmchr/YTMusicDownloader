using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using YTMusicDownloaderLib.RetrieverEngine;

namespace YTMusicDownloaderLib.TrackInformation
{
    public static class TrackInformationFetcher
    {
        public static YTMusicDownloaderLibShared.TrackInformation.TrackInformation GetTrackInformation(PlaylistItem item)
        {
            var information = new YTMusicDownloaderLibShared.TrackInformation.TrackInformation();

            try
            {
                var client = new RestClient("http://ytdownloaderapi.azurewebsites.net");
                var request = new RestRequest("/api/trackInfo");
                request.AddParameter("name", item.Title);

                var result = client.Execute(request);
                if (result.StatusCode != HttpStatusCode.OK)
                    return information;

                information = JsonConvert.DeserializeObject<YTMusicDownloaderLibShared.TrackInformation.TrackInformation>(result.Content);
            }
            catch
            {
                // ignored
            }

            return information;
        }
    }
}
