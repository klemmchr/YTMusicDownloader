using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTunesLib;
using YTMusicDownloader.Model.RetrieverEngine;
using YTMusicDownloader.Model.Workspaces;

namespace YTMusicDownloader.Model.ITunes
{
    // ReSharper disable once InconsistentNaming
    public static class ITunesSync
    {
        private static readonly iTunesApp AppClass = new iTunesApp();

        public static async Task<List<IITPlaylist>> GetAllPlaylists()
        {
            var list = await Task.Run(() => AppClass.LibrarySource.Playlists.Cast<IITPlaylist>().Where(i => i.Kind == ITPlaylistKind.ITPlaylistKindUser).ToList());
            list.RemoveRange(0, 12);
            return list;
        }

        public static IITPlaylist GetPlaylist(int id)
        {
            throw new NotImplementedException();
            // return GetAllPlaylists().FirstOrDefault(playlist => playlist.playlistID == id);
        }

        public static void AddTrack(IITLibraryPlaylist playlist, PlaylistItem item, string path)
        {
            // Check if the track is already in the playlist
            if (playlist.Tracks.Cast<IITTrack>().Any(track => track.Name == item.Title))
            {
                return;
            }

            playlist.AddFile(path);
        }

        public static void RemoveOldTracks(IITLibraryPlaylist playlist, PlaylistItem item)
        {
            foreach (var track in playlist.Tracks.Cast<IITTrack>().ToList())
            {
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if(string.IsNullOrEmpty(fileTrack.Location) || !File.Exists(fileTrack.Location))
                        fileTrack.Delete();
                }
            }
        }
    }
}
