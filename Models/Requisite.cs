using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Requisite : Equipment
    {
        public string? ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
