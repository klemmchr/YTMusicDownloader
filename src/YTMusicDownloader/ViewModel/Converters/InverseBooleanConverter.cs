using System;
using System.Globalization;
using System.Windows.Data;

namespace YTMusicDownloader.ViewModel.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
