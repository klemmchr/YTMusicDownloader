using System;

namespace YTMusicDownloaderLib.Analytics
{
    internal struct PlatformInfoProvider
    {
        public string TrackingId { get; set; }
        public Guid AnonymousCliendId { get; set; }
        public Dimension ScreenResolution { get; set; }
        public Dimension ViewportSize { get; set; }
        public int ScreenColorDepthBits { get; set; }
        public string UserLanguage { get; set; }
        public string UserAgent { get; set; }
    }
}