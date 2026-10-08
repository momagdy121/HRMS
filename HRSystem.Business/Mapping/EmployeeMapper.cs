using HRSystem.Business.DTOs.Employees;
using HRSystem.Common.Constants;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class EmployeeMapper
{
    public static Employee FromDto(CreateEmployeeDto dto) =>
        new()
        {
            UserName = dto.Email.Trim(),
            NormalizedUserName = dto.Email.Trim().ToUpperInvariant(),
            Email = dto.Email.Trim(),
            NormalizedEmail = dto.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DepartmentId = dto.DepartmentId,
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            IsHR = dto.Role == RoleNames.HR,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            IsPasswordChangeRequired = true
        };

    public static void UpdateFromDto(Employee employee, UpdateEmployeeDto dto)
    {
        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.Email = dto.Email.Trim();
        employee.NormalizedEmail = dto.Email.Trim().ToUpperInvariant();
        employee.UserName = dto.Email.Trim();
        employee.NormalizedUserName = dto.Email.Trim().ToUpperInvariant();
        employee.DepartmentId = dto.DepartmentId;
        employee.Salary = dto.Salary;
        employee.HireDate = dto.HireDate;
        employee.IsActive = dto.IsActive;
    }
}
