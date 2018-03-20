using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerceUpo.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerceUpo.Models;
using Microsoft.AspNetCore.Http;

namespace ECommerceUpo.Controllers
{
    public class OrderController : CrudController<ECommerceUpoContext, int, Order>
    {
        public OrderController(ECommerceUpoContext context, ILogger<OrderController> logger) : base(context, logger)
        {
        }

        protected override DbSet<Order> Entities => Context.Order;

        protected override Func<Order, int, bool> FilterById => (e, id) => e.OrderId == id;

        /*
         * Crea un nuovo ordine (CHECKOUT)
         */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //Se non si e' loggati redirige alla login, quando si tenta di acquistare
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return Redirect("/User/Login");
            }

            //legge il carrello da session
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            if (SessionCart == null)
            {
                //se il carrello e' vuoto non si puo' eseguire l'acquisto: rimanda alla pagina del carrello
                return Redirect("/Cart/Index");
            }

            //legge cdUtente da session
            int utente = 0;
            var tmp = HttpContext.Session.GetInt32("UserId");
            if (tmp != null)
                utente = (int)tmp;

            //JOIN dei prodotti in carrello con quelli nel db, per ritrovarne tutte le informazioni (es, per calcolo totale)
            var AddProductsDb = from prodotti in context.Product
                                join carrello in SessionCart on prodotti.ProductId equals carrello.ProductId
                                select new { Price = prodotti.Price, Discount = prodotti.Discount, Quantity = carrello.Quantity };

            //calcola il totale sommando tutti i prezzi scontati dei prodotti nel carrello
            double totale = AddProductsDb.Sum(x => (x.Price - x.Discount) * x.Quantity);

            //crea tabella di link fra ordini e prodotti
            List<OrderProduct> ordProd = new List<OrderProduct>();

            //aggiunge tutti i prodotti da acquistare con relativa quantita'
            foreach (var prod in SessionCart)
            {
                ordProd.Add(new OrderProduct
                {
                    ProductId = prod.ProductId,
                    Quantity = prod.Quantity
                });
            }

            //crea un nuovo ordine collegato all'utente e collega l'elenco di prodotti OrdineProdotto
            Order ordine = new Order
            {
                UserId = utente,
                State = "sent",                 //di default un nuovo ordine assume stato "sent"
                Data = DateTime.Now,   //inserisce con la data corrente
                TotalPrice = totale,
                OrderProduct = ordProd
            };

            //rende persistenti le modifiche chiamando il metodo Create del CrudController
            await base.Create(ordine);

            //rimuove carrello in session
            HttpContext.Session.Remove("Cart");

            return Redirect("/Order/Index");
        }

        /*
         * Modifica proprieta' di un ordine (ADMIN)
         */
        [HttpPost]
        public async Task<IActionResult> Update(string ordine, string stato)
        {
            //riceve parametri dal form
            Int32.TryParse(ordine, out int OrderId);
            Order ToUpdate;

            //cerca nel db l'ordine con codice corrispondente a quello passato dal form
            var query = from ordini in Context.Order
                        where ordini.OrderId.Equals(OrderId)
                        select ordini;

            //prende il primo elemento (l'unico) della query
            ToUpdate = query.First();

            //modifica stato solo se diverso dal precedente
            if (!ToUpdate.State.Equals(stato))
            {
                ToUpdate.State = stato;

                //rende persistente chiamando il metodo Update del CrudController
                await base.Update(ToUpdate);
            }

            return Redirect("/Order/List");
        }


        /*
         * Espone tutti gli ordini di un utente (SOLO USER). Possibilita' di filtrare gli ordini: 
         * se chiamato con parametri (HTTP GET con parametri nell'URL) allora usa i parametri per filtrare
         */
        public async Task<IActionResult> Index(string clear, string start, string end,
            string titolo, string qtaoperator, string qta, string totoperator, string tot, string stato)
        {
            int UserId = 0;
            //prende cdUtente da session
            var tmp = HttpContext.Session.GetInt32("UserId");
            if (tmp != null)
                UserId = (int)tmp;
            //query: tutti gli ordini di un certo utente
            var Query = UserQuery(UserId);
            bool filtered = false;

            //FILTRO: custom IQueryable extension method
            //Query = Query.FilterOrder(ref filtered, clear, start, end, titolo, qtaoperator, qta, totoperator, tot, stato);

            //view deve sapere se e' stato applicato un filtro o no: salva in Request scope
            TempData["OrdineFilter"] = filtered.ToString();

            return View(await Query.ToListAsync());
        }

        /*
         * Espone tutti gli ordini di tutti gli utenti (SOLO ADMIN). Possibilita' di filtrare, logica di prima
         */
        [HttpGet]
        public async Task<IActionResult> List(string clear, string start, string end,
            string titolo, string qtaoperator, string qta, string totoperator, string tot, string stato)
        {
            //prende tutti gli ordini di tutti gli utenti
            var Query = AdminQuery();
            bool filtered = false;

            //FILTRO: custom IQueryable extension method
            //Query = Query.FilterOrder(ref filtered, clear, start, end, titolo, qtaoperator, qta, totoperator, tot, stato);

            TempData["OrdineFilter"] = filtered.ToString();

            return View(await Query.ToListAsync());
        }


        /*
         * Usata da List: restituisce tutti gli ordini di tutti gli utenti, in JOIN con i prodotti che contiene
         *     e username di chi ha acquistato
         */
        private IQueryable<OrderBean> AdminQuery()
        {
            var q = from ordini in Context.Order
                    join utenti in Context.User on ordini.UserId equals utenti.UserId
                    join ordineProdotto in Context.OrderProduct on ordini.OrderId equals ordineProdotto.OrderId
                    join prodotti in Context.Product on ordineProdotto.ProductId equals prodotti.ProductId
                    select new OrderBean
                    {
                        OrderId = ordini.OrderId,
                        State = ordini.State,
                        Email = utenti.Email,
                        Title = prodotti.Title,
                        ProductId = prodotti.ProductId,
                        Data = ordini.Data,
                        Quantity = ordineProdotto.Quantity,
                        TotalPrice = ordini.TotalPrice
                    };

            return q;
        }

        /*
         * Usata da Index: restituisce tutti gli ordini effettuati da un utente specifico,
         *     con anche tutti i prodotti acquistati
         */
        private IQueryable<OrderBean> UserQuery(int UserId)
        {
            //invece di restituire solo gli ordini, fa una join per aggiungere altre informazioni
            var q = from ordini in Context.Order
                    join utenti in Context.User on ordini.UserId equals utenti.UserId
                    join ordineProdotto in Context.OrderProduct on ordini.OrderId equals ordineProdotto.OrderId
                    join prodotti in Context.Product on ordineProdotto.ProductId equals prodotti.ProductId
                    where utenti.UserId.Equals(UserId)
                    select new OrderBean
                    {
                        OrderId = ordini.OrderId,
                        State = ordini.State,
                        Email = utenti.Email,
                        Title = prodotti.Title,
                        ProductId = prodotti.ProductId,
                        Data = ordini.Data,
                        Quantity = ordineProdotto.Quantity,
                        TotalPrice = ordini.TotalPrice
                    };

            return q;
        }
    }
}