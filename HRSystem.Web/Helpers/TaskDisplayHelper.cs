using HRSystem.Data.Models;
using TaskStatus = HRSystem.Common.Enums.TaskStatus;

namespace HRSystem.Web.Helpers;

public static class TaskDisplayHelper
{
    public static string GetFullName(Employee employee) =>
        $"{employee.FirstName} {employee.LastName}";

    public static string GetInitials(Employee employee)
    {
        var first = string.IsNullOrEmpty(employee.FirstName) ? "?" : employee.FirstName[0].ToString();
        var last = string.IsNullOrEmpty(employee.LastName) ? "?" : employee.LastName[0].ToString();
        return $"{first}{last}".ToUpperInvariant();
    }

    public static bool IsOverdue(EmployeeTask task) =>
        task.DueDate.HasValue
        && task.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow)
        && task.Status != TaskStatus.Completed;

    public static IReadOnlyList<TaskStatus> GetAllowedNextStatuses(TaskStatus current) =>
        current switch
        {
            TaskStatus.Pending => [TaskStatus.InProgress],
            TaskStatus.InProgress => [TaskStatus.Completed],
            _ => []
        };

    public static string GetStatusLabel(TaskStatus status) =>
        status switch
        {
            TaskStatus.Pending => "Pending",
            TaskStatus.InProgress => "In Progress",
            TaskStatus.Completed => "Completed",
            _ => status.ToString()
        };

    public static string GetStatusBadgeClass(TaskStatus status) =>
        status switch
        {
            TaskStatus.Pending => "bg-slate-100 text-slate-600",
            TaskStatus.InProgress => "bg-blue-50 text-blue-700",
            TaskStatus.Completed => "bg-green-50 text-green-700",
            _ => "bg-slate-100 text-slate-600"
        };
}
