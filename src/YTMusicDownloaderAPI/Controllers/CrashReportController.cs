using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using YTMusicDownloaderAPINet.Model;

namespace YTMusicDownloaderAPINet.Controllers
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