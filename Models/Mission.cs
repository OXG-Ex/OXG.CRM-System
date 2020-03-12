using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Mission
    {
        public int Id { get; set; }

        public string MissionText { get; set; }

        public DateTime DeadLine { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Result { get; set; }

        public string MissionType { get; set; }

        public int EmployeerId { get; set; }
        public AbstractEmployeer Employeer { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
