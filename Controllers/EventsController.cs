using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    
    public class EventsController : Controller
    {
        private CRMDbContext db;

        public EventsController(CRMDbContext context)
        {
            db = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult New()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var eventDb = await db.Events.Include(e => e.Client)
                                         .Include(e => e.Manager)
                                         .Include(e => e.Designer)
                                         .Include(e => e.Technic)
                                         .Include(e => e.Contract)
                                         .Include(e => e.Missions)
                                         .Where(e => e.Id == id)
                                         .FirstOrDefaultAsync();
            return View(eventDb);
        }

        [Authorize(Roles = "Менеджер")]
        [HttpPost]
        public async Task<IActionResult> New(CreateEventVM model)
        {
            if (ModelState.IsValid)
            {
                var eventDb = new Event(model);
                var clientDb = new Client(model);
                var manager = await db.Managers.Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
                eventDb.Manager = manager;
                clientDb.Manager = manager;
                eventDb.Client = clientDb;
                await db.AddAsync(eventDb);
                await db.SaveChangesAsync();
                return RedirectToAction("Index","Manager");
            }
            else
            {
                ModelState.AddModelError("","Заполните все поля");
                return View();
            }
            
        }
    }
}