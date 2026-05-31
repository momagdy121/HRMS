using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly AppDbContext _context;

    public PayrollRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Payroll?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Payrolls.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsForEmployeeMonthYearAsync(int employeeId, int month, int year, CancellationToken cancellationToken = default) =>
        _context.Payrolls.AnyAsync(
            p => p.EmployeeId == employeeId && p.Month == month && p.Year == year,
            cancellationToken);

    public Task<PagedList<Payroll>> GetByEmployeePagedAsync(int employeeId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Payrolls
            .AsNoTracking()
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Payroll>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Payrolls
            .AsNoTracking()
            .Where(p => _context.Employees.Any(e => e.Id == p.EmployeeId && e.DepartmentId == departmentId && !e.IsDeleted))
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Payroll>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Payrolls
            .AsNoTracking()
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public async Task AddAsync(Payroll payroll, CancellationToken cancellationToken = default) =>
        await _context.Payrolls.AddAsync(payroll, cancellationToken);

    public void Update(Payroll payroll) => _context.Payrolls.Update(payroll);
}
