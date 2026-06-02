using HRSystem.Data.Models;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.Helpers;

public static class TaskWorkflow
{
    public static void UpdateStatus(EmployeeTask task, EmployeeTaskStatus status, string? completionNotes = null)
    {
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        if (status == EmployeeTaskStatus.Completed)
            task.CompletionNotes = completionNotes?.Trim();
    }

    public static void Cancel(EmployeeTask task)
    {
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
    }
}
