using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceUpo.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Controllers
{
    public class SmallPricesController : Controller
    {
        /*
         * Espone i prodotti in offerta
         */
        public async Task<IActionResult> SmallPrices()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //prende i prodotti con sconto > 0
            var query = from prodotto in context.Product
                        where prodotto.Discount > 0
                        select prodotto;

            return View(await query.ToListAsync());
        }
    }
}