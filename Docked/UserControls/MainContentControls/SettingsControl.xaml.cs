using System.Windows.Controls;

namespace Docked.UserControls.MainContentControls
{
   /// <summary>
   /// Interaction logic for SettingsControl.xaml
   /// </summary>
   public partial class SettingsControl : MainContentControlBase
   {
      public SettingsControl() : base("Settings")
      {
         InitializeComponent();

         //foreach (var settings in DockedSettings.Instance.GetAllSettings())
         //{
         //   SettingsContentPanel.Children.Add(settings.Value.GetSettingsControl());
         //   SettingsContentPanel.Children.Add(new Separator() { Margin = new System.Windows.Thickness(5) });
         //}

         //SettingsContentPanel.Children.RemoveAt(SettingsContentPanel.Children.Count - 1);
      }
   }
}
