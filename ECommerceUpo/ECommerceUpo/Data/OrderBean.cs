using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Data
{
    //bean per mantenere i dati i un ordine
    public class OrderBean
    {
        public string Email { get; set; }

        public int OrderId { get; set; }

        public string State { get; set; }

        public DateTime Data { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice { get; set; }

        public string Title { get; set; }

        public int ProductId { get; set; }
    }
}
