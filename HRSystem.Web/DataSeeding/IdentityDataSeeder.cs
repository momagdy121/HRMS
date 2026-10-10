using HRSystem.Common.Constants;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRSystem.Web.DataSeeding;

public static class IdentityDataSeeder
{
    private record SeedAccount(int EmpId, string Email, string Password, string Role);

    private static readonly SeedAccount[] DefaultAccounts =
    [
        new(1, "admin@hr.com", "Admin@1234", RoleNames.HR),
        new(2, "jane@hr.com", "Jane@1234", RoleNames.DepartmentHead),
        new(3, "bob@hr.com", "Bob@1234", RoleNames.DepartmentHead),
        new(4, "alice@it.com", "Alice@1234", RoleNames.Employee),
        new(5, "charlie@hr.com", "Charlie@1234", RoleNames.Employee),
        new(6, "diana@it.com", "Diana@1234", RoleNames.Employee),
        new(7, "evan@hr.com", "Evan@1234", RoleNames.Employee),
        new(8, "fiona@it.com", "Fiona@1234", RoleNames.Employee),
        new(9, "george@hr.com", "George@1234", RoleNames.Employee),
        new(10, "hannah@it.com", "Hannah@1234", RoleNames.Employee),
        new(11, "ian@hr.com", "Ian@1234", RoleNames.Employee),
        new(12, "julia@it.com", "Julia@1234", RoleNames.Employee),
        new(13, "kevin@hr.com", "Kevin@1234", RoleNames.Employee),
        new(14, "laura@it.com", "Laura@1234", RoleNames.Employee),
        new(15, "michael@hr.com", "Michael@1234", RoleNames.Employee)
    ];

    public static async Task SeedIdentityDataAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");

        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<Employee>>();

            await SeedRolesAsync(roleManager, logger);
            await SeedUsersAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding identity roles and users.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager, ILogger logger)
    {
        foreach (var roleName in RoleNames.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role: {Role}", roleName);
                }
                else
                {
                    logger.LogWarning("Failed to seed role {Role}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<Employee> userManager, ILogger logger)
    {
        foreach (var a in DefaultAccounts)
        {
            var user = await userManager.FindByIdAsync(a.EmpId.ToString());
            if (user is null)
                continue;

            var needsUpdate = false;

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                var addPassword = await userManager.AddPasswordAsync(user, a.Password);
                if (!addPassword.Succeeded)
                {
                    logger.LogWarning("Password set failed for {Email}: {Errors}",
                        a.Email, string.Join(", ", addPassword.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, a.Role))
            {
                await userManager.AddToRoleAsync(user, a.Role);
                logger.LogInformation("Assigned role {Role} to {Email}", a.Role, a.Email);
            }

            if (string.IsNullOrEmpty(user.UserName))
            {
                user.UserName = a.Email;
                user.NormalizedUserName = a.Email.ToUpperInvariant();
                needsUpdate = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await userManager.UpdateAsync(user);
            }
        }
    }
}
