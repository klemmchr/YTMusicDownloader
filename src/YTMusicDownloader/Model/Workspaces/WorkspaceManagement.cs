using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NLog;

namespace YTMusicDownloader.Model.Workspaces
{
    internal static class WorkspaceManagement
    {
        #region Fields
        private static readonly string WorkspaceConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YTMusicDownloader", "workspaces.json");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Properties
        public static ObservableCollection<Workspace> Workspaces { get; }
        #endregion

        static WorkspaceManagement()
        {
            Workspaces = new ObservableCollection<Workspace>();

            if(ViewModelBase.IsInDesignModeStatic) return;

            LoadWorkspaces();

            Workspaces.CollectionChanged += WorkspacesOnCollectionChanged;
            new Thread(AutoSave).Start();
        }

        private static void AutoSave()
        {
            while (true)
            {
                Thread.Sleep(new TimeSpan(0, 2, 0));
                SaveWorkspaces();
            }
        }

        private static void WorkspacesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Task.Run(() => SaveWorkspaces());
        }

        private static void LoadWorkspaces()
        {
            if (!File.Exists(WorkspaceConfigPath))
            {
                CreateWorkspaceConfig();
                return;
            }

            try
            {
                var content = File.ReadAllText(WorkspaceConfigPath);
                
                foreach (var workspace in JsonConvert.DeserializeObject<List<Workspace>>(content))
                {
                    if(!Workspaces.Contains(workspace) && Directory.Exists(workspace.Path))
                        Workspaces.Add(workspace);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error reading workspace file {0}", WorkspaceConfigPath);
            }   
        }

        private static void CreateWorkspaceConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(WorkspaceConfigPath));
                File.Create(WorkspaceConfigPath).Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating workspace config {0}", WorkspaceConfigPath);
            }
        }

        public static void SaveWorkspaces()
        {
            try
            {
                foreach (var workspace in Workspaces)
                {
                    workspace.SaveWorkspaceConfig();
                }
                var serialized = JsonConvert.SerializeObject(Workspaces.ToList(), Formatting.Indented);
                File.WriteAllText(WorkspaceConfigPath, serialized);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error saving workspace config {0}", WorkspaceConfigPath);
            }
        }

        public static Workspace AddWorkspace(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var workspace = new Workspace(path)
            {
                Name = Directory.GetParent(path).Name
            };

            if (Workspaces.Contains(workspace))
                return null;

            Workspaces.Add(workspace);
            return workspace;
        }

        public static void RemoveWorkspace(Workspace workspace, DeleteMode deleteMode)
        {
            Workspaces.Remove(workspace);

            if (deleteMode == DeleteMode.DeleteWorkspace)
            {
                try
                {
                    Directory.Delete(workspace.WorkspacePath, true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error deleting workspace folder {0}", workspace.WorkspacePath);
                }
            }
        }

        public static void UpdateWorkspace(Workspace workspace)
        {
            if (Workspaces.Contains(workspace))
            {
                for (var i = 0; i < Workspaces.Count; i++)
                {
                    var current = Workspaces[i];
                    if (current.Equals(workspace))
                    {
                        Workspaces[i] = workspace;
                        break;
                    }
                }
            }
            else
            {
                Workspaces.Add(workspace);
            }

            SaveWorkspaces();
        }
    }
}
