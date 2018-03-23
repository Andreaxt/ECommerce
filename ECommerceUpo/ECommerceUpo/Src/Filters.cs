using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Src
{
    //alcuni metodi per filtrare, si puo' espandere aggiungendo metodi che filtrano in base ad altri parametri
    public class Filters<T>
    {
        //filtri per utenti
        public virtual Func<IQueryable<T>, string, IQueryable<T>> FilterUser { get; set; }
        public virtual Func<IQueryable<T>, string, IQueryable<T>> FilterRole { get; set; }
        //filtro per ordini
        public virtual Func<IQueryable<T>, string, IQueryable<T>> FilterState { get; set; }

    }
}
