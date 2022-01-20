using Docked.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;

namespace Docked.Util.MongoDB
{
   public class ProgramItemListDBManager
   {
      private IMongoCollection<ProgramItem> collection;

      public ProgramItemListDBManager(string collectionName = "program-list")
      {
         collection = MongoDBManager.Instance.GetCollection<ProgramItem>(collectionName);
      }

      public ProgramItemList ReadFromCollection()
      {
         var ret = new ProgramItemList();
         collection.Find(new BsonDocument()).ToList().ForEach(x =>
         {
            ret.ProgramItems.Add(x);
         });
         return ret;
      }

      public void WriteToCollection(ProgramItemList programItems)
      {
         collection.DeleteMany("{}");
         programItems.ProgramItems.ForEach(x => x.Icon.SaveIconToDB());
         collection.InsertMany(programItems.ProgramItems);
         ClearUnusedIcons(programItems.ProgramItems.Select(x => x.Icon.Id).ToList());
      }

      private void ClearUnusedIcons(List<ObjectId> objectIds)
      {
         IEnumerable<ObjectId> allIconIds = MongoDBManager.Instance.IconBucket.Find("{}").ToList().Select(x => x.Id);
         IEnumerable<ObjectId> unusedIconIds = allIconIds.Except(objectIds);
         foreach(var iconId in unusedIconIds)
            MongoDBManager.Instance.IconBucket.Delete(iconId);
      }
   }
}
