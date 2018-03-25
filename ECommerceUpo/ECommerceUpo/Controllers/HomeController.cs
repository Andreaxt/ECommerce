using ECommerceUpo.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ECommerceUpo.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //query che da come risultato i top 10 più venduti dell'ultimo mese, prende solo 1 volta il prodotto con stesso id 
            var query = (from product in context.Product
                         join orderProduct in context.OrderProduct on product.ProductId equals orderProduct.ProductId
                         join orderTable in context.OrderTable on orderProduct.OrderId equals orderTable.OrderId
                         where orderTable.Data > DateTime.Now.AddMonths(-1)
                         orderby orderProduct.Quantity descending
                         select product)
                         .GroupBy(p => p.ProductId).Select(g => g.First()).Take(10);

            return View(await query.ToListAsync());
        }
    }
}
