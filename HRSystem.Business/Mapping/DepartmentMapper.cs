using HRSystem.Business.DTOs.Departments;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class DepartmentMapper
{
    public static Department FromDto(CreateDepartmentDto dto) =>
        new()
        {
            Name = dto.Name.Trim(),
            ManagerId = dto.ManagerId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

    public static void UpdateFromDto(Department department, UpdateDepartmentDto dto) =>
        department.Name = dto.Name.Trim();
}
