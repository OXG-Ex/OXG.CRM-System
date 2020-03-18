using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Layout
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public bool IsConfirm { get; set; }

        public string TechnicalTask { get; set; }

        public string PSD { get; set; }

        public int PrinterId { get; set; }
        public Designer Printer { get; set; }
    }
}
