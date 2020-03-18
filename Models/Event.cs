using OXG.CRM_System.Models.Employeers;
using OXG.CRM_System.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string Status { get; set; }

        public string Adress { get; set; }

        public List<Mission> Missions { get; set; }

        public string EventType { get; set; }

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        [ForeignKey("Contract")]
        public int? ContractId { get; set; }
        public Contract Contract { get; set; }

        [ForeignKey("Manager")]
        public string? ManagerId { get; set; }
        public Manager Manager { get; set; }

        [ForeignKey("Technic")]
        public string? TechnicId { get; set; }
        public Technic Technic { get; set; }

        [ForeignKey("Designer")]
        public string? DesignerId { get; set; }
        public Designer Designer { get; set; }

        [ForeignKey("Artist")]
        public string? ArtistId { get; set; }
        public Artist Artist { get; set; }

        public Event(CreateEventVM model)
        {
            Name = model.EventName;
            CreatedDate = DateTime.Now;
            DeadLine = model.DeadLine;
            Description = model.EventDescription;
            Status = "Создано";
            Adress = model.EventLocation;
            EventType = model.EventType;
        }
        public Event()
        {

        }
    }
}
