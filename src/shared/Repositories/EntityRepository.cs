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
            var client = new MongoClient(mongoConnectionString);
            var mongoUrl = MongoUrl.Create(mongoConnectionString);

            _database = client.GetDatabase(mongoUrl.DatabaseName);
            _collection = _database.GetCollection<Entity>("entities");
        }

        public async Task<Entity> Get(Guid key) {
            return await _collection.Find(e => e.Id == key).FirstAsync();
        }

        public async Task Upsert(Entity data) {
            await _collection.ReplaceOneAsync(e => e.Id == data.Id, data, new UpdateOptions() { IsUpsert = true });
        }
    }
}