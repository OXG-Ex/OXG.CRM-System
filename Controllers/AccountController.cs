using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly CRMDbContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IWebHostEnvironment _appEnvironment;
        public AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager, CRMDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            userManager = _userManager;
            signInManager = _signInManager;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            return RedirectToAction("Index","Home");
        }

        

        public IActionResult Login()
        {
            return View();
        }

        public async Task <IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный Email и(или) пароль");

                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User();
                switch (model.UserType)
                {
                    case "Менеджер":
                        user = new Manager() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Photo= "/images/defaultPhoto.png" };
                        break;
                    case "Техник":
                        user = new Technic() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Photo = "/images/defaultPhoto.png" };
                        break;
                    case "Артист":
                        user = new Artist() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Photo = "/images/defaultPhoto.png" };
                        break;
                }


                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, model.UserType);
                    // установка куки
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
    }
}