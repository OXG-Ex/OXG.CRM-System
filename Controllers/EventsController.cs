using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер ответственный за работу с мероприятиями
    /// </summary>
    [Authorize]
    public class EventsController : Controller
    {
        private readonly CRMDbContext db;

        public EventsController(CRMDbContext context)
        {
            db = context;
        }

        /// <summary>
        /// Возвращает представление для создания нового мероприятия
        /// </summary>
        /// <param name="FromRequest"></param>
        /// <returns></returns>
        public IActionResult New(bool? FromRequest)
        {
            ViewBag.EventTypes = StaticValues.GetEventTypes();
            ViewBag.FromRequest = FromRequest;
            return View();
        }

        /// <summary>
        /// Возвращает представление для просмотра информации о мероприятии
        /// </summary>
        /// <param name="id"></param>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public async Task<IActionResult> View(int id, int? missionId)
        {
            var eventDb = await db.Events.Include(e => e.Client)
                                         .Include(e => e.Manager)
                                         .Include(e => e.Technic)
                                         .Include(e => e.Contract)
                                         .Include(e => e.Missions)
                                         .Include(e => e.Works)
                                         .Where(e => e.Id == id)
                                         .FirstOrDefaultAsync();
            if (missionId != null)
            {
                var mission = await db.Missions.Where(e => e.Id == missionId).FirstOrDefaultAsync();
                ViewBag.Message = $"{mission.MissionText}";
            }

            return View(eventDb);
        }

        /// <summary>
        /// Создает новое мероприятие на основе заполненной модели
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Менеджер")]
        [HttpPost]
        public async Task<IActionResult> New(CreateEventVM model)
        {
            if (ModelState.IsValid)
            {
                var eventDb = new Event(model);
                var clientDb = await db.Clients.Where(c => c.Name == model.ClientName).FirstOrDefaultAsync();
                if (clientDb == null)
                {
                    clientDb = new Client(model);
                }
                clientDb.Description = model.ClientDescription;
                var manager = await db.Managers.Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
                if (model.FromRequest == true)
                {
                    manager.MissionFromRequestNum++;
                }
                eventDb.Manager = manager;
                clientDb.Manager = manager;
                eventDb.Client = clientDb;
                await db.AddAsync(eventDb);
                await db.SaveChangesAsync();

                var workMission = new Mission()
                {
                    Employeer = manager,
                    Event = eventDb,
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddHours(48),
                    MissionType = "Услуги",
                    MissionText = $"Указать услуги для мероприятия '{eventDb.Name}'"
                };

                await db.AddAsync(workMission);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Manager");
            }
            else
            {
                ModelState.AddModelError("", "Заполните все поля");
                ViewBag.EventTypes = StaticValues.GetEventTypes();
                ViewBag.FromRequest = model.FromRequest;
                return View();
            }

        }
    }
}