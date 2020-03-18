using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models.Employeers
{
    /// <summary>
    /// Класс сотрудника отвечающего за заказ и изготовление печатной продукции
    /// </summary>
    public class Designer : User
    {
        public List<Layout> Layouts { get; set; }
    }
}
