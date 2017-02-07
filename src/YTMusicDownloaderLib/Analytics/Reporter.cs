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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTMusicDownloaderLib.Analytics
{
    public static class Reporter
    {
        #region Fields  

        private static readonly Tracker Tracker;
        #endregion

        #region Properties
#if DEBUG
        public static string TrackerId { get; } = "UA-90426913-5";
#else
        public static string TrackerId { get; } = Debugger.IsAttached ? "UA-90426913-5" : "UA-90426913-6";
#endif      
        #endregion

        #region Construction
        static Reporter()
        {
            if (Properties.Settings.Default.UserId.Equals(new Guid()))
            {
                Properties.Settings.Default.UserId = Guid.NewGuid();
            }

            var dimension = new Dimension(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Tracker = new Tracker(new PlatformInfoProvider()
            {
                TrackingId = TrackerId,
                ViewportSize = dimension,
                ScreenResolution = dimension,
                UserLanguage = Thread.CurrentThread.CurrentUICulture.ToString(),
                AnonymousCliendId = Properties.Settings.Default.UserId,
                ScreenColorDepthBits = Screen.PrimaryScreen.BitsPerPixel
            });
        }
        #endregion

        #region Methods

        public static async Task SendEvent(string category, string action, string label, long? value = null)
        {
            if(Properties.Settings.Default.TrackingEnabled)
                await Tracker.SendEvent(category, action, label, value);
        }

        public static async void SendPageview(string pageTitle)
        {
            if (Properties.Settings.Default.TrackingEnabled)
                await Tracker.SendPageview(pageTitle);
        }

        public static async Task StartSession()
        {
            if (Properties.Settings.Default.TrackingEnabled)
                await Tracker.StartSession();
        }

        public static async Task EndSession()
        {
            if (Properties.Settings.Default.TrackingEnabled)
                await Tracker.EndSession();
        }
        #endregion
    }
}