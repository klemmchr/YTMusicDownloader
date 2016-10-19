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

namespace YTMusicDownloaderLib.DownloadManager
{
    public class DownloadProgressChangedEventArgs: EventArgs
    {
        public double ProgressPercentage { get; }
        public DownloadProgressChangedEventArgs(long processedBytes, long totalBytes)
        {
            if (totalBytes == 0)
                ProgressPercentage = 0;
            else
                ProgressPercentage = ((processedBytes * 1.0) / (totalBytes * 1.0)) * 100;
        }

        public DownloadProgressChangedEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
    }
}
