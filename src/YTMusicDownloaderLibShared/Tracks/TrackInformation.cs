namespace YTMusicDownloaderLibShared.Tracks
{
    public class TrackInformation
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public Artwork Artwork { get; set; }

        public override string ToString()
        {
            return $"{Artist} {Name}";
        }
    }
}
