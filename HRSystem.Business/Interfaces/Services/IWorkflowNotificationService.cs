using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IWorkflowNotificationService
{
    Task NotifyLeaveSubmittedAsync(LeaveRequest request, Employee requester, CancellationToken cancellationToken = default);

    Task NotifyLeaveApprovedAsync(LeaveRequest request, Employee requester, CancellationToken cancellationToken = default);

    Task NotifyLeaveRejectedAsync(LeaveRequest request, Employee requester, string rejectionReason, CancellationToken cancellationToken = default);

    Task NotifyTaskAssignedAsync(EmployeeTask task, Employee assignee, Employee assigner, CancellationToken cancellationToken = default);

    Task NotifyTaskUpdatedAsync(EmployeeTask task, Employee assignee, Employee updater, CancellationToken cancellationToken = default);

    Task NotifyPayrollApprovedAsync(Payroll payroll, Employee employee, CancellationToken cancellationToken = default);

    Task NotifyBulkPayrollApprovedAsync(IEnumerable<(Payroll payroll, Employee employee)> items, CancellationToken cancellationToken = default);

    Task NotifyPayslipAvailableAsync(Payroll payroll, Employee employee, CancellationToken cancellationToken = default);

    Task NotifyBulkPayslipsAvailableAsync(IEnumerable<(Payroll payroll, Employee employee)> items, CancellationToken cancellationToken = default);
}
