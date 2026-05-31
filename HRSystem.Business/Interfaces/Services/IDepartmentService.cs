using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Departments;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IDepartmentService
{
    Task<PagedResult<Department>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Department>> GetDeletedAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<Department> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(int departmentId, CancellationToken cancellationToken = default);

    Task RestoreAsync(int departmentId, CancellationToken cancellationToken = default);

    Task ReplaceManagerAsync(int departmentId, int newManagerId, CancellationToken cancellationToken = default);
}
