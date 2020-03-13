using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public int? ManagerId { get; set; }
        public Manager Manager { get; set; }
    }
}
