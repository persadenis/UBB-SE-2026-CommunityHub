using System;
using Microsoft.UI.Xaml.Data;

namespace ChatModule.src.views.Converters
{
    public class StringFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => (value as string) ?? (parameter as string) ?? string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
