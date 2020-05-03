using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;

namespace OXG.CRM_System.Controllers
{
    public class ArtistController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public ArtistController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        [Authorize(Roles = "Артист")]
        public async Task<IActionResult> Index()
        {
            var artist = await db.Artists.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(m => m.Works).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(artist);
        }

        [Authorize(Roles = "Артист")]
        public async Task<IActionResult> Personal()
        {
            var artist = await db.Artists.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(m => m.Works).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(artist);
        }

        public IActionResult ChangePhoto()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveChanges(Artist artist)
        {
            var temp = await db.Managers.Where(e => e.Email == User.Identity.Name && e.UserName == User.Identity.Name).FirstOrDefaultAsync();
            temp.Name = artist.Name;
            temp.PhoneNumber = artist.PhoneNumber;
            temp.Email = artist.Email;
            await db.SaveChangesAsync();
            return RedirectToAction("Personal", "Artist");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.ContentType.Contains("image"))
            {
                var path = "/AccountPhotos/" + uploadedFile.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                var user = await db.Managers.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                user.Photo = path;
                await db.SaveChangesAsync();

                return RedirectToAction("Personal");
            }
            return Content("Некорректный файл");
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

            var notice = new Notice()
            {
                EmployeerName = artist.Name,
                EmployeerId = artist.Id,
                Text = "У вас новое задание",
                IsViewed = false,
                Deadline = DateTime.Now,
                MissionId = mission.Id
            };

            await db.Missions.AddAsync(mission);
            await db.Notices.AddAsync(notice);

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