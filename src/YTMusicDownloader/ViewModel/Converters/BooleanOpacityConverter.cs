using System;
using System.Globalization;
using System.Windows.Data;

namespace YTMusicDownloader.ViewModel.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class BooleanOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return ((bool) parameter) ? 1: 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
