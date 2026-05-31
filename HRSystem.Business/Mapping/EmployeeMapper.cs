using HRSystem.Business.DTOs.Employees;
using HRSystem.Common.Constants;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class EmployeeMapper
{
    public static Employee FromDto(CreateEmployeeDto dto) =>
        new()
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId,
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            IsHR = dto.Role == RoleNames.HR,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

    public static void UpdateFromDto(Employee employee, UpdateEmployeeDto dto)
    {
        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.Email = dto.Email.Trim();
        employee.DepartmentId = dto.DepartmentId;
        employee.Salary = dto.Salary;
        employee.HireDate = dto.HireDate;
        employee.IsActive = dto.IsActive;
    }
}
