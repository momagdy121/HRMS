using HRSystem.Common.Constants;
using HRSystem.Business;
using HRSystem.Data;
using HRSystem.Data.Context;
using HRSystem.Data.Models;
using HRSystem.Web.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHrmsDataServices();
builder.Services.AddHrmsBusinessServices();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<HrmsExceptionFilter>();
});
builder.Services.AddScoped<HrmsExceptionFilter>();

var app = builder.Build();

await SeedRolesAsync(app.Services);
await SeedUsersAsync(app.Services);

// Configure the HTTP request pipeline.
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

// Area routing (must come before default)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task SeedRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    foreach (var roleName in RoleNames.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
    }
}

static async Task SeedUsersAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (await userManager.Users.AnyAsync())
        return;

    (int EmpId, string Email, string Pass, string Role)[] accounts =
    [
        (1, "admin@hr.com", "Admin@1234", RoleNames.HR),
        (2, "jane@hr.com", "Jane@1234", RoleNames.DepartmentHead),
        (3, "bob@hr.com", "Bob@1234", RoleNames.DepartmentHead),
        (4, "alice@it.com", "Alice@1234", RoleNames.Employee),
        (5, "charlie@hr.com", "Charlie@1234", RoleNames.Employee),
        (6, "diana@it.com", "Diana@1234", RoleNames.Employee),
        (7, "evan@hr.com", "Evan@1234", RoleNames.Employee),
        (8, "fiona@it.com", "Fiona@1234", RoleNames.Employee),
        (9, "george@hr.com", "George@1234", RoleNames.Employee),
        (10, "hannah@it.com", "Hannah@1234", RoleNames.Employee),
        (11, "ian@hr.com", "Ian@1234", RoleNames.Employee),
        (12, "julia@it.com", "Julia@1234", RoleNames.Employee),
        (13, "kevin@hr.com", "Kevin@1234", RoleNames.Employee),
        (14, "laura@it.com", "Laura@1234", RoleNames.Employee),
        (15, "michael@hr.com", "Michael@1234", RoleNames.Employee)
    ];

    foreach (var a in accounts)
    {
        var user = new ApplicationUser
        {
            UserName = a.Email,
            Email = a.Email,
            EmployeeId = a.EmpId,
            EmailConfirmed = true,
            IsPasswordChangeRequired = false
        };

        var result = await userManager.CreateAsync(user, a.Pass);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create user {a.Email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, a.Role);
    }
}
