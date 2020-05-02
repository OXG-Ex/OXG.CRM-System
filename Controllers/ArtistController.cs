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
    public class ArtistController : Controller
    {
        private CRMDbContext db;

        public ArtistController(CRMDbContext context)
        {
            db = context;
        }

        [Authorize(Roles = "Артист")]
        public async Task<IActionResult> Index()
        {
            var artist = await db.Artists.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(m => m.Works).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(artist);
        }

        [Authorize(Roles = "Менеджер")]
        public IActionResult AddToEvent(int id)
        {
            ViewBag.Artists = new SelectList(db.Artists, "Id", "Name");
            ViewBag.EventId = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> AddToEvent(string ArtistId, int eventId)
        {
            var artist = await db.Artists.Where(t => t.Id == ArtistId).FirstOrDefaultAsync();
            var eventDb = await db.Events.Include(e => e.Artist).Include(e => e.Missions).Where(t => t.Id == eventId).FirstOrDefaultAsync();

            var mission = new Mission()
            {
                Employeer = artist,
                CreatedDate = DateTime.Now,
                DeadLine = eventDb.DeadLine,
                Event = eventDb,
                MissionType = "Заказ",
                MissionText = "Провести указанные мероприятия"
            };

            await db.Missions.AddAsync(mission);

            if (artist != null && eventDb != null)
            {
                eventDb.Artist = artist;

                foreach (var item in eventDb.Missions)
                {
                    if (item.MissionText.Contains("Указать артиста"))
                    {
                        item.Status = "Закрыто";
                    }
                }
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }
    }
}