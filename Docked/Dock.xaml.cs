using Docked.Controls.UserControls;
using Docked.Controls.UserControls.MainContentControls;
using Docked.Util.MongoDB;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Docked
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class Dock : MetroWindow, INotifyPropertyChanged
   {
      private string _appName = "Docked";
      private bool _startWithWindows;
      private bool _stayOpen;
      private bool _closing = false;
      private int _contentControlIndex;
      public double DesktopHeight { get; private set; } = SystemParameters.WorkArea.Height;
      public ObservableCollection<MainContentControlBase> MainContentControls;

      public bool StayOpen
      {
         get => _stayOpen;
         set
         {
            if(_stayOpen == value)
               return;
            _stayOpen = value;
            OnPropertyChanged();
         }
      }

      public bool StartWithWindows
      {
         get => _startWithWindows;
         set
         {
            if (value != _startWithWindows)
            {
               _startWithWindows = value;
               UpdateWindowsStartup();
               OnPropertyChanged();
            }
         }
      }

      public Dock()
      {
         LoadContext();
         InitializeComponent();

         Left = SystemParameters.WorkArea.Width - Width;

         DockedFlyout.ClosingFinished += (sender, args) => 
         { 
            _closing = false;
            UpdateLayout();
            Dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
         };

         MainContentControls = new ObservableCollection<MainContentControlBase>(GetAllMainContentControls());
         MainContentControl.Content = MainContentControls.First();
         _contentControlIndex = 0;
      }

      private void LoadContext()
      {
         RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
         _startWithWindows = rk.GetValue(_appName) != null;
      }

      private IEnumerable<MainContentControlBase> GetAllMainContentControls(params object[] constructorArgs)
      {
         return Assembly.GetAssembly(typeof(MainContentControlBase))
                        .GetTypes()
                        .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(MainContentControlBase)))
                        .Select(type => (MainContentControlBase)Activator.CreateInstance(type, constructorArgs))
                        .ToList();
      }

      private void UpdateWindowsStartup()
      {
         RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

         if (StartWithWindows)
            rk.SetValue(_appName, Process.GetCurrentProcess().MainModule.FileName);
         else
            rk.DeleteValue(_appName, false);
      }

      private void OnTrayIconClicked(object sender, RoutedEventArgs e)
      {
         if (!_closing)
         {
            Activate();
            DockedFlyout.IsOpen = !DockedFlyout.IsOpen;
         }
      }

      private void OnNextItemClicked(object sender, RoutedEventArgs e)
      {
         _contentControlIndex++;
         if (_contentControlIndex >= MainContentControls.Count)
            _contentControlIndex = 0;
         MainContentControl.Content = MainContentControls[_contentControlIndex];
      }

      private void CloseAll(object sender, RoutedEventArgs e)
      {
         if(MainContentControl.Content is ProgramListControl)
         {
            foreach(ProgramItemControl item in ((ProgramListControl)MainContentControl.Content).ProgramItemsCollectionView.SourceCollection)
            {
               item.CloseEditGrid(this, null);
            }
         }
      }

      protected override void OnDeactivated(EventArgs e)
      {
         if (DockedFlyout.IsOpen && !StayOpen)
         {
            _closing = true;
            DockedFlyout.IsOpen = false;
            ProgramItemListDBManager.UpdateDB();
         }

         base.OnDeactivated(e);
      }

      private void Close(object sender, RoutedEventArgs e)
      {
         Close();
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         TaskbarIcon.Visibility = Visibility.Collapsed;
         ProgramItemListDBManager.UpdateDB();

         base.OnClosing(e);
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
