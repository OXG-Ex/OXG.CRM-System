using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Photo { get; set; }

        public int EmloyeerId { get; set; }
        public AbstractEmployeer Employeer { get; set; }
    }
}
