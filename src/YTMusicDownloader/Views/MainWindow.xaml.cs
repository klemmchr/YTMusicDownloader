using System;
using System.ComponentModel;
using YTMusicDownloaderLib.Workspaces;
using YTMusicDownloader.ViewModel;

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

            if (Properties.Settings.Default.FirstStartup)
                CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WorkspaceManagement.SaveWorkspaces();
            base.OnClosing(e);
            Environment.Exit(0);
        }
    }
}
