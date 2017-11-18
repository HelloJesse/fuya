using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using GetSingleShipInfo.Models;


namespace GetSingleShipInfo.Data
{
    public class TrackInfoRepository
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private string _collectionName = "track_infos";
        private IMongoCollection<TrackInfo> _collection;


        private void CheckAndCreateCollection()
        {
            var collectionList = _database.ListCollections().ToList();
            var collectionNames = new List<string>();

            collectionList.ForEach(b => collectionNames.Add(b["name"].AsString));
            if (!collectionNames.Contains(_collectionName))
            {
                _database.CreateCollection(_collectionName);
            }
            else
            {
                _collection = _database.GetCollection<TrackInfo>(_collectionName);
            }
        }

        public TrackInfoRepository()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("fuya");

            CheckAndCreateCollection();
        }

        public bool ExistsByShipIdAndLasttime(long shipId, long lasttime)
        {
            var list = _collection.Find(t => t.ShipId == shipId && t.Lasttime == lasttime).FirstOrDefault();
            return list != null;
        }

        public List<TrackInfo> GetLatest(int number)
        {
            return _collection.Find(bason => true)
                .SortByDescending(t => t.Lasttime)
                .Limit(number)
                .ToList();
        }

        public void Insert(TrackInfo trackInfo)
        {
            var collection = _database.GetCollection<TrackInfo>(_collectionName);
            collection.InsertOne(trackInfo);
        }

    }
}
