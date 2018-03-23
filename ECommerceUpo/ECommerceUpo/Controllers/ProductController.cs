using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerceUpo.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerceUpo.Models;

namespace ECommerceUpo.Controllers
{
    public class ProductController : CrudController<ECommerceUpoContext, int, Product>
    {
        public ProductController(ECommerceUpoContext context, ILogger<ProductController> logger) : base(context, logger)
        {
        }

        protected override DbSet<Product> Entities => Context.Product;

        protected override Func<Product, int, bool> FilterById => (e, id) => e.ProductId == id;

        //Per utenti admin, si puo modificare prezzo/sconto/disponibilita di un prodotto
        [HttpPost]
        public async Task<IActionResult> Update(string previousUrl, string product, string price, string discount, string disp)
        {
            Product ToUpdate;

            //prende i parametri dal form
            Int32.TryParse(product, out int ProductId);
            double.TryParse(price, out double Price);
            double.TryParse(discount, out double Discount);

            //query che restituisce il prodotto nel db con id corrispondente a quello del form
            var query = from products in Context.Product
                        where products.ProductId.Equals(ProductId)
                        select products;

            //prende il primo e unico elemento restituito dalla query
            ToUpdate = query.First();

            //se prezzo/sconto/disponibilita sono cambiati allora vengono modificati nel record del db, si devono riempire tutti i campi
            if ((!(ToUpdate.Price == Price)) || (!(ToUpdate.Discount == Discount)) || (!(ToUpdate.Disp.Equals(disp))))
            {
                ToUpdate.Price = Price;
                ToUpdate.Discount = Discount;
                ToUpdate.Disp = disp;

                //modifica il record del db
                await base.Update(ToUpdate);
            }

            return Redirect("/Product/List");
        }


        //lista prodotti per admin
        public async Task<IActionResult> List() => View(await Entities.ToListAsync());

        //lista prodotti per user
        public async Task<IActionResult> Index() => View(await Entities.ToListAsync());

        //Mostra i dettagli di un prodotto
        public async Task<IActionResult> ProductDetails(int productId)
        {
            //prende il prodotto con id corrispondente al parametro 
            var query = from products in Context.Product
                        where products.ProductId.Equals(productId)
                        select products;

            return View(await query.ToListAsync());
        }

        //Elenca i prodotti il cui titolo o descrizione contengono la stringa cercata
        [HttpPost]
        public async Task<IActionResult> Search(string input)
        {
            var query = (from products in Context.Product
                         where products.Title.Contains(input) || products.Description.Contains(input)
                         select products).OrderByDescending(x => x.Price);

            return View(await query.ToListAsync());
        }
       
    }
}