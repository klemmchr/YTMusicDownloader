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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace YTMusicDownloaderAPI.Model
{
    public static class RequestProtection
    {
        private static readonly string BlockedClientsPath = Path.Combine(WebApiApplication.ExecutablePath, "blocked.json");
        private static readonly HashSet<Client> Clients;

        public static ObservableCollection<Client> BlockedClients { get; private set; }

        static RequestProtection()
        {
            Clients = new HashSet<Client>();
            var timer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            timer.Elapsed += (s, e) => Clients.Clear();

            LoadSettings();
            BlockedClients.CollectionChanged += (sender, args) => SaveSettings();
        }

        private static void LoadSettings()
        {
            BlockedClients = new ObservableCollection<Client>();

            try
            {
                if (!File.Exists(BlockedClientsPath))
                {
                    File.Create(BlockedClientsPath).Close();
                    SaveSettings();
                }
                else
                {
                    var content = File.ReadAllText(BlockedClientsPath);
                    BlockedClients = JsonConvert.DeserializeObject<ObservableCollection<Client>>(content);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(BlockedClients);
                File.WriteAllText(BlockedClientsPath, json);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static bool AddRequest(string ip, RequestType requestType)
        {
            var client = Clients.FirstOrDefault(c => c.Ip == ip);

            if(client == null)
            {
                var newClient = new Client(ip);
                Clients.Add(newClient);
                if (BlockedClients.Contains(newClient))
                    return false;
            }
            else
            {
                if (BlockedClients.Contains(client))
                    return false;

                switch (requestType)
                {
                    case RequestType.CrashReport:
                    {
                        client.CrashReports++;
                        if (client.CrashReports > Properties.Settings.Default.MaxCrashReportsPerHour*10)
                        {
                            BlockedClients.Add(client);
                            return false;
                        }

                        if (client.CrashReports > Properties.Settings.Default.MaxCrashReportsPerHour)
                            return false;

                    } break;

                    case RequestType.PlaylistRequest:
                    {
                        client.PlaylistRequests++;
                        if (client.PlaylistRequests > Properties.Settings.Default.MaxPlaylistRequestsPerHour*10)
                        {
                            BlockedClients.Add(client);
                            return false;
                        }

                        if (client.PlaylistRequests > Properties.Settings.Default.MaxPlaylistRequestsPerHour)
                            return false;
                    } break;

                    case RequestType.TrackInfoRequest:
                    {
                        client.TrackInfoRequests++;
                        if (client.TrackInfoRequests > Properties.Settings.Default.MaxTrackInfoRequestsPerHour * 10)
                        {
                            BlockedClients.Add(client);
                            return false;
                        }

                        if (client.CrashReports > Properties.Settings.Default.MaxTrackInfoRequestsPerHour)
                            return false;
                    } break;
                } 
            }

            return true;
        }
    }
}