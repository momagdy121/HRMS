using HRSystem.Business.DTOs.Employees;
using HRSystem.Common.Constants;
using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class EmployeeLifecycle
{
    public static Employee CreateManagerValidationCandidate(CreateEmployeeDto dto) =>
        new()
        {
            DepartmentId = dto.DepartmentId,
            IsHR = dto.Role == RoleNames.HR,
            IsActive = true,
            IsDeleted = false
        };

    public static void MarkDeleted(Employee employee)
    {
        employee.IsDeleted = true;
        employee.IsActive = false;
    }

    public static void MarkRestored(Employee employee)
    {
        employee.IsDeleted = false;
        employee.IsActive = true;
    }
}
