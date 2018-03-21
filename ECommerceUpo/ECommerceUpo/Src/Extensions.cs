using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using ECommerceUpo.Data;
using ECommerceUpo.Models;
using ECommerceUpo.Src;

namespace ECommerceUpo
{
    public static class Extensions
    {
        //limiti datetime
        private static readonly string MIN_DATE = "1/1/1754";
        private static readonly string MAX_DATE = "12/31/9998";


        /*
         * Serializza oggetti complessi in array di byte per salvarli in session
         */
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        /*
         * Deserializza gli oggetti in session (byte[]) rimappandoli nell'oggetto originale
         */
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        /*
         * Metodo Generico che accetta delegates
         * filtra tutti i nomi: se trova un argomento != null, chiama il corrispondente Func<> definito nella strategy, il quale filtra.
         * Ordini, Utenti e Prodotti chiamano con parametri diversi e con Strategy diverse
         */
        private static IQueryable<T> GeneralFilter<T>(ref IQueryable<T> Query, ref bool filtered, string clear,
            string start, string end, string titolo, string stato,  //ordine
            string username, string ruolo,                          //utente
            string titoloProd, string disp, string sconto,          //prodotto
            Filters<T> Strategy)
        {
            //limiti DateTime
            DateTime.TryParse(MIN_DATE, out DateTime MIN);
            DateTime.TryParse(MAX_DATE, out DateTime MAX);

            filtered = false;

            //se c'e' clear, non fa niente
            if (clear == null)
            {
              
                //UTENTE
                if (username != null && !username.Equals(""))
                {
                    //Query = Query.Where(u => u.Username.Contains(username));
                    Query = Strategy.FilterUser(Query, username);
                    filtered = true;
                }

                if (ruolo != null && !ruolo.Equals(""))
                {
                    //Query = Query.Where(u => u.Ruolo.Equals(ruolo));
                    Query = Strategy.FilterRole(Query, ruolo);
                    filtered = true;
                }

            }

            return Query;
        }

        /*
        * Filtra fra gli utenti: solo admin (/Utenti/List)
        */
        public static IQueryable<User> FilterUser(this IQueryable<User> Query, ref bool filtered, string clear, string username, string ruolo)
        {
            filtered = false;

            Query = GeneralFilter(ref Query, ref filtered, clear, null, null, null, null, username, ruolo, null, null, null,
                new Filters<User>()
                {
                    FilterUser = (query, usrname) => query.Where(utente => utente.Email.Contains(usrname)),
                    FilterRole = (query, role) => query.Where(utente => utente.Role.Equals(role))
                });

            return Query;
        }


    }
}
