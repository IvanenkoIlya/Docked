using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Docked.Util.Converters
{
   public class MarginWidthSubtractionConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         double margins = double.Parse((string)parameter);
         double leftMargin = (double)value / 2.0;
         return new Thickness(leftMargin, margins, margins, margins);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
