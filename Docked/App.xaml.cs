using ControlzEx.Theming;
using System.Windows;

namespace Docked
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      protected override void OnStartup(StartupEventArgs e)
      {
         ThemeManager.Current.ChangeTheme(this, "BaseDark", "Blue");

         base.OnStartup(e);
      }
   }
}
