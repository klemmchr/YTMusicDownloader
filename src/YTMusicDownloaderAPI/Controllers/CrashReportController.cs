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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using YTMusicDownloaderAPI.Model;

namespace YTMusicDownloaderAPI.Controllers
{
    public class CrashReportController : ApiController
    {
        // POST api/<controller>
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]CrashReport report)
        {
            var ip = GetClientIp();

            if (!RequestProtection.AddRequest(ip))
                return Request.CreateResponse(HttpStatusCode.Forbidden, "Usage limit exceeded");

            var issueId = await GitHubReporter.CreateIssue(ip, report);

            return MailReporter.SendMail(ip, report, issueId) ? Request.CreateResponse(HttpStatusCode.OK, "Success") : Request.CreateResponse(HttpStatusCode.InternalServerError, "Error sending message");
        }

        private static string GetClientIp()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return string.IsNullOrEmpty(ip) ? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : ip;
        }
    }
}