using Docked.Model;
using Docked.Util;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Docked.UserControls
{
   /// <summary>
   /// Interaction logic for ProgramItemControl.xaml
   /// </summary>
   public partial class ProgramItemControl : UserControl, INotifyPropertyChanged
   {

      #region DependencyProperties
      public static readonly DependencyProperty ProgramItemProperty =
         DependencyProperty.Register("ProgramItem", typeof(ProgramItem), typeof(ProgramItemControl),
            new FrameworkPropertyMetadata(new ProgramItem()));

      public ProgramItem ProgramItem
      {
         get => (ProgramItem)GetValue(ProgramItemProperty);
         set => SetValue(ProgramItemProperty, value);
      }
      #endregion

      public ProgramItemControl()
      {
         InitializeComponent();

         Loaded += OnLoaded;
      }

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         var animatedPanelParent = VisualTreeUtil.TryFindParent<AnimatedHexListPanel>(this);
         animatedPanelParent.ArrangeStoryboard.Completed += AnimatedPanelAnimationCompleted;
      }

      private void RunProgram(object sender, RoutedEventArgs e)
      {
         try
         {
            Process.Start(ProgramItem.ExecuteCommand);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.ToString());
         }
      }

      private void ExpandEditGrid(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         HexButton.IsEnabled = false;
         EditGrid.Visibility = Visibility.Visible;
      }

      private void CloseEditGrid(object sender, RoutedEventArgs e)
      {
         EditBox.ApplyAnimationClock(WidthProperty,
            new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(100)) { EasingFunction = new SineEase() }.CreateClock());
         EditGrid.Visibility = Visibility.Collapsed;
         HexButton.IsEnabled = true;
      }

      private void AnimatedPanelAnimationCompleted(object sender, EventArgs e)
      {
         if(EditGrid.Visibility == Visibility.Visible && EditBox.Width == 0)
            EditBox.ApplyAnimationClock(WidthProperty,
               new DoubleAnimation(0, EditGrid.Width - EditBox.Margin.Left - EditBox.Margin.Right, TimeSpan.FromMilliseconds(150)) { EasingFunction = new SineEase() }.CreateClock());
      }

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;
      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion
   }
}
