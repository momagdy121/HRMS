using HRSystem.Business;
using HRSystem.Data;
using HRSystem.Data.Context;
using HRSystem.Data.Models;
using HRSystem.Web.DataSeeding;
using HRSystem.Web.Filters;
using HRSystem.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database & Application Services ──────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHrmsDataServices();
builder.Services.AddHrmsBusinessServices(builder.Configuration);

// ── Identity & Security ──────────────────────────────────────────
builder.Services
    .AddIdentity<Employee, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ── MVC & Filters ────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<HrmsExceptionFilter>();
    options.Filters.AddService<RequirePasswordChangeFilter>();
});
builder.Services.AddScoped<HrmsExceptionFilter>();
builder.Services.AddScoped<RequirePasswordChangeFilter>();

var app = builder.Build();

// ── Data Seeding ─────────────────────────────────────────────────
await app.SeedIdentityDataAsync();

// ── HTTP Pipeline ────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area routing (must precede default route)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
