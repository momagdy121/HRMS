using HRSystem.Business.DTOs.UserAccounts;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class UserAccountMapper
{
    public static UserAccountListItemDto ToDto(Employee employee, string role) =>
        new()
        {
            UserId = employee.Id,
            EmployeeId = employee.Id,
            FullName = ToFullName(employee),
            Email = employee.Email ?? string.Empty,
            Role = role,
            IsPasswordChangeRequired = employee.IsPasswordChangeRequired,
            IsActive = employee.IsActive,
            IsEmployeeDeleted = employee.IsDeleted
        };

    public static string ToFullName(Employee employee) =>
        $"{employee.FirstName} {employee.LastName}";
}
