# CustomerBox – Admin Panel ve Identity Yönetimi

## 1. Proje Hakkında

CustomerBox projesinde ASP.NET Core Identity kullanılarak Admin paneli için kullanıcı girişi ve hesap yönetimi işlemleri gerçekleştirilmektedir.

Sistem aşağıdaki işlemleri destekler:

- Admin kullanıcısının ilk kurulumda oluşturulması.
- Admin rolünün oluşturulması.
- Kullanıcı girişi ve oturum yönetimi.
- Kullanıcı adı değiştirme.
- Şifre değiştirme.
- Admin yetkilendirmesi.
- İlk kurulum tamamlandıktan sonra Setup adresinin kapatılması.

---

## 2. AppDbContext Yapılandırması

Identity kullanabilmek için `AppDbContext` sınıfı `DbContext` yerine `IdentityDbContext<IdentityUser>` sınıfından türetilir.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

Bu sayede Identity'nin ihtiyaç duyduğu kullanıcı, rol ve diğer kimlik tabloları veritabanında oluşturulur.

---

## 3. Program.cs – Identity Yapılandırması

### Identity Servisi

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

- `AddIdentity`: Identity sistemini etkinleştirir.
- `AddEntityFrameworkStores`: Identity verilerinin AppDbContext üzerinden saklanmasını sağlar.
- `AddDefaultTokenProviders`: Şifre sıfırlama ve e-posta doğrulama gibi işlemlerde kullanılabilecek token sağlayıcılarını ekler.

### Şifre Ayarları

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});
```

Şifre oluşturma kuralları belirlenir. Bu yapılandırmada minimum şifre uzunluğu 4 karakterdir.

### Cookie Ayarları

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login/index";
    options.AccessDeniedPath = "/admin/login/index";
});
```

- `LoginPath`: Giriş yapmamış kullanıcıların yönlendirileceği sayfa.
- `AccessDeniedPath`: Yetkisi olmayan kullanıcıların yönlendirileceği sayfa.

### AdminService Kaydı

```csharp
builder.Services.AddScoped<AdminService>();
```

AdminService, Dependency Injection sistemine eklenir.

---

## 4. AdminService – Admin Kullanıcısı Oluşturma

Admin kullanıcısı ve Admin rolü oluşturma işlemleri `Services/AdminService.cs` içerisinde gerçekleştirilir.

```csharp
using Microsoft.AspNetCore.Identity;

namespace CustomerBox.Services
{
    public class AdminService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Check()
        {
            // Sistemde kullanıcı varsa işlem yapma
            if (_userManager.Users.Any())
                return;

            // Admin rolü mevcut mu kontrol et
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
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

            // Kullanıcı oluşturulduysa Admin rolüne ata
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
```

### Çalışma Mantığı

1. Sistemde kullanıcı olup olmadığı kontrol edilir.
2. Kullanıcı varsa işlem sonlandırılır.
3. Kullanıcı yoksa Admin rolü kontrol edilir.
4. Admin rolü bulunmuyorsa oluşturulur.
5. Admin kullanıcısı oluşturulur.
6. Kullanıcı başarıyla oluşturulursa Admin rolüne atanır.

---

## 5. LoginController – Giriş ve İlk Kurulum İşlemleri

Admin giriş ve ilk kurulum işlemleri `LoginController` içerisinde gerçekleştirilir.

### İlk Kurulum – Setup

```csharp
[HttpGet]
public async Task<IActionResult> Setup()
{
    // Sistemde kullanıcı varsa kurulumu kapat
    if (_userManager.Users.Any())
        return NotFound();

    await _adminService.Check();

    return RedirectToAction("Index", "Login", new { area = "Admin" });
}
```

İlk kurulum şu adres üzerinden gerçekleştirilir:

`/admin/login/setup`

**Çalışma mantığı:**

1. Setup adresine istek gönderilir.
2. Veritabanında kullanıcı olup olmadığı kontrol edilir.
3. Kullanıcı varsa `404 Not Found` döndürülür.
4. Kullanıcı yoksa AdminService çalıştırılır.
5. Admin kullanıcısı ve Admin rolü oluşturulur.
6. İşlem tamamlandıktan sonra Login sayfasına yönlendirilir.
7. Sonraki Setup isteklerinde kullanıcı bulunduğu için kurulum adresi kapalı olur.

