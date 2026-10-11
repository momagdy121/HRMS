using HRSystem.Business.DTOs.Email;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.Extensions.Logging;

namespace HRSystem.Business.Services;

public class WorkflowNotificationService : IWorkflowNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly IBackgroundEmailQueue _emailQueue;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowNotificationService> _logger;

    public WorkflowNotificationService(
        INotificationService notificationService,
        IBackgroundEmailQueue emailQueue,
        IUnitOfWork unitOfWork,
        ILogger<WorkflowNotificationService> logger)
    {
        _notificationService = notificationService;
        _emailQueue = emailQueue;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task NotifyLeaveSubmittedAsync(LeaveRequest request, Employee requester, CancellationToken cancellationToken = default)
    {
        var subject = $"Leave Request Submitted: {requester.FirstName} {requester.LastName}";
        var title = "New Leave Request";
        var message = $"{requester.FirstName} {requester.LastName} requested {request.LeaveType} leave ({request.StartDate:MMM dd} - {request.EndDate:MMM dd}).";
        var link = "/DepartmentHead/LeaveRequests";

        // Notify Department Head if present
        int? targetRecipientId = null;
        string? targetEmail = null;
        string? targetName = null;

        var department = await _unitOfWork.Departments.GetByIdAsync(requester.DepartmentId, cancellationToken);
        if (department != null && department.ManagerId > 0 && department.ManagerId != requester.Id)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(department.ManagerId, cancellationToken);
            if (manager != null)
            {
                targetRecipientId = manager.Id;
                targetEmail = manager.Email;
                targetName = $"{manager.FirstName} {manager.LastName}";
            }
        }

        // If no manager or requester is the manager, notify HR
        if (targetRecipientId == null)
        {
            link = "/HR/LeaveRequests";
            var hrEmployees = await _unitOfWork.Employees.GetActiveForPayrollAsync(null, cancellationToken);
            var hrAdmin = hrEmployees.FirstOrDefault(e => e.IsHR && e.Id != requester.Id);
            if (hrAdmin != null)
            {
                targetRecipientId = hrAdmin.Id;
                targetEmail = hrAdmin.Email;
                targetName = $"{hrAdmin.FirstName} {hrAdmin.LastName}";
            }
        }

        if (targetRecipientId.HasValue)
        {
            await _notificationService.CreateAsync(
                targetRecipientId.Value,
                NotificationKind.LeaveSubmitted,
                title,
                message,
                link,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(targetEmail))
            {
                var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #1e3a8a;'>New Leave Request Pending Approval</h2>
    <p>Hello <strong>{targetName}</strong>,</p>
    <p><strong>{requester.FirstName} {requester.LastName}</strong> has submitted a new <strong>{request.LeaveType}</strong> leave request:</p>
    <ul>
        <li><strong>Dates:</strong> {request.StartDate:MMMM dd, yyyy} to {request.EndDate:MMMM dd, yyyy}</li>
        <li><strong>Reason:</strong> {(string.IsNullOrWhiteSpace(request.Reason) ? "No reason specified" : request.Reason)}</li>
    </ul>
    <p>Please log in to your HRMS portal to review and take action.</p>
</div>";
                await TrySendEmailAsync(targetEmail, targetName ?? "Manager", subject, html, cancellationToken);
            }
        }
    }

    public async Task NotifyLeaveApprovedAsync(LeaveRequest request, Employee requester, CancellationToken cancellationToken = default)
    {
        var title = "Leave Request Approved";
        var message = $"Your {request.LeaveType} leave request for {request.StartDate:MMM dd} - {request.EndDate:MMM dd} was approved.";
        var link = "/Employee/Leave";

        await _notificationService.CreateAsync(
            requester.Id,
            NotificationKind.LeaveApproved,
            title,
            message,
            link,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(requester.Email))
        {
            var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #059669;'>Leave Request Approved 🎉</h2>
    <p>Hello <strong>{requester.FirstName}</strong>,</p>
    <p>Your <strong>{request.LeaveType}</strong> leave request from <strong>{request.StartDate:MMMM dd, yyyy}</strong> to <strong>{request.EndDate:MMMM dd, yyyy}</strong> has been approved.</p>
    <p>Have a great time away!</p>
</div>";
            await TrySendEmailAsync(requester.Email, $"{requester.FirstName} {requester.LastName}", "Leave Request Approved", html, cancellationToken);
        }
    }

    public async Task NotifyLeaveRejectedAsync(LeaveRequest request, Employee requester, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var title = "Leave Request Declined";
        var message = $"Your {request.LeaveType} leave ({request.StartDate:MMM dd} - {request.EndDate:MMM dd}) was declined. Reason: {rejectionReason}";
        var link = "/Employee/Leave";

        await _notificationService.CreateAsync(
            requester.Id,
            NotificationKind.LeaveRejected,
            title,
            message,
            link,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(requester.Email))
        {
            var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #dc2626;'>Leave Request Update</h2>
    <p>Hello <strong>{requester.FirstName}</strong>,</p>
    <p>Your <strong>{request.LeaveType}</strong> leave request from <strong>{request.StartDate:MMMM dd, yyyy}</strong> to <strong>{request.EndDate:MMMM dd, yyyy}</strong> could not be approved at this time.</p>
    <p><strong>Reason provided:</strong> {rejectionReason}</p>
    <p>If you have any questions, please speak directly with your department manager or HR.</p>
</div>";
            await TrySendEmailAsync(requester.Email, $"{requester.FirstName} {requester.LastName}", "Leave Request Update", html, cancellationToken);
        }
    }

    public async Task NotifyTaskAssignedAsync(EmployeeTask task, Employee assignee, Employee assigner, CancellationToken cancellationToken = default)
    {
        var title = "New Task Assigned";
        var dueStr = task.DueDate.HasValue ? $" Due: {task.DueDate.Value:MMM dd, yyyy}." : string.Empty;
        var message = $"'{task.Title}' assigned by {assigner.FirstName} {assigner.LastName}.{dueStr}";
        var link = "/Employee/Tasks";

        await _notificationService.CreateAsync(
            assignee.Id,
            NotificationKind.TaskAssigned,
            title,
            message,
            link,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(assignee.Email))
        {
            var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #2563eb;'>New Task Assigned: {task.Title}</h2>
    <p>Hello <strong>{assignee.FirstName}</strong>,</p>
    <p><strong>{assigner.FirstName} {assigner.LastName}</strong> has assigned you a new task:</p>
    <ul>
        <li><strong>Task:</strong> {task.Title}</li>
        <li><strong>Due Date:</strong> {(task.DueDate.HasValue ? task.DueDate.Value.ToString("MMMM dd, yyyy") : "Not set")}</li>
    </ul>
    <p>{task.Description}</p>
    <p>Log in to your HRMS dashboard to view and update task progress.</p>
</div>";
            await TrySendEmailAsync(assignee.Email, $"{assignee.FirstName} {assignee.LastName}", $"New Task: {task.Title}", html, cancellationToken);
        }
    }

    public async Task NotifyPayrollApprovedAsync(Payroll payroll, Employee employee, CancellationToken cancellationToken = default)
    {
        var title = "Payroll Approved";
        var period = $"{new DateTime(payroll.Year, payroll.Month, 1):MMMM yyyy}";
        var message = $"Your payroll for {period} has been approved and is being processed for payment.";
        var link = $"/Employee/Payroll/Payslip/{payroll.Id}";

        // In-app notification only — no email at this stage
        await _notificationService.CreateAsync(
            employee.Id,
            NotificationKind.PayrollApproved,
            title,
            message,
            link,
            cancellationToken);
    }

    public async Task NotifyBulkPayrollApprovedAsync(IEnumerable<(Payroll payroll, Employee employee)> items, CancellationToken cancellationToken = default)
    {
        var notifications = new List<UserNotification>();
        foreach (var (payroll, employee) in items)
        {
            var period = $"{new DateTime(payroll.Year, payroll.Month, 1):MMMM yyyy}";
            notifications.Add(new UserNotification
            {
                EmployeeId = employee.Id,
                Kind = NotificationKind.PayrollApproved,
                Title = "Payroll Approved",
                Message = $"Your payroll for {period} has been approved and is being processed for payment.",
                LinkUrl = $"/Employee/Payroll/Payslip/{payroll.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (notifications.Count > 0)
        {
            await _notificationService.CreateManyAsync(notifications, cancellationToken);
        }
    }

    public async Task NotifyPayslipAvailableAsync(Payroll payroll, Employee employee, CancellationToken cancellationToken = default)
    {
        var title = "Monthly Payslip Ready";
        var period = $"{new DateTime(payroll.Year, payroll.Month, 1):MMMM yyyy}";
        var message = $"Your payslip for {period} is now available. Net pay: ${payroll.NetSalary:N2}.";
        var link = $"/Employee/Payroll/Payslip/{payroll.Id}";

        await _notificationService.CreateAsync(
            employee.Id,
            NotificationKind.PayslipAvailable,
            title,
            message,
            link,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #1e3a8a;'>Your Payslip is Ready</h2>
    <p>Hello <strong>{employee.FirstName}</strong>,</p>
    <p>Your monthly payslip for <strong>{period}</strong> has been processed and payment has been completed.</p>
    <p><strong>Net Amount:</strong> ${payroll.NetSalary:N2}</p>
    <p>You can view and print your full payslip breakdown in the HRMS Portal.</p>
</div>";
            await TrySendEmailAsync(employee.Email, $"{employee.FirstName} {employee.LastName}", $"Payslip Ready - {period}", html, cancellationToken);
        }
    }

    public async Task NotifyBulkPayslipsAvailableAsync(IEnumerable<(Payroll payroll, Employee employee)> items, CancellationToken cancellationToken = default)
    {
        var notifications = new List<UserNotification>();
        var emailMessages = new List<EmailMessage>();

        foreach (var (payroll, employee) in items)
        {
            var period = $"{new DateTime(payroll.Year, payroll.Month, 1):MMMM yyyy}";
            notifications.Add(new UserNotification
            {
                EmployeeId = employee.Id,
                Kind = NotificationKind.PayslipAvailable,
                Title = "Monthly Payslip Ready",
                Message = $"Your payslip for {period} is now available. Net pay: ${payroll.NetSalary:N2}.",
                LinkUrl = $"/Employee/Payroll/Payslip/{payroll.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var html = $@"
<div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
    <h2 style='color: #1e3a8a;'>Your Payslip is Ready</h2>
    <p>Hello <strong>{employee.FirstName}</strong>,</p>
    <p>Your monthly payslip for <strong>{period}</strong> has been processed and payment has been completed.</p>
    <p><strong>Net Amount:</strong> ${payroll.NetSalary:N2}</p>
    <p>You can view and print your full payslip breakdown in the HRMS Portal.</p>
</div>";
                emailMessages.Add(new EmailMessage
                {
                    ToEmail = employee.Email,
                    ToName = $"{employee.FirstName} {employee.LastName}",
                    Subject = $"Payslip Ready - {period}",
                    HtmlContent = html
                });
            }
        }

        if (notifications.Count > 0)
        {
            await _notificationService.CreateManyAsync(notifications, cancellationToken);
        }

        if (emailMessages.Count > 0)
        {
            try
            {
                await _emailQueue.QueueEmailsAsync(emailMessages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enqueue bulk payslip emails. Notifications were preserved.");
            }
        }
    }

    private async Task TrySendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        try
        {
            await _emailQueue.QueueEmailAsync(new EmailMessage
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = subject,
                HtmlContent = htmlContent
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not enqueue email to {Email}. In-app notification was preserved.", toEmail);
        }
    }
}
