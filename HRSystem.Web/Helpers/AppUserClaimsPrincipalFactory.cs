using System.Security.Claims;
using HRSystem.Data.Context;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HRSystem.Web.Helpers;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Employee, IdentityRole<int>>
{
    private readonly AppDbContext _context;

    public AppUserClaimsPrincipalFactory(
        UserManager<Employee> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        AppDbContext context)
        : base(userManager, roleManager, optionsAccessor)
    {
        _context = context;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Employee user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrEmpty(fullName))
            fullName = user.UserName ?? user.Email ?? "User";

        identity.AddClaim(new Claim("FullName", fullName));
        identity.AddClaim(new Claim("FirstName", user.FirstName));
        identity.AddClaim(new Claim("LastName", user.LastName));
        identity.AddClaim(new Claim("DepartmentId", user.DepartmentId.ToString()));

        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == user.DepartmentId);

        if (department != null)
        {
            identity.AddClaim(new Claim("DepartmentName", department.Name));
        }

        return identity;
    }
}
