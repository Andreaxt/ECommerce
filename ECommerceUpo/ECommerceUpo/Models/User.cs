using System;
using System.Collections.Generic;

namespace ECommerceUpo.Models
{
    public class User
    {
        public User()
        {
            OrderTable = new HashSet<OrderTable>();
        }

        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public virtual ICollection<OrderTable> OrderTable { get; set; }
    }
}