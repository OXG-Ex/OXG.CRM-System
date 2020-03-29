using OXG.CRM_System.Models.Employeers;
using OXG.CRM_System.ViewModels;
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

        public string Email { get; set; }

        public string Description { get; set; }

        public List<Event> Events { get; set; }

        public List<ClientVK>ClientVK { get; set; }

        public string? ManagerId { get; set; }
        public Manager Manager { get; set; }

        public Client(CreateEventVM model)
        {
            Name = model.ClientName;
            Phone = model.ClientPhone;
            Email = model.ClientEmail;
            Description = model.ClientDescription;
        }

        public Client()
        {

        }
    }
}
