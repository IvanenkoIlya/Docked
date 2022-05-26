using System.Windows.Controls;

namespace Docked.Controls.UserControls
{
   /// <summary>
   /// Interaction logic for TestProgramItemControl.xaml
   /// </summary>
   public partial class TestProgramItemControl : UserControl
   {
      public static int Instances = 0;

      public TestProgramItemControl()
      {
         Instances++;
         InitializeComponent();
      }

      ~TestProgramItemControl()
      {
         Instances--;
      }
   }
}
