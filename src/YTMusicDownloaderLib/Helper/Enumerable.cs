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

using System.Collections.Generic;
using System.Linq;

namespace YTMusicDownloaderLib.Helper
{
    public static class Enumerable
    {
        #region Methods

        /// <summary>
        /// Synchronizes the specified source list with the specified compare list.
        /// Removes the elements from the source list which are not in the compare list.
        /// Adds the elements to the source list which are not in the source list but in the compare list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList">The source list.</param>
        /// <param name="compareList">The compare list.</param>
        /// <returns>A new synchronized list.</returns>
        public static List<T> Sync<T>(IList<T> sourceList, IList<T> compareList)
        {
            var newList = new List<T>(sourceList);

            var addItems = compareList.Except(sourceList);
            var removeItems = sourceList.Except(compareList);

            newList.AddRange(addItems);
            newList.RemoveAll(x => removeItems.Contains(x));

            return newList;
        }
        #endregion
    }
}