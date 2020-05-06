using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер отвечающий за задачи пользователей
    /// </summary>
    [Authorize]
    public class MissionController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public MissionController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// ВОзвращает представление создания нового мероприятия после заявки
        /// </summary>
        /// <returns></returns>
        public IActionResult NewEvent()
        {
            ViewBag.EventTypes = StaticValues.GetEventTypes();
            ViewBag.FromRequest = true;
            return RedirectToAction("New", "Events", new { FromRequest = true });
        }

        /// <summary>
        /// Создаёт новую заявку для текущего пользователя
        /// </summary>
        /// <param name="RequestText">Текст заявки</param>
        /// <returns></returns>
        public async Task<IActionResult> NewRequest(string RequestText)
        {
            var request = new Mission
            {
                CreatedDate = DateTime.Now,
                DeadLine = DateTime.Now.AddDays(1),
                MissionText = RequestText,
                MissionType = "Заявка",
                Status = "Создано",
                Event = await db.Events.Where(e => e.Name == "TempEvent").FirstOrDefaultAsync(),
                Employeer = await db.Managers.Where(m => m.Email == User.Identity.Name).FirstOrDefaultAsync()
            };
            await db.Missions.AddAsync(request);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }

        /// <summary>
        /// Возвращает представление для отклонения заявки
        /// </summary>
        /// <param name="id">id заявки</param>
        /// <returns></returns>
        public IActionResult Reject(int id)
        {
            ViewBag.mId = id;
            ViewBag.RejectCauses = new SelectList(StaticValues.RejectCauses);
            return View();
        }

        /// <summary>
        /// Закрывает заявку
        /// </summary>
        /// <param name="RejPrin">Причина отклонения заявки</param>
        /// <param name="id">id заявки</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Reject(string RejPrin, int id)
        {

            var mission = await db.Missions.Where(m => m.Id == id).FirstOrDefaultAsync();
            mission.Status = "Закрыто";
            mission.MissionText += $"| Заявка отклонена, причина: {RejPrin}";
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }

        /// <summary>
        /// Увеличивает время на обработку заявки
        /// </summary>
        /// <param name="Id">Id заявки</param>
        /// <param name="postHour">Кол-во часов на которое откладывается дедлайн</param>
        /// <returns></returns>
        public async Task<IActionResult> Postpound(int Id, int postHour)
        {
            var mission = await db.Missions.Where(m => m.Id == Id).FirstOrDefaultAsync();
            mission.DeadLine = mission.DeadLine.AddHours(postHour);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }

        /// <summary>
        /// Возвращает представление для действий над заданием 
        /// </summary>
        /// <param name="id">ID задания</param>
        /// <returns></returns>
        public async Task<IActionResult> Close(int id)
        {
            var mission = await db.Missions.Include(e => e.Employeer).Include(e => e.Event).Where(e => e.Id == id).FirstOrDefaultAsync();
            var type = mission.MissionType;
            if (type == "Заявка" || type == "Автоматическая заявка")
            {
                return View("CloseRequest", mission);
            }
            return RedirectToAction("View", "Events", new { id = mission.EventId, missionId = mission.Id });
        }
    }
}