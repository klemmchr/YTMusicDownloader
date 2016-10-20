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
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace YTMusicDownloaderAPI.Model
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
            sb.Append("**Report time:** ");
            sb.AppendLine(report.Time);
            sb.Append("**Assembly version:** ");
            sb.AppendLine(report.AssemblyVersion);
            sb.Append("**Memory allocated:** ");
            sb.AppendLine(report.MemoryAllocated.ToString());
            sb.Append("**GUID:** ");
            sb.AppendLine(report.Guid);
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