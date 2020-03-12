using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models.Employeers
{
    /// <summary>
    /// Класс сотрудника непосредственно работающего на мероприятии
    /// </summary>
    public class Artist : User
    {
        public List<Costume> Costumes { get; set; }

        public List<Requisite> Requisites { get; set; }

        public string Specialty { get; set; }
    }
}
