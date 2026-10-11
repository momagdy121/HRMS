using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface INotificationService
{
    Task<UserNotification> CreateAsync(
        int employeeId,
        NotificationKind kind,
        string title,
        string message,
        string? linkUrl = null,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserNotification>> GetRecentAsync(int employeeId, int take = 10, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int notificationId, int employeeId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(int employeeId, CancellationToken cancellationToken = default);
}
