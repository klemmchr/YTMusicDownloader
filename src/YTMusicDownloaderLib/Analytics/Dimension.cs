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
namespace YTMusicDownloaderLib.Analytics
{
    internal class Dimension
    {
        #region Fields        
        #endregion

        #region Properties
        public int Width { get; }
        public int Height { get; }
        #endregion

        #region Construction
        public Dimension(int width, int height)
        {
            Width = width;
            Height = height;
        }
        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        #endregion
    }
}