Böylece Admin kullanıcısı her Login sayfası açıldığında oluşturulmaya çalışılmaz. Kurulum yalnızca Setup adresi üzerinden gerçekleştirilir.

> Not: Bu yöntem normal tekrarlı istekleri engeller. Ancak aynı anda gelen isteklerde yarış durumu oluşabilir. Ayrıca ilk kurulum tamamlandıktan sonra Setup endpoint'ini projeden kaldırmak veya erişimini ayrıca kısıtlamak daha güvenlidir.

### Login Sayfası

```csharp
[HttpGet]
public IActionResult Index()
{
    if (User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Index", "Default", new { area = "Admin" });
    }

    return View();
}
```

Login sayfası açıldığında Admin oluşturma işlemi yapılmaz. Yalnızca kullanıcının oturum durumu kontrol edilir.

### Login İşlemi

```csharp
[HttpPost]
public async Task<IActionResult> Index(string userName, string password)
{
    if (!ModelState.IsValid)
        return View();

    var result = await _signInManager.PasswordSignInAsync(
        userName, password, true, false);

    if (result.Succeeded)
        return RedirectToAction("Index", "Default", new { area = "Admin" });

    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");

    return View();
}
```

Giriş işlemi başarılı olduğunda kullanıcı Admin paneline yönlendirilir.

---

## 6. AccountController – Hesap Yönetimi

Admin hesabına ait işlemler `AccountController` içerisinde gerçekleştirilir.

### Yetkilendirme

```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AccountController : Controller
{
}
```

Bu sayede yalnızca Admin rolüne sahip kullanıcılar hesap yönetimi sayfalarına erişebilir.

### Kullanıcı Bilgilerini Getirme

```csharp
var user = await _userManager.GetUserAsync(User);
```

Giriş yapan kullanıcının bilgileri alınır.

### Kullanıcı Adı Değiştirme

```csharp
var result = await _userManager.SetUserNameAsync(user, newUsername);
```

Yeni kullanıcı adı Identity üzerinden güncellenir.

### Şifre Değiştirme

```csharp
var result = await _userManager.ChangePasswordAsync(
    user,
    currentPassword,
    newPassword
);
```

Mevcut şifre doğruysa yeni şifre kaydedilir.

---

## 7. Middleware Yapılandırması

Program.cs içerisinde Authentication ve Authorization middleware'leri doğru sırayla eklenmelidir.

```csharp
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Default}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
```

- `UseAuthentication`: Kullanıcının oturumunu doğrular.
- `UseAuthorization`: Kullanıcının erişim yetkisini kontrol eder.

---

## 8. Proje Klasör Yapısı

```text
CustomerBox
│
├── Areas
│   └── Admin
│       ├── Controllers
│       │   ├── LoginController.cs
│       │   ├── AccountController.cs
│       │   └── DefaultController.cs
│       │
│       └── Views
│           ├── Login
│           ├── Account
│           └── Default
│
├── Data
│   └── AppDbContext.cs
│
├── Services
│   └── AdminService.cs
│
├── Program.cs
└── README.md
```

---

## 9. Önemli Güvenlik Notları

- `admin123` örnek şifredir. Canlı ortamda güçlü ve güvenli bir başlangıç şifresi kullanılmalıdır.
- Setup adresi ilk kurulumdan sonra erişime kapatılmalıdır.
- Setup işlemi için Rate Limiting uygulanması önerilir.
- Admin hesabı oluşturma işleminin başarılı olup olmadığı kontrol edilmelidir.
- Admin şifresi kaynak kod içerisinde saklanmamalıdır.

---

## Sonuç

CustomerBox projesinde ASP.NET Core Identity kullanılarak Admin kullanıcı yönetimi, giriş, yetkilendirme ve hesap işlemleri merkezi bir yapıda yönetilmektedir.

İlk Admin kullanıcısı `/admin/login/setup` adresinden oluşturulur. Kurulum tamamlandıktan sonra sistemde kullanıcı bulunduğu için bu adres tekrar kullanılamaz.