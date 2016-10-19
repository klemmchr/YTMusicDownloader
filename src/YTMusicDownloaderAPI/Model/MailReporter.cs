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
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace YTMusicDownloaderAPI.Model
{
    public static class MailReporter
    {
        private static readonly SmtpClient Server;
        static MailReporter()
        {
            Server = new SmtpClient(Properties.Settings.SmtpServer)
            {
                Port = Properties.Settings.SmtpPort,
                Credentials =
                    new NetworkCredential(Properties.Settings.SmptUsername,
                        Properties.Settings.SmtpPassword),
                EnableSsl = true
            };
        }

        
        public static bool SendMail(string ip, CrashReport report, int reportId)
        {
            var sb = new StringBuilder();
            
            if (reportId != -1)
            {
                sb.AppendLine("This is your automated crash report notifier.");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("A new crash report for the YT Music downloader was created in GitHub.");
                sb.AppendLine();
                sb.Append($"You can find the issue directly under https://github.com/chris579/YTMusicDownloader/issues/");
                sb.Append(reportId);
            }
            else
            {
                sb.AppendLine("Automated crash reporter for app YTMusicDownloader");
                sb.AppendLine();
                sb.AppendLine("The app crashed unexpectly! An issue in GitHub could not be created. Therefore the relevant information is passed here.");
                sb.AppendLine();
                sb.Append("IP: ");
                sb.AppendLine(ip);
                sb.AppendLine();
                sb.Append("Time: ");
                sb.AppendLine(report.Time);
                sb.Append("Assembly version: ");
                sb.AppendLine(report.AssemblyVersion);
                sb.Append("Memory allocated: ");
                sb.AppendLine(report.MemoryAllocated.ToString());
                sb.Append("GUID: ");
                sb.AppendLine(report.Guid);
                sb.Append("Machine Name: ");
                sb.AppendLine(report.MachineName);
                sb.AppendLine();
                sb.AppendLine("Exception trace");
                sb.AppendLine("===================================================================================================");
                sb.AppendLine();
                sb.Append(report.Exception);
            }

            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("chris579@web.de"),
                    Subject = "YT Downloader Crash Report",
                    Body = sb.ToString()
                };

                mail.To.Add("chris579@web.de");

                Server.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error sending mail: " + ex);
            }

            return false;
        }
    }
}