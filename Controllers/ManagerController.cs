using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;

namespace OXG.CRM_System.Controllers
{
    [Authorize(Roles = "Менеджер")]
    public class ManagerController : Controller
    {
        CRMDbContext db;
        IWebHostEnvironment _appEnvironment;
        public ManagerController(CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChangePhoto()
        {
            return View();
        }


        ///image/jpeg
        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.ContentType.Contains("image"))
            {
                // путь к папке Files
                var ttt = uploadedFile.ContentType;
                var path = "/AccountPhotos/" + uploadedFile.FileName;
                //сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                var user = await db.Managers.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                user.Photo = path;
                await db.SaveChangesAsync();

                return RedirectToAction("Personal","Manager");
            }
            return View();
        }

        public async Task<IActionResult> Personal()
        {
            var user = await db.Managers.Include(e => e.Missions).Include(e => e.Clients).Include(e => e.Contracts).Include(e => e.Events).Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(user);
        }
    }
}