using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class AccountVM
    {
        public string Email { get; set; }

        public string Position { get; set; }

        public string Phone { get; set; }

        public string Photo { get; set; }

        public string Name { get; set; }

        public int? NumTask { get; set; }

        public int? NumClient { get; set; }

        public int? NumEvent { get; set; }

        //public AccountVM(IUser user, bool manager)
        //{
        //    Name = user.Name;
        //    Email = user.Email;
        //    Phone = user.PhoneNumber;
        //    Photo = user.Photo;
        //    Position = user.RoleName;
        //    NumTask = user.Missions.Count();
        //    if (manager)
        //    {
        //        NumClient = (user as Manager).Clients.Count();
        //        NumEvent = (user as Manager).Events.Count();
        //    }
        //}
    }
}
