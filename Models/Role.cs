using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Role
    {
        public int Id { get; set; }

        public int Name { get; set; }

        public List<AbstractEmployeer> Employeers { get; set; }

        public Role()
        {
            Employeers = new List<AbstractEmployeer>();
        }
    }
}
