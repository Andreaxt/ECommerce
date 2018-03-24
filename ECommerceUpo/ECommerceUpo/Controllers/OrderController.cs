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
    public class OrderController : CrudController<ECommerceUpoContext, int, OrderTable>
    {
        public OrderController(ECommerceUpoContext context, ILogger<OrderController> logger) : base(context, logger)
        {
        }

        protected override DbSet<OrderTable> Entities => Context.OrderTable;

        protected override Func<OrderTable, int, bool> FilterById => (e, id) => e.OrderId == id;

        //crea un nuovo ordine
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //Controlla che l'utente sia loggato se no manda alla pagina di login
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return Redirect("/User/Login");
            }

            //legge il carrello 
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            if (SessionCart == null)
            {
                //se il carrello e' vuoto manda alla pagina principale del carrello
                return Redirect("/Cart/Index");
            }

            //legge idUtente dal bean 
            int utente = 0;
            var tmp = HttpContext.Session.GetInt32("UserId");
            if (tmp != null)
                utente = (int)tmp;

            //query per ottenere le informazioni dei prodotti nel carrello
            var AddProductsDb = from prodotti in context.Product
                                join carrello in SessionCart on prodotti.ProductId equals carrello.ProductId
                                select new { Price = prodotti.Price, Discount = prodotti.Discount, Quantity = carrello.Quantity };

            //calcola il prezzo totale tenendo conto di eventuali sconti
            double totale = AddProductsDb.Sum(x => (x.Price - x.Discount) * x.Quantity);

            //oggetto che contiene i prodotti dell'ordine
            List<OrderProduct> ordProd = new List<OrderProduct>();

            //prende i prodotti nel carrello e li aggiunge all'oggetto
            foreach (var prod in SessionCart)
            {
                ordProd.Add(new OrderProduct
                {
                    ProductId = prod.ProductId,
                    Quantity = prod.Quantity
                });
            }

            //associo ordine all'utente
            OrderTable ordine = new OrderTable
            {
                UserId = utente,
                State = "sent",                 
                Data = DateTime.Now,   
                TotalPrice = totale,
                OrderProduct = ordProd
            };

            //crea un record nel dataBase
            await base.Create(ordine);

            //finito l'acquisto rimuovo il carrello 
            HttpContext.Session.Remove("Cart");

            return Redirect("/Order/Index");
        }

        //modifica lo stato del prodotto
        [HttpPost]
        public async Task<IActionResult> Update(string ordine, string stato)
        {
            //prende i parametri dal form
            Int32.TryParse(ordine, out int OrderId);
            OrderTable ToUpdate;

            //query per trovare l'ordine corrispondete al form
            var query = from ordini in Context.OrderTable
                        where ordini.OrderId.Equals(OrderId)
                        select ordini;

            //prende il primo e unico elemento che la query restituisce
            ToUpdate = query.First();

            //modifica lo stato solo se è diverso
            if (!ToUpdate.State.Equals(stato))
            {
                ToUpdate.State = stato;

                // modifica il record del database
                await base.Update(ToUpdate);
            }

            return Redirect("/Order/List");
        }


        //Per utenti di tipo user mostra gli ordini fatti
        public async Task<IActionResult> Index()
        {
            int UserId = 0;
            //prende idUtente da bean
            var tmp = HttpContext.Session.GetInt32("UserId");
            if (tmp != null)
                UserId = (int)tmp;
            //query che restituisce tutti gli ordini effettuati da un utente
            var Query = UserQuery(UserId);

            return View(await Query.ToListAsync());
        }

        //Per utenti di tipo Admin mostra tutti gli ordini fatti da tutti gli utenti User
        // si possono filtrare per stato
        [HttpGet]
        public async Task<IActionResult> List(string clear, string state)
        {
            //Query restituisce tutti gli ordini degli User
            var Query = AdminQuery();
            bool filtered = false;

            //Filtra ordini per stato
            Query = Query.FilterState(ref filtered, clear, state);

            TempData["OrdineFilter"] = filtered.ToString();

            return View(await Query.ToListAsync());
        }


        //Query che restituisce tutti gli ordini degli User
        private IQueryable<OrderBean> AdminQuery()
        {
            var q = from ordini in Context.OrderTable
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

       
        //Query che ritorno tutti gli ordini fatti da un Utente User
        private IQueryable<OrderBean> UserQuery(int UserId)
        {
           
            var q = from ordini in Context.OrderTable
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