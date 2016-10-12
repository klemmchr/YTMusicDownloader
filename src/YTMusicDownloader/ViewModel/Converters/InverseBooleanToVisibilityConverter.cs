using System;
using System.Globalization;
using System.Windows.Data;

namespace YTMusicDownloader.ViewModel.Converters
{
    // [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    internal class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType != typeof(System.Windows.Visibility))
                throw new InvalidOperationException("The target must be System.Windows.Visibility");

            return (!((bool)value)) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
