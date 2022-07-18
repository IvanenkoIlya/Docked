using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Docked.Model
{
   public class ProgramItemList : IDisposable
   {
      #region Private properties
      private bool _disposed = false;

      private Dictionary<string, int> _allTagsInternal = new Dictionary<string, int>();
      #endregion

      #region Public properties
      public ObservableCollection<string> AllTags { get; private set; }
      public ObservableCollection<ProgramItem> ProgramItems { get; private set; }
      #endregion

      #region Constructors
      public ProgramItemList()
      {
         AllTags = new ObservableCollection<string>();
         ProgramItems = new ObservableCollection<ProgramItem>();
         ProgramItems.CollectionChanged += OnCollectionChanged;
      }
      #endregion

      #region Finalizer
      ~ProgramItemList()
      {
         if (!_disposed)
            Dispose();
      }
      #endregion

      #region EventHandlers
      private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         switch (e.Action)
         {
            case NotifyCollectionChangedAction.Add:
               AddNewItems(e.NewItems);
               break;
            case NotifyCollectionChangedAction.Remove:
               RemoveOldItems(e.OldItems);
               break;
            case NotifyCollectionChangedAction.Replace:
               RemoveOldItems(e.OldItems);
               AddNewItems(e.NewItems);
               break;
            case NotifyCollectionChangedAction.Reset:
            case NotifyCollectionChangedAction.Move:
            default:
               break;
         }
      }

      private void OnTagsChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         switch (e.Action)
         {
            case NotifyCollectionChangedAction.Add:
               foreach (string tag in e.NewItems)
                  AddTag(tag);
               break;
            case NotifyCollectionChangedAction.Remove:
               foreach (string tag in e.OldItems)
                  RemoveTag(tag);
               break;
            case NotifyCollectionChangedAction.Replace:
               foreach (string tag in e.NewItems)
                  AddTag(tag);
               foreach (string tag in e.OldItems)
                  RemoveTag(tag);
               break;
            case NotifyCollectionChangedAction.Reset:
            case NotifyCollectionChangedAction.Move:
            default:
               break;
         }
      }
      #endregion

      #region Private helper functions
      private void AddNewItems(IList items)
      {
         foreach (ProgramItem item in items)
         {
            item.Tags.CollectionChanged += OnTagsChanged;
            foreach (string tag in item.Tags)
               AddTag(tag);
         }
      }

      private void RemoveOldItems(IList items)
      {
         foreach (ProgramItem item in items)
         {
            item.Tags.CollectionChanged -= OnTagsChanged;
            foreach (string tag in item.Tags)
               RemoveTag(tag);
         }
      }

      private void AddTag(string tag)
      {
         if (!_allTagsInternal.ContainsKey(tag))
         {
            _allTagsInternal.Add(tag, 0);
            AllTags.Add(tag);
         }
         _allTagsInternal[tag]++;
      }

      private void RemoveTag(string tag)
      {
         if (_allTagsInternal[tag] == 1)
         {
            _allTagsInternal.Remove(tag);
            AllTags.Remove(tag);
         }
         else
            _allTagsInternal[tag]--;
      }
      #endregion

      #region Public helper functions
      public void ClearProgramItems(ObservableCollection<ProgramItem> collection)
      {
         _allTagsInternal.Clear();
         AllTags.Clear();
         foreach (var item in collection)
            item.Tags.CollectionChanged -= OnTagsChanged;
      }

      public void ClearTags(ObservableCollection<string> collection)
      {
         foreach (var tag in collection)
            RemoveTag(tag);
      }
      #endregion

      public override string ToString()
      {
         return $"AllTags: ({string.Join(", ", AllTags)})\n\t{string.Join("\n\t", ProgramItems)}";
      }

      public void Dispose()
      {
         ProgramItems.CollectionChanged -= OnCollectionChanged;
         _disposed = true;
      }
   }
}
