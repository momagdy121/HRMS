using HRSystem.Business.DTOs.Tasks;
using HRSystem.Data.Models;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.Mapping;

public static class EmployeeTaskMapper
{
    public static EmployeeTask FromDto(AssignTaskDto dto, int assignedById, int assignedToId) =>
        new()
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            AssignedById = assignedById,
            AssignedToId = assignedToId,
            Status = EmployeeTaskStatus.Pending,
            DueDate = dto.DueDate,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

    public static void UpdateFromDto(EmployeeTask task, EditTaskDto dto)
    {
        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.DueDate = dto.DueDate;
    }
}
