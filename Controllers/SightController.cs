using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер отвечающий за просмотр всякого
    /// </summary>
    [Authorize]
    public class SightController : Controller
    {
        private readonly CRMDbContext db;
        private readonly IWebHostEnvironment _appEnvironment;
        public SightController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Mission(int id)
        {
            var mission = await db.Missions.Include(m => m.Employeer).Where(m => m.Id == id).FirstOrDefaultAsync();
            return View(mission);
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

        public async Task<IActionResult> Client(int id)
        {
            var client = await db.Clients.Include(m => m.ClientVK).Include(m => m.Events).Include(m => m.Manager).Where(m => m.Id == id).FirstOrDefaultAsync();
            return View(client);
        }
    }
}