using Docked.Model;
using Docked.Util;
using Docked.Util.Extentions;
using Docked.Util.MongoDB;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Docked.Controls.UserControls.MainContentControls
{
   /// <summary>
   /// Interaction logic for ProgramList.xaml
   /// </summary>
   public partial class ProgramListControl : MainContentControlBase, INotifyPropertyChanged
   {
      private static readonly string _controlName = nameof(ProgramListControl);
      private static string[] validFileTypes = new string[] { ".exe", ".lnk", ".url" };

      private ProgramItemListDBManager _dbManager;

      private bool _isTagsDropDownOpen;
      public bool IsTagsDropDownOpen
      {
         get => _isTagsDropDownOpen;
         set
         {
            if (_isTagsDropDownOpen == value)
               return;

            _isTagsDropDownOpen = value;
            OnPropertyChanged();
         }
      }

      private bool _searchBarOpen;
      public bool SearchBarOpen
      {
         get => _searchBarOpen;
         set
         {
            if (_searchBarOpen == value)
               return;
            _searchBarOpen = value;
            if (_searchBarOpen)
               OpenSearchBar();
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

      public ListCollectionView ProgramItemsCollectionView { get; set; }

      public ProgramListControl() : base(nameof(_controlName))
      {
         InitializeComponent();

         _dbManager = ProgramItemListDBManager.GetManager();
         ProgramItemList = _dbManager.ProgramItemList;

         EventManager.RegisterClassHandler(typeof(MetroWindow), Keyboard.KeyDownEvent, new KeyEventHandler(KeyDown), true);

         Loaded += OnLoaded;
         CreateProgramItemsCollectionView();

         //ProgramList.ItemsSource = ProgramItemsCollectionView;
         var comp = ProgramList.ItemsSource as CompositeCollection;
         var container = new CollectionContainer()
         {
            Collection = ProgramItemsCollectionView
         };
         comp.Insert(0, container);
      }

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         HideSearchBar(10);
      }

      private void CreateProgramItemsCollectionView()
      {
         ProgramItemsCollectionView = new ListCollectionViewEx(ProgramItemList.ProgramItems.Select(x => new ProgramItemControl() { DataContext = x }).ToList());
         ProgramItemsCollectionView.Filter = VisibleItemsFilter;
         ProgramItemsCollectionView.SortDescriptions.Add(new SortDescription("DataContext.ProgramName", ListSortDirection.Ascending));
      }

      private void AddNewProgramItem(object sender, RoutedEventArgs e)
      {
         var newProgramItem = new ProgramItem($"Program{ProgramItemList.ProgramItems.Count}", "");
         AddProgramItem(newProgramItem, true);
      }

      public void RemoveProgramItem(ProgramItemControl programItem)
      {
         ProgramItemsCollectionView.Remove(programItem);
         //_dbManager.DeleteFromCollection(programItem.DataContext as ProgramItem);
         ProgramItemList.ProgramItems.Remove(programItem.DataContext as ProgramItem);
      }

      private new void KeyDown(object sends, KeyEventArgs e)
      {
         if (Keyboard.FocusedElement is TextBox && Keyboard.FocusedElement != SearchBar)
            return;

         if (e.Key == Key.Enter && ProgramItemsCollectionView.Count == 1)
         {
            ((ProgramItemControl)ProgramItemsCollectionView.GetItemAt(0)).RunProgram(this, null);
            if (!string.IsNullOrEmpty(SearchBar.Text))
               SearchBar.Text = "";
            else
               SelectedTags.UnselectAll();
            HideSearchBar();
         }

         Keyboard.Focus(SearchBar);
         SearchBarOpen = true;

         if(e.Key == Key.Escape)
         {
            if (!string.IsNullOrEmpty(SearchBar.Text))
               SearchBar.Text = "";
            else
               SelectedTags.UnselectAll();
            HideSearchBar();
         }
      }

      private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
      {
         ProgramItemsCollectionView.Refresh();
      }

      private void BackgroundRightClick(object sender, MouseButtonEventArgs e)
      {
         if (e.OriginalSource is AnimatedHexListPanel)
            SearchBarOpen = true;
      }

      private void OnSelectedTagsChanged(object sender, SelectionChangedEventArgs e)
      {
         ProgramItemsCollectionView.Refresh();
      }

      private void OpenTagList(object sender, RoutedEventArgs e)
      {
         // TODO this kinda doesn't work
         IsTagsDropDownOpen = !IsTagsDropDownOpen;
      }

      private void RemoveTag(object sender, RoutedEventArgs e)
      {
         var item = VisualTreeUtil.TryFindParent<ListBoxItem>(e.OriginalSource as Button);
         SelectedTags.SelectedItems.Remove(item.Content);
         HideSearchBar();
      }

      #region Helper functions
      private void OpenSearchBar(int duration = 350)
      {
         if(SearchBarOpen)
            SearchPanel.ApplyAnimationClock(MarginProperty,
                    new ThicknessAnimation(new Thickness(10, 10, 10, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = new SineEase() }.CreateClock());
      }

      private void HideSearchBar(int duration = 350)
      {
         if(SelectedTags.SelectedItems.Count == 0 && string.IsNullOrEmpty(SearchBar.Text))
         {
            SearchPanel.ApplyAnimationClock(MarginProperty,
                    new ThicknessAnimation(new Thickness(10, -SearchPanel.ActualHeight, 10, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = new SineEase() }.CreateClock());
            SelectedTagsPopup.IsOpen = false;
            SearchBarOpen = false;
         }
      }

      private bool VisibleItemsFilter(object obj)
      {
         ProgramItem item = (ProgramItem)((ProgramItemControl)obj).DataContext;

         if (!item.ProgramName.ToLower().Contains(SearchBar.Text.ToLower()))
            return false;

         if (SelectedTags.SelectedItems.Count == 0)
            return true;

         return SelectedTags.SelectedItems.Cast<string>().All(x => item.Tags.Contains(x));
      }
      #endregion

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;
      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion

      private void ProgramListDragEnter(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            ProgamListDropOverlay.Visibility = Visibility.Visible;
      }

      private void ProgramListDragLeave(object sender, DragEventArgs e)
      {
         ProgamListDropOverlay.Visibility = Visibility.Hidden;
      }

      private void ProgramListDrop(object sender, DragEventArgs e)
      {
         ProgamListDropOverlay.Visibility = Visibility.Hidden;

         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(var file in files.Where(x => IsValidFileType(x)))
            {
               var programItem = ProgramItem.FromFile(file);
               AddProgramItem(programItem);
            }
         }
      }

      private void AddProgramItem(ProgramItem programItem, bool openItem = false)
      {
         ProgramItemList.ProgramItems.Add(programItem);
         var newProgramItemControl = new ProgramItemControl() { DataContext = programItem };
         if(openItem)
            newProgramItemControl.TogglEditGrid(this, null);
         ProgramItemsCollectionView.AddNewItem(newProgramItemControl);
         ProgramItemsCollectionView.CommitNew();
      }

      private bool IsValidFileType(string file)
      {
         return validFileTypes.Contains(file.Substring(file.LastIndexOf('.')));
      }
   }
}
