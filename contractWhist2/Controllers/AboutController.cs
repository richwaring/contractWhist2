using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using contractWhist2.Models;

namespace contractWhist2.Controllers
{
    public class AboutController : Controller
    {
        private readonly ILogger<HomeController> _logger;

 
        public IActionResult Index()
        {
            return View();
        }

    }
}
