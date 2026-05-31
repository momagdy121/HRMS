using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Employees;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IEmployeeService
{
    Task<PagedResult<Employee>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Employee>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Employee>> GetDeletedAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<Employee> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateEmployeeDto dto, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(int employeeId, CancellationToken cancellationToken = default);

    Task RestoreAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<bool> CanDeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
