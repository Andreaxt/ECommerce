using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Models
{
    public class Product
    {
        public Product()
        {
            OrderProduct = new HashSet<OrderProduct>();
        }

        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public byte[] Image { get; set; }
        public string Disp { get; set; }

        public virtual ICollection<OrderProduct> OrderProduct { get; set; }

    }
}
