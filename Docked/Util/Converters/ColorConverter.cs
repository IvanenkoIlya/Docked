using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Docked.Util.Converters
{
   public class ColorDarkerConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var color = ((SolidColorBrush)value).Color;
         var coef = double.Parse(parameter.ToString());
         if (coef < 0.0 || coef > 1.0)
            throw new ArgumentException("Parameter must be between 0.0 and 1.0");
         coef = 1.0 - coef;
         return new SolidColorBrush(Color.FromArgb(color.A, 
            (byte)(color.R * coef), 
            (byte)(color.G * coef), 
            (byte)(color.B * coef)));
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

   public class ColorLighterConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var color = ((SolidColorBrush)value).Color;
         var coef = double.Parse(parameter.ToString());
         if (coef < 0.0 || coef > 1.0)
            throw new ArgumentException("Parameter must be between 0.0 and 1.0");
         return new SolidColorBrush(Color.FromArgb(color.A, 
            (byte)((255- color.R) * coef + color.R),
            (byte)((255 - color.G) * coef + color.G),
            (byte)((255 - color.B) * coef + color.B)));
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

   public class BrushToColorConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var brush = (value as SolidColorBrush);
         if (brush == null)
            throw new ArgumentException("Value must be a SolidColorBrush");
         return brush.Color;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var color = (Color)value;
         if(color == null)
            throw new ArgumentException("Value must be a Color");
         return new SolidColorBrush(color);
      }
   }
}
