using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Docked.Util.MongoDB
{
   public class MongoDBManager
   {
      private readonly string connectionString = "mongodb://127.0.0.1:27017";
      private readonly string dbName = "docked-config";

      private MongoClient client;
      private IMongoDatabase db;

      private static MongoDBManager _instance;
      public static MongoDBManager Instance
      {
         get
         {
            if (_instance == null)
               _instance = new MongoDBManager();
            return _instance;
         }
      }

      public GridFSBucket IconBucket;

      private MongoDBManager()
      {
         client = new MongoClient(connectionString);
         db = client.GetDatabase(dbName);
         IconBucket = new GridFSBucket(db, new GridFSBucketOptions
         {
            BucketName = "icons",
            ChunkSizeBytes = 1048576 //1MB
         });
      }

      public IMongoCollection<T> GetCollection<T>(string collectionName)
      {
         return db.GetCollection<T>(collectionName);
      }
   }
}
