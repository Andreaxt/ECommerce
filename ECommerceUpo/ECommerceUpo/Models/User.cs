using System;
using System.Collections.Generic;

namespace ECommerceUpo.Models
{
    public class User
    {
        public User()
        {
            Order = new HashSet<Order>();
        }

        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Order> Order { get; set; }
    }
}