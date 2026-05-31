using HRSystem.Business.DTOs.UserAccounts;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class UserAccountMapper
{
    public static UserAccountListItemDto ToDto(UserAccountRow row, string role) =>
        new()
        {
            UserId = row.User.Id,
            EmployeeId = row.Employee.Id,
            FullName = ToFullName(row.Employee),
            Email = row.User.Email ?? row.Employee.Email,
            Role = role,
            IsPasswordChangeRequired = row.User.IsPasswordChangeRequired,
            IsActive = row.Employee.IsActive,
            IsEmployeeDeleted = row.Employee.IsDeleted
        };

    public static string ToFullName(Employee employee) =>
        $"{employee.FirstName} {employee.LastName}";
}
