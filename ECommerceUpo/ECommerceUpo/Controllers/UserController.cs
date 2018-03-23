using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerceUpo.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerceUpo.Models;
using Microsoft.AspNetCore.Http;
using ECommerceUpo.Src;

namespace ECommerceUpo.Controllers
{
    public class UserController : CrudController<ECommerceUpoContext, int, User>
    {
        public UserController(ECommerceUpoContext context, ILogger<UserController> logger) : base(context, logger)
        {
        }

        protected override DbSet<User> Entities => Context.User;

        protected override Func<User, int, bool> FilterById => (e, id) => e.UserId == id;

        //GET Utente/Create -> pagina col form di registrazione
        //POST Utente/Create -> legge il form di registrazione

        /*
         * Passa alla pagina contenente il form di registrazione
         */
        [HttpGet]
        public IActionResult Create() => View();

        /*
         * Processa le informazioni provenienti dal form di registrazione
         */
        [HttpPost]
        public async Task<IActionResult> Create(string user, string pass)
        {
            //controlla che non esista gia' lo stesso username desiderato
            var query = from utenti in Context.User
                        where utenti.Email.Equals(user)
                        select utenti;

            //se username e' gia' presente nel db allora rimanda alla view con un messaggio
            if (query.Count() != 0)
            {
                TempData["LoginMsg"] = "Email gia' esistente";

                return View();
            }

            //se username non esiste gia':
            //assume password gia' controllate in javascript (wwwroot/js/valdate.js):
            //rende persistente il nuovo utente chiamando il metodo Create del CrudController
            await base.Create(new User { Email = user, Password = pass, Role = "user" });

            //recupera dal db l'utente con CdUtente appena inserito
            query = from utenti in Context.User
                    where utenti.Email.Equals(user)
                    select utenti;

            //utente appena inserito:
            User New = query.First();

            //SALVA IN SESSION DATI LOGIN dell'utente appena inserito (Effettua la login)
            HttpContext.Session.SetInt32("UserId", New.UserId);
            HttpContext.Session.SetString("Email", New.Email);
            HttpContext.Session.SetString("Role", New.Role);
            TempData["LoginMsg"] = "Registrazione avvenuta con successo";

            return Redirect("/Home/Index");
        }

        //GET Utente/Login -> pagina col form di login
        //POST Utente/Login -> legge il form di login

        /*
         * Rimanda alla pagina di login
         */
        [HttpGet]
        public IActionResult Login() => View();

        /*
         * Processa i dati del form di login
         */
        [HttpPost]
        public IActionResult Login(string user, string pass)
        {
            User loggato;

            //controlla se esiste utente con quella password nel db, per sicurezza
            var query = from utenti in Context.User
                        where utenti.Email.Equals(user) && utenti.Password.Equals(pass)
                        select utenti;

            //se trova un risultato, salva in session codice e ruolo
            if (query.Count() == 1)
            {
                //unico elemento della lista: l'utente con CdUtente
                loggato = query.First();

                //SALVA IN SESSION DATI LOGIN (Effettua login)
                HttpContext.Session.SetInt32("UserId", loggato.UserId);
                HttpContext.Session.SetString("Email", loggato.Email);
                HttpContext.Session.SetString("Role", loggato.Role == null ? "none" : loggato.Role);

                return Redirect("/Home");
            }
            //altrimenti rimanda alla login con messaggio
            else
            {
                TempData["LoginMsg"] = "Email o password errati";

                return Redirect("/User/Login");
            }
        }

        /*
         * Effettua il logout
         */
        [HttpGet]
        public IActionResult Logout(string user, string pass)
        {
            //rimuove da session tutti i dati di login
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("Role");
            HttpContext.Session.Remove("LoginMsg");
            HttpContext.Session.Remove("Cart");

            return Redirect("/Home");
        }

        /*
         * Aggiorna i dati relativi ad un utente (SOLO ADMIN)
         */
        [HttpPost]
        public async Task<IActionResult> Update(string user, string ruolo)
        {
            //riceve parametri dal form
            Int32.TryParse(user, out int UserId);
            User ToUpdate;

            //cerca nel db l'utente con cdUtente corrispondente a quello ricevuto dal form
            var query = from utenti in Context.User
                        where utenti.UserId.Equals(UserId)
                        select utenti;

            //prende il primo elemento (l'unico) della query
            ToUpdate = query.First();

            //modifica ruolo solo se diverso
            if (!ToUpdate.Role.Equals(ruolo))
            {
                ToUpdate.Role = ruolo;

                //rende le modifiche persistenti chiamando il metodo Update del CrudController
                await base.Update(ToUpdate);
            }

            return Redirect("/User/List");
        }

        /*
         * Espone la lista di tutti gli utenti registrati (SOLO ADMIN), con possibilita' di filtrare
         */
        public IActionResult List(string clear, string username, string ruolo)
        {
            //seleziona dal db tutti  gli utenti
            var Query = from utenti in Context.User
                        select utenti;
            bool filtered = false;

            //FILTRO: custom IQueryable extension method
            Query = Query.FilterUser(ref filtered, clear, username, ruolo);

            TempData["UtenteFilter"] = filtered.ToString();
            return View(Query.ToList());
        }

        public IActionResult Index() => Redirect("/Utente/List");

    }
}