using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    public class AccountController : Controller
    {
        CRMDbContext db;
        UserManager<User> userManager;
        SignInManager<User> signInManager;
        public AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager, CRMDbContext context)
        {
            db = context;
            userManager = _userManager;
            signInManager = _signInManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
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
                        user = new Manager() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                        break;
                    case "Техник":
                        user = new Technic() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                        break;
                    case "Печатник":
                        user = new Printer() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                        break;
                    case "Артист":
                        user = new Artist() { Name = model.Name, UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                        break;
                }


                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
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