using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models.Employeers
{
    /// <summary>
    /// Класс сотрудника отвечающего за работу с клиентами
    /// </summary>
    public class Manager : User
    {
        public List<Client> Clients { get; set; }

        public List<Contract> Contracts { get; set; }

        public List<Event> Events { get; set; }
    }
}
