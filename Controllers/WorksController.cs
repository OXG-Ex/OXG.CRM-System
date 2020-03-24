using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;

namespace OXG.CRM_System.Controllers
{
    public class WorksController : Controller
    {
        CRMDbContext db;
        public WorksController(CRMDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DeleteFromEvent(int id, int eid)
        {
            var eventDb = await db.Events.Include(e => e.Works).Where(e => e.Id == eid).FirstOrDefaultAsync();
            var work = eventDb.Works.Where(w => w.Id == id).FirstOrDefault();
            eventDb.Works.Remove(work);
            await db.SaveChangesAsync();
            return RedirectToAction("View", "Events", new { id = eid });
        }

        public async Task<IActionResult> AddToEvent(int id)
        {
            var eventDb = await db.Events.Include(e => e.Works).Where(e => e.Id == id).FirstOrDefaultAsync();
            ViewBag.WorkList = new SelectList(db.Works,"Id","Name");
            return View(eventDb);
        }

        [HttpPost]
        public async Task<IActionResult> AddToEvent(int WorkId, int WorkNum, int EventID)
        {
            var eventDb = await db.Events.Include(e => e.Works).Where(e => e.Id == EventID).FirstOrDefaultAsync();
            var work = await db.Works.Where(w => w.Id == WorkId).FirstOrDefaultAsync();
            if (work.Num!= WorkNum)
            {
                var workTemp = new Work() { Name=work.Name, Price=work.Price, Num = WorkNum };
                eventDb.Works.Add(workTemp);
                eventDb.TotalPrice += workTemp.Sum;
            }
            else
            {
                eventDb.Works.Add(work);
                eventDb.TotalPrice += work.Sum;
            }
            await db.SaveChangesAsync();
            return RedirectToAction("View","Events",new {id =EventID });
        }
    }
}