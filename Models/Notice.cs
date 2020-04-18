using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Notice
    {
        public int Id { get; set; }

        public bool IsViewed { get; set; }

        public string EmployeerId { get; set; }

        public int MissionId { get; set; }

        public string Text { get; set; }
    }
}
