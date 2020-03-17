using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime DeadLine { get; set; }

        public string Description { get; set; }

        public string Adress { get; set; }

        public List<Mission> Missions { get; set; }

        public string EventType { get; set; }

        public int? ClientId { get; set; }
        public Client Client { get; set; }

        public int? ManagerId { get; set; }
        public Manager Manager { get; set; }

        public int? TechnicId { get; set; }
        public Technic Technic { get; set; }

        public int? PrinterId { get; set; }
        public Printer Printer { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
