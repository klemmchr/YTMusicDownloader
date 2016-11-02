using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using YTMusicDownloaderLib.Helpers;

namespace YTMusicDownloader.ViewModel
{
    internal class AboutTabViewModel: ViewModelBase
    {
        public string Version { get; }

        public RelayCommand OpenLicenseCommand => new RelayCommand(() => Process.Start("http://www.apache.org/licenses/LICENSE-2.0"));
        public RelayCommand OpenGitHubCommand => new RelayCommand(() => Process.Start("https://github.com/chris579/YTMusicDownloader"));

        public AboutTabViewModel()
        {
            Version = Assembly.GetAssemblyVersion();
        }
    }
}
