using YTMusicDownloaderLib.Workspaces;

namespace YTMusicDownloader.ViewModel.Messages
{
    internal class AddWorkspaceMessage
    {
        public AddWorkspaceMessage(Workspace workspace)
        {
            Workspace = workspace;
        }

        public Workspace Workspace { get; }
    }
}