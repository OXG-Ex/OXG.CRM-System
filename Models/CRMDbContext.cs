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

        public DbSet<Printer> Printers { get; set; }

        public DbSet<Technic> Technics { get; set; }

        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Costume> Costumes { get; set; }

        public DbSet<Equipment> Equipment { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Layout> Layouts { get; set; }

        public DbSet<Mission> Missions { get; set; }

        public DbSet<Requisite> Requisites { get; set; }
    }
}
