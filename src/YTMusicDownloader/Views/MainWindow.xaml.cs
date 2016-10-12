using System;
using System.ComponentModel;
using YTMusicDownloader.Model.Workspaces;

namespace YTMusicDownloader.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WorkspaceManagement.SaveWorkspaces();
            base.OnClosing(e);
            Environment.Exit(0);
        }
    }
}
