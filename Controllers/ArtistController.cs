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
    /// Контроллер для сотрудников работающих непосредственно на мероприятии
    /// </summary>
    public class ArtistController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public ArtistController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// Возвращает страницу с текущими мероприятиями
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Артист")]
        public async Task<IActionResult> Index()
        {
            var artist = await db.Artists.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(m => m.Works).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(artist);
        }
        /// <summary>
        /// Возвращает страницу личного кабинета
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Артист")]
        public async Task<IActionResult> Personal()
        {
            var artist = await db.Artists.Include(t => t.Missions).ThenInclude(m => m.Event).ThenInclude(m => m.Works).Where(t => t.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(artist);
        }

        /// <summary>
        /// представление для загрузки нового изображения профиля
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Артист")]
        public IActionResult ChangePhoto()
        {
            return View();
        }

        /// <summary>
        /// Метод сохранения изменений личных данных 
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод сохраняющий новую фотографию пользователя
        /// </summary>
        /// <param name="uploadedFile">Загружаемая фотография</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile uploadedFile)
        {
            //Проверка типа файла
            if (uploadedFile != null && uploadedFile.ContentType.Contains("image"))
            {
                var path = "/AccountPhotos/" + uploadedFile.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                var user = await db.Artists.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                user.Photo = path;
                await db.SaveChangesAsync();

                return RedirectToAction("Personal");
            }
            return Content("Некорректный файл");
        }

        /// <summary>
        /// Возвращает представление для добавления артиста в качестве ответственного к мероприятию
        /// </summary>
        /// <param name="id">id мероприятия</param>
        /// <returns>Представление</returns>
        [Authorize(Roles = "Менеджер")]
        public IActionResult AddToEvent(int id)
        {
            ViewBag.Artists = new SelectList(db.Artists, "Id", "Name");
            ViewBag.EventId = id;
            return View();
        }

        /// <summary>
        /// Добавляет артиста в мероприятие
        /// </summary>
        /// <param name="ArtistId">id артиста</param>
        /// <param name="eventId">id мероприятия</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> AddToEvent(string ArtistId, int eventId)
        {
            var artist = await db.Artists.Where(t => t.Id == ArtistId).FirstOrDefaultAsync();
            var eventDb = await db.Events.Include(e => e.Artist).Include(e => e.Missions).Where(t => t.Id == eventId).FirstOrDefaultAsync();

            //новое задание для артиста
            var mission = new Mission()
            {
                Employeer = artist,
                CreatedDate = DateTime.Now,
                DeadLine = eventDb.DeadLine,
                Event = eventDb,
                MissionType = "Заказ",
                MissionText = "Провести указанные мероприятия"
            };

            //уведомление артиста о новом задании
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