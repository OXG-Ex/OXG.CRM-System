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
using OXG.CRM_System.Models.Employeers;


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

        public async Task<IActionResult> Index()
        {
            var user = await db.Managers.Include(e => e.Missions).Include(e => e.Clients).Include(e => e.Contracts).Include(e => e.Events).Where(e => e.Email == User.Identity.Name).FirstOrDefaultAsync();
            return View(user);
        }

        public IActionResult ChangePhoto()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.ContentType.Contains("image"))
            {
                var path = "/AccountPhotos/" + uploadedFile.FileName;
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

        [HttpPost]
        public async Task<IActionResult> SaveChanges(Manager manager)
        {
            var temp = await db.Managers.Where(e => e.Email == User.Identity.Name && e.UserName == User.Identity.Name).FirstOrDefaultAsync();
            temp.Name = manager.Name;
            temp.PhoneNumber = manager.PhoneNumber;
            temp.Email = manager.Email;
            await db.SaveChangesAsync();
            return RedirectToAction("Personal","Manager");
        }
    }
}