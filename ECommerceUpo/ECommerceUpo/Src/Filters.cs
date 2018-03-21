using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Src
{
    public class Filters<T>
    {
        public virtual Func<IQueryable<T>, string, IQueryable<T>> FilterUser { get; set; }
        public virtual Func<IQueryable<T>, string, IQueryable<T>> FilterRole { get; set; }
    }
}
