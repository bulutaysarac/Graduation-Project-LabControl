using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LabControl
{
    public class TransferColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int valueInt = (int)value;
            byte valueByte = (byte)(valueInt * 255 / 100);
            return new SolidColorBrush(Color.FromArgb(255, (byte)(255 - valueByte), (byte)(valueByte * 0.65), 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
