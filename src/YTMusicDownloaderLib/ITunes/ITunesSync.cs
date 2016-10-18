using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTunesLib;
using YTMusicDownloaderLib.RetrieverEngine;

namespace YTMusicDownloaderLib.ITunes
{
    // ReSharper disable once InconsistentNaming
    public static class ITunesSync
    {
        private static readonly iTunesApp AppClass = new iTunesApp();

        public static List<IITPlaylist> GetAllPlaylists()
        {
            var list = AppClass.LibrarySource.Playlists.Cast<IITPlaylist>().Where(i => i.Kind == ITPlaylistKind.ITPlaylistKindUser).ToList();
            list.RemoveRange(0, 12);
            return list;
        }

        public static IITPlaylist GetPlaylist(int id)
        {
            throw new NotImplementedException();
            // return GetAllPlaylists().FirstOrDefault(playlist => playlist.playlistID == id);
        }

    public static void AddTrack(IITPlaylist playlist, PlaylistItem item, string path)
    {
        foreach (var track in AppClass.LibraryPlaylist.Tracks.Cast<IITTrack>().ToList())
        {
            if (track.Kind == ITTrackKind.ITTrackKindFile)
            {
                var fileTrack = track as IITFileOrCDTrack;
                if (fileTrack?.Location == path || fileTrack?.Name == item.Title)
                    return;
            }
        }

        AppClass.LibraryPlaylist.AddFile(path);
        // Add new song to playlist here
    }

        public static bool RemoveTrack(IITPlaylist playlist, PlaylistItem item)
        {
            foreach (var track in playlist.Tracks.Cast<IITTrack>().ToList())
            {
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if (fileTrack?.Name == item.Title)
                        return true;
                }
            }

            return false;
        }

        public static void RemoveOldTracks(IITPlaylist playlist)
        {
            foreach (var track in playlist.Tracks.Cast<IITTrack>().ToList())
            {
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if(string.IsNullOrEmpty(fileTrack?.Location) || !File.Exists(fileTrack.Location))
                        fileTrack?.Delete();
                }
            }
        }
    }
}
