using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер ответственный за администрирование системы
    /// </summary>
    [Authorize(Roles = "Администратор")]
    public class AdminController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment appEnvironment;
        public AdminController(CRMDbContext context, IWebHostEnvironment _appEnvironment)
        {
            db = context;
            appEnvironment = _appEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            //Вызов метода поиска про*баных дедлайнов
            await WatchDog.FindDeadlineAsync(db);
            //объект хранящий данные отображаемые в графиках на странице Admin/Index
            var data = new AdminIndexVM
            {
                Last30Days = new List<string>(),
                EventsSum = new List<decimal>(),
                RejectNum = new List<int>(),
                WorksName = new List<string>(),
                WorksNum = new List<int>(),
                TypesName = new List<string>(),
                TypesCount = new List<int>(),
                ManagerName = new List<string>(),
                ManagerRequestCount = new List<int>()
            };
            //Получение последних 30 дней и суммы заказов услуг за конкретный день
            for (int i = 0; i < 30; i++)
            {
                data.Last30Days.Add(DateTime.Now.AddDays(-i).ToShortDateString());
                data.EventsSum.Add(await db.Events.Where(e => e.CreatedDate.DayOfYear == DateTime.Now.AddDays(-i).DayOfYear).SumAsync(e => e.TotalPrice));
            }

            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Спам")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Высокая стоимость услуг")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободной аппаратуры")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободного реквизита")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободных артистов")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Недостаточный ассортимент услуг")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Не соответствие площадки ТБ")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Слишком далеко")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет разрешения от спец. служб")).CountAsync());

            //Получение списка услуг и кол-ва заказов каждой услуги
            foreach (var work in db.Works)
            {
                data.WorksName.Add(work.Name);
                data.WorksNum.Add(work.OrdersCount);
            }

            //Получение списка типов мероприятий и их кол-ва
            data.TypesName = StaticValues.GetEventTypesList();
            for (int i = 0; i < data.TypesName.Count(); i++)
            {
                data.TypesCount.Add(await db.Events.Where(e => e.EventType == data.TypesName[i]).CountAsync());
            }

            //Получение списка менеджеров и кол-ва одобренных заявок
            foreach (var item in db.Managers)
            {
                data.ManagerName.Add(item.Name);
                data.ManagerRequestCount.Add(item.MissionFromRequestNum);
            }

            data.EventsSum.Reverse();
            data.Last30Days.Reverse();
            return View(data);
        }

        //Список сотрудников
        public async Task<IActionResult> Employeers()
        {
            var data = new AdminEmployeersVM
            {
                Managers = await db.Managers.ToListAsync(),
                Technics = await db.Technics.ToListAsync(),
                Artists = await db.Artists.ToListAsync()
            };
            return View(data);
        }

        /// <summary>
        /// Удаляет сотрудника
        /// </summary>
        /// <param name="id">ID сотрудника</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteEmployeer(string id)
        {
            var emp = await db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            db.Users.Remove(emp);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Возвращает страницу с настройками приложения 
        /// </summary>
        public IActionResult Setting()
        {
            var model = new AdminSettingVM
            {
                CompanyName = StaticValues.CompanyName,
                EmailLogin = StaticValues.EmailLogin,
                EmailPassword = StaticValues.EmailPassword
            };
            return View(model);
        }
        /// <summary>
        /// Редактирование списка услуг
        /// </summary>
        /// <returns></returns>
        public IActionResult Works()
        {
            var model = db.Works.Select(e => e.Name);
            return View(model);
        }

        /// <summary>
        /// Список клиентов
        /// </summary>
        /// <returns></returns>
        public IActionResult Clients()
        {
            var model = db.Clients;
            model.Remove(model.Where(m => m.Name == "Temp").FirstOrDefault());
            return View(model);
        }

        /// <summary>
        /// Сохранение изменений в услуге или создание новой
        /// </summary>
        /// <param name="WorkName">Наименование</param>
        /// <param name="WorkPrice">Стоимость</param>
        /// <param name="WorkDescription">Описание</param>
        /// <returns></returns>
        public async Task<IActionResult> EditWork(string WorkName, decimal WorkPrice, string WorkDescription)
        {
            //Получение услуги из БД
            var work = await db.Works.Where(w => w.Name == WorkName).FirstOrDefaultAsync();
            //Если услуга существует, то внести изменения, иначе создать новую
            if (work != null)
            {
                work.Price = WorkPrice;
                work.Description = WorkDescription;
                await db.SaveChangesAsync();
            }
            else
            {
                var wrk = new Work
                {
                    Name = WorkName,
                    Price = WorkPrice,
                    Description = WorkDescription
                };
                await db.Works.AddAsync(wrk);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Works");
        }

        /// <summary>
        /// Удаление услуги из БД
        /// </summary>
        /// <param name="WorkName">Наименование</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteWork(string WorkName)
        {
            var work = await db.Works.Where(w => w.Name == WorkName).FirstOrDefaultAsync();
            db.Works.Remove(work);
            await db.SaveChangesAsync();
            return RedirectToAction("Works");
        }

        /// <summary>
        /// Сохранение изменений настройки аккаунта Email  
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveEmailSetting(AdminSettingVM model)
        {
            if (ModelState.IsValid)
            {
                StaticValues.CompanyName = model.CompanyName;
                StaticValues.EmailLogin = model.EmailLogin;
                StaticValues.EmailPassword = model.EmailPassword;
                return RedirectToAction("Setting");
            }
            return View("Setting");
        }

        /// <summary>
        /// Получение текущего шаблона для генерации договора на оказание услуг
        /// </summary>
        /// <returns>Текущий шаблон договора в формате .docx</returns>
        public IActionResult ContractTemplate()
        {
            return PhysicalFile(appEnvironment.ContentRootPath + "\\wwwroot\\files\\template.docx", "application / docx", "template.docx");
        }

        /// <summary>
        /// Загрузка нового шаблона договора
        /// </summary>
        /// <param name="uploadedFile">Новый файл договора</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> NewTemplate(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.ContentType.Contains("officedocument.wordprocessingml.document"))
            {
                var path = "/files/" + "template.docx";
                using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                ViewBag.Message = "Файл успешно загружен";
                return View("Setting");
            }
            ViewBag.BadMessage = "Некорректный файл";
            return View("Setting");
        }
    }
}