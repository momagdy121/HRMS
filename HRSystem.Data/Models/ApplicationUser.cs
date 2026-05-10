using Microsoft.AspNetCore.Identity;

namespace HRSystem.Data.Models;

public class ApplicationUser : IdentityUser<int>
{
    public int EmployeeId { get; set; }

    public bool IsPasswordChangeRequired { get; set; } = true;
}
