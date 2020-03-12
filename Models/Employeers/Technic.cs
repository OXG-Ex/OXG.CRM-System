using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models.Employeers
{
    /// <summary>
    /// Класс для сотрудника ответственного за реквизит, аппаратуру и техническое согласование
    /// </summary>
    public class Technic : User
    {
        public List<Equipment> Equipment { get; set; }

        public List<Certificate> Certificates { get; set; }
    }
}
