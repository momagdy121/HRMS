using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IPayrollRepository
{
    Task<Payroll?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForEmployeeMonthYearAsync(int employeeId, int month, int year, CancellationToken cancellationToken = default);

    Task<PagedList<Payroll>> GetByEmployeePagedAsync(int employeeId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Payroll>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Payroll>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Payroll payroll, CancellationToken cancellationToken = default);

    void Update(Payroll payroll);
}
