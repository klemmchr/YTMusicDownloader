using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace YTMusicDownloaderAPINet.Model
{
    public static class GitHubReporter
    {
        private static readonly GitHubClient Client;

        static GitHubReporter()
        {
            Client = new GitHubClient(new ProductHeaderValue("YTMusicDownloaderAPI"))
            {
                Credentials = new Credentials(Properties.Settings.GitHubToken)
            };
        }

        public static async Task<int> CreateIssue(string ip, CrashReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("## Automated crash report");
            sb.AppendLine();
            sb.Append("**IP:** ");
            sb.AppendLine(ip);
            sb.AppendLine();
            sb.Append("**Report time:** ");
            sb.AppendLine(report.Time);
            sb.Append("**Assembly version:** ");
            sb.AppendLine(report.AssemblyVersion);
            sb.Append("**Memory allocated:** ");
            sb.AppendLine(report.MemoryAllocated.ToString());
            sb.Append("**GUID:** ");
            sb.AppendLine(report.Guid);
            sb.Append("**Machine name:** ");
            sb.AppendLine(report.MachineName);
            sb.AppendLine();
            sb.AppendLine("### Exception trace");
            sb.AppendLine("___");
            sb.Append(report.Exception);

            var createIssue = new NewIssue("Automated crash report")
            {
                Body = sb.ToString()
            };

            createIssue.Labels.Add("crash report");
            createIssue.Labels.Add("bug");

            var issue = await Client.Issue.Create(Properties.Settings.GitHubRepoOwner,
                Properties.Settings.GitHubRepoName, createIssue);

            if (issue == null)
                return -1;

            return issue.Number;
        }
    }
}