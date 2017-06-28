using System;
using Shared.Messaging;
using Shared.Models;
using Shared.Repositories;

namespace Worker
{
    class Program
    {
        public static string AppUrl = Environment.GetEnvironmentVariable("AppUrl") ?? "http://localhost:5000/";
        public static string AMQPUrl = Environment.GetEnvironmentVariable("AmqpUri") ?? "amqp://guest:guest@localhost:5672/";
        public static string QueueName = Environment.GetEnvironmentVariable("QueueName") ?? "ExampleQueue";
        public static string MongoUri = Environment.GetEnvironmentVariable("MongoUri") ?? "mongodb://localhost:27017/example";

        static void Main(string[] args)
        {
            Console.WriteLine("Worker Initializing");
            var bus = new MessageBus(AMQPUrl);
            var repo = new EntityRepository(MongoUri);
            Console.WriteLine("Worker Initialized");
            
            Console.WriteLine("Worker Started");
            bus.Subscribe<CreateMessage>(QueueName, async model => {
                
                Console.WriteLine("Message Recieved");
                await repo.Upsert(new Entity {
                    Key = model.Id,
                    Data = new {
                        Created = model.CreatedAt,
                        Other = "Data"
                    }
                });
                Console.WriteLine("Message Handled");
            });
        }
    }
}
