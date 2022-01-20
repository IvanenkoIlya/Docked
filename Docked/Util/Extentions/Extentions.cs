using System;
using System.Collections.ObjectModel;

namespace Docked.Util.Extentions
{
   public static class ObservableCollectionExtentions
   {
      public static void Clear<T>(this ObservableCollection<T> collection, Action<ObservableCollection<T>> unhookAction)
      {
         unhookAction.Invoke(collection);
         collection.Clear();
      }
   }
}
