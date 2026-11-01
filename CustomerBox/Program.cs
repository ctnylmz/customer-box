using CustomerBox;
using CustomerBox.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ENV
Env.Load();

var builder = WebApplication.CreateBuilder(args);


// Mysql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 0, 0))));


// Add services to the container.
builder.Services.AddControllersWithViews();


// Identity ekliyoruz
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false; // Rakam gereksinimi kapal?
    options.Password.RequiredLength = 4;   // Minimum 4 karakter
    options.Password.RequireNonAlphanumeric = false; // Özel karakter gereksinimi kapal?
    options.Password.RequireUppercase = false; // Büyük harf gereksinimi kapal?
    options.Password.RequireLowercase = false; // Küçük harf gereksinimi kapal?
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login/index";
    options.AccessDeniedPath = "/admin/login/index";
});
// Identity Finish

// Servies
builder.Services.AddScoped<AdminService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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


app.Run();
