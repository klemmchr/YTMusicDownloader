using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace YTMusicDownloaderAPI.Controllers
{
    public class PlaylistDataController : ApiController
    {
        public HttpResponseMessage Get(string playlistId, string pageToken)
        {
            var client = new RestClient("https://www.googleapis.com");
            var request = new RestRequest("/youtube/v3/playlistItems", Method.GET);

            request.AddParameter("key", Properties.Settings.GoogleApiKey);
            request.AddParameter("part", "snippet");
            request.AddParameter("playlistId", playlistId);
            request.AddParameter("maxResults", 50);
            request.AddParameter("pageToken", pageToken);

            var response = client.Execute(request);

            return Request.CreateResponse(response.StatusCode, JObject.Parse(response.Content));
        }
    }
}