using HRSystem.Data.Models;
using EmployeeTaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Business.Helpers;

public static class TaskWorkflow
{
    public static void UpdateStatus(EmployeeTask task, EmployeeTaskStatus status)
    {
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
    }

    public static void Cancel(EmployeeTask task)
    {
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
    }
}
