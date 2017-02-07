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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTMusicDownloaderLib.Analytics
{
    internal class Tracker
    {
        #region Fields        
        #endregion

        #region Properties
        public PlatformInfoProvider PlatformInfoProvider { get; set; }
        #endregion

        #region Construction
        public Tracker(PlatformInfoProvider platformInfoProvider)
        {
            PlatformInfoProvider = platformInfoProvider;
        }
        #endregion

        #region Methods

        public async Task SendEvent(string category, string action, string label, long? value = null)
        {
            if(value != null && value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            var request = new Request(PlatformInfoProvider)
            {
                EventCategory = category,
                EventAction = action,
                EventLabel =  label,
                EventValue = value
            };

            await request.SendRequest();
        }

        public async Task SendPageview(string pageTitle)
        {
            if(string.IsNullOrEmpty(pageTitle))
                throw new ArgumentNullException();

            var request = new Request(PlatformInfoProvider)
            {
                DocumentPath = pageTitle
            };

            await request.SendRequest();
        }

        public async Task StartSession()
        {
            var request = new Request(PlatformInfoProvider)
            {
                EventCategory = "app_metrics",
                EventAction = "session",
                EventLabel = "start",
                SessionControl = "start"
            };

            await request.SendRequest();
        }

        public async Task EndSession()
        {
            var request = new Request(PlatformInfoProvider)
            {
                EventCategory = "app_metrics",
                EventAction = "session",
                EventLabel = "end",
                SessionControl = "end"
            };

            await request.SendRequest();
        }
        #endregion
    }
}