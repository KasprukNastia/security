using System;
using Lab5_6.Business;
using Lab5_6.DAL;
using Lab5_6.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lab5_6.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager _userManager;

        public AccountController(IUserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserViewModel registerUserModel)
        {
            if (ModelState.IsValid)
            {
                if(_userManager.CreateUser(registerUserModel))
                    return RedirectToAction("WelcomeUser", "Home", new { userName = registerUserModel.Email });
                else
                    return RedirectToAction("Error", "Home", new { errorMessage = "Error occured while storing user" });
            }

            return View(registerUserModel);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserViewModel loginUserModel)
        {
            if (ModelState.IsValid)
            {
                if (_userManager.LoginUser(loginUserModel))
                    return RedirectToAction("WelcomeUser", "Home", new { userName = loginUserModel.Email });
                else
                    ModelState.AddModelError("", "Wrong Email and/or Password");
            }

            return View(loginUserModel);
        }
    }
}
