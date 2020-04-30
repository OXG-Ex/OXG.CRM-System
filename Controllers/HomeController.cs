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
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        private readonly CRMDbContext db;
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
            await WatchDog.FindNewRequestAsync(db);
            await db.SaveChangesAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}