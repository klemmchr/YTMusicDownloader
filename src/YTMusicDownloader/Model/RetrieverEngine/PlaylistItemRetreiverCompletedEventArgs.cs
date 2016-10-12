using System;
using System.Collections.Generic;

namespace YTMusicDownloader.Model.RetrieverEngine
{
    public class PlaylistItemRetreiverCompletedEventArgs: EventArgs
    {
        public List<PlaylistItem> Result { get; }

        public PlaylistItemRetreiverCompletedEventArgs(List<PlaylistItem> result)
        {
            Result = result;
        }
    }
}
