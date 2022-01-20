using Docked.Model;
using Docked.Util.MongoDB;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Docked.UserControls.MainContentControls
{
   /// <summary>
   /// Interaction logic for ProgramList.xaml
   /// </summary>
   public partial class ProgramListControl : MainContentControlBase, INotifyPropertyChanged
   {
      private static readonly string _controlName = nameof(ProgramListControl);

      private string _searchString = "";
      public string SearchString
      {
         get => _searchString;
         set
         {
            if (value == _searchString)
               return;

            _searchString = value;
            ProgramItems.Refresh();

            if (string.IsNullOrEmpty(_searchString))
               HideSearchBar();
            else
               ShowSearchBar();
         }
      }

      private ProgramItemList _programItemList;

      public ProgramItemList ProgramItemList {
         get => _programItemList; 
         set
         {
            _programItemList = value;
            OnPropertyChanged();
         }
      }

      public ListCollectionView ProgramItems;

      public ProgramListControl() : base(nameof(_controlName))
      {
         InitializeComponent();

         var manager = new ProgramItemListDBManager("program-list");
         ProgramItemList = manager.ReadFromCollection();

         EventManager.RegisterClassHandler(typeof(MetroWindow), Keyboard.KeyDownEvent, new KeyEventHandler(KeyDown), true);
         HideSearchBar(10);

         ReloadProgramItems();
      }

      private new void KeyDown(object sends, KeyEventArgs e)
      {
         if (!(Keyboard.FocusedElement is TextBox) || Keyboard.FocusedElement == SearchBar)
         {
            if (e.Key == Key.Escape)
            {
               HideSearchBar();
               SearchBar.Text = "";
            }
            Keyboard.Focus(SearchBar);
         }
         else
         {
            HideSearchBar();
         }
      }

      private void BackgroundRightClick(object sender, MouseButtonEventArgs e)
      {
         ShowSearchBar();
      }

      private void ReloadProgramItems()
      {
         ProgramItems = CollectionViewSource.GetDefaultView(ProgramItemList.ProgramItems) as ListCollectionView;
         ProgramItems.Filter = x => ((ProgramItem)x).ProgramName.ToLower().Contains(_searchString.ToLower());
         ProgramItems.SortDescriptions.Add(new SortDescription("ProgramName", ListSortDirection.Ascending));
         ProgramItems.IsLiveSorting = true;
         ProgramItems.IsLiveFiltering = true;

         ProgramList.ItemsSource = ProgramItems;
      }

      #region Helper functions
      private void ShowSearchBar(int duration = 350)
      {
         SearchPanel.ApplyAnimationClock(MarginProperty,
                    new ThicknessAnimation(new Thickness(10, 10, 10, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = new SineEase() }.CreateClock());
      }

      private void HideSearchBar(int duration = 350)
      {
         SearchPanel.ApplyAnimationClock(MarginProperty,
                    new ThicknessAnimation(new Thickness(10, -SearchPanel.ActualHeight, 10, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = new SineEase() }.CreateClock());
      }
      #endregion

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;
      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion
   }
}
