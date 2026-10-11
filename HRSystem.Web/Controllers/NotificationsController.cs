using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
    {
        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        var unreadCount = await _notificationService.GetUnreadCountAsync(employee.Id, cancellationToken);
        var recent = await _notificationService.GetRecentAsync(employee.Id, 10, cancellationToken);

        var items = recent.Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.LinkUrl,
            n.IsRead,
            CreatedAt = n.CreatedAt.ToString("o"),
            TimeAgo = GetTimeAgo(n.CreatedAt),
            Icon = GetIcon(n.Kind),
            IconColor = GetIconColor(n.Kind)
        });

        return Json(new { unreadCount, items });
    }

    [HttpPost("mark-read/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        await _notificationService.MarkAsReadAsync(id, employee.Id, cancellationToken);
        var unreadCount = await _notificationService.GetUnreadCountAsync(employee.Id, cancellationToken);
        return Json(new { success = true, unreadCount });
    }

    [HttpPost("mark-all-read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var employee = await _currentUser.GetCurrentEmployeeAsync(cancellationToken);
        await _notificationService.MarkAllReadAsync(employee.Id, cancellationToken);
        return Json(new { success = true, unreadCount = 0 });
    }

    private static string GetTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
        return dt.ToString("MMM dd");
    }

    private static string GetIcon(NotificationKind kind) => kind switch
    {
        NotificationKind.TaskAssigned => "assignment",
        NotificationKind.LeaveSubmitted => "event_note",
        NotificationKind.LeaveApproved => "check_circle",
        NotificationKind.LeaveRejected => "cancel",
        NotificationKind.PayslipAvailable => "payments",
        _ => "notifications"
    };

    private static string GetIconColor(NotificationKind kind) => kind switch
    {
        NotificationKind.TaskAssigned => "text-blue-500 bg-blue-50",
        NotificationKind.LeaveSubmitted => "text-amber-500 bg-amber-50",
        NotificationKind.LeaveApproved => "text-emerald-500 bg-emerald-50",
        NotificationKind.LeaveRejected => "text-rose-500 bg-rose-50",
        NotificationKind.PayslipAvailable => "text-indigo-500 bg-indigo-50",
        _ => "text-slate-500 bg-slate-50"
    };
}
