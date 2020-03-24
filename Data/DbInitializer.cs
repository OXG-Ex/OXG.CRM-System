﻿using Microsoft.AspNetCore.Identity;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Data
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, CRMDbContext db)
        {
            string adminEmail = "admin@admin.com";
            string password = "Admin_123";

            string managerEmail = "manager212@gmail.com";
            string managerPassword = "Manager_123";

            string technicEmail = "technic212@gmail.com";
            string technicPassword = "Technic_123";

            if (db.Works.Count() < 1)
            {
                await db.Works.AddAsync(new Work() { Name = "Огненное шоу 'Жасмин'", Price = 5500 });
                await db.Works.AddAsync(new Work() { Name = "Огненное шоу 'Человек-дракон'", Price = 7500 });
                await db.Works.AddAsync(new Work() { Name = "Пиротехническое шоу 'SuperNova'", Price = 8500 });
                await db.Works.AddAsync(new Work() { Name = "Шоу Тесла", Price = 10000 });
                await db.Works.AddAsync(new Work() { Name = "Светодиодное шоу", Price = 3500 });
                await db.Works.AddAsync(new Work() { Name = "Аниматор", Price = 3000 });
                await db.Works.AddAsync(new Work() { Name = "Ведущий", Price = 5000 });
                await db.Works.AddAsync(new Work() { Name = "Ходулисты", Price = 3000 });
                await db.Works.AddAsync(new Work() { Name = "Фрик шоу", Price = 5000 });
                await db.Works.AddAsync(new Work() { Name = "Молекулярное шоу", Price = 6500 });
                await db.Works.AddAsync(new Work() { Name = "Танцовщицы", Price = 3500 });
                await db.SaveChangesAsync();
            }

            if (await roleManager.FindByNameAsync("Администратор") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Администратор"));
            }
            if (await roleManager.FindByNameAsync("Менеджер") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Менеджер"));
            }
            if (await roleManager.FindByNameAsync("Дизайнер") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Дизайнер"));
            }
            if (await roleManager.FindByNameAsync("Техник") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Техник"));
            }
            if (await roleManager.FindByNameAsync("Артист") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Артист"));
            }


            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User admin = new User { Email = adminEmail, UserName = adminEmail };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Администратор");
                    await userManager.AddToRoleAsync(admin, "Менеджер");
                    await userManager.AddToRoleAsync(admin, "Дизайнер");
                    await userManager.AddToRoleAsync(admin, "Техник");
                    await userManager.AddToRoleAsync(admin, "Артист");
                }
            }

            if (await userManager.FindByNameAsync(managerEmail) == null)
            {
                Manager manager = new Manager { Email = managerEmail, UserName = managerEmail, Name ="Менеджер Тест Тест", Photo = "/images/defaultPhoto.png" };
                IdentityResult result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Менеджер");
                }
            }

            if (await userManager.FindByNameAsync(technicEmail) == null)
            {
                Technic technic = new Technic { Email = technicEmail, UserName = technicEmail, Name = "Техник Тест Тест", Photo = "/images/defaultPhoto.png" };
                IdentityResult result = await userManager.CreateAsync(technic, technicPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(technic, "Техник");
                }
            }
        }
    }
}
