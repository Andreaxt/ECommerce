using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceUpo.Controllers
{
    public class HomeAdminController : Controller
    {
        /*
         * Passa semplicemente alla view
         */
        public IActionResult Index() => View();
    }
}