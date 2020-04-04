using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;

namespace OXG.CRM_System.Controllers
{
    [Authorize]
    public class MissionController : Controller
    {
        CRMDbContext db;
        IWebHostEnvironment _appEnvironment;
        public MissionController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NewEvent()
        {
            ViewBag.EventTypes = TypesAndStaticValues.GetEventTypes();
            ViewBag.FromRequest = true;
            return View();
        }

        public async Task<IActionResult> NewRequest(string RequestText)
        {
            var request = new Mission();
            request.CreatedDate = DateTime.Now;
            request.DeadLine = DateTime.Now.AddDays(1);
            request.MissionText = RequestText;
            request.MissionType = "Заявка";
            request.Status = "Создано";
            request.Event = await db.Events.Where(e => e.Name == "TempEvent").FirstOrDefaultAsync();
            request.Employeer = await db.Managers.Where(m => m.Email == User.Identity.Name).FirstOrDefaultAsync();
            await db.Missions.AddAsync(request);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Manager");
        }

        public IActionResult Reject(int id)
        {
            ViewBag.mId = id;
            ViewBag.RejectCauses = new SelectList(TypesAndStaticValues.RejectCauses);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string RejPrin, int id)
        {
            var mission = await db.Missions.Where(m => m.Id == id).FirstOrDefaultAsync();
            mission.Status = "Закрыто";
            mission.MissionText += $"| Заявка отклонена, причина: {RejPrin}";
            await db.SaveChangesAsync();
            return RedirectToAction("Index","Manager");
        }

        public async Task<IActionResult> Postpound(int Id, int postHour)
        {
            var mission = await db.Missions.Where(m => m.Id == Id).FirstOrDefaultAsync();
            mission.DeadLine = mission.DeadLine.AddHours(postHour);
            await db.SaveChangesAsync();
            return RedirectToAction("Index","Manager");
        }

        public async Task<IActionResult> Close(int id)
        {
            var mission = await db.Missions.Include(e => e.Employeer).Include(e => e.Event).Where(e => e.Id == id).FirstOrDefaultAsync();
            var type = mission.MissionType;
            if (type == "Заявка")
            {
                return View("CloseRequest", mission);
            }
            return RedirectToAction("View", "Events", new { id = mission.EventId, missionId =mission.Id });
        }
    }
}