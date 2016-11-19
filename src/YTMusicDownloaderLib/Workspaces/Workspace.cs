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
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using RestSharp.Extensions.MonoHttp;

namespace YTMusicDownloaderLib.Workspaces
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Workspace
    {
        #region Construction

        [JsonConstructor]
        public Workspace(string path)
        {
            Path = path;
            WorkspacePath = System.IO.Path.Combine(path, ".workspace");
            _workspaceConfigFile = System.IO.Path.Combine(WorkspacePath, ".workspace.json");
            Name = new DirectoryInfo(path).Name;

            if (!Directory.Exists(WorkspacePath) && !File.Exists(_workspaceConfigFile))
                CreateWorkspaceConfig();

            ReadWorkspaceConfig();
        }

        #endregion

        #region Fields

        private readonly string _workspaceConfigFile;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        [JsonProperty]
        public string Path { get; }

        [JsonProperty]
        public string Name { get; set; }

        public WorkspaceSettings Settings { get; private set; }

        public string PlaylistId { get; private set; }

        public string WorkspacePath { get; }

        #endregion

        #region Methods

        private void CreateWorkspaceConfig()
        {
            try
            {
                // Create workspace directory
                var info = Directory.CreateDirectory(WorkspacePath);
                info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                // Create workspace file
                File.Create(System.IO.Path.Combine(WorkspacePath, ".workspace.json")).Close();

                Logger.Debug("Created workspace config {0}", WorkspacePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating workspace config {0}", WorkspacePath);
            }
        }

        private void ReadWorkspaceConfig()
        {
            try
            {
                var content = File.ReadAllText(_workspaceConfigFile);
                Settings = JsonConvert.DeserializeObject<WorkspaceSettings>(content);
                Settings.PropertyChanged += SettingsOnPropertyChanged;

                Logger.Debug("Successfully read workspace config {0}", WorkspacePath);
            }
            catch (Exception ex)
            {
                Settings = new WorkspaceSettings();
                Logger.Error(ex, "Error reading workspace config {0}", WorkspacePath);
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            SaveWorkspaceConfig();
        }

        public async void SaveWorkspaceConfig()
        {
            await Task.Run(() =>
            {
                try
                {
                    var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                    File.WriteAllText(_workspaceConfigFile, json);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error saing workspace config file for workspace {0}", Path);
                }
            });
        }

        public void SetPlaylistUrl(string url)
        {
            Settings.PlaylistUrl = url;

            try
            {
                var uri = new Uri(url);
                PlaylistId = HttpUtility.ParseQueryString(uri.Query).Get("list");
            }
            catch (Exception)
            {
                PlaylistId = null;
            }
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var workspace = obj as Workspace;

            return (workspace != null) && (workspace.Path == Path);
        }

        #endregion
    }
}