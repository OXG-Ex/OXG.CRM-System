using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OXG.CRM_System.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        private readonly CRMDbContext db;
        public HomeController(ILogger<HomeController> logger, CRMDbContext context, RoleManager<IdentityRole> role, UserManager<User> user)
        {
            _logger = logger;
            db = context;
            roleManager = role;
            userManager = user;
        }

        public async Task<IActionResult> Index()
        {
            await DbInitializer.InitializeAsync(userManager, roleManager, db);//Инициализация БД
            await WatchDog.FindDeadlineAsync(db);//Вызов метода поиска про*баных дедлайнов
            await WatchDog.FindNewRequestAsync(db);//Вызов метода поиска новых задач, на основе уже проводившихся мероприятий
            await db.SaveChangesAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}