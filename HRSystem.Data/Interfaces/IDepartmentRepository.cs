using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Department?> GetByManagerIdAsync(int managerId, CancellationToken cancellationToken = default);

    Task<PagedList<Department>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Department>> GetDeletedPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountActiveEmployeesAsync(int departmentId, CancellationToken cancellationToken = default);

    Task AddAsync(Department department, CancellationToken cancellationToken = default);

    void Update(Department department);
}
