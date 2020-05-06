using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер отвечающий за работу с услугами event-агенства
    /// </summary>
    public class WorksController : Controller
    {
        private readonly CRMDbContext db;
        public WorksController(CRMDbContext context)
        {
            db = context;
        }

        /// <summary>
        /// Удаляет услугу из списка
        /// </summary>
        /// <param name="id">Id улуги</param>
        /// <param name="eid">Id мероприятия</param>
        /// <returns></returns>
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> DeleteFromEvent(int id, int eid)
        {
            var eventDb = await db.Events.Include(e => e.Works).Include(e => e.Manager).Where(e => e.Id == eid).FirstOrDefaultAsync();
            var work = eventDb.Works.Where(w => w.Id == id).FirstOrDefault();
            eventDb.Works.Remove(work);
            var workT = await db.Works.Where(e => e.Name == work.Name).FirstOrDefaultAsync();
            workT.OrdersCount--;

            if (workT.OrdersCount == 0)
            {
                var workMission = new Mission()
                {
                    Employeer = eventDb.Manager,
                    Event = eventDb,
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddHours(48),
                    MissionType = "Услуги",
                    MissionText = $"Указать услуги для мероприятия '{eventDb.Name}'"
                };
                await db.AddAsync(workMission);
            }

            await db.SaveChangesAsync();
            return RedirectToAction("View", "Events", new { id = eid });
        }

        /// <summary>
        /// Возвращает представление для добавления услуги в список услуг мероприятия
        /// </summary>
        /// <param name="id">Id услуги</param>
        /// <returns></returns>
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> AddToEvent(int id)
        {
            var eventDb = await db.Events.Include(e => e.Works).Where(e => e.Id == id).FirstOrDefaultAsync();
            ViewBag.WorkList = new SelectList(db.Works, "Id", "Name");
            return View(eventDb);
        }

        /// <summary>
        /// Добавляет услугу к мероприятию
        /// </summary>
        /// <param name="WorkId">Id Услуги</param>
        /// <param name="WorkNum">Кол-во услуг</param>
        /// <param name="EventID">Id мероприятия</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> AddToEvent(int WorkId, int WorkNum, int EventID)
        {
            var eventDb = await db.Events.Include(e => e.Works).Include(e => e.Missions).Include(e => e.Manager).Where(e => e.Id == EventID).FirstOrDefaultAsync();
            var work = await db.Works.Where(w => w.Id == WorkId).FirstOrDefaultAsync();
            var workTemp = new EventWork(work, WorkNum);
            await db.EventWorks.AddAsync(workTemp);
            await db.SaveChangesAsync();
            eventDb.Works.Add(workTemp);
            eventDb.TotalPrice += workTemp.Sum;
            work.OrdersCount++;
            if (work.OrdersCount > 0 && work.OrdersCount < 2)
            {
                var artistMission = new Mission()
                {
                    Employeer = eventDb.Manager,
                    Event = eventDb,
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddHours(3),
                    MissionType = "Указать артиста",
                    MissionText = $"Указать артиста для мероприятия '{eventDb.Name}'"
                };
                var technicMission = new Mission()
                {
                    Employeer = eventDb.Manager,
                    Event = eventDb,
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddHours(3),
                    MissionType = "Указать техника",
                    MissionText = $"Указать техника для мероприятия '{eventDb.Name}'"
                };
                await db.AddAsync(artistMission);
                await db.AddAsync(technicMission);
                var mission = eventDb.Missions.Where(m => m.MissionText.Contains("Указать услуги")).FirstOrDefault();
                mission.Status = "Закрыто";
            }
            await db.SaveChangesAsync();
            return RedirectToAction("View", "Events", new { id = EventID });
        }
    }
}