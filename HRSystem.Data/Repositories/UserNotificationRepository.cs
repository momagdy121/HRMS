using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class UserNotificationRepository : IUserNotificationRepository
{
    private readonly AppDbContext _context;

    public UserNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        _context.UserNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<int> GetUnreadCountAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.UserNotifications.CountAsync(n => n.EmployeeId == employeeId && !n.IsRead, cancellationToken);

    public async Task<IReadOnlyList<UserNotification>> GetRecentAsync(int employeeId, int take, CancellationToken cancellationToken = default) =>
        await _context.UserNotifications
            .AsNoTracking()
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<UserNotification?> GetByIdForEmployeeAsync(int id, int employeeId, CancellationToken cancellationToken = default) =>
        _context.UserNotifications.FirstOrDefaultAsync(n => n.Id == id && n.EmployeeId == employeeId, cancellationToken);

    public Task MarkAllReadAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.UserNotifications
            .Where(n => n.EmployeeId == employeeId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

    public void Update(UserNotification notification) =>
        _context.UserNotifications.Update(notification);
}
