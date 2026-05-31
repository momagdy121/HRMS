using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IEmployeeTaskRepository
{
    Task<EmployeeTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<EmployeeTask>> GetByAssignedByPagedAsync(int assignedById, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<EmployeeTask>> GetByAssignedToPagedAsync(int assignedToId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> SoftDeleteAllForEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    Task AddAsync(EmployeeTask task, CancellationToken cancellationToken = default);

    void Update(EmployeeTask task);
}
