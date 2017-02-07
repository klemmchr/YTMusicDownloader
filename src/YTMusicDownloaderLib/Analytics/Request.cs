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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;

namespace YTMusicDownloaderLib.Analytics
{
    internal class Request
    {
        #region Fields
        private static readonly Dictionary<string, string> Params = ParseParams();
        #endregion

        #region Properties
        public Uri Url { get; set; } = new Uri("http://google-analytics.com");
        public string UrlPath { get; set; } = "collect";
        
        #region AnalyticsParameters
        [AnalyticsParameter("v")]
        public string Version { get; set; } = "1";

        [AnalyticsParameter("tid")]
        public string TrackingId { get; set; }

        [AnalyticsParameter("aip")]
        public bool AnonymizeIp { get; set; }

        [AnalyticsParameter("ds")]
        public string DataSource { get; set; } = "app";

        [AnalyticsParameter("cid")]
        public Guid ClientId { get; set; }

        [AnalyticsParameter("sc")]
        public string SessionControl { get; set; }

        [AnalyticsParameter("ua")]
        public string UserAgent { get; set; }

        [AnalyticsParameter("sr")]
        public string ScreenResolution { get; set; }

        [AnalyticsParameter("vp")]
        public Dimension ViewportSize { get; set; }

        [AnalyticsParameter("sd")]
        public int ScreenColors { get; set; }

        [AnalyticsParameter("ul")]
        public string UserLanguage { get; set; }

        [AnalyticsParameter("ec")]
        public string EventCategory { get; set; }

        [AnalyticsParameter("ea")]
        public string EventAction { get; set; }

        [AnalyticsParameter("el")]
        public string EventLabel { get; set; }

        [AnalyticsParameter("ev")]
        public long? EventValue { get; set; }

        [AnalyticsParameter("dp")]
        public string DocumentPath { get; set; }
        #endregion
        #endregion

        #region Construction
        public Request(PlatformInfoProvider platformInfoProvider)
        {
            TrackingId = platformInfoProvider.TrackingId;
            ClientId = platformInfoProvider.AnonymousCliendId;
            ScreenResolution = platformInfoProvider.TrackingId;
            ViewportSize = platformInfoProvider.ViewportSize;
            ScreenColors = platformInfoProvider.ScreenColorDepthBits;
            UserLanguage = platformInfoProvider.UserLanguage;
        }
        #endregion

        #region Methods

        public async Task SendRequest()
        {
            await Task.Run(() => SendRequestInternal());
        }

        private void SendRequestInternal()
        {
            var properties = typeof(Request).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(AnalyticsParameterAttribute)));
            
            var client = new RestClient(Url);
            var request = new RestRequest(UrlPath, Method.POST);

            foreach (var property in properties)
            {
                var value = property.GetValue(this);

                if (value is bool)
                    value = (bool)value ? 1 : 0;

                if (value != null)
                {
                    var param = Params[property.Name];
                    request.AddParameter(param, value);
                }
            }

            client.Execute(request);
        }

        private static Dictionary<string, string> ParseParams()
        {
            var dict = new Dictionary<string, string>();

            var props = typeof(Request).GetProperties();
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    var authAttr = attr as AnalyticsParameterAttribute;
                    if (authAttr != null)
                    {
                        var propName = prop.Name;
                        var param = authAttr.Param;

                        dict.Add(propName, param);
                    }
                }
            }

            return dict;
        }
        #endregion
    }
}