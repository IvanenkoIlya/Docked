﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Docked.Util.Converters
{
   public class VisibilityToBooleanConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var visibility = (Visibility)value;
         return visibility == Visibility.Visible;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
