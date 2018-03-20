using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ECommerceUpo.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using ECommerceUpo.Data;


namespace ECommerceUpo.Controllers
{
    public class CartController : Controller
    {
        /*
         * Mostra lo stato del carrello
         */
        public async Task<IActionResult> Index()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //legge codice prodotti salvati in session
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");

            //se non c'e' nessun prodotto in session, carrello vuoto
            if (SessionCart == null)
                return View(new List<CartBean>());

            //con i codici prodotto in session, recupera tutte le info sui prodotti da db
            var query = from prodotti in context.Product
                        join carrello in SessionCart on prodotti.ProductId equals carrello.ProductId
                        select new CartBean
                        {
                            ProductId = prodotti.ProductId,
                            Title = prodotti.Title,
                            Description = prodotti.Description,
                            Price = prodotti.Price,
                            Discount = prodotti.Discount,
                            Image = prodotti.Image,
                            Quantity = carrello.Quantity
                        };

            return View(await query.ToListAsync());
        }

        /*
         * Aggiunge un prodotto al carrello
         */
        [HttpPost]
        public IActionResult Add(string prodotto, int qta)
        {
            List<OrderProduct> newCart;

            Int32.TryParse(prodotto, out int cdprodotto);

            //controlla se c'e' gia' qualche prodotto nel carrello in session:
            var exCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            if (exCart == null)
            {
                //se non esiste, crea nuovo carrello aggiungendo il primo prodotto
                newCart = new List<OrderProduct>();
                newCart.Add(new OrderProduct { ProductId = cdprodotto, Quantity = qta });
            }
            //se invece esiste gia' un carrello:
            else
            {
                newCart = exCart;

                //cerca se esiste gia' il prodotto nella lista (nel carrello)
                var prod = newCart.FirstOrDefault(p => p.ProductId == cdprodotto);
                if (prod == null)
                {
                    //se non esiste, lo aggiunge come nuovo
                    newCart.Add(new OrderProduct { ProductId = cdprodotto, Quantity = qta });
                }
                //se esiste, incrementa la quantita'
                else
                {
                    newCart.Remove(prod);
                    newCart.Add(new OrderProduct { ProductId = cdprodotto, Quantity = prod.Quantity + qta });
                }
            }

            //salva carrello in session
            HttpContext.Session.SetObjectAsJson("Cart", newCart);

            return Redirect("/Carrello/Index");
        }

        /*
         * Rimuove prodotto dal carrello
         */
        [HttpPost]
        public IActionResult Remove(string prodotto)
        {
            Int32.TryParse(prodotto, out int cdprodotto);

            //legge il carrello
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");

            //rimuove il prodotto
            SessionCart.RemoveAll(x => x.ProductId == cdprodotto);

            //se ora il carrello e' vuoto, elimina oggetto in session
            if (SessionCart.Count() == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                //se non era l'ultimo prodotto, aggiorna il carrello in session
                HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
            }

            return Redirect("/Carrello/Index");
        }

        /*
         * Svuota tutto il carrello
         */
        [HttpGet]
        public IActionResult Empty()
        {
            HttpContext.Session.Remove("Cart");

            return Redirect("/Carrello/Index");
        }

        /*
         * Aggiorna quantita' di un prodotto nel carrello
         */
        [HttpPost]
        public IActionResult Update(string prodotto, int qta)
        {
            Int32.TryParse(prodotto, out int cdprodotto);

            //prende il carrello in session:
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            if (SessionCart != null)
            {
                //cerca il prodotto nella lista
                var prod = SessionCart.FirstOrDefault(p => p.ProductId == cdprodotto);
                if (prod != null)
                {
                    //rimuove temporaneamente il prodotto
                    SessionCart.Remove(prod);

                    //se quantita' da aggiornare e' 0, non ri-aggiunge il prodotto al carrello
                    if (qta != 0)
                    {
                        //altrimenti aggiunge nuovo prodotto nel carrello con la quantita' specificata
                        SessionCart.Add(new OrderProduct { ProductId = cdprodotto, Quantity = qta });
                        //salva "nuovo" carrello in session
                        HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
                    }
                    else
                    {
                        //se ora il carrello e' vuoto, elimina oggetto in session
                        if (SessionCart.Count() == 0)
                        {
                            HttpContext.Session.Remove("Cart");
                        }
                        else
                        {
                            //se quantita' era 0 e non era l'ultimo prodotto nel carrello, lo ha rimosso e ora aggiorna il carrello
                            HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
                        }
                    }
                }
            }

            return Redirect("/Carrello/Index");
        }
    }
}