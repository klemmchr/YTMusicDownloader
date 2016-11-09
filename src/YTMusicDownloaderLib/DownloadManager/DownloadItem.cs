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
using System.IO;
using System.Net;
using System.Runtime;
using System.Threading;
using NAudio.Wave;
using NLog;
using TagLib;
using YTMusicDownloaderLib.RetrieverEngine;
using YTMusicDownloaderLib.Tracks;
using File = System.IO.File;

namespace YTMusicDownloaderLib.DownloadManager
{
    public class DownloadItem : DownloadManagerItem
    {
        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private WebClient _webClient;
        private Thread _downloadThread;

        #endregion

        #region Properties

        public string SavePath { get; }
        public bool Overwrite { get; }
        public DownloadFormat DownloadFormat { get; }

        #endregion

        public DownloadItem(PlaylistItem item, string savePath, DownloadFormat downloadFormat, bool overwrite = false)
            : base(item)
        {
            SavePath = savePath;
            Overwrite = overwrite;
            DownloadFormat = downloadFormat;
        }

        public override void StartDownload()
        {
            _downloadThread = new Thread(StartDownloadInternal);
            _downloadThread.Start();
        }

        private void StartDownloadInternal()
        {
            try
            {
                if (File.Exists(SavePath) && !Overwrite)
                {
                    OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true));
                    return;
                }

                OnDownloadItemDownloadStarted(null);
                Item.RetreiveDownloadUrl();

                if (string.IsNullOrEmpty(Item.DownloadUrl))
                {
                    OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, false,
                        new InvalidOperationException("Could not retreive download url")));
                    return;
                }

                using (_webClient = new WebClient())
                {
                    try
                    {
                        _webClient.OpenReadCompleted += WebClientOnOpenReadCompleted;

                        _webClient.OpenReadAsync(new Uri(Item.DownloadUrl));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Error downloading track {0}", Item.VideoId);
                        OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, false, ex));
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // ignored
            }
        }

        private void WebClientOnOpenReadCompleted(object sender, OpenReadCompletedEventArgs openReadCompletedEventArgs)
        {
            _webClient.Dispose();

            if (openReadCompletedEventArgs.Cancelled)
            {
                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, true, openReadCompletedEventArgs.Error));
                return;
            }

            if (!Overwrite && File.Exists(SavePath))
                return;

            var totalLength = 0;
            try
            {
                totalLength = int.Parse(((WebClient)sender).ResponseHeaders["Content-Length"]);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                long processed = 0;
                var tmpPath = Path.GetTempFileName();

                using (var stream = openReadCompletedEventArgs.Result)
                using (var fs = File.Create(tmpPath))
                {
                    var buffer = new byte[81920];
                    int read;

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);

                        processed += read;
                        OnDownloadItemDownloadProgressChanged(new DownloadProgressChangedEventArgs(processed, totalLength));
                    }
                }

                switch (DownloadFormat)
                {
                    case DownloadFormat.M4A:
                    {
                        File.Move(tmpPath, SavePath);
                    } break;

                    case DownloadFormat.MP3:
                    {
                        OnDownloadItemConvertionStarted(null);
                        using (var reader = new MediaFoundationReader(tmpPath))
                        {
                            MediaFoundationEncoder.EncodeToMp3(reader, SavePath);
                        }

                        File.Delete(tmpPath);
                    } break;
                }

                var information = TrackInformationFetcher.GetTrackInformation(Item);
                using (var file = TagLib.File.Create(SavePath))
                {
                    file.Tag.Title = information.Name;
                    file.Tag.Performers = new[] {information.Artist};
                    file.Tag.Album = information.Album;

                    if (!string.IsNullOrEmpty(information.CoverUrl))
                    {
                        using (var client = new WebClient())
                        {
                            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                            var data = client.DownloadData(information.CoverUrl);

                            file.Tag.Pictures = new IPicture[]
                            {
                                new Picture
                                {
                                    Data = new ByteVector(data),
                                    Type = PictureType.FrontCover,
                                    Description = "Cover"
                                }
                            };
                        }
                    }
                    
                    file.Save();       
                }

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(false));
            }
            catch (Exception ex)
            {
                OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, false, ex));
            }
        }

        public override void StopDownload()
        {
            _downloadThread?.Abort();
            _webClient?.CancelAsync();
            _webClient?.Dispose();

            OnDownloadItemDownloadCompleted(new DownloadCompletedEventArgs(true, true));
        }

        public override void Dispose()
        {
            _webClient?.Dispose();
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as DownloadItem;

            return Item.Equals(item?.Item);
        }
    }
}
