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

        public List<Mission> Missions { get; set; }

        public string EventType { get; set; }

        public Client Client { get; set; }

        public Manager Manager { get; set; }

        public Technic Technic { get; set; }

        public Printer Printer { get; set; }

        public List<Artist> Artists { get; set; }
    }
}
