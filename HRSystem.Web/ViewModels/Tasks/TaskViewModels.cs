using System.ComponentModel.DataAnnotations;
using TaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.ViewModels.Tasks;

public class TaskEmployeeOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TaskListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonInitials { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public TaskStatus Status { get; set; }
    public string? CompletionNotes { get; set; }
    public bool CanEdit { get; set; }
    public bool CanCancel { get; set; }
    public bool CanUpdateStatus { get; set; }
}

public class AssignTaskViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select an employee.")]
    [Display(Name = "Assign To")]
    public int AssignedToId { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateOnly? DueDate { get; set; }
}

public class EditTaskViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateOnly? DueDate { get; set; }
}

public class CancelTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AssigneeName { get; set; } = string.Empty;
}

public class UpdateTaskStatusViewModel : IValidatableObject
{
    public int Id { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public TaskStatus CurrentStatus { get; set; }

    [Required]
    [Display(Name = "New Status")]
    public TaskStatus Status { get; set; }

    [StringLength(2000)]
    [Display(Name = "Completion Details")]
    public string? CompletionNotes { get; set; }

    public IReadOnlyList<TaskStatus> AllowedNextStatuses { get; set; } = [];

    public bool RequiresCompletionNotes => Status == TaskStatus.Completed;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == TaskStatus.Completed && string.IsNullOrWhiteSpace(CompletionNotes))
        {
            yield return new ValidationResult(
                "Please provide completion details (e.g. link, token, or notes).",
                [nameof(CompletionNotes)]);
        }
    }
}

public class DeptHeadTaskIndexViewModel
{
    public IReadOnlyList<TaskListItemViewModel> Tasks { get; set; } = [];
    public IReadOnlyList<TaskEmployeeOptionViewModel> Assignees { get; set; } = [];
    public AssignTaskViewModel AssignForm { get; set; } = new();
    public EditTaskViewModel? EditForm { get; set; }
    public CancelTaskViewModel? CancelTarget { get; set; }
    public bool ShowAssignModal { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class EmployeeTaskIndexViewModel
{
    public IReadOnlyList<TaskListItemViewModel> Tasks { get; set; } = [];
    public UpdateTaskStatusViewModel? UpdateStatusForm { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
