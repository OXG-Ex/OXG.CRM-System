using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class CRMDbContext : IdentityDbContext
    {
        public CRMDbContext(DbContextOptions<CRMDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<Manager> Managers { get; set; }

        public DbSet<Technic> Technics { get; set; }

        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<ClientVK> ClientsVK { get; set; }

        public DbSet<Costume> Costumes { get; set; }

        public DbSet<Equipment> Equipment { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventWork> EventWorks { get; set; }

        public DbSet<Mission> Missions { get; set; }

        public DbSet<Notice> Notices { get; set; }

        public DbSet<User> Employeers { get; set; }

        public DbSet<Work> Works { get; set; }

        public DbSet<Requisite> Requisites { get; set; }


        public Manager GetMustFreedomManager()
        {
            var managers = Managers.Include(m => m.Missions);
            var min = 999999;
            var id = "";
            foreach (var item in managers)
            {
                if (item.Missions.Count < min)
                {
                    id = item.Id;
                }
            }
            var manager = managers.Where(m => m.Id == id).FirstOrDefault();
            return manager;
        }
    }
}
