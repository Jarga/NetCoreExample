using System;
using Shared.Messaging;
using Shared.Models;
using Shared.Repositories;

namespace Worker
{
    class Program
    {
        public static string AppUrl = Environment.GetEnvironmentVariable("AppUrl") ?? "http://localhost:5000/";
        public static string AMQPUrl = Environment.GetEnvironmentVariable("AMQPUrl") ?? "amqp://guest:guest@localhost:5672/";
        public static string QueueName = Environment.GetEnvironmentVariable("AMQPUrl") ?? "ExampleQueue";
        public static string MongoConnectionString = Environment.GetEnvironmentVariable("MongoConnectionString") ?? "MongoConnectionString";

        static void Main(string[] args)
        {
            var bus = new MessageBus(AMQPUrl);
            var repo = new EntityRepository(MongoConnectionString);
            
            bus.Subscribe<CreateMessage>(QueueName, async model => { 
                await repo.Upsert(new Entity {
                    Id = model.Id,
                    Data = new {
                        Created = model.CreatedAt,
                        Other = "Data"
                    }
                });
            });
        }
    }
}
