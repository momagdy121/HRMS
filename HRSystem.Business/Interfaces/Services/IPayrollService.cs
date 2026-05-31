using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Payroll;
using HRSystem.Common.Enums;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IPayrollService
{
    Task<Payroll> ProcessAsync(ProcessPayrollDto dto, CancellationToken cancellationToken = default);

    Task<PayrollItem> AddPayrollItemAsync(AddPayrollItemDto dto, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(int payrollId, PayrollStatus status, CancellationToken cancellationToken = default);

    Task<PagedResult<Payroll>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Payroll>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Payroll>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
