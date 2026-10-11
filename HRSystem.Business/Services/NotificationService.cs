using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;

namespace HRSystem.Business.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserNotification> CreateAsync(
        int employeeId,
        NotificationKind kind,
        string title,
        string message,
        string? linkUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new UserNotification
        {
            EmployeeId = employeeId,
            Kind = kind,
            Title = title.Trim(),
            Message = message.Trim(),
            LinkUrl = linkUrl?.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserNotifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public Task<int> GetUnreadCountAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _unitOfWork.UserNotifications.GetUnreadCountAsync(employeeId, cancellationToken);

    public Task<IReadOnlyList<UserNotification>> GetRecentAsync(int employeeId, int take = 10, CancellationToken cancellationToken = default) =>
        _unitOfWork.UserNotifications.GetRecentAsync(employeeId, take, cancellationToken);

    public async Task MarkAsReadAsync(int notificationId, int employeeId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.UserNotifications.GetByIdForEmployeeAsync(notificationId, employeeId, cancellationToken);
        if (notification == null || notification.IsRead)
            return;

        notification.IsRead = true;
        _unitOfWork.UserNotifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task MarkAllReadAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _unitOfWork.UserNotifications.MarkAllReadAsync(employeeId, cancellationToken);
}
