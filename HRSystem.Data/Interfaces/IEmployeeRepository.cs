using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PagedList<Employee>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Employee>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Employee>> GetDeletedPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<bool> IsManagerOfAnyDepartmentAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    void Update(Employee employee);
}
