using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;


namespace OXG.CRM_System.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        CRMDbContext db; 
        public HomeController(ILogger<HomeController> logger, CRMDbContext context, RoleManager<IdentityRole> role, UserManager<User> user)
        {
            _logger = logger;
            db = context;
            roleManager = role;
            userManager = user;
        }

        public async Task<IActionResult> Index()
        {
            await DbInitializer.InitializeAsync(userManager, roleManager, db);
            await WatchDog.FindDeadlineAsync(db);
            await db.SaveChangesAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GetUserPhoto(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var t = user.Photo;
            if (!string.IsNullOrWhiteSpace(t))
            {
                return Content(t);
            }
            else
            {
                return Ok();
            } 
        }

        public async Task<IActionResult> GetNoticeNum(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var id = user.Id;
            var num = 0;
            if (!User.IsInRole("Администратор"))
            {
                num = await db.Notices.Where(n => n.EmployeerId == id && !n.IsViewed).CountAsync();
            }
            else
            {
                num = await db.Notices.Where(n =>!n.IsViewed).CountAsync();
            }
            return Content(num.ToString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
