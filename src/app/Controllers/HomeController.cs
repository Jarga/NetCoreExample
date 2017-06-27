using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreExample.Controllers
{
    public class HomeController : Controller
    {
        public static ConcurrentDictionary<Guid, object> ValidationSource = new ConcurrentDictionary<Guid, object>();

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
        {   var data = ValidationSource?[guid];
            
            if(data == null) return NotFound();

            return Json(data);
        }

        [HttpPut]
        public IActionResult Put(Guid guid, object data)
        {
            ValidationSource.AddOrUpdate(guid, data, (key, value) => data );

            return CreatedAtAction("Get", new { guid = guid });
        }

        [HttpPost]
        public IActionResult Exists(Guid guid)
        {
            return Json(new { exists = ValidationSource.ContainsKey(guid) });
        }
    }
}
