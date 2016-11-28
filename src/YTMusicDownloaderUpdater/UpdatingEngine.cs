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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace YTMusicDownloaderUpdater
{
    internal static class UpdatingEngine
    {
        #region Methods

        internal static bool CheckForZip(string path)
        {
            try
            {
                ZipFile.OpenRead(path).Dispose();
                Console.WriteLine($"Verified zip {path}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void CleanupDirectory(string path)
        {
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles())
            {
                Console.WriteLine($"Deleting file {file}");
                file.Delete();
            }

            foreach (var directory in di.GetDirectories())
            {
                Console.WriteLine($"Deleting directory {directory}");
                directory.Delete(true);
            }
        }

        internal static void ExtractAsset(string zipPath, string targetPath)
        {
            ZipFile.ExtractToDirectory(zipPath, targetPath);
            Console.WriteLine($"Extracted zip {zipPath} to directory {targetPath}");
        }

        internal static void OpenBaseApp(string path)
        {
            try
            {
                Process.Start(path);
                Console.WriteLine($"Opened base app {path}");
            }
            catch
            {
                // ignored
            }

            Environment.Exit(0);
        }

        #endregion
    }
}