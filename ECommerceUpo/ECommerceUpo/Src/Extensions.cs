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
        //massimo e minimo possibili per le date
        private static readonly string MIN_DATE = "1/1/1754";
        private static readonly string MAX_DATE = "12/31/9998";


        //trasforma in array di bytes degli oggetti complessi tipo immagini per salvarli in sessione
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        //ritrasforma nell'oggetto originale a partire dall'array di bytes
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
        private static IQueryable<T> GeneralFilter<T>(ref IQueryable<T> Query, ref bool filtered, string clear, string state, string email, string role, Filters<T> filter)
        {
            //massimo e minimo possibili per le date
            DateTime.TryParse(MIN_DATE, out DateTime MIN);
            DateTime.TryParse(MAX_DATE, out DateTime MAX);

            filtered = false;

            //se c'è clear non fa niente
            if (clear == null)
            {
              
                //Per filtrare gli utenti
                if (email != null && !email.Equals(""))
                {
                    Query = filter.FilterUser(Query, email);
                    filtered = true;
                }

                if (role != null && !role.Equals(""))
                {
                    Query = filter.FilterRole(Query, role);
                    filtered = true;
                }
                //Per filtrare gli ordini
                if (state != null && !state.Equals(""))
                {
                    Query = filter.FilterState(Query, state);
                    filtered = true;
                }

            }

            return Query;
        }

        //Filtra gli utenti in base a email e ruolo, solo per utenti di tipo admin
        public static IQueryable<User> FilterUser(this IQueryable<User> Query, ref bool filtered, string clear, string email, string role)
        {
            filtered = false;

            Query = GeneralFilter(ref Query, ref filtered, clear, null, email, role,
                new Filters<User>()
                {
                    FilterUser = (query, emailuser) => query.Where(utente => utente.Email.Contains(emailuser)),
                    FilterRole = (query, roleuser) => query.Where(utente => utente.Role.Equals(roleuser))
                });

            return Query;
        }

        //Filtra gli ordini sent/processed, solo per utenti di tipo admin
        public static IQueryable<OrderBean> FilterState(this IQueryable<OrderBean> Query, ref bool filtered, string clear, string state)
        {
            filtered = false;

            Query = GeneralFilter(ref Query, ref filtered, clear, state, null, null,
                new Filters<OrderBean>()
                {
                    FilterState = (query, stato) => query.Where(ordine => ordine.State.Equals(stato)),
                });

            return Query;
        }


    }
}
