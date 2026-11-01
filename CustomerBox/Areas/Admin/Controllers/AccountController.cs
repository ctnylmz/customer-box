using CustomerBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerBox.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.Username = user.UserName;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                ModelState.AddModelError("", "Kullanıcı adı boş olamaz.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var existingUser = await _userManager.FindByNameAsync(newUsername);

            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanılıyor.");
                return View();
            }

            var result = await _userManager.SetUserNameAsync(user, newUsername);

            if (result.Succeeded)
            {
                TempData["Message"] = "Kullanıcı adı güncellendi.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Güncelleme başarısız.");
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Login", new { area = "Admin" });

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Message"] = "Şifre başarıyla değiştirildi.";
                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}
