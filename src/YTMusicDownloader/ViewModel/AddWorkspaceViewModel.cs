using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using YTMusicDownloader.ViewModel.Messages;
using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel
{
    internal class AddWorkspaceViewModel : ViewModelBase
    {
        #region Fields

        private string _addWorkspacePath;

        #endregion

        #region Properties

        public string AddWorkspacePath
        {
            get { return _addWorkspacePath; }
            set
            {
                _addWorkspacePath = value;
                RaisePropertyChanged(nameof(AddWorkspacePath));
            }
        }

        public RelayCommand CloseCommand
            => new RelayCommand(() => Messenger.Default.Send(new CloseAddWorkspaceFlyoutMessage()));

        public RelayCommand SelectFolderCommand => new RelayCommand(SelectFolder);
        public RelayCommand AddWorkspaceCommand => new RelayCommand(AddWorkspace);

        #endregion

        #region Methods

        private void SelectFolder()
        {
            var fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            fbd.ShowDialog();

            AddWorkspacePath = fbd.SelectedPath;
        }

        private void AddWorkspace()
        {
            if (string.IsNullOrEmpty(AddWorkspacePath))
                return;

            try
            {
                var workspace = WorkspaceManagement.AddWorkspace(AddWorkspacePath);
                if (workspace == null)
                    return;

                Messenger.Default.Send(new AddWorkspaceMessage(workspace));
                Messenger.Default.Send(new CloseAddWorkspaceFlyoutMessage());
                AddWorkspacePath = default(string);
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}