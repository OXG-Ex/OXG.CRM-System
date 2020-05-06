using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TemplateEngine.Docx;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер отвечающий за работу с договорами
    /// </summary>
    [Authorize]
    public class ContractController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment appEnvironment;
        public ContractController(CRMDbContext context, IWebHostEnvironment _appEnvironment)
        {
            db = context;
            appEnvironment = _appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Отправляет email, с договором об оказании услуг клиенту
        /// </summary>
        /// <param name="id">Id мероприятия</param>
        /// <returns></returns>
        public async Task<IActionResult> SendToEmail(int id)
        {
            var eventDb = await db.Events.Include(e => e.Contract).Include(e => e.Manager).Include(e => e.Missions).Include(e => e.Client).Where(e => e.Id == id).FirstOrDefaultAsync();
            if (eventDb.Contract == null)
            {
                return Content("Ошибка: не найден контракт мероприятия");
            }

            if (eventDb.Client.Email == null)
            {
                return View("IncorrectEmail");
            }
            await EmailService.SendEmailAsync(eventDb.Client.Email, "Договор", $"Высылаю вам договор об оказании услуг на вашем мероприятии, ознакомьтесь с ним и в случае отсутствия вопросов жду вас в нашем офисе для подписания. По всем вопросам можете обращаться на этот адрес или по телефону {eventDb.Manager.PhoneNumber}", eventDb.Contract.Path);
            ViewBag.SendToAdress = eventDb.Client.Email;
            ViewBag.ClientName = eventDb.Client.Name;
            ViewBag.ContractName = eventDb.Contract.Name;

            var mission = eventDb.Missions.Where(m => m.MissionText.Contains("Отправить договор клиенту")).FirstOrDefault();
            mission.Status = "Закрыто";
            await db.SaveChangesAsync();
            return View();
        }

        /// <summary>
        /// Генерирует договор для мероприятия со списком услуг
        /// </summary>
        /// <param name="id">Id мероприятия</param>
        /// <returns></returns>
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Create(int id)
        {
            var pathTemplate = appEnvironment.ContentRootPath + "\\wwwroot\\files\\template.docx";
            var eventDb = await db.Events.Include(e => e.Client).Include(e => e.Manager).Include(e => e.Missions).Include(e => e.Works).Include(e => e.Contract).Where(e => e.Id == id).FirstOrDefaultAsync();
            if (eventDb.Works.Count == 0)
            {
                return Content("Ошибка: невозможно создать договор для мероприятия без услуг");
            }
            var workTable = new TableContent("Works");
            var i = 0;
            var filename = $"{eventDb.Id}" + ".docx";
            var pathDocument = appEnvironment.ContentRootPath + $"\\wwwroot\\files\\contracts\\{filename}";
            var docFile = new FileInfo(pathTemplate);

            docFile.CopyTo(appEnvironment.ContentRootPath + $"\\wwwroot\\files\\contracts\\{filename}", true);

            foreach (var item in eventDb.Works)
            {
                i++;
                workTable.AddRow(
                    new FieldContent("WorkIndex", $"{i}"),
                    new FieldContent("WorkName", item.Name),
                    new FieldContent("WorkNum", $"{item.Num}"),
                    new FieldContent("WorkSum", $"{item.Sum}"));
            }

            var valuesToFill = new Content(new FieldContent("docDate", DateTime.Now.ToShortDateString()),
                                           new FieldContent("docNum", $"{eventDb.Id}"),
                                           new FieldContent("EventTotalPrice", $"{eventDb.TotalPrice}"),
                                           new FieldContent("docClient", $"{eventDb.Client.Name}"),
                                           workTable);

            using (var outputDocument = new TemplateProcessor(pathDocument)
                .SetRemoveContentControls(false))
            {
                outputDocument.FillContent(valuesToFill);
                outputDocument.SaveChanges();
            }



            var contract = new Contract() { Event = eventDb, Manager = eventDb.Manager, Name = filename, CreatedDate = DateTime.Now, Client = eventDb.Client, Path = $"\\files\\contracts\\{filename}" };

            await db.AddAsync(contract);
            await db.SaveChangesAsync();

            var newMission = new Mission();
            if (eventDb.Client.Email == null)
            {
                newMission = new Mission() { CreatedDate = DateTime.Now, DeadLine = DateTime.Now.AddDays(1), Employeer = eventDb.Manager, Event = eventDb, MissionType = "Звонок", MissionText = $"Позвонить клиенту {eventDb.Client.Name} и согласовать итоговую стоимость мероприятия, в случае соглашения пригласить клиента в офис для подписания договора" };
            }
            else
            {
                newMission = new Mission() { CreatedDate = DateTime.Now, DeadLine = DateTime.Now.AddDays(1), Employeer = eventDb.Manager, Event = eventDb, MissionType = "Email", MissionText = $"Отправить договор об оказании услуг клиенту {eventDb.Client.Name}" };
            }
            await db.Missions.AddAsync(newMission);

            var mission = eventDb.Missions.Where(m => m.MissionText.Contains("Создать договор")).FirstOrDefault();
            mission.Status = "Закрыто";
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }
    }
}