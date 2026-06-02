using System.ComponentModel.DataAnnotations;

namespace HRSystem.Data.Models;

public class EmployeeTask
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int AssignedById { get; set; }

    public int AssignedToId { get; set; }

    public global::HRSystem.Common.Enums.TaskStatus Status { get; set; } =
        global::HRSystem.Common.Enums.TaskStatus.Pending;

    public DateOnly? DueDate { get; set; }

    [MaxLength(2000)]
    public string? CompletionNotes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
