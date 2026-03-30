using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Top5.Models;

namespace Top5.Converters
{
    public class StateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ControlState state)
            {
                return state switch
                {
                    ControlState.B => Brushes.LightGreen,
                    ControlState.AA => Brushes.Orange,
                    ControlState.NC => Brushes.Tomato,
                    // Gris très clair qui indique que c'est un bouton cliquable, mais inactif
                    _ => new SolidColorBrush(Color.FromRgb(225, 225, 225))
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}