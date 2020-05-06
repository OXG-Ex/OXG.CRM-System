using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер для сотрудника-менеджера
    /// </summary>
    [Authorize(Roles = "Менеджер")]
    public class ManagerController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public ManagerController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// Возвращает представление основной рабочей зоны менеджера с мероприятиями/заявками/задачами
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            await WatchDog.FindDeadlineAsync(db);

            var user = await db.Managers.Include(e => e.Missions).Include(e => e.Clients).Include(e => e.Contracts).Include(e => e.Events).Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
            var failedNum = 0;
            var warNum = 0;
            foreach (var item in user.Missions)
            {
                if (item.LeftTime.TotalSeconds < 0 && item.Status != "Закрыто")
                {
                    failedNum++;
                    ViewBag.BadMessage = $"У вас {failedNum} проваленных дедлайнов, срочно разберитесь с этим";
                    item.Status = "Дедлайн провален";
                    continue;
                }
                if (item.LeftTime.TotalHours < 2 && item.Status != "Закрыто")
                {
                    item.Status = "Дедлайн близок к провалу";
                    warNum++;
                    ViewBag.WarningMessage = $"У вас {warNum} заданий близких к провалу дедлайна, разберитесь с этим";
                }
            }
            return View(user);
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
                var user = await db.Managers.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                user.Photo = path;
                await db.SaveChangesAsync();

                return RedirectToAction("Personal", "Manager");
            }
            return Content("Некорректный файл");
        }

        /// <summary>
        /// Возвращает страницу личного кабинета
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Personal(string Id)
        {
            var user = new Manager();
            if (Id == null)
            {
                user = await db.Managers.Include(e => e.Missions).Include(e => e.Clients).Include(e => e.Contracts).Include(e => e.Events).Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
            }
            else
            {
                user = await db.Managers.Include(e => e.Missions).Include(e => e.Clients).Include(e => e.Contracts).Include(e => e.Events).Where(e => e.Id == Id).FirstOrDefaultAsync();
            }
            return View(user);
        }

        /// <summary>
        /// Сохранение изменений в аккаунте пользователя
        /// </summary>
        /// <param name="manager">Модель менеджера полученная из представления</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveChanges(Manager manager)
        {
            var temp = await db.Managers.Where(e => e.Email == User.Identity.Name && e.UserName == User.Identity.Name).FirstOrDefaultAsync();
            temp.Name = manager.Name;
            temp.PhoneNumber = manager.PhoneNumber;
            temp.Email = manager.Email;
            temp.VkAdress = manager.VkAdress;
            temp.TgAdress = manager.TgAdress;
            await db.SaveChangesAsync();
            return RedirectToAction("Personal", "Manager");
        }
    }
}