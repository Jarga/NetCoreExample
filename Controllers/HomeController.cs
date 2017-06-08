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
        public static ConcurrentBag<Guid> ValidationSource = new ConcurrentBag<Guid>();

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

        public IActionResult TestCreate()
        {
            var guid = Guid.NewGuid();
            ValidationSource.Add(guid);

            return Json(new { Guid = guid });
        }

        [HttpPost]
        public IActionResult TestValidate(Guid guid)
        {
            return Json(new { exists = ValidationSource.Contains(guid) });
        }
    }
}
