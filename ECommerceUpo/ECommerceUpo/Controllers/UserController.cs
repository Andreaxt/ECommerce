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

        //Manda alla pagina di sign in
        [HttpGet]
        public IActionResult Create() => View();

        //Crea un nuovo utente
        [HttpPost]
        public async Task<IActionResult> Create(string email, string password)
        {
            //controlla l'email non sia gia registrata
            var query = from utenti in Context.User
                        where utenti.Email.Equals(email)
                        select utenti;

            //se lo è manda un messaggio
            if (query.Count() != 0)
            {
                TempData["LoginMsg"] = "Email gia' esistente";

                return View();
            }

            //altrimenti aggiunge il record nel db, il form ha già uno script con cui controlla che le password siano uguali
            await base.Create(new User { Email = email, Password = password, Role = "user" });

            //query che restituisce i dati dell'utente appena inserito
            query = from utenti in Context.User
                    where utenti.Email.Equals(email)
                    select utenti;

            User New = query.First();

            //salva i dati nel bean dell'utente e manda alla home page
            HttpContext.Session.SetInt32("UserId", New.UserId);
            HttpContext.Session.SetString("Email", New.Email);
            HttpContext.Session.SetString("Role", New.Role);
            TempData["LoginMsg"] = "Registrazione avvenuta con successo";

            return Redirect("/Home/Index");
        }

        //Manda alla pagina di login
        [HttpGet]
        public IActionResult Login() => View();

        //Fa il login di un utente
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            User loggato;

            //controlla se esiste gia un utente con quella email e quella password
            var query = from utenti in Context.User
                        where utenti.Email.Equals(email) && utenti.Password.Equals(password)
                        select utenti;

            //se esiste lo salva nel bean
            if (query.Count() == 1)
            {
               
                loggato = query.First();

                HttpContext.Session.SetInt32("UserId", loggato.UserId);
                HttpContext.Session.SetString("Email", loggato.Email);
                HttpContext.Session.SetString("Role", loggato.Role == null ? "none" : loggato.Role);

                return Redirect("/Home");
            }
            //altrimenti rimanda alla pagina di login con messaggio per segnalare l'errore
            else
            {
                TempData["LoginMsg"] = "Email o password errati";

                return Redirect("/User/Login");
            }
        }

        //Fa il logout
        [HttpGet]
        public IActionResult Logout(string email, string password)
        {
            //rimuove i dati dal bean utente e dal suo carrello
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("Role");
            HttpContext.Session.Remove("LoginMsg");
            HttpContext.Session.Remove("Cart");

            return Redirect("/Home");
        }

        //Modifica il ruolo di un utente
        [HttpPost]
        public async Task<IActionResult> Update(string email, string role)
        {
            //legge i parametri dal form
            Int32.TryParse(email, out int UserId);
            User ToUpdate;

            //query che ritorna un utente con id corrispondente a quello del form
            var query = from utenti in Context.User
                        where utenti.UserId.Equals(UserId)
                        select utenti;

            ToUpdate = query.First();

            //se il ruolo è stato modificato lo cambia anche nel record del db
            if (!ToUpdate.Role.Equals(role))
            {
                ToUpdate.Role = role;

                //modifica il record del db
                await base.Update(ToUpdate);
            }

            return Redirect("/User/List");
        }

        //Per gli utenti admin mostra la lista di tutti gli utenti, si possono filtrare per nome e ruolo
        public IActionResult List(string clear, string email, string role)
        {
            //query che ritorna tutti gli utenti nel db
            var Query = from utenti in Context.User
                        select utenti;
            bool filtered = false;

            //si puo filtrare per nome e ruolo
            Query = Query.FilterUser(ref filtered, clear, email, role);

            TempData["UtenteFilter"] = filtered.ToString();
            return View(Query.ToList());
        }

        public IActionResult Index() => Redirect("/Utente/List");

    }
}