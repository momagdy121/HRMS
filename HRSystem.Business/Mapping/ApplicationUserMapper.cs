using HRSystem.Business.DTOs.UserAccounts;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class ApplicationUserMapper
{
    public static ApplicationUser FromDto(CreateApplicationUserDto dto) =>
        new()
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            EmployeeId = dto.EmployeeId,
            EmailConfirmed = true,
            IsPasswordChangeRequired = true
        };
}
