using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using Shared.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Models;
using System.Threading;
using System.Diagnostics;

namespace NetCoreExample.Controllers
{
    [Route("api/[controller]")]
    public class EntitiesController : Controller
    {
        public static string AMQPUrl = Environment.GetEnvironmentVariable("AmqpUri") ?? "amqp://guest:guest@localhost:5672/";
        public static string QueueName = Environment.GetEnvironmentVariable("QueueName") ?? "ExampleQueue";
        public static string MongoUri = Environment.GetEnvironmentVariable("MongoUri") ?? "mongodb://localhost:27017/example";

        private static MessageBus _bus;
        private readonly EntityRepository _repo;
        private readonly ILogger _logger;

        public EntitiesController(ILogger<HomeController> logger) {
            try 
            {
                //This is lazy~
                lock(_bus)
                {
                    if(_bus == null) _bus = new MessageBus(AMQPUrl);
                }
                _repo = new EntityRepository(MongoUri);
            } 
            catch (Exception e) 
            {
                logger.LogError(1000, e, "Failed to create bus or repo");
            }
            
            _logger = logger;
        }

        // GET api/entities/GUID
        [Route("{guid:guid}")]
        public async Task<IActionResult> Get(Guid guid)
        {   
            var timeout = 2 * 60 * 1000; //2 minutes
            var interval = 5 * 1000; //5 seconds

            Entity data = await _repo.Get(guid);
            
            //This is awful, never do this, this is for demo purposes only.
            var watch = Stopwatch.StartNew();
            while(data == null && watch.ElapsedMilliseconds < timeout) {
                Thread.Sleep(interval);
                data = await _repo.Get(guid);
            }

            if(data == null) return NotFound();

            return Json(data);
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Guid? guid = null)
        {
            guid = guid ?? Guid.NewGuid();

            _bus.Publish(new CreateMessage { Id = guid.Value }, QueueName);

            return CreatedAtAction("Get", new { guid = guid });
        }
    }
}
