using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// API контроллер возвращающий запрошенную через JS информацию в формате строкового значения или JSON
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AJAXController : ControllerBase
    {
        private readonly CRMDbContext db;
        public AJAXController(CRMDbContext context)
        {
            db = context;
        }

        /// <summary>
        /// Возвращает информацию о клиенте
        /// </summary>
        /// <param name="id">id клиента</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetClient(int id)
        {
            var client = await db.Clients.Where(c => c.Id == id).FirstOrDefaultAsync();
            return new JsonResult(client);
        }

        /// <summary>
        /// Возвращает список клиентов 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetClients()
        {
            var clients = db.Clients;
            var client = await db.Clients.Where(c => c.Name == "Temp").FirstOrDefaultAsync();
            ///Удалить из списка клиента с именем Temp, этот клиент используется в боте ВК
            clients.Remove(client);
            return new JsonResult(clients);
        }

        /// <summary>
        /// Возвращает фотографию профиля пользователя в шапке приложения,
        /// по хорошему нужно путь к фотографии нужно хранить в куках, чтобы не обращаться каждый раз к серверу
        /// </summary>
        /// <param name="name">Email пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserPhoto(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var t = user.Photo;
            if (!string.IsNullOrWhiteSpace(t))
            {
                return Content(t);
            }
            else
            {
                return Ok();
            }
        }

        /// <summary>
        /// Устанавливает статус всех уведомлений пользователя в "Простмотрено" 
        /// </summary>
        /// <param name="name">UserName(Email) Пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SetViewed(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var id = user.Id;
            IQueryable<Notice> notices;
            if (!User.IsInRole("Администратор"))
            {
                notices = db.Notices.Where(n => n.EmployeerId == id && !n.IsViewed);
            }
            else
            {
                notices = db.Notices.Where(n => !n.IsViewed);
            }
            foreach (var item in notices)
            {
                item.IsViewed = true;
            }
            await db.SaveChangesAsync();
            return new OkResult();
        }

        /// <summary>
        /// Возвращает последние 25 уведомлений пользователя в формате JSON
        /// Если пользователь в роли Администратор то возвращает уведомления всех пользователей
        /// </summary>
        /// <param name="name">Email текущего пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetNotices(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var id = user.Id;
            IQueryable<Notice> notices;
            if (!User.IsInRole("Администратор"))
            {
                notices = db.Notices.Where(n => n.EmployeerId == id);
            }
            else
            {
                notices = db.Notices;
            }
            var model = new List<NoticeVM>();
            foreach (var item in notices)
            {
                var notice = new NoticeVM(item);
                model.Add(notice);
            }
            if (model.Count > 25)
            {
                var mod = model.TakeLast(25);
                mod = mod.Reverse();

                return new JsonResult(mod);
            }
            model.Reverse();
            return new JsonResult(model);
        }

        /// <summary>
        /// Возвращает число непросмотренных уведомлений пользователя
        /// </summary>
        /// <param name="name">Email пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ContentResult> GetNoticesNum(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var id = user.Id;
            var notices = 0;
            if (User!= null)
            {
                if (!User.IsInRole("Администратор"))
                {
                    notices = await db.Notices.Where(n => n.EmployeerId == id && !n.IsViewed).CountAsync();
                }
                else
                {
                    notices = await db.Notices.Where(n => !n.IsViewed).CountAsync();
                }
            }
            return Content(notices.ToString());
        }

        /// <summary>
        /// Возвращает информацию об услуге
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetWork(string name)
        {
            var work = await db.Works.Where(u => u.Name == name).FirstOrDefaultAsync();
            return new JsonResult(work);
        }

        /// <summary>
        /// Возвращает информацию о сотруднике
        /// </summary>
        /// <param name="name">Имя сотрудника</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetEmployeer(string name)
        {
            var emp = await db.Employeers.Include(u => u.Missions).Where(u => u.Name == name).FirstOrDefaultAsync();
            return new JsonResult(new AdminEmployeerVM(emp));
        }

        /// <summary>
        /// Проверка работы контроллера
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return Content("Ok");
        }
    }
}