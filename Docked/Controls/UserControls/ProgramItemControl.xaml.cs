using Docked.Controls.UserControls.MainContentControls;
using Docked.Model;
using Docked.Util;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Docked.Controls.UserControls
{
   /// <summary>
   /// Interaction logic for ProgramItemControl.xaml
   /// </summary>
   public partial class ProgramItemControl : UserControl, INotifyPropertyChanged, IDisposable
   {
      public static int Instances = 0;
      private static string lastImageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

      private bool _isPopupOpen;
      public bool IsPopupOpen
      {
         get => _isPopupOpen;
         set
         {
            if (_isPopupOpen == value)
               return;
            _isPopupOpen = value;
            AdjustEditBoxMargin();
            OnPropertyChanged();
         }
      }

      public ProgramItemControl()
      {
         Instances++;
         InitializeComponent();
         ExtraPopup.DataContext = this;
         Loaded += OnLoaded;
      }

      ~ProgramItemControl()
      {
         Instances--;
      }

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         Loaded -= OnLoaded;
         var animatedPanelParent = VisualTreeUtil.TryFindParent<AnimatedHexListPanel>(this);
         if (animatedPanelParent != null)
            animatedPanelParent.ArrangeStoryboard.Completed += AnimatedPanelAnimationCompleted;

         TagListBox.ItemsSource = (DataContext as ProgramItem).Tags;
      }

      // TODO move this functionality to ProgramItem class and trigger via command
      public void RunProgram(object sender, RoutedEventArgs e)
      {
         try
         {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = (DataContext as ProgramItem).ExecuteCommand;
            startInfo.Arguments = (DataContext as ProgramItem).ArgumentParameters;
            Process.Start(startInfo);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.ToString());
         }
      }

      public void TogglEditGrid(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         if (EditGrid.Visibility == Visibility.Collapsed || EditGrid.Visibility == Visibility.Hidden)
            EditGrid.Visibility = Visibility.Visible;
         else
            CloseEditGrid(this, null);
      }

      public void CloseEditGrid(object sender, RoutedEventArgs e)
      {
         EditBox.ApplyAnimationClock(WidthProperty,
            new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(100)) { EasingFunction = new SineEase() }.CreateClock());
         EditGrid.Visibility = Visibility.Collapsed;
      }

      private async void DeleteItem(object sender, RoutedEventArgs e)
      {
         var title = "Delete Confirmation";
         var message = $"Are you sure you want to delete \"{(DataContext as ProgramItem).ProgramName}\"";
         var window = (Application.Current.MainWindow as MetroWindow);
         var settings = new MetroDialogSettings()
         {
            MaximumBodyHeight = 50,
            AffirmativeButtonText = "Yes",
            NegativeButtonText = "No",
            CustomResourceDictionary = new ResourceDictionary()
            {
               Source = new Uri("/Docked;component/Resources/Styles/CustomMessageDialogStyle.xaml", UriKind.RelativeOrAbsolute)
            }
         };
         var result = await window.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, settings);

         if(result == MessageDialogResult.Affirmative)
         {
            Dispose();
         }
      }

      private void OpenEditGrid(object sender, RoutedEventArgs e)
      {
         EditGrid.Visibility = Visibility.Visible;
         EditBox.ApplyAnimationClock(WidthProperty,
               new DoubleAnimation(EditBox.Width, EditGrid.Width - EditBox.Margin.Left - EditBox.Margin.Right, TimeSpan.FromMilliseconds(150)) { EasingFunction = new SineEase() }.CreateClock());
      }

      private void AnimatedPanelAnimationCompleted(object sender, EventArgs e)
      {
         if (EditGrid.Visibility == Visibility.Visible)
            EditBox.ApplyAnimationClock(WidthProperty,
               new DoubleAnimation(EditBox.Width, EditGrid.Width - EditBox.Margin.Left - EditBox.Margin.Right, TimeSpan.FromMilliseconds(150)) { EasingFunction = new SineEase() }.CreateClock());
      }

      private void ChangeIcon(object sender, RoutedEventArgs e)
      {
         OpenFileDialog dlg = new OpenFileDialog
         {
            InitialDirectory = lastImageDirectory,
            FilterIndex = 5,
            Filter = $"{ImageUtil.ImageFilter}|All Files (*.*)|*.*",
            Multiselect = false
         };

         if (dlg.ShowDialog() == true)
         {
            lastImageDirectory = Path.GetDirectoryName(dlg.FileName);
            (DataContext as ProgramItem).SetIcon(dlg.FileName);
         }
      }

      private void ChangeColor(object sender, RoutedEventArgs e)
      {
         PopupTabControl.SelectedIndex = 0;
         IsPopupOpen = true;
      }

      private void SelectProgram(object sender, RoutedEventArgs e)
      {
         OpenFileDialog dlg = new OpenFileDialog()
         {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
            Filter = $"Executable (*.exe)|*.exe|All Files (*.*)|*.*",
            DefaultExt = ".exe",
            Multiselect = false
         };

         if (dlg.ShowDialog() == true)
         {
            (DataContext as ProgramItem).ExecuteCommand = dlg.FileName;
         }
      }

      private void OpenTagsPopup(object sender, RoutedEventArgs e)
      {
         PopupTabControl.SelectedIndex = 1;
         IsPopupOpen = true;
      }

      private void AdjustEditBoxMargin()
      {
         var corderRadius = new CornerRadius(0, 10, 0, 0);
         if(!IsPopupOpen)
            corderRadius.BottomRight = 10;
         EditBox.CornerRadius = corderRadius;
      }

      private void AddTag(object sender, RoutedEventArgs e)
      {
         if (!string.IsNullOrEmpty(AddTagText.Text))
            (DataContext as ProgramItem).Tags.Add(AddTagText.Text);
         AddTagText.Text = "";
      }

      private void RemoveTag(object sender, RoutedEventArgs e)
      {
         var item = VisualTreeUtil.TryFindParent<ListBoxItem>(e.OriginalSource as Button);
         if(item != null)
            (DataContext as ProgramItem).Tags.Remove((string)item.Content);
      }

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;
      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion

      #region IDisposable implementation
      public void Dispose()
      {
         var animatedPanelParent = VisualTreeUtil.TryFindParent<AnimatedHexListPanel>(this);
         if (animatedPanelParent != null)
            animatedPanelParent.ArrangeStoryboard.Completed -= AnimatedPanelAnimationCompleted;

         var programListControl = VisualTreeUtil.TryFindParent<ProgramListControl>(this);
         if (programListControl != null)
            programListControl.RemoveProgramItem(this);

         DataContext = null;
         GC.ReRegisterForFinalize(this);
      }
      #endregion
   }
}
