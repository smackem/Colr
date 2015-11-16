using Colr.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Colr.DesktopApp.Util
{
    class NotNullToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null
                   ? Visibility.Visible
                   : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ColorHsvToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hsv = value as ColorHsv?;

            if (hsv == null)
                return Binding.DoNothing;

            return new SolidColorBrush(hsv.Value.ToColor(255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ColorHsvToSaturationBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hsvObj = value as ColorHsv?;

            if (hsvObj == null)
                return Binding.DoNothing;

            var hsv = hsvObj.Value;

            return new LinearGradientBrush(
                ColorHsv.FromHsv(hsv.H, 0.0, hsv.V).ToColor(255),
                ColorHsv.FromHsv(hsv.H, 1.0, hsv.V).ToColor(255),
                0.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ColorHsvToValueBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hsvObj = value as ColorHsv?;

            if (hsvObj == null)
                return Binding.DoNothing;

            var hsv = hsvObj.Value;

            return new LinearGradientBrush(
                ColorHsv.FromHsv(hsv.H, hsv.S, 0.0).ToColor(255),
                ColorHsv.FromHsv(hsv.H, hsv.S, 1.0).ToColor(255),
                0.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
