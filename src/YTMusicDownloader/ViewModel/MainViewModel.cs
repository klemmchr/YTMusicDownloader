using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using YTMusicDownloader.Model.Workspaces;
using YTMusicDownloader.Properties;

namespace YTMusicDownloader.ViewModel
{
    
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private WorkspaceViewModel _selectedWorkspace;
        private int _selectedTabIndex;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Properties

        public WorkspaceViewModel SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                _selectedWorkspace = value;
                IsWorkspaceSelected = true;

                RaisePropertyChanged(nameof(SelectedWorkspace));
                RaisePropertyChanged(nameof(SelectedWorkspaceName));
                RaisePropertyChanged(nameof(SelectedWorkspaceVisible));
                RaisePropertyChanged(nameof(IsWorkspaceSelected));
            }
        }
        public string SelectedWorkspaceName => _selectedWorkspace?.Workspace.ToString();
        public bool SelectedWorkspaceVisible => _selectedWorkspace != null;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                RaisePropertyChanged(nameof(SelectedTabIndex));

                if(value == 2)
                    SelectedWorkspace?.WorkspaceSettingsViewModel.SettingsPageSelected();
            }
        }

        public int SelectedWorkspaceIndex { get; set; }
        public bool IsWorkspaceSelected { get; private set; }

        public ObservableCollection<WorkspaceViewModel> Workspaces { get; }

        public RelayCommand AddWorkspaceCommand => new RelayCommand(AddWorkspace);
        public RelayCommand SelectWorkspaceCommand => new RelayCommand(SelectWorkspace, () => SelectedWorkspaceIndex != -1);
        public RelayCommand RemoveWorkspaceCommand => new RelayCommand(RemoveWorkspace, () => SelectedWorkspaceIndex != -1);
        #endregion

        public MainViewModel()
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            Workspaces = new ObservableCollection<WorkspaceViewModel>();

            if (IsInDesignMode)
            {
                IsWorkspaceSelected = true;
                return;
            }

            LoadWorkspaces();

#if DEBUG
            Logger.Trace("Initialized Main View Model: {0} ms", watch.ElapsedMilliseconds);
#else
            Logger.Debug("Initialized Main View Model");
#endif
        }

#region Methods
        private void LoadWorkspaces()
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            foreach (var workspace in WorkspaceManagement.Workspaces)
            {
                Workspaces.Add(new WorkspaceViewModel(workspace));
            }
#if DEBUG
            Logger.Trace("Loaded all workspaces: {0} ms", watch.ElapsedMilliseconds);
#else
            Logger.Debug("Loaded all workspaces");
#endif
        }

        private void AddWorkspace()
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            var workspace = WorkspaceManagement.AddWorkspace(fbd.SelectedPath);
            if(workspace != null)
                Workspaces.Add(new WorkspaceViewModel(workspace));
        }

        private void SelectWorkspace()
        {
            if(SelectedWorkspaceIndex > Workspaces.Count - 1)
                return;
            
            SelectedWorkspace = Workspaces[SelectedWorkspaceIndex];
            SelectedWorkspace.Init();

            SelectedTabIndex = 1;

            Logger.Debug("Selected workspace {0}", SelectedWorkspace.Workspace.Name);
        }

        private void RemoveWorkspace()
        {
            if (SelectedWorkspaceIndex > Workspaces.Count - 1 || SelectedWorkspaceIndex < 0)
                return;

            var index = SelectedWorkspaceIndex;

            if(Workspaces[index] == null)
                return;

            var result = MessageBox.Show(Resources.MainViewModel_RemoveWorkspace_Description, Resources.MainViewModel_RemoveWorkspace_Title, MessageBoxButtons.YesNoCancel);
            if(result == DialogResult.Cancel)
                return;

            var deleteMode = result == DialogResult.Yes ? DeleteMode.DeleteWorkspace : DeleteMode.KeepWorkspace;
            
            if (SelectedWorkspace != null && SelectedWorkspace.Workspace.Equals(Workspaces[index].Workspace))
                SelectedWorkspace = null;

            var workspace = Workspaces[index].Workspace;

            WorkspaceManagement.RemoveWorkspace(workspace, deleteMode);
            Workspaces.RemoveAt(index);

            Logger.Debug("Removed workspace {0} - deleteMode: {1}", workspace.Name, deleteMode);
        }
#endregion
    }
}