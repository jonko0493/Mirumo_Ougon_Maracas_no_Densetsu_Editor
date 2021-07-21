using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mirumo_Ougon_Maracas_no_Densetsu_Editor
{
    public class Helpers
    {
    }

    public class MessageTextBox : TextBox
    {
        public Message Message { get; set; }
    }

    public class MultiplyByFactor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double.TryParse((string)parameter, out double param);
            return param * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double.TryParse((string)parameter, out double param);
            return (double)value / param;
        }
    }
}
