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
    public class TechnicController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public TechnicController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> Index()
        {
            var tech = await db.Technics.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(e => e.Client).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(tech);
        }

        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> Personal()
        {
            var tech = await db.Technics.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(e => e.Client).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(tech);
        }

        public IActionResult ChangePhoto()
        {
            return View();
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
            eventDb.Status = "Ожидает согласования техника";
            var mission = new Mission()
            {
                Employeer = technic,
                CreatedDate = DateTime.Now,
                DeadLine = DateTime.Now.AddDays(7),
                Event = eventDb,
                MissionType = "Согласование площадки",
                MissionText = "Согласовать соответствие площадки требованиям для мероприятия"
            };

            var notice = new Notice()
            {
                EmployeerName = technic.Name,
                EmployeerId = technic.Id,
                Text = "У вас новое задание",
                IsViewed = false,
                Deadline = DateTime.Now,
                MissionId = mission.Id
            };

            await db.Missions.AddAsync(mission);
            await db.Notices.AddAsync(notice);

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

        [HttpPost]
        public async Task<IActionResult> SaveChanges(Technic technic)
        {
            var temp = await db.Managers.Where(e => e.Email == User.Identity.Name && e.UserName == User.Identity.Name).FirstOrDefaultAsync();
            temp.Name = technic.Name;
            temp.PhoneNumber = technic.PhoneNumber;
            temp.Email = technic.Email;
            await db.SaveChangesAsync();
            return RedirectToAction("Personal", "Technic");
        }

        [HttpPost]
        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> ConfirmEvent(Technic technic)
        {
            return View();
        }
    }
}