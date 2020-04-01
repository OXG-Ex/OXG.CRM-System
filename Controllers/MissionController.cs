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
            if (true)
            {

            }
            return RedirectToAction("View", "Events", new { id = mission.EventId, missionId =mission.Id });
        }
    }
}