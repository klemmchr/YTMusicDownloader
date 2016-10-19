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
namespace YTMusicDownloaderAPI.Model
{
    public class CrashReport
    {
        public string ProcessName { get; set; }
        public string Time { get; set; }
        public string AssemblyVersion { get; set; }
        public int MemoryAllocated { get; set; }
        public string Guid { get; set; }
        public string MachineName { get; set; }
        public string Exception { get; set; }
    }
}