using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public abstract class User : IdentityUser
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public List<Mission> Missions { get; set; }
    }
}
