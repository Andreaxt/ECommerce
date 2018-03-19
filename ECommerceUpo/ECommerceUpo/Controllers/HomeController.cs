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

            //query top 10:
            //prende i prodotti che compaiono in acquisti con data > (data di oggi -1 mese) cioè ultimo mese
            var query = (from products in context.Product
                         join orderProducts in context.OrderProduct on products.ProductId equals orderProducts.ProductId
                         join orders in context.Order on orderProducts.OrderId equals orders.OrderId
                         where orders.Data > DateTime.Now.AddMonths(-1)
                         orderby orderProducts.Quantity
                         select products)
                         .GroupBy(p => p.ProductId).Select(g => g.First()).Take(10);
            //raggruppa per codice prodotto e prende solo il primo (per evitare duplicati)

            return View(await query.ToListAsync());
        }
    }
}
