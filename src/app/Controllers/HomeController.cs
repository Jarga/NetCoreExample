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

namespace NetCoreExample.Controllers
{
    public class HomeController : Controller
    {
        public static string AMQPUrl = Environment.GetEnvironmentVariable("AMQPUrl") ?? "amqp://guest:guest@localhost:5672/";
        public static string QueueName = Environment.GetEnvironmentVariable("AMQPUrl") ?? "ExampleQueue";
        public static string MongoConnectionString = Environment.GetEnvironmentVariable("MongoConnectionString") ?? "MongoConnectionString";

        private readonly MessageBus _bus;
        private readonly EntityRepository _repo;
        private readonly ILogger _logger;

        public HomeController(ILogger<HomeController> logger) {
            try 
            {
                //This is lazy~
                _bus = new MessageBus(AMQPUrl);
                _repo = new EntityRepository(MongoConnectionString);
            } 
            catch (Exception e) 
            {
                logger.LogError(1000, e, "Failed to create bus or repo");
            }
            
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Get(Guid guid)
        {   var data = _repo.Get(guid);
            
            if(data == null) return NotFound();

            return Json(data);
        }

        [HttpPut]
        public IActionResult Create(Guid? guid = null)
        {
            guid = guid ?? Guid.NewGuid();

            _bus.Publish(new CreateMessage { Id = guid.Value }, QueueName);

            return CreatedAtAction("Get", new { guid = guid });
        }
    }
}
