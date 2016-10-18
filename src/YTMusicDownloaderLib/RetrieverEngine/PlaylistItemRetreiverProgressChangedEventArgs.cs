using System;

namespace YTMusicDownloaderLib.RetrieverEngine
{
    public class PlaylistItemRetreiverProgressChangedEventArgs: EventArgs
    {
        public double Progress { get; }

        public PlaylistItemRetreiverProgressChangedEventArgs(int currentItem, int totalItems)
        {
            Progress = (double) currentItem / totalItems * 100;
        }
    }
}
