using Microsoft.AspNetCore.Identity;

namespace CustomerBox.Services
{
    public class AdminService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)

        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Check()
        {
            // Sistemde hiç kullanıcı var mı kontrol et
            if (!_userManager.Users.Any())
            {
                // Admin rolü mevcut mu kontrol et
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    // Admin rolü yoksa oluştur
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Admin kullanıcısını oluştur
                var user = new IdentityUser
                {
                    UserName = "Admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "admin123");

                // Kullanıcı başarıyla oluşturulduysa Admin rolüne ata
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
