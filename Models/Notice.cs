using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Notice
    {
        public int Id { get; set; }

        public string NoticeText { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
