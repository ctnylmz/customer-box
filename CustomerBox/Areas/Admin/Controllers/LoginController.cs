using CustomerBox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerBox.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AdminService _adminService;

        public LoginController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, AdminService adminService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Default", new { area = "Admin" });
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(string userName, string password)
        {
            if (!ModelState.IsValid)
                return View();

            var result = await _signInManager.PasswordSignInAsync(userName, password, true, false);


            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Default", new { area = "Admin" });
            }
               

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            // Sistemde kullanıcı varsa kurulumu kapat
            if (_userManager.Users.Any())
                return NotFound();

            await _adminService.Check();

            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login", new { area = "Admin" }); // Admin login sayfasına yönlendir
        }
    }
}
