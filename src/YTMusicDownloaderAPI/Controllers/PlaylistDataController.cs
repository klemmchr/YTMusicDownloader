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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using RestSharp;
using YTMusicDownloaderAPI.Model;

namespace YTMusicDownloaderAPI.Controllers
{
    public class PlaylistDataController : ApiController
    {
        public HttpResponseMessage Get(string playlistId, string pageToken = "")
        {
            if (!RequestProtection.AddRequest(WebApiApplication.GetClientIp(), RequestType.PlaylistRequest))
                return Request.CreateResponse(HttpStatusCode.Forbidden, "Usage limit exceeded");
            try
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
            catch
            {
                // ignored
            }

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}