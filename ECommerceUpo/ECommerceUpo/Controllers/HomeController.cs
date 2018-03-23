using ECommerceUpo.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerceUpo.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //query che da come risultato i top 10 più venduti dell'ultimo mese, prende solo 1 volta il prodotto con stesso id 
            var query = (from products in context.Product
                         join orderProducts in context.OrderProduct on products.ProductId equals orderProducts.ProductId
                         join orders in context.Order on orderProducts.OrderId equals orders.OrderId
                         where orders.Data > DateTime.Now.AddMonths(-1)
                         orderby orderProducts.Quantity
                         select products)
                         .GroupBy(p => p.ProductId).Select(g => g.First()).Take(10);
         

            return View(await query.ToListAsync());
        }
    }
}
