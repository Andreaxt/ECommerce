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
        //Pagina principale x il carrello con il contenuto attuale
        public async Task<IActionResult> Index()
        {
            ECommerceUpoContext context = new ECommerceUpoContext();

            //legge id prodotti dal bean del carrello che è in sessione se è pieno altrimenti ne crea uno nuovo
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");

            if (SessionCart == null)
                return View(new List<CartBean>());

            //query per recuperare tutti i dati realitivi ai prodotti a partire dal loro id che abbiamo in sessione nel carrello
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

        //Aggiunge un prodotto al carrello
        [HttpPost]
        public IActionResult Add(string prodotto, int quantita)
        {
            List<OrderProduct> newCart;
            Int32.TryParse(prodotto, out int idproduct);

            //controlla se c'è gia qualcosa nel carrello
            var existingCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            //se il carrello non esiste ancora lo crea e aggiunge il primo prodotto
            if (existingCart == null)
            {
                newCart = new List<OrderProduct>();
                newCart.Add(new OrderProduct { ProductId = idproduct, Quantity = quantita });
            }
            //se esiste gia il carrello
            else
            {
                newCart = existingCart;
                //controlla se il prodotto è gia presente nel carrello
                var prod = newCart.FirstOrDefault(p => p.ProductId == idproduct);
                //se non è gia presente lo aggiunge
                if (prod == null)
                {
                    newCart.Add(new OrderProduct { ProductId = idproduct, Quantity = quantita });
                }
                //se è gia presente aumenta la quantita
                else
                {
                    newCart.Remove(prod);
                    newCart.Add(new OrderProduct { ProductId = idproduct, Quantity = prod.Quantity + quantita });
                }
            }

            //salva carrello in session
            HttpContext.Session.SetObjectAsJson("Cart", newCart);

            return Redirect("/Cart/Index");
        }

        //Rimuove un prodotto da carrello
        [HttpPost]
        public IActionResult Remove(string prodotto)
        {
            Int32.TryParse(prodotto, out int idproduct);

            //legge il carrello
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");

            //rimuove il prodotto
            SessionCart.RemoveAll(x => x.ProductId == idproduct);

            //se ora il carrello è vuoto viene eliminato
            if (SessionCart.Count() == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                //se non è vuoto aggiorna si aggiorna il carrello
                HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
            }

            return Redirect("/Cart/Index");
        }

        //Svuota il carrello
        [HttpGet]
        public IActionResult Empty()
        {
            HttpContext.Session.Remove("Cart");

            return Redirect("/Cart/Index");
        }

        //Aggiorna le quantita dei prodotti nel carrello
        [HttpPost]
        public IActionResult Update(string prodotto, int quantita)
        {
            Int32.TryParse(prodotto, out int idproduct);

            //legge il carrello in sessione
            var SessionCart = HttpContext.Session.GetObjectFromJson<List<OrderProduct>>("Cart");
            if (SessionCart != null)
            {
                //cerca il prodotto nel carrello
                var prod = SessionCart.FirstOrDefault(p => p.ProductId == idproduct);
                if (prod != null)
                {
                    //rimuove temporaneamente il prodotto
                    SessionCart.Remove(prod);

                    
                    if (quantita != 0)
                    {
                        //se la quantita' non è 0 allora rimette il prodotto nel carrello con la quantita' aggiornata
                        SessionCart.Add(new OrderProduct { ProductId = idproduct, Quantity = quantita });
                        //aggiorna il carrello in sessione
                        HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
                    }
                    else
                    {
                        //se ora il carrello è vuoto viene eliminato
                        if (SessionCart.Count() == 0)
                        {
                            HttpContext.Session.Remove("Cart");
                        }
                        else
                        {
                            //se quantita' da aggiornare è 0 e il carrello non è vuoto, non rimette il prodotto al carrello e aggiorna il carrello in sessione
                            HttpContext.Session.SetObjectAsJson("Cart", SessionCart);
                        }
                    }
                }
            }

            return Redirect("/Cart/Index");
        }
    }
}