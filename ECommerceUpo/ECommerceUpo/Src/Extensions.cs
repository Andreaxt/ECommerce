using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using ECommerceUpo.Data;
using ECommerceUpo.Models;


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



    }
}
