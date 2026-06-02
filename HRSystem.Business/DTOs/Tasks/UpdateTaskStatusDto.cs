using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.DTOs.Tasks;

public class UpdateTaskStatusDto
{
    public int TaskId { get; set; }
    public EmployeeTaskStatus Status { get; set; }
    public string? CompletionNotes { get; set; }
}
