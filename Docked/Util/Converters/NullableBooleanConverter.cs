using System;
using System.Globalization;
using System.Windows.Data;

namespace Docked.Util.Converters
{
   public class NullableBooleanConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var b = value as bool?;
         if (b == null)
            throw new ArgumentException("Nullable bool in null state, cannot convert");
         return (bool)b;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (bool?)value;
      }
   }
}
