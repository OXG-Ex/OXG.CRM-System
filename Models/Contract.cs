using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? EventId { get; set; }
        public Event Event { get; set; }

        public int? ClientId { get; set; }
        public Client Client { get; set; }

        public string? ManagerId { get; set; }
        public Manager Manager { get; set; }
    }
}
