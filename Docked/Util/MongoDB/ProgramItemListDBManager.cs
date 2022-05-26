using Docked.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;

namespace Docked.Util.MongoDB
{
   public class ProgramItemListDBManager
   {
      private static Dictionary<string, ProgramItemListDBManager> _instances = new Dictionary<string, ProgramItemListDBManager>();

      private IMongoCollection<ProgramItem> collection;

      private ProgramItemList _pil;
      public ProgramItemList ProgramItemList
      {
         get
         {
            if (_pil == null)
               _pil = ReadFromCollection();
            return _pil;
         }
      }

      private ProgramItemListDBManager(string collectionName = "program-list")
      {
         collection = MongoDBManager.Instance.GetCollection<ProgramItem>(collectionName);
      }

      public static ProgramItemListDBManager GetManager(string collectionName = "program-list")
      {
         if (!_instances.ContainsKey(collectionName))
            _instances.Add(collectionName, new ProgramItemListDBManager(collectionName));
         return _instances[collectionName];
      }

      private ProgramItemList ReadFromCollection()
      {
         var ret = new ProgramItemList();
         collection.Find(new BsonDocument()).ToList().ForEach(x =>
         {
            ret.ProgramItems.Add(x);
         });
         return ret;
      }

      public static void UpdateDB()
      {
         foreach (var manager in _instances.Values)
         {
            var existingIds = manager.collection.Find("{}")
               .Project(Builders<ProgramItem>.Projection.Include(p => p.Id)).ToList();

            manager.UpdateEntries(existingIds);
            manager.WriteNewEntries(existingIds.Select(x => (ObjectId)x["_id"]).ToList());
            manager.RemoveUnusedEntries();
         }
      }


      public void UpdateEntries(List<BsonDocument> existingIds)
      {
         foreach (var id in existingIds)
         {
            var programItem = _pil.ProgramItems.FirstOrDefault(x => x.Id == (ObjectId)id["_id"]);

            if(programItem != null)
            {
               programItem.Icon.SaveIconToDB();
               collection.FindOneAndReplace(id, programItem);
            }
         }
      }

      public void WriteNewEntries(List<ObjectId> existingIds)
      {
         var newEntries = _pil.ProgramItems.Except(_pil.ProgramItems.Where(x => existingIds.Contains(x.Id))).ToList();
         
         if(newEntries.Count > 0)
         {
            foreach (var entry in newEntries)
               entry.Icon.SaveIconToDB();
            collection.InsertMany(newEntries);
         }
      }

      public void RemoveUnusedEntries()
      {
         string unusedProgramItemsFilter = "{_id:{$nin:[" + string.Join(",", _pil.ProgramItems.Select(x => $"ObjectId('{x.Id}')").ToArray()) + "]}}";
         var delRes = collection.DeleteMany(unusedProgramItemsFilter);
         RemoveUnusedIcons(_pil.ProgramItems.Select(x => x.Icon.Id).ToList());
      }

      public void DeleteFromCollection(ProgramItem programItem)
      {
         collection.DeleteOne(Builders<ProgramItem>.Filter.Eq("_id", programItem.Id));
      }

      public void RemoveUnusedIcons(List<ObjectId> objectIds)
      {
         IEnumerable<ObjectId> allIconIds = MongoDBManager.Instance.IconBucket.Find("{filename:{$nin:['defaultIcon']}}").ToList().Select(x => x.Id);
         IEnumerable<ObjectId> unusedIconIds = allIconIds.Except(objectIds);
         foreach (var iconId in unusedIconIds)
            MongoDBManager.Instance.IconBucket.Delete(iconId);
      }
   }
}
