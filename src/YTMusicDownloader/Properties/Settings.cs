using System;
using System.ComponentModel;
using NLog;

namespace YTMusicDownloader.Properties {
    internal sealed partial class Settings
    {
        public Settings() {
            try
            {
                Upgrade();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Warn(ex, "Could not upgrade settings");
            }

            PropertyChanged += OnPropertyChanged;
        }

        private new void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Save();
        }
    }
}
