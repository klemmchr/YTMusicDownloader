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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YTMusicDownloaderLibTest.Helpers.List
{
    [TestClass]
    public class Sync
    {
        #region Methods

        [TestMethod]
        public void SyncSuccess()
        {
            var list1 = new List<TestObject>
            {
                new TestObject(7),
                new TestObject(6),
                new TestObject(5),
                new TestObject(4),
                new TestObject(3),
                new TestObject(2),
                new TestObject(1)
            };

            var list2 = new List<TestObject>
            {
                new TestObject(6),
                new TestObject(7),
                new TestObject(8),
                new TestObject(9)
            };

            var expectedList = new List<TestObject>
            {
                new TestObject(9),
                new TestObject(8),
                new TestObject(7),
                new TestObject(6)
            };

            var syncedList = YTMusicDownloaderLib.Helpers.List.Sync(list1, list2).ToList();

            Assert.IsTrue(expectedList.SequenceEqual(syncedList));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SyncFailure()
        {
            YTMusicDownloaderLib.Helpers.List.Sync<TestObject>(null, null);
        }

        #endregion
    }
}