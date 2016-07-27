namespace PsMixer.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using PsMixer.Enums;

    public class ChannelNameToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(ChannelFriendlyName) &&
                targetType == typeof(ImageSource))
            {
                var path = string.Format("/PsMixer;component/Images/MixerLane/mixer_label_{0}.png", value);
                return new BitmapImage(new Uri(path, UriKind.Relative));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}