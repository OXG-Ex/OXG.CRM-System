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
    public class SightController : Controller
    {
        CRMDbContext db;
        IWebHostEnvironment _appEnvironment;
        public SightController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Manager(string id)
        {
            var manager = await db.Managers.Include(e => e.Clients).Include(e => e.Events).Where(m => m.Id == id).FirstOrDefaultAsync();
            return View(manager);
        }

        public async Task<IActionResult> Contract(int id)
        {
            var contract = await db.Contracts.Where(c => c.Id == id).FirstOrDefaultAsync();
            return File(contract.Path, "application / docx", contract.Name);
        }
    }
}