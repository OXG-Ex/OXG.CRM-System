using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public abstract class AbstractEmployeer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public List<Mission> Missions { get; set; }
    }
}
