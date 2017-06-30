using System;
using Shared.Messaging;
using Shared.Models;
using Shared.Repositories;

namespace Worker
{
    class Program
    {
        public static string AMQPUrl = Environment.GetEnvironmentVariable("AmqpUri") ?? "amqp://guest:guest@localhost:5672/";
        public static string QueueName = Environment.GetEnvironmentVariable("QueueName") ?? "ExampleQueue";
        public static string MongoUri = Environment.GetEnvironmentVariable("MongoUri") ?? "mongodb://localhost:27017/example";

        static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString()} Worker Initializing");
            var bus = new MessageBus(AMQPUrl);
            var repo = new EntityRepository(MongoUri);
            Console.WriteLine($"{DateTime.UtcNow.ToString()} Worker Initialized");
            
            Console.WriteLine($"{DateTime.UtcNow.ToString()} Worker Started");
            bus.Subscribe<CreateMessage>(QueueName, async model => {
                
                Console.WriteLine($"{DateTime.UtcNow.ToString()} Message Recieved");
                await repo.Upsert(new Entity {
                    Key = model.Id,
                    Data = new {
                        Created = model.CreatedAt,
                        Other = "Data"
                    }
                });
                Console.WriteLine($"{DateTime.UtcNow.ToString()} Message Handled");
            });
        }
    }
}
