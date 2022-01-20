using Docked.Util;
using System.ComponentModel;
using System.Windows.Controls;

namespace Docked.UserControls.MainContentControls
{
   [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<MainContentControlBase, UserControl>))]
   public abstract class MainContentControlBase : UserControl
   {
      public string ControlName { get; private set; }

      protected MainContentControlBase(string controlName)
      {
         ControlName = controlName;
         Width = 480;
      }
   }
}
