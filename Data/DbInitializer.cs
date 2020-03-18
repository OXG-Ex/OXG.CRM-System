using Microsoft.AspNetCore.Identity;
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
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "admin@admin.com";
            string password = "Admin_123";

            string managerEmail = "manager212@gmail.com";
            string managerPassword = "Manager_123";

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
                Manager manager = new Manager { Email = managerEmail, UserName = managerEmail, Name ="Менеджер Тест Тест" };
                IdentityResult result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Менеджер");
                }
            }
        }
    }
}
