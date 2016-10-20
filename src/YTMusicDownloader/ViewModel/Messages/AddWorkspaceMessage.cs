using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel.Messages
{
    class AddWorkspaceMessage
    {
        public Workspace Workspace { get; }
        public AddWorkspaceMessage(Workspace workspace)
        {
            Workspace = workspace;
        }
    }
}
