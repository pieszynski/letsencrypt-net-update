using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pieszynski.LenuAzureAspMiddleware.Example
{
    public class HomeController : ControllerBase
    {
        [Route("/")]
        public IActionResult Index()
            => new ContentResult() { Content = $"now: {DateTime.Now}" };
    }
}
