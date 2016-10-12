using System;
using GalaSoft.MvvmLight;

namespace YTMusicDownloader.ViewModel
{
    public class SettingsViewModel: ViewModelBase
    {
        public string ParallelDownloads
        {
            get { return Properties.Settings.Default.ParallelDownloads.ToString(); }
            set
            {
                int parsed;
                if(int.TryParse(value, out parsed))
                    Properties.Settings.Default.ParallelDownloads = Math.Min(20, parsed);

                RaisePropertyChanged(nameof(ParallelDownloads));
            }
        }

        public string PlaylistReceiveMaximum
        {
            get { return Properties.Settings.Default.PlaylistReceiveMaximum.ToString(); }
            set
            {
                int parsed;
                if (int.TryParse(value, out parsed))
                    Properties.Settings.Default.PlaylistReceiveMaximum = Math.Min(15000, parsed);

                RaisePropertyChanged(nameof(PlaylistReceiveMaximum));
            }
        }
    }
}
