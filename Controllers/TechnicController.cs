using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;

namespace OXG.CRM_System.Controllers
{
    public class TechnicController : Controller
    {
        private CRMDbContext db;

        public TechnicController(CRMDbContext context)
        {
            db = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Менеджер")]
        public IActionResult AddToEvent(int id)
        {
            ViewBag.Technics = new SelectList(db.Technics,"Id","Name");
            ViewBag.EventId = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles ="Менеджер")]
        public async Task<IActionResult> AddToEvent(string TechnicId, int eventId)
        {
            var technic = await db.Technics.Where(t => t.Id == TechnicId).FirstOrDefaultAsync();
            var eventDb = await db.Events.Include(e => e.Technic).Include(e => e.Missions).Where(t => t.Id == eventId).FirstOrDefaultAsync();

            if (technic != null && eventDb != null)
            {
                eventDb.Technic = technic;

                foreach (var item in eventDb.Missions)
                {
                    if (item.MissionText.Contains("Указать техника"))
                    {
                        item.Status = "Закрыто";
                    }
                }
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index","Manager");
        }
    }
}