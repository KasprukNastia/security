using Lab5_6.Business;
using Lab5_6.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Lab5_6.Controllers
{
    public class StoreController : Controller
    {
        private readonly IUserManager _userManager;

        public StoreController(IUserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public IActionResult StoreSensitiveData()
        {
            return View();
        }

        [HttpPost]
        public IActionResult StoreSensitiveData(SensitiveDataViewModel sensitiveDataViewModel)
        {
            if (ModelState.IsValid)
            {
                if (_userManager.StoreSensitiveData(sensitiveDataViewModel))
                    return RedirectToAction("RetrieveSensitiveData", new { userName = sensitiveDataViewModel.Email });
                else
                    return RedirectToAction("Error", "Home", new { errorMessage = "Error occured while storing user sensitive data" });
            }

            return View(sensitiveDataViewModel);
        }

        [HttpGet]
        public IActionResult RetrieveSensitiveData([FromQuery] string userName)
        {
            SensitiveDataViewModel sensitiveData = _userManager.GetSensitiveData(userName);
            return View("StoreSensitiveData", sensitiveData);
        }
    }
}
