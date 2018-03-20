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

        /*
         * Modifica informazioni sul prodotto (SOLO ADMIN)
         */
        [HttpPost]
        public async Task<IActionResult> Update(string previousUrl, string product, string price, string discount, string disp)
        {
            Product ToUpdate;

            //riceve parametri dal form: codice prodotto, nuovo prezzo nuovo sconto, nuova disponibilita'
            Int32.TryParse(product, out int ProductId);
            double.TryParse(price, out double Price);
            double.TryParse(discount, out double Discount);

            //cerca nel db il prodotto da modificare
            var query = from products in Context.Product
                        where products.ProductId.Equals(ProductId)
                        select products;

            //prende il primo elemento (l'unico) della query
            ToUpdate = query.First();

            //modifica tutte le informazioni (solo se ci sono stati cambiamenti)
            //i campi del form non possono essere lasciati vuoti e i campi numerici devono contenere numeri, pertanto
            //si suppone che, a questo punto, i dati inseriti siano validi
            if ((!(ToUpdate.Price == Price)) || (!(ToUpdate.Discount == Discount)) || (!(ToUpdate.Disp.Equals(disp))))
            {
                ToUpdate.Price = Price;
                ToUpdate.Discount = Discount;
                ToUpdate.Disp = disp;

                //rende persistenti le modifiche
                await base.Update(ToUpdate);
            }

            return Redirect("/Product/List");
        }

        /*
         * Redirigono nelle pagine di elenco di tutti i prodotti
         */

        //solo per admin: pagina con i prodotti e pulsanti MODIFICA PRODOTTO
        public async Task<IActionResult> List() => View(await Entities.ToListAsync());

        //pagina con i prodotti e pulsante aggiungi per USER, no pulsanti per ADMIN
        public async Task<IActionResult> Index() => View(await Entities.ToListAsync());

        /*
         * Seleziona uno specifico prodotto e tutte le sue informazioni (codice prodotto nell'URL)
         */
        public async Task<IActionResult> ProductDetails(int productId)
        {
            //prende il prodotto che ha codice corrispondente al parametro 
            var query = from products in Context.Product
                        where products.ProductId.Equals(productId)
                        select products;

            return View(await query.ToListAsync());
        }

        /*
         * Seleziona l'insieme di prodotti il cui titolo o la cui descrizione contiene la stringa in input
         */
        [HttpPost]
        public async Task<IActionResult> Search(string input)
        {
            var query = (from products in Context.Product
                         where products.Title.Contains(input) || products.Description.Contains(input)
                         select products).OrderByDescending(x => x.Price);

            return View(await query.ToListAsync());
        }

        /*
         * Ricerca avanzata: usa lo stesso filtro di ordini e utenti, applicato ai prodotti.
         * Filtra, fra tutti i prodotti, quelli che rispecchiano le caratteristiche indicate in input (titolo, prezzo, sconto, disponibilita')
         */
        public async Task<IActionResult> Advanced(string apply, string clear, string title, string priceoperator, string price,
            string discount, string disp)
        {
            //seleziona tutti i prodotti
            var Query = from products in Context.Product
                        select products;

            bool filtered = false;

            //FILTRO: custom IQueryable extension method
            //Query = Query.FilterProd(ref filtered, clear, title, disp, priceoperator, price, discount);

            //per sapere se e' la prima volta che si apre a pagina o se si sta applicando un filtro
            if (apply != null)
            {
                filtered = true;
            }
            TempData["AdvancedFilter"] = filtered.ToString();

            return View(await Query.ToListAsync());
        }
    }
}