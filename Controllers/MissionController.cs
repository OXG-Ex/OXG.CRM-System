using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult New()
        {
            return View();
        }

        public async Task<IActionResult> Close(int id)
        {
            var mission = await db.Missions.Include(e => e.Employeer).Include(e => e.Event).Where(e => e.Id == id).FirstOrDefaultAsync();
            var type = mission.MissionType;
            switch (type)
            {//TODO: Добавить больше типов мероприятий
                case "Клиент":
                    ViewBag.Message = $"{mission.MissionText}";
                    return RedirectToAction("Client", "Sight", new { id = mission.Event.ClientId });
                    
                case "Артист":
                    ViewBag.Message = $"{mission.MissionText}";
                    return RedirectToAction("Artist", "Sight", new { id = mission.Event.ArtistId });
                   
                case "Техник":
                    ViewBag.Message = $"{mission.MissionText}";
                    return RedirectToAction("Technic", "Sight", new { id = mission.Event.TechnicId });
                    
                case "Договор":
                    ViewBag.Message = $"{mission.MissionText}";
                    return RedirectToAction("Contract", "Sight", new { id = mission.Event.ContractId });
                    
                default:
                    ViewBag.Message = $"!!!ЭТО ОТЛАДОЧНАЯ ИНФОРМАЦИЯ!!!{mission.MissionText}";
                    return StatusCode(404);
            }
        }
    }
}