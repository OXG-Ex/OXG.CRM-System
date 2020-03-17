using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class User : IdentityUser
    {
        public string Photo { get; set; }

        public string Name { get; set; }

        public List<Mission> Missions { get; set; }
        
    }
}
