using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Models;

namespace Shared.Repositories
{
    public class EntityRepository
    {
        private IMongoDatabase _database;
        private IMongoCollection<Entity> _collection;

        public EntityRepository(string mongoConnectionString) {
            var mongoUrl = MongoUrl.Create(mongoConnectionString);
            
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            settings.GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard;

            var client = new MongoClient(settings);

            _database = client.GetDatabase(mongoUrl.DatabaseName);
            _collection = _database.GetCollection<Entity>("entities");
        }

        public async Task<Entity> Get(Guid key) {
            return await _collection.Find(e => e.Key == key).FirstOrDefaultAsync();
        }

        public async Task Upsert(Entity data) {
            await _collection.ReplaceOneAsync(e => e.Key == data.Key, data, new UpdateOptions() { IsUpsert = true });
        }
    }
}