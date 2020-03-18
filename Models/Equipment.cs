using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Equipment
    {
        public int Id { get; set; }

        public int InventNum { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string? TechnicId { get; set; }
        public Technic Technic { get; set; }
    }
}
