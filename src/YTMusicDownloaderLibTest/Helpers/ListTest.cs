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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YTMusicDownloaderLibTest.Helpers
{
    [TestClass]
    public class ListTest
    {
        #region Methods
        [TestMethod]
        public void TestSync()
        {
            var list1 = new List<int> { 7, 6, 5, 4, 3, 2, 1 };
            var list2 = new List<int> { 6, 7, 8, 9 };
            var expectedList = new List<int> { 9, 8, 7, 6 };

            var syncedList = YTMusicDownloaderLib.Helper.List.Sync(list1, list2).ToList();

            Assert.IsTrue(expectedList.SequenceEqual(syncedList));
        }
        #endregion
    }
}