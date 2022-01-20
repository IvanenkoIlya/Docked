using System.Windows;
using System.Windows.Media;

namespace Docked.Util
{
   public class VisualTreeUtil
   {
      public static T TryFindParent<T>(DependencyObject current) where T : class
      {
         DependencyObject parent = VisualTreeHelper.GetParent(current);
         if (parent == null)
            parent = LogicalTreeHelper.GetParent(current);
         if (parent == null)
            return null;

         if (parent is T)
            return parent as T;
         else
            return TryFindParent<T>(parent);
      }

      public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
      {
         T child = default(T);

         int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
         for(int i = 0; i < numVisuals; i++)
         {
            Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null)
               child = GetVisualChild<T>(v);
            if (child != null)
               break;
         }

         return child;
      }
   }
}
