using Docked.Util;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Docked.Model
{
   public class ProgramItemList
   {
      public ObservableDictionary<string, int> AllTags { get; private set; }
      public ObservableCollection<ProgramItem> ProgramItems { get; private set; }

      public ProgramItemList()
      {
         AllTags = new ObservableDictionary<string, int>();
         ProgramItems = new ObservableCollection<ProgramItem>();
         ProgramItems.CollectionChanged += OnCollectionChanged;
      }

      ~ProgramItemList()
      {
         ProgramItems.CollectionChanged -= OnCollectionChanged;
      }

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
         if (!AllTags.ContainsKey(tag))
            AllTags.Add(tag, 0);
         AllTags[tag]++;
      }

      private void RemoveTag(string tag)
      {
         if (AllTags[tag] == 1)
            AllTags.Remove(tag);
         else
            AllTags[tag]--;
      }

      public void ClearProgramItems(ObservableCollection<ProgramItem> collection)
      {
         AllTags.Clear();
         foreach (var item in collection)
            item.Tags.CollectionChanged -= OnTagsChanged;
      }

      public void ClearTags(ObservableCollection<string> collection)
      {
         foreach (var tag in collection)
            RemoveTag(tag);
      }

      public override string ToString()
      {
         return $"AllTags: ({string.Join(", ", AllTags)})\n\t{string.Join("\n\t", ProgramItems)}";
      }
   }
}
