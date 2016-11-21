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
            var list =
                AppClass.LibrarySource.Playlists.Cast<IITPlaylist>()
                    .Where(i => i.Kind == ITPlaylistKind.ITPlaylistKindUser)
                    .ToList();
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
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if ((fileTrack?.Location == path) || (fileTrack?.Name == item.Title))
                        return;
                }

            AppClass.LibraryPlaylist.AddFile(path);
            // Add new song to playlist here
        }

        public static bool RemoveTrack(IITPlaylist playlist, PlaylistItem item)
        {
            foreach (var track in playlist.Tracks.Cast<IITTrack>().ToList())
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if (fileTrack?.Name == item.Title)
                        return true;
                }

            return false;
        }

        public static void RemoveOldTracks(IITPlaylist playlist)
        {
            foreach (var track in playlist.Tracks.Cast<IITTrack>().ToList())
                if (track.Kind == ITTrackKind.ITTrackKindFile)
                {
                    var fileTrack = track as IITFileOrCDTrack;
                    if (string.IsNullOrEmpty(fileTrack?.Location) || !File.Exists(fileTrack.Location))
                        fileTrack?.Delete();
                }
        }
    }
}