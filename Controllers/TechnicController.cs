using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер для сотрудника-техника
    /// </summary>
    public class TechnicController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public TechnicController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// Возвращает представление рабочей зоны техника
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> Index()
        {
            var tech = await db.Technics.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(e => e.Client).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(tech);
        }

        /// <summary>
        /// Возвращает страницу личного кабинета
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> Personal()
        {
            var tech = await db.Technics.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(e => e.Client).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(tech);
        }

        /// <summary>
        /// представление для загрузки нового изображения профиля
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePhoto()
        {
            return View();
        }

        /// <summary>
        /// Метод сохраняющий новую фотографию пользователя
        /// </summary>
        /// <param name="uploadedFile">Загружаемая фотография</param>
        /// <returns></returns>
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
                var user = await db.Technics.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                user.Photo = path;
                await db.SaveChangesAsync();

                return RedirectToAction("Personal");
            }
            return Content("Некорректный файл");
        }

        /// <summary>
        /// Возвращает представление для указания техника мероприятия
        /// </summary>
        /// <param name="id">Id мероприятия</param>
        /// <returns></returns>
        [Authorize(Roles = "Менеджер")]
        public IActionResult AddToEvent(int id)
        {
            ViewBag.Technics = new SelectList(db.Technics, "Id", "Name");
            ViewBag.EventId = id;
            return View();
        }

        /// <summary>
        /// Добавляет техника к мероприятию
        /// </summary>
        /// <param name="TechnicId">Id Техника</param>
        /// <param name="eventId">Id мероприятия</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Менеджер")]
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
            return RedirectToAction("Index", "Manager");
        }

        /// <summary>
        /// Сохранение изменений в аккаунте пользователя
        /// </summary>
        /// <param name="technic">Модель техника полученная из представления</param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает представление для подтверждения/отклонения мероприятия
        /// </summary>
        /// <param name="id">Id мероприятия</param>
        /// <returns></returns>
        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> ConfirmEvent(int id)
        {
            var evnt = await db.Events.Include(e => e.Works).Where(e => e.Id == id).FirstOrDefaultAsync();
            return View(evnt);
        }

        /// <summary>
        /// Сохраняет ответ техника касаемо подтверждения/отклонения мероприятия
        /// </summary>
        /// <param name="sub">Заключение техника</param>
        /// <param name="managerMessage">Сообщение для менеджера</param>
        /// <param name="eventId">Id мероприятия</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Техник")]
        public async Task<IActionResult> ConfirmEvent(string sub, string managerMessage, int eventId)
        {
            var evnt = await db.Events.Include(e => e.Works).Include(e => e.Manager).Include(e => e.Client).Include(e => e.Missions).Where(e => e.Id == eventId).FirstOrDefaultAsync();

            if (sub == "Отклонить")
            {
                var mission = new Mission()
                {
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddDays(2),
                    MissionType = "Звонок",
                    MissionText = $"Техник не согласовал проведение услуг. Сообщение:{managerMessage}",
                    Employeer = evnt.Manager,
                    Status = "Создано",
                    Event = evnt,
                };

                await db.Missions.AddAsync(mission);
            }
            if (sub == "Подтвердить")
            {
                var contractMission = new Mission()
                {
                    Employeer = evnt.Manager,
                    Event = evnt,
                    CreatedDate = DateTime.Now,
                    DeadLine = DateTime.Now.AddHours(48),
                    MissionType = "Договор",
                    MissionText = $"Создать договор для клиента '{evnt.Client.Name}' по мероприятию '{evnt.Name}'"
                };
                evnt.Status = "Согласовано";
                await db.Missions.AddAsync(contractMission);
            }
            var techMission = evnt.Missions.Where(m => m.MissionType.Contains("Согласование площадки")).FirstOrDefault();
            techMission.Status = "Закрыто";
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Technic");
        }
    }
}