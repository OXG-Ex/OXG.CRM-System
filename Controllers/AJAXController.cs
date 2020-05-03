using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AJAXController : ControllerBase
    {
        private readonly CRMDbContext db;
        public AJAXController( CRMDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<JsonResult> GetClient(int id)
        {
            var client = await db.Clients.Where(c => c.Id == id).FirstOrDefaultAsync();
            return new JsonResult(client);
        }

        [HttpGet]
        public async Task<JsonResult> GetClients()
        {
            var clients = db.Clients;
            var client = await db.Clients.Where(c => c.Name == "Temp").FirstOrDefaultAsync();
            clients.Remove(client);
            return new JsonResult(clients);
        }

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
            return new JsonResult(model);
        }

        [HttpGet]
        public async Task<ContentResult> GetNoticesNum(string name)
        {
            var user = await db.Employeers.Where(u => u.Email == name).FirstOrDefaultAsync();
            var id = user.Id;
            int notices;
            if (!User.IsInRole("Администратор"))
            {
                notices =await db.Notices.Where(n => n.EmployeerId == id && !n.IsViewed).CountAsync();
            }
            else
            {
                notices =await db.Notices.Where(n => !n.IsViewed).CountAsync();
            }
            return Content(notices.ToString());
        }

        [HttpGet]
        public async Task<JsonResult> GetWork(string name)
        {
            var work = await db.Works.Where(u => u.Name == name).FirstOrDefaultAsync();
            return new JsonResult(work);
        }


        [HttpGet]
        public async Task<JsonResult> GetEmployeer(string name)
        {
            var emp = await db.Employeers.Include(u => u.Missions).Where(u => u.Name == name).FirstOrDefaultAsync();
            return new JsonResult(new AdminEmployeerVM(emp));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Content("Ok");
        }
    }
}