using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceUpo.Controllers
{
    public class HomeAdminController : Controller
    {
        //non fa niente, va solo alla vista
        public IActionResult Index() => View();
    }
}