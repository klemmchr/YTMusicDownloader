using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace YTMusicDownloaderAPINet.Model
{
    public static class RequestProtection
    {
        private static readonly HashSet<Client> Clients;
        
        static RequestProtection()
        {
            Clients = new HashSet<Client>();
            var timer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            timer.Elapsed += (s, e) => Clients.Clear();
        }

        public static bool AddRequest(string ip)
        {
            var client = Clients.FirstOrDefault(c => c.Ip == ip);

            if(client == null)
            {
               Clients.Add(new Client(ip));
            }
            else
            {
                client.Requests++;
                if (client.Requests >= Properties.Settings.MaxRequestsPerHour)
                    return false;
            }

            return true;
        }
    }
}