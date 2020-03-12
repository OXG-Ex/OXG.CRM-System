using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{/// <summary>
/// Удостоверение (сертификат)для техника (напр. удостоверение пиротехника)
/// </summary>
    public class Certificate
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string IssuedBy { get; set; }

        public string Scan { get; set; }

        public int TechnicId { get; set; }
        public Technic Technic { get; set; }
    }
}
