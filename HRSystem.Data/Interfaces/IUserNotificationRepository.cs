using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IUserNotificationRepository
{
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserNotification>> GetRecentAsync(int employeeId, int take, CancellationToken cancellationToken = default);

    Task<UserNotification?> GetByIdForEmployeeAsync(int id, int employeeId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(int employeeId, CancellationToken cancellationToken = default);

    void Update(UserNotification notification);
}
