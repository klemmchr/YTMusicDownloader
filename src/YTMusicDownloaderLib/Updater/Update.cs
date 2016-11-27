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

namespace YTMusicDownloaderLib.Updater
{
    public class Update
    {
        #region Properties
        public Version Version { get; }
        public List<Asset> Assets { get; }
        public string MainAssemblyPath { get; }
        #endregion

        #region Construction
        public Update(Version version, List<Asset> assets, string mainAssemblyPath)
        {
            Version = version;
            Assets = assets;
            MainAssemblyPath = mainAssemblyPath;
        }
        #endregion

        #region Methods

        public Asset GetMatchingAsset()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                var result = Assets.Find(a => a.Architecture == Architecture.x64);
                if (result != null)
                    return result;
            }

            return Assets.Find(a => a.Architecture != Architecture.x64);
        }

        #endregion

        #region Override
        public override string ToString()
        {
            return Version.ToString();
        }
        #endregion
    }
}