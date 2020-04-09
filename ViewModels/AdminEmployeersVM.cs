using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class AdminEmployeersVM
    {
        public List<Manager> Managers { get; set; }

        public List<Technic> Technics { get; set; }

        public List<Artist> Artists { get; set; }
    }
}
