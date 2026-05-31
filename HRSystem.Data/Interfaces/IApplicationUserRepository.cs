using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IApplicationUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<PagedList<UserAccountRow>> GetAllWithEmployeePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public sealed class UserAccountRow
{
    public ApplicationUser User { get; init; } = null!;
    public Employee Employee { get; init; } = null!;
}
