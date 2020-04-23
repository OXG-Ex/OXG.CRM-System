using System;
using System.Collections.Generic;
using System.Linq;
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
                notices = db.Notices.Where(n => n.EmployeerId == id && !n.IsViewed);
            }
            else
            {
                notices = db.Notices.Where(n => !n.IsViewed);
            }
            var model = new List<NoticeVM>();
            foreach (var item in notices)
            {
                model.Add(new NoticeVM(item));
                model.Last().NoticeNum = await notices.CountAsync();
            }

            return new JsonResult(model);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Content("Ok");
        }
    }
}