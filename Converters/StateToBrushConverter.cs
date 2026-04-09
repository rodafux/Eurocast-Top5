using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Top5.Models;

namespace Top5.Converters // <-- CETTE LIGNE EST CRUCIALE
{
    public class StateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ControlState state)
            {
                return state switch
                {
                    ControlState.B => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                    ControlState.AA => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12")),
                    ControlState.NC => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}