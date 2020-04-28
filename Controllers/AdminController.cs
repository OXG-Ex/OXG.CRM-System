using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
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
            await WatchDog.FindDeadlineAsync(db);
            var data = new AdminIndexVM();
            data.Last30Days = new List<string>();
            data.EventsSum = new List<decimal>();
            data.RejectNum = new List<int>();
            data.WorksName = new List<string>();
            data.WorksNum = new List<int>();
            data.TypesName = new List<string>();
            data.TypesCount = new List<int>();
            data.ManagerName = new List<string>();
            data.ManagerRequestCount = new List<int>();
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


            foreach (var work in db.Works)
            {
                data.WorksName.Add(work.Name);
                data.WorksNum.Add(work.OrdersCount);
            }

            data.TypesName = StaticValues.GetEventTypesList();
            for (int i = 0; i < data.TypesName.Count(); i++)
            {
                data.TypesCount.Add(await db.Events.Where(e => e.EventType == data.TypesName[i]).CountAsync());
            }


            foreach (var item in db.Managers)
            {
                data.ManagerName.Add(item.Name);
                data.ManagerRequestCount.Add(item.MissionFromRequestNum);
            }

            data.EventsSum.Reverse();
            data.Last30Days.Reverse();
            return View(data);
        }

        public async Task<IActionResult> Employeers()
        {
            var data = new AdminEmployeersVM();
            data.Managers = await db.Managers.ToListAsync();
            data.Technics = await db.Technics.ToListAsync();
            data.Artists = await db.Artists.ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> DeleteEmployeer(string id)
        {
            var emp = await db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            db.Users.Remove(emp);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Setting(string id)
        {
            var model = new AdminSettingVM();
            model.CompanyName = StaticValues.CompanyName;
            model.EmailLogin = StaticValues.EmailLogin;
            model.EmailPassword = StaticValues.EmailPassword;
            return View(model);
        }

        public async Task<IActionResult> Works()
        {
            var model = db.Works.Select(e => e.Name);
            return View(model);
        }

        public async Task<IActionResult> Clients()
        {
            var model = db.Clients;
            model.Remove(model.Where(m => m.Name=="Temp").FirstOrDefault());
            return View(model);
        }

        public async Task<IActionResult> EditWork(string WorkName, decimal WorkPrice, string WorkDescription)
        { 
            var work = await db.Works.Where(w => w.Name == WorkName).FirstOrDefaultAsync();
            if (work!=null)
            {
                work.Price = WorkPrice;
                work.Description = WorkDescription;
                await db.SaveChangesAsync();
            }
            else
            {
                var wrk = new Work();
                wrk.Name = WorkName;
                wrk.Price = WorkPrice;
                wrk.Description = WorkDescription;
                await db.Works.AddAsync(wrk);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Works");
        }

        public async Task<IActionResult> DeleteWork(string WorkName)
        {
            var work = await db.Works.Where(w => w.Name == WorkName).FirstOrDefaultAsync();
            db.Works.Remove(work);
            await db.SaveChangesAsync();
            return RedirectToAction("Works");
        }

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

        public IActionResult ContractTemplate()
        {
            return PhysicalFile(appEnvironment.ContentRootPath + "\\wwwroot\\files\\template.docx", "application / docx", "template.docx");
        }

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

        public async Task<IActionResult> Notices()
        {
            var model = db.Notices;
            return View(model);
        }

    }
}