using OXG.CRM_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class AdminEmployeerVM
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int MissionNum { get; set; }

        public int FailedNum { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Photo { get; set; }

        public AdminEmployeerVM()
        {

        }

        public AdminEmployeerVM(User user)
        {
            Id = user.Id;
            Name = user.Name;
            MissionNum = user.Missions.Where(m => m.Status != "Закрыто" && !m.IsFailed).Count();
            FailedNum = user.Missions.Where(m => m.IsFailed).Count();
            Phone = user.PhoneNumber;
            Email = user.Email;
            Photo = user.Photo;
        }
    }
}
