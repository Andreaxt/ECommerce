using ECommerceUpo.Models;
using System;
using System.Collections.Generic;

namespace ECommerceUpo.Models
{
    public class OrderTable
    {
        public OrderTable()
        {
            OrderProduct = new HashSet<OrderProduct>();
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime Data { get; set; }
        public string State { get; set; }
        public double TotalPrice { get; set; }

        public virtual ICollection<OrderProduct> OrderProduct { get; set; }
        public virtual User UserIdNavigation { get; set; }
    }
}
