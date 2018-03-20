using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceUpo.Data
{
    /*
     * Oggetto utilizzato pe rappresentare le informazioni di un utente nelle views
     */
    public class UserBean
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}

