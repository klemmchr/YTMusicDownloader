using System;
using System.Globalization;
using System.Windows.Data;

namespace YTMusicDownloader.ViewModel.Converters
{
    internal class PageDisplayConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var currentPage = values[0].ToString();
            var maxPages = values[1].ToString();

            if (string.IsNullOrEmpty(currentPage) || string.IsNullOrEmpty(maxPages)) return null;

            return $"{currentPage} of {maxPages}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
