using System;
using System.Globalization;
using System.Windows.Data;

namespace Docked.Util.Converters
{
   public class SquareInCircleConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         double length = double.Parse(value.ToString());
         double ret = Math.Sqrt(2) * (length / 2);
         if (parameter != null)
         {
            double scalar = double.Parse(parameter.ToString());
            return ret * scalar;
         }
         return ret;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
